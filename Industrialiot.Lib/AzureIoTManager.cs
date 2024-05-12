using Industrialiot.Lib.Data;
using Microsoft.Azure.Devices.Client;

namespace IndustrialiotConsole
{
    class AzureIoTManager {

        string _connectionString;
        Dictionary<string, DeviceClient> _devices;

        public AzureIoTManager(string connectionString, List<DeviceIdentifier> devices)
        {
            _connectionString = connectionString;
            _devices = new Dictionary<string, DeviceClient>();

            foreach(DeviceIdentifier device in devices)
            {
                var client = DeviceClient.CreateFromConnectionString(_connectionString + device.azureDeviceConnection);

                if (client == null)
                {
                    Console.Error.WriteLine($"DEVICE: {1} CAN'T BE GETTED FROM AZURE", device.deviceName);
                    continue;
                }

                _devices.Add(device.deviceName, client);
            }
        }

        public async Task sendMessage(Message message, string deviceId) {
            var deviceClient = _devices[deviceId];

            if (deviceClient == null)
            {
                Console.Error.WriteLine("DEVICE: {1} ISN'T IN AZURE");
                return;
            }

            await deviceClient.SendEventAsync(message);
        }

    }
}
