using Industrialiot.Lib.Data;
using IndustrialiotConsole;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;


namespace Industrialiot.Lib
{

    public class DeviceManager
    {
        const int DELAY_TIME = 10;

        OpcManager _opcManager;
        AzureIoTManager _azureIotManager;

        List<string> _deviceNames;

        CancellationTokenSource? _cancellationTokenSource;

        public DeviceManager(string opcConnectionString, string azureConnectionString, List<DeviceIdentifier> deviceIdentifierList)
        {
            _opcManager = new OpcManager(opcConnectionString, deviceIdentifierList);
            Console.WriteLine("opcCreated");
            _azureIotManager = new AzureIoTManager(azureConnectionString, deviceIdentifierList);

            _deviceNames = deviceIdentifierList.Select(device => device.deviceName).ToList();

            SetDesiredProductionRateOnMachines();
        }

        public async void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await PeiodicSendingMetadata(TimeSpan.FromSeconds(DELAY_TIME));
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
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

            _opcManager.Connect();

            foreach (var deviceName in _deviceNames)
            {
                var deviceMetadata = _opcManager.GetDeviceMetadata(deviceName);

                var dataString = JsonConvert.SerializeObject(deviceMetadata);
                Message msg = new Message(Encoding.UTF8.GetBytes(dataString));

                var task =  _azureIotManager.sendMessage(msg, deviceName);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            _opcManager.Disconnect();
        }

        public async void SetDesiredProductionRateOnMachines()
        {
            foreach (var deviceName in _deviceNames)
            {
                var desired = await _azureIotManager.GetTwinDesiredProps(deviceName);

                if (!desired.Contains("productionRate"))
                {
                    continue;
                }

                var desiredProductionRate = desired["productionRate"].Value;

                _opcManager.SetDeviceProductionRate(deviceName, (int)desiredProductionRate);
            }
        }
    }
}
