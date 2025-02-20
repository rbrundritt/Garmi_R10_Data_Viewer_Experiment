namespace GarminR10MauiAdapter
{
    public class GerminR10Settings: LaunchMonitorSettings
    { 
        /// <summary>
        /// Name of the Garmin R10 device.
        /// </summary>
        public string DeviceName { get; } = "Approach R10";

        /// <summary>
        /// The ammount of time in seconds to wait before making another attempt to connect to the device.
        /// </summary>
        public int ConnectionRetryInterval { get; set; } = 10;

        /// <summary>
        /// Number of times to attempt to reconnect to the device before giving up.
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 5;

        /// <summary>
        /// Wake device if falls asleep.
        /// </summary>
        public bool AutoWake { get; set; } = true;

        /// <summary>
        /// Recalibrate tilt at the start of every session.
        /// </summary>
        public bool CalibrateTiltOnConnect { get; set; } = true;

        /// <summary>
        /// Distance from R10 to ball in feet or meters based on Units.
        /// </summary>
        public float TeeDistance { get; set; } = 7;

        /// <summary>
        /// Units that the TeeDistance is in.
        /// </summary>
        public DistanceUnit TeeDistanceUnits { get; set; } = DistanceUnit.Feet;

        /// <summary>
        /// Specifies if shots with zero launch angle should be ignored.
        /// </summary>
        public bool IgnoreZeroLaunchAngleShots { get; set; } = false;

        /// <summary>
        /// Specifies if practice swings should be ignored.
        /// </summary>
        public bool IgnorePracticeSwings { get; set; } = false;
    }
}
