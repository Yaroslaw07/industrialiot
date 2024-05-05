using Industrialiot.Lib.Data;
using IndustrialiotConsole;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Text;

namespace Industrialiot.Lib
{
    public class DeviceManager(OpcClient _client, IoTHubManager _iotHubManager, List<DeviceIndificator> _deviceIndificators)
    {
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async Task Start()
        {
            await PeiodicSendingMetadata(TimeSpan.FromSeconds(10));
        }

        public void Stop()
        {
           _cancellationTokenSource.Cancel();
        }

        private async Task PeiodicSendingMetadata(TimeSpan interval)
        {
            while (true)
            {
                await SendDevicesMetadata();
                await Task.Delay(interval, _cancellationTokenSource.Token);
            }
        }

        public async Task SendMachinesMetadata()
        {
            _client.Connect();

            var tasks = new List<Task>();

            foreach (var indificator in _deviceIndificators)
            {
                var fields = GetSingleMachineMetadata(indificator.opcNodeId);

                var dataString = JsonConvert.SerializeObject(fields);
                Message msg = new Message(Encoding.UTF8.GetBytes(dataString));

                var task =  _iotHubManager.SendMessageAsync(msg, indificator.azureDeviceId);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            
            _client.Disconnect();
        }

        private DeviceMetadata GetSingleMachineMetadata(string machineName)
        {
            var props = typeof(DeviceMetadata).GetProperties();

            List<OpcReadNode> commands = new List<OpcReadNode>();

            foreach (var prop in props)
            {
                commands.Add(new OpcReadNode(machineName + "/" + prop.Name));
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
