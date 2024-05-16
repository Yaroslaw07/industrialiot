namespace Industrialiot.Agent.Data.Entities
{
    sealed public record DeviceMetadata
    {
        public int ProductionStatus { get; set; }
        public string WorkorderId { get; set; }
        public long GoodCount { get; set; }
        public long BadCount { get; set; }
        public double Temperature { get; set; }

        public DeviceMetadata(int ProductionStatus, string WorkorderId, long GoodCount, long BadCount, double Temperature)
        {
            this.ProductionStatus = ProductionStatus;
            this.WorkorderId = WorkorderId;
            this.GoodCount = GoodCount;
            this.BadCount = BadCount;
            this.Temperature = Temperature;
        }
    }
}
