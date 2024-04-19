using IndustrialiotConsole;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Opc.Ua;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Net.Mime;
using System.Text;

namespace Industrialiot.Lib
{ 
    public class DeviceManager(OpcClient _client, IoTHubManager _iotHubManager, List<DeviceMapper> _deviceMappers)
    {
        public async Task SendDevicesMetadata()
        {
            _client.Connect();
            var data = GetSingleMachineLocalMetadata();
            _client.Disconnect();
            
            var dataString = JsonConvert.SerializeObject(data);
            Message msg = new Message(Encoding.UTF8.GetBytes(dataString));

            //var device = await _iotHubManager.CreateDeviceAsync("Device1");

            await _iotHubManager.SendMessageAsync(msg, "Device1");
        }


        private DeviceMetadata GetSingleMachineLocalMetadata(string nodeIdPrefix = "ns=2;s=", string machineName = "Device 1")
        {
            string prefix = nodeIdPrefix + machineName;
            var props = typeof(DeviceMetadata).GetProperties();

            List<OpcReadNode> commands = new List<OpcReadNode>();

            foreach (var prop in props)
            {
                commands.Add(new OpcReadNode(prefix + "/" + prop.Name));
            }

            var data = _client.ReadNodes(commands.ToArray()).ToArray();

            var productionStatus = (int)data[0].Value; 
            var workorderId = (string)data[1].Value;
            var goodCount = (long)data[2].Value;
            var badCount = (long)data[3].Value;
            var temperature = (double)data[4].Value;

            var metadata = new DeviceMetadata(productionStatus, workorderId, goodCount, badCount, temperature);

            return metadata;
        }
    }
}
