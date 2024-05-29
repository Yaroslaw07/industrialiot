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
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
