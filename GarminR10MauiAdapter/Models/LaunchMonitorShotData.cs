using GarminR10MauiAdapter.OpenConnect;

namespace GarminR10MauiAdapter
{
    /// <summary>
    /// Represents a shot from the launch monitor.
    /// Some unset properties will be automatically calculated from other properties.(e.g. smash factor, face path, )
    /// Some other properties are placeholders to be set by the user or sim software (e.g. units of measurement, the club being used, altitude of where the user is)
    /// Not all launch monitors provide all of the data.
    /// https://mygolfsimulator.com/launch-monitor-data/
    /// https://www.bigteesgolfworld.com/launch-monitor-data/
    /// </summary>
    public class LaunchMonitorShotData : ICloneable
    {
        #region Private Properties

        private float? ballSpeed = null;
        private float? backSpin = null;
        private float? sideSpin = null;
        private float? spinAxis = null;
        private float? spinRate = null;
        private float? horizontalLaunchAngle = null;

        private float? clubSpeed = null;
        private float? smashFactor = null;
        private float? faceAngle = null;
        private float? facePath = null;
        private float? dynamicLoft = null;
        private float? clubPath = null;
        private float? angleOfAttack = null;
        private float? spinLoft = null;

        private float? carryDistance = null;
        private float? carryLateralDistance = null;
        private float? totalDistance = null;
        private float? totalLateralDistance = null;

        private TimeSpan? backswingTime = null;
        private TimeSpan? downswingTime = null;
        private float? tempo = null;

        #endregion

        #region General information

        /// <summary>
        /// Whether the player is left or right handed.
        /// </summary>
        public Handed? PlayerHanded { get; set; } = Handed.RH;

        /// <summary>
        /// The type of swing the user took. Practice or Normal.
        /// </summary>
        public SwingType SwingType { get; set; } = SwingType.Normal;


        /// <summary>
        /// The date and time the shot was taken.
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// The shot number.
        /// </summary>
        public int ShotNumber { get; set; }

        /// <summary>
        /// The measurement units of the data. 
        /// When metric is selected, the units are in meters and meters per second.
        /// When imperial is selected, the units are in yards and miles per hour.
        /// Angles are in degrees.
        /// Spin rates are in RPM.
        /// </summary>
        public Units Units { get; set; } = Units.Metric;

        /// <summary>
        /// The units of measurement for distances.
        /// </summary>
        public DistanceUnit DistanceUnits
        {
            get
            {
                return Units == Units.Metric ? DistanceUnit.Meters : DistanceUnit.Yards;
            }
        }

        /// <summary>
        /// The units of measurement for speeds.
        /// </summary>
        public SpeedUnit SpeedUnits
        {
            get
            {
                return Units == Units.Metric ? SpeedUnit.MPS : SpeedUnit.MPH;
            }
        }

        /// <summary>
        /// The units of measurement for heights, altitudes, and elevations.
        /// </summary>
        public DistanceUnit HeightUnits
        {
            get
            {
                return Units == Units.Metric ? DistanceUnit.Meters : DistanceUnit.Feet;
            }
        }

        /// <summary>
        /// The alititude of the location where the shot was taken.
        /// Launch monitors may not provide this data. 
        /// This property can be set mauanlly.
        /// </summary>
        public float? Altitude { get; set; } = null;

        /// <summary>
        /// The club used to hit the ball.
        /// </summary>
        public Club? Club { get; set; } = null;

        #endregion

        #region Ball Data

        /// <summary>
        /// Speed of the ball just after impact.
        /// </summary>
        public float? BallSpeed
        {
            get
            {
                return ballSpeed;
            }
            set
            {
                ballSpeed = value;
                TrySetSmashFactor();
            }
        }

        /// <summary>
        /// A measure of the angle of the ball's take off relative to the slope of the ground.
        /// Also known as VLA, Vertical Launch Angle, Vertical Launch, and Launch Angle.
        /// </summary>
        public float? VerticalLaunchAngle { get; set; } = null;

        /// <summary>
        /// The angle at which the ball starts it's flight, relative to a straight target line (left/right).
        /// A positive launch direction indicates a ball that starts right of the target and a negative launch direction indicates a ball that starts left of the target.
        /// Also known as HLA, Horizontal Launch, Launch direction, Side angle, and Azimuth.
        /// </summary>
        public float? HorizontalLaunchAngle
        {
            get
            {
                return horizontalLaunchAngle;
            }
            set
            {
                horizontalLaunchAngle = value;
                TrySetShotShape();
                TrySetCurveDistance();
            }
        }

        /// <summary>
        /// How the spin data was captured.
        /// </summary>
        public SpinMethod SpinMethod { get; set; }

        /// <summary>
        /// How fast the ball is spinning backwards, causing lift.
        /// Measured in RPM.
        /// </summary>
        public float? BackSpin
        {
            get
            {
                return backSpin;
            }
            set
            {
                backSpin = value;
                TrySetSpinRateAxis();
            }
        }

        /// <summary>
        /// How fast the ball is spinning sideways, causing curve.
        /// Measured in RPM.
        /// </summary>
        public float? SideSpin
        {
            get
            {
                return sideSpin;
            }
            set
            {
                sideSpin = value;
                TrySetSpinRateAxis();
            }
        }

        /// <summary>
        /// The angle of the ball's spin axis relative to the target line.
        /// Negative values indicate the ball is spinning left and positive values indicate the ball is spinning right.
        /// Also known as Spin Axis, Spin Tilt, and Spin Tilt Axis.
        /// </summary>
        public float? SpinAxis
        {
            get
            {
                return spinAxis;
            }
            set
            {
                spinAxis = value;
                TrySetBackSideSpin();
                TrySetShotShape();
                TrySetFacePathType();
            }
        }

        /// <summary>
        /// Measured in RPM.
        /// </summary>
        public float? SpinRate
        {
            get
            {
                return spinRate;
            }
            set
            {
                spinRate = value;
                TrySetBackSideSpin();
            }
        }

        #endregion

        #region Club Data

        /// <summary>
        /// Speed of the club the moment before impact with the ball.
        /// Also known as Swing speed.
        /// </summary>
        public float? ClubSpeed
        {
            get
            {
                return clubSpeed;
            }
            set
            {
                clubSpeed = value;
                TrySetSmashFactor();
            }
        } 
        
        /// <summary>
        /// Describes whether the club is travelling in an upwards or downwards direction relative to the ground, as it approaches the golf ball. 
        /// The angle of attack can be positive or negative. 
        /// </summary>
        public float? AngleOfAttack
        {
            get
            {
                return angleOfAttack;
            }
            set
            {
                angleOfAttack = value;
                TrySetSpinLoft();
            }
        }

        /// <summary>
        /// The direction the club is moving as it strikes the golf ball.
        /// Also known as Swing path, and swing direction.
        /// </summary>
        public float? ClubPath
        {
            get
            {
                return clubPath;
            }
            set
            {
                clubPath = value;
                TrySetFacePath();
                TrySetSpinLoft();
            }
        }

        /// <summary>
        /// The angle of the club face relative to the target line.
        /// Also known as Face Angle.
        /// </summary>
        public float? FaceToTarget
        {
            get
            {
                return faceAngle;
            }
            set
            {
                faceAngle = value;
                TrySetFacePath();
                TrySetSpinLoft();
            }
        }

        /// <summary>
        /// The difference between the face angle and the club path.
        /// A club face that's open relative to the path of the club will produce a fade, whilst a face that's closed to the path will produce a draw.
        /// A positive value means the face is pointed to the right of the club path.
        /// A negative value means the face is pointed to the left of the club path.
        /// </summary>
        public float? FacePath
        {
            get
            {
                return facePath;
            }
        }

        public FacePathType? FacePathType { get; private set; } = null;

        /// <summary>
        /// The direction of the clubface in the vertical axis when the ball is struck.
        /// </summary>
        public float? DynamicLoft
        {
            get
            {
                return dynamicLoft;
            }
            set
            {
                dynamicLoft = value;
                TrySetSpinLoft();
            }
        }

        /// <summary>
        /// The three-dimensional angle between the direction the club head is moving (both club path and attack angle) 
        /// and the direction the club face is pointing (both face angle and dynamic loft).
        /// A simple approximation is to subtract the angle of attack from the dynamic loft when face path is near zero.
        /// </summary>
        public float? SpinLoft { get; set; } = null; 

        #endregion

        #region Swing Data        

        /// <summary>
        /// The time it takes to swing the club from the start of the backswing to the top of the backswing.
        /// </summary>
        public TimeSpan? BackswingTime
        {
            get
            {
                return backswingTime;
            }
            set
            {
                backswingTime = value;
                TrySetTempo();
            }
        }

        /// <summary>
        /// The time it takes to swing the club from the top of the backswing to the ball.
        /// </summary>
        public TimeSpan? DownswingTime
        {
            get
            {
                return downswingTime;
            }
            set
            {
                downswingTime = value;
                TrySetTempo();
            }
        }

        /// <summary>
        /// The ratio of backswing time to downswing time.
        /// A 3 to 1 ratio, or 3.0, is the ideal.
        /// </summary>
        public float? Tempo
        {
            get
            {
                return tempo;
            }
        }

        /// <summary>
        /// A golfer's smash factor is a measure of how effectively their club speed is transferred into ball speed. 
        /// Smash factor is calculated by dividing the ball speed by club speed.
        /// A higher smash factor, the better energy transfer from club to ball.
        /// The target  for a golfer assuming they are driving off the tee is a smash factor of approximately 1.5. 
        /// Whereas if you're using a pitching wedge, you would expect a number that's closer to 1.24.
        /// </summary>
        public float? SmashFactor
        {
            get
            {
                return smashFactor;
            }
        }

        #endregion

        #region Ball Flight Data

        /// <summary>
        /// The speed of the ball when it first impacts the ground.
        /// </summary>
        public float? BallSpeedAtImpact { get; set; } = null;

        /// <summary>
        /// The distance the ball flies before first impact with the ground.
        /// </summary>
        public float? CarryDistance
        {
            get
            {
                return carryDistance;
            }
            set
            {
                carryDistance = value;
                TrySetCurveDistance();
                TrySetRollDistance();
            }
        }

        /// <summary>
        /// How far the ball has travelled left or right of the target line at the first pimpact (same time as carry distance).
        /// Negative values indicate the ball has travelled left of the target line and positive values indicate the ball has travelled right of the target line.
        /// Also known as Side, Carry Lateral Distance, and Lateral landing.
        /// </summary>
        public float? CarryDistanceOffline
        {
            get
            {
                return carryLateralDistance;
            }
            set 
            { 
                carryLateralDistance = value;
                TrySetCurveDistance();
            }
        }

        /// <summary>
        /// Defined by the amount of movement to the horizontal side in a direction perpendicular to the launch of your golf ball. 
        /// It tells you the curvature of your golf ball, without having to take into account the starting line.
        /// Negative values are curves to the left, positive values are curves to the right.
        /// </summary>
        public float? CarryCurveDistance { get; set; } = null;

        /// <summary>
        /// The total distance the ball travels.
        /// </summary>
        public float? TotalDistance
        {
            get
            {
                return totalDistance;
            }
            set
            {
                totalDistance = value;
                TrySetCurveDistance();
                TrySetRollDistance();
            }
        }

        /// <summary>
        /// How far the ball has travelled left or right of the target line when the ball comes to rest (same time as total distance).
        /// Negative values indicate the ball has travelled left of the target line and positive values indicate the ball has travelled right of the target line.
        /// Also known as Side total, and Total Lateral Distance.
        /// </summary>
        public float? TotalDistanceOffline
        {
            get
            {
                return totalLateralDistance;
            }
            set
            {
                totalLateralDistance = value;
                TrySetCurveDistance();
            }
        }

        /// <summary>
        /// Defined by the amount of movement to the horizontal side in a direction perpendicular to the launch of your golf ball. 
        /// It tells you the curvature of your golf ball, without having to take into account the starting line.
        /// Negative values are curves to the left, positive values are curves to the right.
        /// </summary>
        public float? TotalCurveDistance { get; set; } = null;

        /// <summary>
        /// The maximum height the ball reaches relative to the elevation of where the ball was initially hit from.
        /// Also known as Peak height, and Apex height.
        /// </summary>
        public float? MaxHeight { get; set; } = null;

        /// <summary>
        /// The distance from the target line to the highest point of the ball's flight.
        /// </summary>
        public float? MaxHeightDistanceOffline { get; set; } = null;

        /// <summary>
        /// The distance from the initial impact to the highest point of the ball's flight.
        /// </summary>
        public float? MaxHeightDistance { get; set; } = null;

        /// <summary>
        /// How long the ball is in the air for. 
        /// Time from initial impace to first impact with the ground.
        /// Captured at the same time as carry distance.
        /// </summary>
        public TimeSpan? FlightTime { get; set; } = null;

        /// <summary>
        /// The angle the ball descends relative to the ground.
        /// The lower the value, the more roll out. 
        /// Also known as Descent Angle.
        /// </summary>
        public float? DescentAngle { get; set; } = null;

        /// <summary>
        /// They of shot that was hit, draw, push, straight, pull, fade, hook, slice, etc.
        /// This will usually be calculated from the launch angle, spin rate, and spin axis.
        /// </summary>
        public ShotShape? ShotShape { get; set; } = null;  //TODO: Calculate this and validate comment.

        /// <summary>
        /// A simple estimate of the roll distance (total distance - carry distance).
        /// </summary>
        public float? RollDistance { get; set; } = null;

        #endregion

        #region Excluded Data

        /**
         * The following properties are commented out. 
         * Launch monitors may provide this data, but may have low value for most users.
         */

        /*
         
        /// <summary>
        /// Specifies where on the clubface the ball was struck.
        /// Also known as impact location.
        /// Excluded as it is fairly easy to determine and usually only provided by the highest end launch monitors.
        /// </summary>
        public float? ImpactPoint { get; set; } = null;

         */

        /*
        
        /// <summary>
        /// The angle made between the ground and the 3D circle made by the movement of the club towards the bottom of the arc of the swing. 
        /// </summary>
        public float? SwingPlane { get; set; } = null;
         
         */

        /*

        /// <summary>
        /// The point at which the club head reaches its lowest point in relation to the ball and it, generally, 
        /// comes after the ball has been launched and the club drifts farther down towards the ground.
        /// </summary>
        public float? LowPoint { get; set; } = null;

         */

        /*
         
        /// <summary>
        /// The angle of the club face at rest.
        /// </summary>
        public float? ClubLoftAngle { get; set; } = null;

         */


        /*
         
        /// <summary>
        /// The angle between the shaft and the ground.
        /// </summary>
        public float? LieAngle { get; set; } = null;

         */

        #endregion

        #region Public Methods

        /// <summary>
        /// Clones the shot data.
        /// </summary>
        public LaunchMonitorShotData Clone()
        {
            return new LaunchMonitorShotData()
            {
                PlayerHanded = PlayerHanded,
                SwingType = SwingType,
                DateTime = DateTime,
                ShotNumber = ShotNumber,
                Units = Units,
                Altitude = Altitude,
                Club = Club,
                BallSpeed = BallSpeed,
                VerticalLaunchAngle = VerticalLaunchAngle,
                HorizontalLaunchAngle = HorizontalLaunchAngle,
                SpinMethod = SpinMethod,
                BackSpin = BackSpin,
                SideSpin = SideSpin,
                SpinAxis = SpinAxis,
                SpinRate = SpinRate,
                ClubSpeed = ClubSpeed,
                AngleOfAttack = AngleOfAttack,
                ClubPath = ClubPath,
                FaceToTarget = FaceToTarget,
                DynamicLoft = DynamicLoft,
                BackswingTime = BackswingTime,
                DownswingTime = DownswingTime,
                BallSpeedAtImpact = BallSpeedAtImpact,
                CarryDistance = CarryDistance,
                CarryDistanceOffline = CarryDistanceOffline,
                CarryCurveDistance = CarryCurveDistance,
                TotalDistance = TotalDistance,
                TotalDistanceOffline = TotalDistanceOffline,
                TotalCurveDistance = TotalCurveDistance,
                MaxHeight = MaxHeight,
                MaxHeightDistanceOffline = MaxHeightDistanceOffline,
                MaxHeightDistance = MaxHeightDistance,
                FlightTime = FlightTime,
                DescentAngle = DescentAngle,
                ShotShape = ShotShape
            };
        }

        /// <summary>
        /// Clones the shot data.
        /// </summary>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Converts the shot data to an OpenConnectApiMessage.
        /// </summary>
        public OpenConnect.OpenConnectApiMessage ToOpenConnectApiMessage()
        {
            BallData? ballData = null;

            if (BallSpeed != null && 
                VerticalLaunchAngle != null && 
                HorizontalLaunchAngle != null && 
                SpinAxis != null && 
                SpinRate != null)
            {
                ballData = new BallData()
                {
                    Speed = BallSpeed.Value,
                    CarryDistance = CarryDistance,
                    VLA = VerticalLaunchAngle.Value,
                    HLA = HorizontalLaunchAngle.Value,
                    BackSpin = BackSpin,
                    SideSpin = SideSpin,
                    SpinAxis = SpinAxis.Value,
                    TotalSpin = SpinRate.Value
                };
            }

            ClubData? clubData = null;
            if (ClubSpeed != null)
            {
                clubData = new ClubData()
                {
                    Speed = ClubSpeed.Value,
                    AngleOfAttack = AngleOfAttack,
                    FaceToTarget = FaceToTarget,
                    Path = ClubPath,
                    SpeedAtImpact = ClubSpeed.Value
                };
            }

            return new OpenConnect.OpenConnectApiMessage()
            {
                Units = Units == Units.Imperial? "Yards" : "Meters",
                ShotNumber = ShotNumber,
                BallData = ballData,
                ClubData = clubData,
                ShotDataOptions = new OpenConnect.ShotDataOptions()
                {
                    ContainsBallData = ballData != null,
                    ContainsClubData = clubData != null
                }
            };
        }

        #endregion

        #region Private Methods

        private void TrySetSmashFactor()
        {
            if (smashFactor == null && ballSpeed != null && clubSpeed != null)
            {
                smashFactor = ballSpeed.Value / clubSpeed.Value;
            }
        }  

        private void TrySetSpinRateAxis()
        {
            //Only try to calculate spin rate and spin axis if they are null and we have back spin and side spin.
            if (backSpin != null && sideSpin != null)
            {
                //Calculate total spin rate in RPM.
                if (spinRate == null)
                {
                    spinRate = (float)Math.Sqrt(backSpin.Value * backSpin.Value + sideSpin.Value * sideSpin.Value);
                }

                //Calculate spin axis angle in degrees.
                if (spinAxis == null)
                {
                    spinAxis = (float)(Math.Atan2(sideSpin.Value, backSpin.Value) * (180 / Math.PI));
                    TrySetFacePathType();
                    TrySetShotShape();
                }
            }
        }

        private void TrySetBackSideSpin()
        {
            //Only try to calculate back spin and side spin if they are null and we have spin rate and spin axis.
            if ((sideSpin == null || backSpin == null) && spinRate != null && spinAxis != null)
            {
                double spinAxisRadians = spinAxis.Value * (Math.PI / 180);

                //Calculate back spin and side spin in RPM.
                if (backSpin == null)
                {
                    backSpin = (float)(spinRate.Value * Math.Cos(spinAxisRadians));
                }

                if (sideSpin == null)
                {
                    sideSpin = (float)(spinRate.Value * Math.Sin(spinAxisRadians));
                }
            }
        }
        
        private void TrySetTempo()
        {
            //Calculate tempo if it is not set and we have backswing and downswing times.
            if (tempo == null && backswingTime != null && downswingTime != null)
            {
                tempo = (float)(backswingTime.Value.TotalSeconds / downswingTime.Value.TotalSeconds);
            }
        }   

        private void TrySetFacePath()
        {
            //Calculate face path if it is not set and we have face angle and club path.
            if (facePath == null && faceAngle != null && clubPath != null)
            {
                facePath = faceAngle - clubPath;
            } 
        }

        private void TrySetFacePathType()
        {
            if (FacePathType == null && spinAxis != null)
            {
                FacePathType = GolfShotClassifier.GetFacePathType(spinAxis, PlayerHanded);
            }
        }

        public void TrySetSpinLoft()
        {
            //Check to see if attach angle and dynamic loft are set.
            if (spinLoft == null && dynamicLoft != null && angleOfAttack != null)
            {
                //If face path is not set, use the simple approximation.
                spinLoft = dynamicLoft - angleOfAttack;
            }
        }

        private void TrySetShotShape()
        {
            if (ShotShape == null && horizontalLaunchAngle != null && spinAxis != null)
            {
                ShotShape = GolfShotClassifier.GetShotShape(spinAxis.Value, horizontalLaunchAngle.Value, PlayerHanded);
            }
        }

        private void TrySetCurveDistance()
        {
            if (horizontalLaunchAngle != null) {
                var tanHVL = (float)Math.Tan(horizontalLaunchAngle.Value * (Math.PI / 180));

                if (CarryCurveDistance == null && carryDistance != null && carryLateralDistance != null)
                {
                    CarryCurveDistance = carryLateralDistance - ((float)Math.Sqrt((double)(carryDistance*carryDistance - carryLateralDistance * carryLateralDistance)) * tanHVL);
                }

                if (TotalCurveDistance == null && totalDistance != null && totalLateralDistance != null)
                {
                    TotalCurveDistance = totalLateralDistance - ((float)Math.Sqrt((double)(totalDistance * totalDistance - totalLateralDistance * totalLateralDistance)) * tanHVL);
                }
            }
        }

        private void TrySetRollDistance()
        {
            //For some reason the accepted roll distance calculation is total distance - carry distance,
            //rather than a trigonometric calculation of the true distance between the balls initial impact location and the final location.
            if (RollDistance == null && totalDistance != null && carryDistance != null)
            {
                RollDistance = totalDistance - carryDistance;
            }
        }

        #endregion
    }
}
