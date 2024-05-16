using Industrialiot.Agent.Data.Entities;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace Industrialiot.Agent.Opc;

public class OpcManager
{
    private Dictionary<string, string> _devices;
    private OpcClient _client;

    OpcSubscription _subscription;

    public OpcManager(string opcConnectionString, List<DeviceIdentifier> deviceIdentifierList)
    {
        _client = new OpcClient(opcConnectionString);
        _devices = [];

        foreach (var device in deviceIdentifierList)
        {
            _devices.Add(device.deviceName, device.opcNodeId);
        }
    }

    public void Connect()
    {
        _client.Connect();
        _subscription = _client.SubscribeNodes();
        _subscription.ChangeMonitoringMode(OpcMonitoringMode.Reporting);
    }

    public void Disconnect()
    {
        _subscription.ChangeMonitoringMode(OpcMonitoringMode.Disabled);
        _client.Disconnect();
    }

    private string GetDeviceOpcNodeId(string deviceName)
    {
        var res = _devices[deviceName];

        return res ?? throw new Exception($"NOT OPC CONNECTION FOR {deviceName}");
    }

    public void SubscribeToNodeDataChange(string deviceName, List<OpcDataChangeHandlerMapper> changeHandlers)
    {
        if (changeHandlers.Count == 0) return;

        var opcNodeId = GetDeviceOpcNodeId(deviceName);

        foreach (var changeHandler in changeHandlers)
        {
            var item = new OpcMonitoredItem(opcNodeId + "/" + changeHandler.opcNodeDataName, OpcAttribute.Value);

            item.Tag = deviceName;
            item.DataChangeReceived += changeHandler.handler;

            _subscription.AddMonitoredItem(item);
        }
        
        _subscription.ApplyChanges();
    }

    public DeviceMetadata? GetDeviceMetadata(string deviceName)
    {
        var props = typeof(DeviceMetadata).GetProperties();
        List<OpcReadNode> commands = new List<OpcReadNode>();

        string optNodeId = GetDeviceOpcNodeId(deviceName);

        foreach (var prop in props)
        {
            commands.Add(new OpcReadNode(optNodeId + "/" + prop.Name));
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

    public void SetDeviceNodeData(string deviceName, string variableName, int newValue)
    {
        var opcNodeId = GetDeviceOpcNodeId(deviceName);

        var result = _client.WriteNode(opcNodeId + "/" + variableName, newValue);

        if (!result.IsGood) throw new Exception($"CAN'T UPDATE {variableName} FOR {opcNodeId}");
    }

    public void CallDeviceMethod(string deviceName, string methodName)
    {
        var opcNodeId = GetDeviceOpcNodeId(deviceName);
        _client.CallMethod(opcNodeId, opcNodeId + "/" +  methodName);
    }
}