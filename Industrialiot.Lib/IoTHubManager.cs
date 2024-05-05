using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;

namespace IndustrialiotConsole
{
    public class IoTHubManager
    {
        private readonly ServiceClient _client;
        private readonly RegistryManager _registryManager;

        public IoTHubManager(ServiceClient client, RegistryManager registryManager) { 
            _client = client;
            _registryManager = registryManager;
        }

        public async Task<Device> CreateDeviceAsync(string deviceId)
        {
            var device = new Device(deviceId);
            try
            {
                device = await _registryManager.AddDeviceAsync(device);
                Console.WriteLine("Device created successfully");
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceId);
                Console.WriteLine("Device already exists");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating device: " + ex.Message);
            }

            return device;
        }

        public async Task SendMessageAsync(Message message, string deviceId)
        {
            message.MessageId = Guid.NewGuid().ToString();
            await _client.SendAsync(deviceId, message);
        }

        public async Task<Twin> GetDeviceTwinAsync(string deviceId)
        {
            var twin =  await _registryManager.GetTwinAsync(deviceId);
            return twin;
        }

        public async Task SetDeviceTwinDesiredPropAsync(string deviceId, string propName, string propValue)
        {   
            var twin = await GetDeviceTwinAsync(deviceId);
            twin.Properties.Desired[propName] = propValue;
            await _registryManager.UpdateTwinAsync(twin.DeviceId,twin,twin.ETag);
        } 

        public async Task SetDeviceTwinRepotedPropAsync(string deviceId, string propName, string propValue)
        {
            var twin = await GetDeviceTwinAsync(deviceId);
            twin.Properties.Reported[propName] = propValue;
            await _registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
        }


        public async Task DeleteDeviceAsync(string deviceId)
        {
            try
            {
                await _registryManager.RemoveDeviceAsync(deviceId);
                Console.WriteLine("Device deleted successfully");
            }
            catch (DeviceNotFoundException)
            {
                Console.WriteLine("Device not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting device: " + ex.Message);
            }
        }
    }
}
