using Microsoft.Extensions.Configuration;
using Industrialiot.Lib;
using Industrialiot.Lib.Data;

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
    .Select(d => new DeviceIdentifier(
        d["deviceName"]!,
        d["opcNodeId"]!,
        d["azureDeviceId"]!))
    .ToList();


if (list == null || list.Count() == 0)
{
    Console.WriteLine("DEVICES LIST ARE EMPTY");
    return;
}

var manager = new DeviceManager(opcConnectionString, azureConnectionString, list);

manager.Start();

while (true) ;
