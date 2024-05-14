using Industrialiot.Lib.Data;
using IndustrialiotConsole;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace Industrialiot.Lib
{
    public partial class DevicesManager
    {
        const int DELAY_TIME_IN_SECONDS = 10;

        OpcManager _opcManager;
        AzureIoTManager _azureIotManager;

        List<string> _deviceNames;

        CancellationTokenSource? _cancellationTokenSource;

        public DevicesManager(string opcConnectionString, string azureConnectionString, List<DeviceIdentifier> deviceIdentifierList)
        {
            _opcManager = new OpcManager(opcConnectionString, deviceIdentifierList);
            _azureIotManager = new AzureIoTManager(azureConnectionString, deviceIdentifierList);

            _deviceNames = deviceIdentifierList.Select(device => device.deviceName).ToList();
        }

        public async void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _opcManager.Connect();

            SubscribeToDataNodeChanges();

            SetDesiredProductionRateOnMachines();
            SetDirectMethodsForAllDevices();

            await PeiodicSendingMetadata(TimeSpan.FromSeconds(DELAY_TIME_IN_SECONDS));
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

                var task = _azureIotManager.sendMessage(msg, deviceName);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }


        private void SubscribeToDataNodeChanges()
        {
            var list = new List<OpcDataChangeHandlerMapper>{
                new("ProductionRate",ProductionRateChangeHandler),
                new("DeviceError",DeviceErrorChangeHandler)};

            foreach (var deviceName in _deviceNames)
                _opcManager.SubscribeToNodeDataChange(deviceName,list);
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

        public async void SetDesiredProductionRateOnMachines()
        {
            foreach (var deviceName in _deviceNames)
            {
                var desired = await _azureIotManager.GetTwinDesiredProps(deviceName);

                if (!desired.Contains("productionRate")) continue;

                var desiredProductionRate = desired["productionRate"].Value;

                _opcManager.SetDeviceNodeData(deviceName, "ProductionRate", (int)desiredProductionRate);
            }
        }
    }
}
