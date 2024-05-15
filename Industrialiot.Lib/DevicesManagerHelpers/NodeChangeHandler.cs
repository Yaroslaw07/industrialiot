using Industrialiot.Lib.Data;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Opc.UaFx.Client;
using System.Text;

namespace Industrialiot.Lib
{
    partial class DevicesManager
    {
        private Dictionary<string, DeviceError> _deviceErrors;

        private async void ProductionRateChangeHandler(object sender, OpcDataChangeReceivedEventArgs e)
        {
            var newValue = e.Item.Value.Value;
            var deviceName = ((OpcMonitoredItem)sender).Tag.ToString();
            await _azureIotManager.SetTwinReportedProp(deviceName!, "productionRate", newValue);
        }

        private async void DeviceErrorChangeHandler(object sender, OpcDataChangeReceivedEventArgs e)
        {
            var newValue = e.Item.Value.Value;
            var deviceName = ((OpcMonitoredItem)sender).Tag.ToString();

            if (deviceName == null)
            {
                return;
            }

            DeviceError currentError = (DeviceError)newValue;
            DeviceError prevError = _deviceErrors.GetValueOrDefault(deviceName, DeviceError.None);

            _deviceErrors[deviceName] = currentError;

            DeviceError newErrors = (currentError & ~prevError);
            uint newErrorsCount = newErrors.CountSetBits();

            var dataString = JsonConvert.SerializeObject(new DeviceErrorMessage(currentError, newErrorsCount));
            Message msg = new Message(Encoding.UTF8.GetBytes(dataString));

            await _azureIotManager.sendMessage(msg, IotMessageTypes.DeviceError, deviceName!);
            await _azureIotManager.SetTwinReportedProp(deviceName!, "deviceErrors", newValue);
        }
    }
}
