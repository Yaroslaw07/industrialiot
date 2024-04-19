using Industrialiot.Lib;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Opc.UaFx.Client;
using IndustrialiotConsole;

var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

var azureConnectionString = configuration["AZURE_IOT_CONNECTION_STRING"];
var optConnectionString = configuration["OPT_CONNECTION_STRING"];


var optClient = new OpcClient(optConnectionString);

var iotHubClient = ServiceClient.CreateFromConnectionString(azureConnectionString);
var iotHubRegister = RegistryManager.CreateFromConnectionString(azureConnectionString);
var iotHub = new IoTHubManager(iotHubClient, iotHubRegister);

var manager = new DeviceManager(optClient,iotHub, new List<DeviceMapper>());

await manager.SendDevicesMetadata();

