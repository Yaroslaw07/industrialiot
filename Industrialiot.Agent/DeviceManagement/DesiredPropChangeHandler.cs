using Industrialiot.Agent.Data;
using Microsoft.Azure.Devices.Shared;

namespace Industrialiot.Agent.DeviceManagment
{
    partial class DevicesManager
    {
        private async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            var value = desiredProperties["productionRate"].Value;

            var deviceName = ((MethodUserContext)userContext).deviceName;

            _opcManager.SetDeviceNodeData(deviceName!, "ProductionRate", (int) value);
        }
    }
}
