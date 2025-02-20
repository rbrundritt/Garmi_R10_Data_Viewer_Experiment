using System.Globalization;
using System.Text;

namespace GarminR10MauiAdapter.IO
{
    /// <summary>
    /// Write launch monitor data to a CSV file.
    /// Based on the CSV export format used by Awesome Golf for simplictity and consistency, and easy import to existing tools like Golf Shot Analytics (https://www.golfshotanalytics.com/).
    /// </summary>
    public class CsvShotDataWriter : IDisposable
    {
        private Units _outputUnits;
        private Units _inputUnits;
        private StreamWriter? _writer = null;

        private SpeedUnit _inputSpeedUnit;
        private SpeedUnit _outputSpeedUnit;

        private DistanceUnit _inputDistanceUnit;
        private DistanceUnit _outputDistanceUnit;

        private DistanceUnit _heightUnits;

        #region Constructor

        /// <summary>
        /// Write launch monitor data to a CSV file.
        /// Based on the CSV export format used by Awesome Golf for simplictity and consistency, and easy import to existing tools like Golf Shot Analytics (https://www.golfshotanalytics.com/).
        /// </summary>
        /// <param name="outputStream">The stream to write to (usually a writable file stream)</param>
        /// <param name="outputUnits"></param>
        /// <param name="inputUnits"></param>
        public CsvShotDataWriter(Stream outputStream, Units outputUnits = Units.Metric, Units inputUnits = Units.Metric)
        {
            _outputUnits = outputUnits;
            _inputUnits = inputUnits;

            _inputSpeedUnit = _inputUnits == Units.Metric ? SpeedUnit.MPS : SpeedUnit.MPH;
            _outputSpeedUnit = _outputUnits == Units.Metric ? SpeedUnit.MPS : SpeedUnit.MPH;

            _inputDistanceUnit = _inputUnits == Units.Metric ? DistanceUnit.Meters : DistanceUnit.Yards;
            _outputDistanceUnit = _outputUnits == Units.Metric ? DistanceUnit.Meters : DistanceUnit.Yards;

            _heightUnits = _outputUnits == Units.Metric ? DistanceUnit.Meters : DistanceUnit.Feet;

            _writer = new StreamWriter(outputStream, Encoding.UTF8, 65536);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Flushes the stream and disposes it.
        /// </summary>
        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Dispose();
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public void Flush()
        {
            if (_writer != null)
            {
                _writer.Flush();
            }
        }

        /// <summary>
        /// Write the header to the CSV file.
        /// </summary>
        public void WriteHeader()
        {
            if (_writer != null)
            {
                _writer.WriteLine("Date,Club Type,Club Description,Altitude,Club Speed,Ball Speed,Carry Distance,Total Distance,Roll Distance,Smash,Vertical Launch,Peak Height,Descent Angle,Horizontal Launch,Carry Lateral Distance,Total Lateral Distance,Carry Curve Distance,Total Curve Distance,Attack Angle,Dynamic Loft,Spin Loft,Spin Rate,Spin Axis,Spin Reading,Low Point,Club Path,Face Path,Face Target,Swing Plane Tilt,Swing Plane Rotation,Shot Classification");

                if (_outputUnits == Units.Metric)
                {
                    _writer.WriteLine(",,,[m],[m/s],[m/s],[m],[m],[m],,[deg],[m],[deg],[deg],[m],[m],[m],[m],[deg],[deg],[deg],[rpm],[deg],,[cm],[deg],[deg],[deg],[deg],[deg],");
                }
                else
                {
                    _writer.WriteLine(",,,[ft],[mph],[mph],[yd],[yd],[yd],,[deg],[ft],[deg],[deg],[yd],[yd],[yd],[yd],[deg],[deg],[deg],[rpm],[deg],,[in],[deg],[deg],[deg],[deg],[deg],");
                }
            }
        }

        /// <summary>
        /// Write a shot to the CSV file.
        /// </summary>
        /// <param name="shot"></param>
        public void WriteShot(LaunchMonitorShotData shot)
        {
            if (_writer != null)
            {
                //Date - Format: "yyyy-MM-dd HH:mm:ss" "3/6/2023  3:17:49 PM"                
                _writer.Write(shot.DateTime.ToString("G", CultureInfo.InvariantCulture));

                //Club Type
                _writer.Write("," + Utils.ClubName(shot.Club));

                //Club Description - Not supported currently
                _writer.Write(",");

                //Altitude (m or ft) - Players altitude when taking the shot. - Not supported currently
                WriteHeight(shot.Altitude);
                
                //Club Speed (m/s or mph) - Club head speed at impact.
                WriteSpeed(shot.ClubSpeed);

                //Ball Speed (m/s or mph) - Ball speed at impact.
                WriteSpeed(shot.BallSpeed);

                //Carry Distance (m or yds) - Distance the ball carried.
                WriteDistance(shot.CarryDistance);

                //Total Distance (m or yds) - Total distance the ball traveled. Not outputted by the R10
                WriteDistance(shot.TotalDistance);

                //Roll Distance (m or yds) - Distance the ball rolled. Not outputted by the R10
                WriteDistance(shot.CarryDistance);

                //Smash - Ratio of ball speed to club speed. 
                WriteNumber(shot.SmashFactor);

                //Vertical Launch (deg) - Angle the ball launched at.
                WriteNumber(shot.VerticalLaunchAngle);

                //Peak Height (m or ft) - Maximum height the ball reached. Not outputted by the R10
                WriteHeight(shot.MaxHeight);

                //Descent Angle (deg) - Angle the ball descended at. Not outputted by the R10
                WriteNumber(shot.DescentAngle);

                //Horizontal Launch (deg) - Direction left/right the ball launched at.
                WriteNumber(shot.HorizontalLaunchAngle);

                //Carry Lateral Distance (m or yds) - Distance the ball carried left/right. Not outputted by the R10
                WriteDistance(shot.CarryDistanceOffline);

                //Total Lateral Distance (m or yds) - Total distance the ball traveled left/right. Not outputted by the R10
                WriteDistance(shot.TotalDistanceOffline);

                //Carry Curve Distance (m or yds) - Distance the ball carried in/out. Not outputted by the R10
                WriteDistance(shot.CarryCurveDistance);

                //Total Curve Distance (m or yds) - Total distance the ball traveled in/out. Not outputted by the R10
                WriteDistance(shot.TotalCurveDistance);

                //Attack Angle (deg) - Angle the club head was moving at impact.
                WriteNumber(shot.AngleOfAttack);

                //Dynamic Loft (deg)
                WriteNumber(shot.DynamicLoft);

                //Spin Loft (deg) - Not outputted by the R10
                WriteNumber(shot.SpinLoft);

                //Spin Rate (rpm)
                WriteNumber(shot.SpinRate);

                //Spin Axis (deg) - Not outputted by the R10
                WriteNumber(shot.SpinAxis);

                //Spin Reading
                _writer.Write("," + Enum.GetName(typeof(SpinMethod), shot.SpinMethod));

                //Low Point (cm or in) - Not outputted by the R10
                _writer.Write(",");

                //Club Path
                WriteNumber(shot.ClubPath);

                //Face Path (deg) - Not outputted by the R10
                WriteNumber(shot.FacePath);

                //Face Target / Face Target (deg)
                WriteNumber(shot.FaceToTarget);

                //Swing Plane Tilt (deg)
                _writer.Write(",");

                //Swing Plane Rotation (deg)
                _writer.Write(",");

                //Shot Classification
                _writer.Write(",");

                if (shot.ShotShape != null)
                {
                    _writer.Write(Enum.GetName(typeof(SpinMethod), shot.ShotShape));
                } 
            }
        }

        /// <summary>
        /// Write multiple shots to the CSV file.
        /// </summary>
        /// <param name="shots"></param>
        public void WriteShots(IEnumerable<LaunchMonitorShotData> shots)
        {
            if (_writer != null)
            {
                foreach (var shot in shots)
                {
                    WriteShot(shot);
                }
            }
        }

        #endregion

        #region Private Methods

        private void WriteNumber(float? value)
        {
            if (_writer != null)
            {
                _writer.Write(",");

                if (value != null)
                {
                    _writer.Write(value.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void WriteSpeed(float? value)
        {
            if (_writer != null)
            {
                _writer.Write(",");

                if (value != null)
                {
                    _writer.Write(Utils.ConvertSpeed(value.Value, _inputSpeedUnit, _outputSpeedUnit).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void WriteDistance(float? value)
        {
            if (_writer != null)
            {
                _writer.Write(",");

                if (value != null)
                {
                    _writer.Write(Utils.ConvertDistance(value.Value, _inputDistanceUnit, _outputDistanceUnit).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void WriteHeight(float? value)
        {
            if (_writer != null)
            {
                _writer.Write(",");

                if (value != null)
                {
                    _writer.Write(Utils.ConvertDistance(value.Value, _inputDistanceUnit, _heightUnits).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        #endregion
    }
}