using Industrialiot.Agent.Data.Entities;
using Industrialiot.Agent.DeviceManagment;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

var opcConnectionString = configuration["OPC_CONNECTION_STRING"];
var azureConnectionString = configuration["AZURE_IOT_CONNECTION_STRING"];

if (opcConnectionString == null || azureConnectionString == null)
{
    Console.Error.WriteLine("OPC_CONNECTION_STRING OR AZURE_IOT_CONNECTION_STRING IS NOT PROVIDED");
    return;
}

var list = configuration.GetSection("DEVICES").GetChildren()
    .Select(d => {
        var deviceName = d["deviceName"];
        var opcNodeId = d["opcNodeId"];
        var azureDeviceId = d["azureDeviceId"];

        if (opcNodeId == null || azureDeviceId == null || deviceName == null)
        {
            Console.Error.WriteLine($"{d} DON'T HAVE ENOUGH CONFIG DATA. IT WILL BE MISSED");
            return null;
        }

        return new DeviceIdentifier(
            deviceName,
            opcNodeId,
            azureDeviceId);
    })
    .Where(d => d != null).ToList();


if (list == null || list.Count() == 0)
{
    Console.WriteLine("DEVICES LIST ARE EMPTY");
    return;
}

var manager = new DevicesManager(opcConnectionString, azureConnectionString, list);

manager.Start();

while (true) ;
