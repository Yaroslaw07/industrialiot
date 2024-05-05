using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Opc.UaFx.Client;
using IndustrialiotConsole;
using Industrialiot.Lib;
using Industrialiot.Lib.Data;
using Industrialiot.Agent;

var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("devices.json", optional: false)
            .Build();

var azureConnectionString = configuration["AZURE_IOT_CONNECTION_STRING"];
var optConnectionString = configuration["OPT_CONNECTION_STRING"];

var list = configuration.GetSection("Devices").Get<List<DeviceIndificator>>();
if (list == null) list = new List<DeviceIndificator>();

var optClient = new OpcClient(optConnectionString);

var iotHubClient = ServiceClient.CreateFromConnectionString(azureConnectionString);
var iotHubRegister = RegistryManager.CreateFromConnectionString(azureConnectionString);
var iotHub = new IoTHubManager(iotHubClient, iotHubRegister);

var manager = new DeviceManager(optClient,iotHub, list);

await manager.Start();
Console.WriteLine("Agent is started");

int input;

do
{
    Menu.DisplayMenu();
    input = Menu.ReadInput();
    await Menu.Execute(input, manager);
} while (true);
