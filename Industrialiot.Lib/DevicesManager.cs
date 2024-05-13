using Industrialiot.Lib.Data;
using IndustrialiotConsole;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Opc.UaFx.Client;
using System.Text;


namespace Industrialiot.Lib
{
    public class DeviceManager
    {
        const int DELAY_TIME_IN_SECONDS = 10;

        OpcManager _opcManager;
        AzureIoTManager _azureIotManager;

        List<string> _deviceNames;

        CancellationTokenSource? _cancellationTokenSource;

        public DeviceManager(string opcConnectionString, string azureConnectionString, List<DeviceIdentifier> deviceIdentifierList)
        {
            _opcManager = new OpcManager(opcConnectionString, deviceIdentifierList);
            _azureIotManager = new AzureIoTManager(azureConnectionString, deviceIdentifierList);

            _deviceNames = deviceIdentifierList.Select(device => device.deviceName).ToList();
        }

        public async void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _opcManager.Connect();

            SubscribeDeviceErrorsChange();
            SubsribeProductionRateChange();

            SetDesiredProductionRateOnMachines();

            SetDirectMethodsForAllDevices();

            //await PeiodicSendingMetadata(TimeSpan.FromSeconds(DELAY_TIME_IN_SECONDS));
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _opcManager?.Disconnect();
        }

        private async Task PeiodicSendingMetadata(TimeSpan interval)
        {
            while (true)
            {
                await SendMachinesMetadata();
                await Task.Delay(interval, _cancellationTokenSource!.Token);
            }
        }

        public async Task SendMachinesMetadata()
        {
            var tasks = new List<Task>();

            foreach (var deviceName in _deviceNames)
            {
                var deviceMetadata = _opcManager.GetDeviceMetadata(deviceName);

                var dataString = JsonConvert.SerializeObject(deviceMetadata);
                Message msg = new Message(Encoding.UTF8.GetBytes(dataString));

                var task =  _azureIotManager.sendMessage(msg, deviceName);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

       
        public void SubsribeProductionRateChange()
        {
            foreach (var deviceName in _deviceNames)
                _opcManager.SubscribeToProductionRateChange(deviceName, ProductionRateChangeHandler); 
        }

        public void SubscribeDeviceErrorsChange()
        {
            foreach (var deviceName in _deviceNames)
                _opcManager.SubscribeToDeviceErrorChange(deviceName, DeviceErrorChangeHandler);
        }

        private async void ProductionRateChangeHandler(object sender, OpcDataChangeReceivedEventArgs e)
        {
            var newValue = e.Item.Value.Value;
            var deviceName = ((OpcMonitoredItem)sender).Tag.ToString();
            await _azureIotManager.SetTwinReportedProp(deviceName!, "productionRate", newValue);
        }

        private async void DeviceErrorChangeHandler(object sender, OpcDataChangeReceivedEventArgs e)
        {
            var newValue = e.Item.Value.Value;
            var deviceName = ((OpcMonitoredItem)sender).Tag.ToString();

            DeviceErrors errors = (DeviceErrors)newValue;
            var strErrors = DeviceErrorsConverter.ToMessage(errors);
            Message msg = new Message(Encoding.UTF8.GetBytes(strErrors));

            await _azureIotManager.sendMessage(msg, deviceName!);
            await _azureIotManager.SetTwinReportedProp(deviceName!, "deviceErrors", newValue);
        }

        public async void SetDesiredProductionRateOnMachines()
        {
            foreach (var deviceName in _deviceNames)
            {
                var desired = await _azureIotManager.GetTwinDesiredProps(deviceName);

                if (!desired.Contains("productionRate")) continue;

                var desiredProductionRate = desired["productionRate"].Value;

                _opcManager.SetDeviceVariable(deviceName,"ProductionRate",(int)desiredProductionRate);
            }
        }

        private async void SetDirectMethodsForAllDevices()
        {
            var tasks = new List<Task>();
            
            foreach (var deviceName in _deviceNames)
            {
                tasks.Add(_azureIotManager.SetDirectMethodHandler(deviceName, "emergencyStop", CallEmergencyStop));
                tasks.Add(_azureIotManager.SetDirectMethodHandler(deviceName, "resetErrorStatus", CallReserErrorStatus));
            }

            await Task.WhenAll(tasks);
        }

        private async Task<MethodResponse> CallEmergencyStop(MethodRequest methodRequest, object userContext)
        {
            var deviceName =  ((MethodUserContext)userContext).deviceName;

            await Task.Run(() => _opcManager.CallDeviceMethod(deviceName, "EmergencyStop"));

            return new MethodResponse(200);
        }

        private async Task<MethodResponse> CallReserErrorStatus(MethodRequest methodRequest, object userContext)
        {
            var deviceName = ((MethodUserContext)userContext).deviceName;

            await Task.Run(() => _opcManager.CallDeviceMethod(deviceName, "ResetErrorStatus"));

            return new MethodResponse(200);
        }
    }
}
