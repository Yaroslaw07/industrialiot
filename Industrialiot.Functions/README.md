# App Function

This document provides an overview of the Azure Function DeviceEmergencyStop, which triggers an emergency stop on IoT devices based on events from an Event Hub.

## Function Overview

The DeviceEmergencyStop function listens to the "emergency-stop" Event Hub. When an event is received, it triggers the "emergencyStop" method on the specified IoT device using Azure IoT Hub.

## Configuration

Ensure the following environment variables are set:

- `EventHubConnectionString` - Connection string for the Event Hub.
- `IoTHubConnectionString` - Connection string for the IoT Hub.

## Deploying

Deploy the function app using Azure CLI or Azure portal, ensuring the required environment variables are set in the application settings of your function app.

All deploy instructions can be find [here](https://learn.microsoft.com/en-us/azure/azure-functions/functions-deployment-technologies?tabs=linux)
