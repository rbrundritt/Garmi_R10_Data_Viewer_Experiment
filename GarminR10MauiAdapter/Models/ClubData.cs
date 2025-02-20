namespace GarminR10MauiAdapter
{
    /// <summary>
    /// The club data.
    /// </summary>
    public class ClubData
    {
        #region OpenConnect Properties

        //https://mygolfsimulator.com/launch-monitor-data/
        //https://www8.garmin.com/manuals/webhelp/truswing/EN-US/GUID-6927870A-CF73-4EE3-A4D9-43D84BA05742.html

        /// <summary>
        /// The speed the club is moving at. 
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// The angle of attack in degrees.
        /// </summary>
        public float? AngleOfAttack { get; set; }

        /// <summary>
        /// The face to target angle in degrees.
        /// </summary>
        public float? FaceToTarget { get; set; }      

        /// <summary>
        /// The path of the club.
        /// </summary>
        public float? Path { get; set; }

        /// <summary>
        /// The speed at impact.
        /// </summary>
        public float? SpeedAtImpact { get; set; }

        /// <summary>
        /// Not provided by Garmin R10.
        /// The lie of the club.
        /// </summary>
        public float? Lie { get; set; } = null;

        /// <summary>
        /// Not provided by Garmin R10.
        /// The loft of the club in degrees.
        /// </summary>
        public float? Loft { get; set; } = null;

        /// <summary>
        /// Not provided by Garmin R10.
        /// The vertical face impact.
        /// </summary>
        public float? VerticalFaceImpact { get; set; } = null;

        /// <summary>
        /// Not provided by Garmin R10.
        /// The horizontal face impact.
        /// </summary>
        public double? HorizontalFaceImpact { get; set; } = null;

        /// <summary>
        /// Not provided by Garmin R10.
        /// The rate at which the club face is closing.
        /// </summary>
        public float? ClosureRate { get; set; } = null;

        #endregion

        #region Extra Properties

        private float? facePath = null;

        /// <summary>
        /// Face path is the difference between the face to target angle and the club path.
        /// </summary>
        public float? FacePath
        {
            get { 
                //Check to see if the face path has been explicitly set.
                if(facePath != null)
                {
                    return facePath;
                } 
                else if(Path != null && FaceToTarget != null)
                {
                    //If the face path has not been set, calculate it from the club path and face to target.
                    return FaceToTarget - Path;
                }

                return null;
            }
            set { facePath = value; }
        }

        private float? spinLoft = null;

        /// <summary>
        /// Spin loft is approximately the angle between the dynamic loft and attack angle.
        /// More spin loft = more spin, less distance, maximising back spin.
        /// Less spin loft = less spin, more distance, maximising driving distance.
        /// </summary>
        public float? SpinLoft
        {
            get
            {
                //Check to see if the spin loft has been explicitly set.
                if (spinLoft != null)
                {
                    return spinLoft;
                }
                else if (Path != null && FaceToTarget != null)
                {
                    //If the spin loft has not been set, calculate it from the dynamic loft and attack angle.
                    return Loft - AngleOfAttack;
                }

                return null;
            }
            set { facePath = value; }
        }

        #endregion
    }
}
