using Industrialiot.Lib.Data;
using Microsoft.Azure.Devices.Client;

namespace Industrialiot.Lib
{
    partial class DevicesManager
    {
        private async Task<MethodResponse> CallEmergencyStop(MethodRequest methodRequest, object userContext)
        {
            var deviceName = ((MethodUserContext)userContext).deviceName;

            await Task.Run(() => _opcManager.CallDeviceMethod(deviceName, "EmergencyStop"));

            return new MethodResponse(200);
        }

        private async Task<MethodResponse> CallReserErrorStatus(MethodRequest methodRequest, object userContext)
        {
            var deviceName = ((MethodUserContext)userContext).deviceName;

            await Task.Run(() => _opcManager.CallDeviceMethod(deviceName, "ResetErrorStatus"));

            return new MethodResponse(200);
        }
    }
}
