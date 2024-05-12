using Industrialiot.Lib.Data;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace Industrialiot.Lib;

public class OpcManager
{
    private Dictionary<string, string> _devices;
    private OpcClient _client;

    public OpcManager(string opcConnectionString, List<DeviceIdentifier> deviceIdentifierList)
    {
        _client = new OpcClient(opcConnectionString);
        _devices = new Dictionary<string, string>();

        foreach (var device in deviceIdentifierList)
        {
            _devices.Add(device.deviceName, device.opcNodeId);
        }
    }

    public void Connect()
    {
        _client.Connect();
    }

    public void Disconnect()
    {
        _client.Disconnect();
    }

    private string GetDeviceOpcNodeId(string deviceName)
    {
        var res = _devices[deviceName];

        return res == null ? throw new Exception($"NOT OPC_CONNECTION FOR {deviceName}") : res;
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

    public void SetDeviceProductionRate(string deviceName, int newValue)
    {
        var opcNodeId = GetDeviceOpcNodeId(deviceName);

        var result = _client.WriteNode(opcNodeId + "/ProductionRate", newValue);

        if (!result.IsGood)
        {
            throw new Exception($"CAN'T UPDATE PRODUCTION RATE FOR {opcNodeId}");
        }
    }
}