using GarminR10MauiAdapter;
using Microsoft.ML.Data;

namespace GarminR10DataViewer.ML.Models
{
    internal class GolfShotModelInput
    {
        public GolfShotModelInput()
        {
        }

        public GolfShotModelInput(LaunchMonitorShotData shot)
        {
            if (shot.BallSpeed != null || shot.VerticalLaunchAngle != null || shot.HorizontalLaunchAngle != null || shot.SpinRate != null || shot.SpinAxis != null)
            {
                this.BallSpeedMPS = Utils.ConvertSpeed(shot.BallSpeed.Value, shot.Units == Units.Imperial ? SpeedUnit.MPH : SpeedUnit.MPS, SpeedUnit.MPS);
                this.VerticalLaunchAngleDeg = shot.VerticalLaunchAngle.Value;
                this.HorizontalLaunchAngleDeg = shot.HorizontalLaunchAngle.Value;
                this.SpinRateRPM = shot.SpinRate.Value;
                this.SpinAxisDeg = shot.SpinAxis.Value;
            }
        }

        [LoadColumn(0)]
        [ColumnName(@"BallSpeedMPS")]
        public float BallSpeedMPS { get; set; }

        [LoadColumn(1)]
        [ColumnName(@"VerticalLaunchAngleDeg")]
        public float VerticalLaunchAngleDeg { get; set; }

        [LoadColumn(2)]
        [ColumnName(@"HorizontalLaunchAngleDeg")]
        public float HorizontalLaunchAngleDeg { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"SpinRateRPM")]
        public float SpinRateRPM { get; set; }

        [LoadColumn(4)]
        [ColumnName(@"SpinAxisDeg")]
        public float SpinAxisDeg { get; set; }
    }
}
