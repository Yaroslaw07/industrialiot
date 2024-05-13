namespace Industrialiot.Lib.Data
{
    [Flags]
    public enum DeviceErrors
    {
        None = 0,
        EmergencyStop = 1,
        PowerFailure = 2,
        SensorFailure = 4,
        Unknown = 8
    }

    public static class DeviceErrorsConverter
    {
        public static string ToMessage(this DeviceErrors errors)
        {
            var titles = new List<string>();

            if ((errors & DeviceErrors.EmergencyStop) != 0)
            {
                titles.Add("EMERGENCY_STOP");
            }

            if ((errors & DeviceErrors.PowerFailure) != 0)
            {
                titles.Add("POWER_FAILURE");
            }

            if ((errors & DeviceErrors.SensorFailure) != 0)
            {
                titles.Add("SENSOR_FAILURE");
            }

            if ((errors & DeviceErrors.Unknown) != 0)
            {
                titles.Add("UNKNOWN");
            }

            return string.Join("|", titles);
        }
    }
}
