using Industrialiot.Lib.Data;
using Microsoft.Azure.Devices.Client;
using Opc.UaFx.Client;
using System.Text;

namespace Industrialiot.Lib
{
    partial class DevicesManager
    {
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

            DeviceErrors errors = (DeviceErrors)newValue;
            var strErrors = errors.ToMessage();
            Message msg = new Message(Encoding.UTF8.GetBytes(strErrors));

            await _azureIotManager.sendMessage(msg, deviceName!);
            await _azureIotManager.SetTwinReportedProp(deviceName!, "deviceErrors", newValue);
        }
    }
}
