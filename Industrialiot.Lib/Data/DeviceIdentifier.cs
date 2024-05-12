namespace Industrialiot.Lib.Data
{
    public class DeviceIdentifier
    {

        public string deviceName { get; set; }
        public string opcNodeId { get; set; }
        public string azureDeviceConnection { get; set; }

        public DeviceIdentifier(string deviceName,string opcNodeId, string azureDeviceConnection)
        {
            this.deviceName = deviceName;
            this.opcNodeId = opcNodeId;
            this.azureDeviceConnection = azureDeviceConnection;
        }
    }
}
