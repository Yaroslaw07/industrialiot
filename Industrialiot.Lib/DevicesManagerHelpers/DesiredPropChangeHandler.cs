using Industrialiot.Lib.Data;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrialiot.Lib
{
    partial class DevicesManager
    {
        private async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("onDesired", desiredProperties["productionRate"].Value);

            var value = desiredProperties["productionRate"].Value;

            var deviceName = ((MethodUserContext)userContext).deviceName;

            _opcManager.SetDeviceNodeData(deviceName!, "ProductionRate", (int) value);
        }
    }
}
