using GarminR10DataViewer.Models;
using GarminR10MauiAdapter;
using Microsoft.Maui.Graphics;

namespace GarminR10DataViewer.Controls
{
    public class ShotProfileView: GraphicsView
    {

        public ShotProfileView()
        {
            WidthRequest = 400;
            HeightRequest = 400;

            Drawable = ShotProfileDrawable = new ShotProfileDrawable();
        }

        #region Public Properties

        public ShotProfileDrawable ShotProfileDrawable { get; set; }

        /// <summary>
        /// The shot to render.
        /// </summary>
        public LaunchMonitorShotData? Shot
        {
            get => (LaunchMonitorShotData)GetValue(ShotProperty);
            set => SetValue(ShotProperty, value);
        }

        public static readonly BindableProperty ShotProperty =
            BindableProperty.Create(nameof(Shot), typeof(LaunchMonitorShotData), typeof(ShotProfileView), null, propertyChanged: OnShotPropertyChanged);

        #endregion

        #region Private Methods

        private static void OnShotPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != newValue && bindable is ShotProfileView view)
            {
                //Update the shot data.
                if (view.ShotProfileDrawable != null)
                {
                    view.ShotProfileDrawable.Shot = (LaunchMonitorShotData)newValue;
                }

                //Trigger a redraw.
                view.Invalidate();
            }
        }

        #endregion
    }

    public class ShotProfileDrawable : IDrawable
    {
        #region Private Properties

        private int heightIncrements = 10;
        private int distanceIncrements = 20;
        private float maxDistance = 300;
        private float maxHeight = 50;

        //Actual canvas instance to draw on.
        private ICanvas _canvas;

        //Holds information about the dimensions, etc.
        private RectF canvasArea;

        private float scale = 1;
        private float overheadViewCenterY = 0;
        private float paddingX = 35;
        private float drawWidth = 0;
        private float drawHeight = 0;

        private RectF overheadViewRect;
        private RectF sideViewRect;

        #endregion

        #region Public Properties

        public LaunchMonitorShotData? Shot { get; set; } = null;

        #endregion


        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _canvas = canvas;
            canvasArea = dirtyRect;

            //Clear canvas.
            _canvas.FillColor = Colors.Transparent;
            _canvas.FillRectangle(canvasArea);

            _canvas.Antialias = true;

            if (Shot != null
                && Shot.TotalDistance != null
                && Shot.CarryDistance != null && Shot.CarryDistanceOffline != null
                && Shot.MaxHeight != null && Shot.MaxHeightDistance != null && Shot.MaxHeightDistanceOffline != null)
            {
                var h = canvasArea.Height;
                var w = canvasArea.Width;

                sideViewRect = new RectF(0, 0, w, h / 2);
                overheadViewRect = new RectF(0, h / 2, w, h);

                if (Shot.TotalDistance.Value > 150)
                {
                    distanceIncrements = 50;
                }
                else
                {
                    distanceIncrements = 20;

                }

                maxDistance = Math.Max(Shot.TotalDistance.Value, Shot.CarryDistance.Value) + distanceIncrements;
                maxHeight = Shot.MaxHeight.Value + heightIncrements;

                if (Shot.Units == Units.Imperial)
                {
                    //Height is in feet, need to convert to yards.
                    maxHeight /= 3;
                }

                drawWidth = w - paddingX * 2;
                drawHeight = h / 2 - paddingX * 2;

                //Determine the scale to use for the drawing.
                scale = drawWidth / maxDistance;

                var scaleY = drawHeight / maxHeight;

                scale = Math.Min(scale, scaleY);

                //Calculate the center of the overhead view.
                overheadViewCenterY = h / 4 * 3;

                //Set the font for the drawing.
                _canvas.SetFont(new Microsoft.Maui.Graphics.Font("OpenSansRegular"), 14, Colors.Black);

                DrawShotOverheadView();
                DrawShotSideProfile();
            }
        }

        /// <summary>
        /// Draw center line and distance marker lines and labels. 
        /// </summary>
        private void DrawShotOverheadView()
        {
            //Draw background.
            _canvas.SetHatchPatternFill(Colors.ForestGreen, Colors.Green);
            _canvas.FillRectangle(overheadViewRect);

            //Draw center line.
            _canvas.SetStrokeStyle(Colors.White, 3);
            _canvas.DrawLine(paddingX, overheadViewCenterY, canvasArea.Width - paddingX, overheadViewCenterY);

            //Draw distance markers.
            _canvas.SetStrokeStyle(Colors.Black, 2);
            for (int i = 0; i < maxDistance; i += distanceIncrements)
            {
                if (i > 0)
                {
                    var x = i * scale + paddingX;
                    var yOffset = x / 10;

                    _canvas.DrawLine(x, overheadViewCenterY - yOffset, x, overheadViewCenterY + yOffset);
                }
            }

            //Draw the shot overhead view path.
            var overheadPoints = new List<PointF> {
                new PointF(paddingX, overheadViewCenterY),
                new PointF(Shot.MaxHeightDistance.Value * scale + paddingX, overheadViewCenterY + Shot.MaxHeightDistanceOffline.Value * scale),
                new PointF(Shot.CarryDistance.Value * scale + paddingX, overheadViewCenterY + Shot.CarryDistanceOffline.Value * scale)
            };

            //Calculate cardinal spline from tee, to max height distance/offline, to carry distance/offline.
            var splinePath = MathHelper.CardinalSpline(overheadPoints, 20, 0.5f);

            _canvas.SetStrokeStyle(Colors.Yellow, 5);
            _canvas.DrawPath(splinePath);

            //Draw distance labels.
            for (int i = 0; i < maxDistance; i += distanceIncrements)
            {
                if (i > 0)
                {
                    var x = i * scale + paddingX;

                    //Write the distance near top of drawing area so it can be used by both views.
                    _canvas.DrawString(i.ToString(), x, overheadViewRect.Top + paddingX, HorizontalAlignment.Center);
                }
            }
        }


        private void DrawShotSideProfile()
        {
            //Draw sky background.
            _canvas.FillColor = Colors.LightSkyBlue;
            _canvas.FillRectangle(sideViewRect);

            //Draw ground.
            _canvas.FillColor = Colors.ForestGreen;
            _canvas.FillRectangle(0, sideViewRect.Bottom - 35, canvasArea.Width, 35);

            //Draw some clouds.
            _canvas.FillColor = Colors.White;
            _canvas.FillCircle(120, 60, 30);
            _canvas.FillCircle(160, 50, 40);
            _canvas.FillCircle(190, 60, 30);

            _canvas.FillCircle(320, 50, 30);
            _canvas.FillCircle(360, 40, 40);
            _canvas.FillCircle(390, 50, 30);

            float heightUnitScale = 1;

            if (Shot.Units == Units.Imperial)
            {
                //Height is in feet, need to convert to yards.
                heightUnitScale = 0.33f;
                maxHeight /= heightUnitScale;
                heightIncrements = 30;
            }
            else
            {
                heightIncrements = 10;
            }

            //Draw horizontal grid lines to represent height.
            float baseLineY = sideViewRect.Bottom - 25;
            float maxY = 0;

            _canvas.SetStrokeStyle(Colors.Black, 1);
            for (int i = 0; i < maxHeight; i += heightIncrements)
            {
                var y = baseLineY - i * scale * heightUnitScale;

                _canvas.DrawLine(paddingX, y, canvasArea.Width - paddingX, y);

                maxY = y;
            }

            //Draw vertical grid lines to represent distance.
            for (int i = 0; i < maxDistance; i += distanceIncrements)
            {
                if (i > 0)
                {
                    var x = i * scale + paddingX;

                    _canvas.DrawLine(x, maxY, x, baseLineY);
                }
            }

            _canvas.DrawLine(canvasArea.Width - paddingX, maxY, canvasArea.Width - paddingX, baseLineY);

            //Draw the shot side view path.
            var overheadPoints = new List<PointF>
            {
                new PointF(paddingX, baseLineY),
                new PointF(Shot.MaxHeightDistance.Value * scale + paddingX, baseLineY - Shot.MaxHeight.Value * scale * heightUnitScale),
                new PointF(Shot.CarryDistance.Value * scale + paddingX, baseLineY)
            };

            //Calculate cardinal spline from tee, to max height distance/offline, to carry distance/offline.
            var splinePath = MathHelper.CardinalSpline(overheadPoints, 100);

            if (Shot.TotalDistance.Value > Shot.CarryDistance.Value)
            {
                int numBounces = 2;
                float bounceMagnitude = 1f;
                float rollDistance = (Shot.TotalDistance.Value - Shot.CarryDistance.Value) * scale;
                float maxBounceDistance = rollDistance / (numBounces + 2);

                float x = Shot.CarryDistance.Value * scale + paddingX;

                //Add a couple bounces to the total distance.
                for (int i = 1; i <= numBounces; i++)
                {
                    var bounceHeight = Shot.MaxHeight.Value * heightUnitScale * bounceMagnitude / i;
                    var halfBounceDistance = maxBounceDistance / 2;

                    x += halfBounceDistance;
                    splinePath.QuadTo(x, baseLineY - bounceHeight, x + halfBounceDistance, baseLineY);
                    x += halfBounceDistance;
                }

                //Draw a striaght line to the total distance.
                splinePath.LineTo(Shot.TotalDistance.Value * scale + paddingX, baseLineY);
            }
            else
            {
                //Draw a striaght line to the total distance. Like a backspin caused the ball to roll back without bouncing.
                splinePath.LineTo(Shot.TotalDistance.Value * scale + paddingX, baseLineY);
            }

            _canvas.SetStrokeStyle(Colors.Yellow, 5);
            _canvas.DrawPath(splinePath);

            //Draw height labels.
            for (int i = 0; i < maxHeight; i += heightIncrements)
            {
                if (i > 0)
                {
                    var y = baseLineY - i * scale * heightUnitScale;
                    _canvas.DrawString(i.ToString(), 15, y + 5, HorizontalAlignment.Center);
                }
            }
        }
    }
}