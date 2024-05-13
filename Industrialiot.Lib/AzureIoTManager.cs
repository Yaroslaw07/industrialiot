using Industrialiot.Lib.Data;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace IndustrialiotConsole
{
    class AzureIoTManager {

        string _connectionString;
        readonly Dictionary<string, DeviceClient> _devices;

        public AzureIoTManager(string connectionString, List<DeviceIdentifier> devices)
        {
            _connectionString = connectionString;
            _devices = new Dictionary<string, DeviceClient>();

            foreach(DeviceIdentifier device in devices)
            {
                var client = DeviceClient.CreateFromConnectionString(_connectionString + device.azureDeviceConnection, TransportType.Mqtt);

                if (client == null)
                {
                    Console.Error.WriteLine($"DEVICE: {device.deviceName} CAN'T BE GETTED FROM AZURE");
                    continue;
                }

                _devices.Add(device.deviceName, client);
            }
        }

        private DeviceClient GetDeviceClient(string deviceName) {
            if (_devices.ContainsKey(deviceName))
                return _devices[deviceName];
            else
                throw new Exception($"No Azure device client for {deviceName}");
        }

        public async Task sendMessage(Message message, string deviceName) {
            var deviceClient = GetDeviceClient(deviceName);

            await deviceClient.SendEventAsync(message);
        }

        public async Task<TwinCollection> GetTwinDesiredProps(string deviceName)
        {
            var deviceClient = GetDeviceClient(deviceName);
            var twin = await deviceClient.GetTwinAsync();

            return twin.Properties.Desired;
        }

        public async Task SetTwinReportedProp(string deviceName, string property, dynamic value) 
        {
            var deviceClient = GetDeviceClient(deviceName);

            var reported = new TwinCollection();
            reported[property] = value;

            await deviceClient.UpdateReportedPropertiesAsync(reported);
        }

        public async Task SetDirectMethodHandler(string deviceName, string methodName, MethodCallback handler)
        {
            var deviceClient = GetDeviceClient(deviceName);

            await deviceClient.SetMethodHandlerAsync(methodName, handler, new MethodUserContext(deviceName));
        }
    }
}
