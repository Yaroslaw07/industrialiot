namespace Industrialiot.Lib
{
    public class DeviceMapper
    {
        public string OpcNodeId { get; set; }
        public string IoTDeviceId { get; set; }

        public DeviceMapper(string opcNodeId, string ioTDeviceId)
        {
            OpcNodeId = opcNodeId;
            IoTDeviceId = ioTDeviceId;
        }

        public static string GenerateDeviceId(string nodeId)
        {
            var parts = nodeId.Split(';');
            if (parts.Length < 2)
            {
                throw new ArgumentException("Invalid node ID format", nameof(nodeId));
            }
            var idPart = parts[1];

            var idParts = idPart.Split('=');
            if (idParts.Length < 2)
            {
                throw new ArgumentException("Invalid node ID format", nameof(nodeId));
            }
            var idValue = idParts[1];

            var formattedId = new string(idValue.Where(c => char.IsLetterOrDigit(c)).ToArray());

            return formattedId;
        }
    }
}
