# Industrialiot Agent

This section introduces the agent application, a key component that acts as a bridge between your OPC Server and Azure IoT Hub. The agent facilitates communication by translating data from the OPC Server into a format compatible with Azure IoT Hub for seamless data transfer to the cloud.

## Prerequisites

### Clone this project

```shell
git clone https://github.com/Yaroslaw07/Industrialiot.git
```

### Install C# SDK

https://dotnet.microsoft.com/en-us/download

### Config

To config agent we use `appSettings.json` file. Create this file in agent project inside `Industraliot.Agent` folder.

After fullfil this file with next config:

```json
{
  "OPC_CONNECTION_STRING": "opc.tcp://localhost:4840/", // standard for local development
  "AZURE_IOT_CONNECTION_STRING": "HostName=%azure-iot-host-name%;",

  "DEVICES": [
    {
      "deviceName": "device-name",
      "opcNodeId": "%opc-node-id%",
      "azureDeviceId": "DeviceId=%device-name;SharedAccessKey=%device-shared-key%"
    }
    //...
  ]
}
```

- `OPC_CONNECTION_STRING` is string for connection to OPC server

- `AZURE_IOT_CONNECTION_STRING` is base prefix string for all device connection strings for Azure

- #### `DEVICES`

  - `deviceName` is string that identifies device. Just name for device, not from OPC or Azure IoTHub Must be unique

  - `opcNodeId` is string of Opc Node Id. Used to connect to Opc Device

  - `azureDeviceId` is a part connection string to Azure IoTHub Device. With `AZURE_IOT_CONNECTION_STRING` prefix creates a full connection string to IotHub device

### Run application

```shell
  dotnet run .
```

> inside `Industraliot.Agent`

---

## OPC part

Opc connection made by OPC UA SDK [docs](https://docs.traeger.de/en/software/sdk/opc-ua/net/client.development.guide).

Connection open when agent starts.

#### Agent use OPC for

- Read all data nodes
- Write to `ProductionRate` data node
- Subscribe to `DeviceError` and `Production` data nodes
- Call `EmergencyStatus` and `ResetErrorStatus` method nodes

## Azure IoTHub part

For working with Azure is used Azure Client Device SDK [docs](https://www.nuget.org/packages/Microsoft.Azure.Devices.Client).

### D2C messages

D2C messages are send in this occasions:

- Each 10 second Telemetry from each OPC device send

```json
{
  "body": {
    "ProductionStatus": 0,
    "WorkorderId": "00000000-0000-0000-0000-000000000000",
    "GoodCount": 0,
    "BadCount": 0,
    "Temperature": 24.642212891435044
  },
  "enqueuedTime": "Sun May 19 2024 13:40:56 GMT+0200 (Central European Summer Time)",
  "properties": {
    "type": "Telemetry"
  }
}
```

- On each change of `DeviceError` of OPC device

  - `deviceError` - current state of error
  - `newErrorsCount` - number of new occurred errors

```json
{
  "body": {
    "deviceError": 2,
    "newErrorsCount": 1
  },
  "enqueuedTime": "Sun May 19 2024 13:41:57 GMT+0200 (Central European Summer Time)",
  "properties": {
    "type": "DeviceError"
  }
}
```

---

#### Each message contains in properties type.

- `Telemetry` - telemetry of device
- `DeviceError` - deviceError

### Device Twin

Our agent both use desired and reported properties of device twin

- #### Desired

  - Subscription for `productionRate`: When the agent starts, it updates the OPC UA device's `ProductionRate` by reading it from the Device Twin. If the value is not present in the Device Twin, it remains unchanged. Additionally, each time there is a change in the Device Twin, the `ProductionRate` is updated accordingly.

- #### Reported

  - On each change of `ProductionRate` we change this property on device Twin.

  - On each change of `DeviceError` we change this property on device Twin.

### Direct Methods

Direct methods are exposed and can be used for each device:

- `emergencyStop` - invoke built-in OPC device's method `emergencyStop`

- `resetErrorStatus` - invoke built-in OPC device's method `emergencyStop`
