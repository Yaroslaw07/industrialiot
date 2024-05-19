using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Industrialiot.Functions.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions
{
    public static class DeviceEmergencyStop
    {
        [FunctionName("DeviceEmergencyStop")]
        public static async Task Run([EventHubTrigger("emergency-stop", Connection = "EventHubConnectionString")] EventData[] events, ILogger log)
        {
            string iotHubConnectionString = Environment.GetEnvironmentVariable("IoTHubConnectionString");

            var serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
            var emergencyStopMethod = new CloudToDeviceMethod("emergencyStop");
            emergencyStopMethod.ResponseTimeout = TimeSpan.FromSeconds(20);

            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    var stopEvent = JsonConvert.DeserializeObject<EmergyStopEvent>(eventData.EventBody.ToString());

                    await serviceClient.InvokeDeviceMethodAsync(stopEvent.ConnectionDeviceId, emergencyStopMethod);

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
