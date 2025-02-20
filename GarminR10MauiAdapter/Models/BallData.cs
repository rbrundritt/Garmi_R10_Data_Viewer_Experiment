namespace GarminR10MauiAdapter
{
    /// <summary>
    /// Ball data from launch monitor. Aligns with OpenConnectApiMessage.
    /// </summary>
    public class BallData
    {
        #region OpenConnect Properties

        /// <summary>
        /// The speed of the ball in MPH or MPS depending on the specified units.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// The spin axis of the ball.
        /// </summary>
        public float SpinAxis { get; set; }

        /// <summary>
        /// Total spin of the ball in RPM.
        /// </summary>
        public float TotalSpin { get; set; }

        /// <summary>
        /// The back spin of the ball in RPM.
        /// </summary>
        public float? BackSpin { get; set; } = null;

        /// <summary>
        /// The side spin of the ball in RPM.
        /// </summary>
        public float? SideSpin { get; set; } = null;

        /// <summary>
        /// Horizontal launch angle in degrees.
        /// </summary>
        public float HLA { get; set; }

        /// <summary>
        /// Vertical launch angle in degrees.
        /// </summary>
        public float VLA { get; set; }

        /// <summary>
        /// Not provided by Garmin R10.
        /// The carry distance of the ball in yards or meters depending on the specified units.
        /// </summary>
        public float? CarryDistance { get; set; } = null;

        #endregion
    }
}
