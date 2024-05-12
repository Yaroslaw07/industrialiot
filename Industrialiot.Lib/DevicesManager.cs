using Industrialiot.Lib.Data;
using IndustrialiotConsole;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace Industrialiot.Lib
{
    public class DeviceManager
    {
        OpcManager _opcManager;
        AzureIoTManager _azureIotManager;

        List<string> _deviceNames;

        public DeviceManager(string opcConnectionString, string azureConnectionString, List<DeviceIdentifier> deviceIdentifierList)
        {
            _opcManager = new OpcManager(opcConnectionString, deviceIdentifierList);
            Console.WriteLine("opcCreated");
            _azureIotManager = new AzureIoTManager(azureConnectionString, deviceIdentifierList);

            _deviceNames = deviceIdentifierList.Select(device => device.deviceName).ToList();

            PeiodicSendingMetadata(TimeSpan.FromSeconds(10));
        }

        private async Task PeiodicSendingMetadata(TimeSpan interval)
        {
            while (true)
            {
                await SendMachinesMetadata();
                await Task.Delay(interval);
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
    }
}
