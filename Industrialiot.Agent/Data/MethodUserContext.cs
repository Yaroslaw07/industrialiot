namespace Industrialiot.Agent.Data
{
    internal class MethodUserContext
    {
        public string deviceName { get; set; }

        public MethodUserContext(string deviceName) { this.deviceName = deviceName; }
    }
}
