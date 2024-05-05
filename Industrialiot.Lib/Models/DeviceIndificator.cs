namespace Industrialiot.Lib.Data
{
    public class DeviceIndificator
    {
        public string opcNodeId { get; set; }
        public string azureDeviceId { get; set; }

        public DeviceIndificator(string opcNodeId, string azureDeviceId)
        {
            this.opcNodeId = opcNodeId;
            this.azureDeviceId = azureDeviceId;
        }

        override public string ToString()
        {
            return opcNodeId + " : " + azureDeviceId;
        }
    }
}
