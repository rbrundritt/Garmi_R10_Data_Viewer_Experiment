using GarminR10MauiAdapter;
using System.Collections.ObjectModel;

namespace GarminR10DataViewer.Controls
{
    public class ShotClusterView: GraphicsView
    {
        public ShotClusterView()
        {
            WidthRequest = 400;
            HeightRequest = 400;

            Drawable = ShotClusterDrawable = new ShotClusterDrawable();
        }

        #region Public Properties

        public ShotClusterDrawable ShotClusterDrawable { get; set; }

        public ObservableCollection<string> ClusterProperties { get; set; } = new ObservableCollection<string>() { "Carry Distance", "Total Distance" };

        private string selectedItem = "Carry Distance";
        public string SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                ShotClusterDrawable.Metric = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The shot to render.
        /// </summary>
        public ObservableCollection<LaunchMonitorShotData>? Shots
        {
            get => (ObservableCollection<LaunchMonitorShotData>)GetValue(ShotsProperty);
            set => SetValue(ShotsProperty, value);
        }

        public static readonly BindableProperty ShotsProperty =
            BindableProperty.Create(nameof(ShotsProperty), typeof(ObservableCollection<LaunchMonitorShotData>), typeof(ShotClusterView), propertyChanged: OnShotPropertyChanged);

        #endregion

        #region Private Methods

        private static void OnShotPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != newValue && bindable is ShotClusterView view)
            {
                //Update the shot data.
                if (view.ShotClusterDrawable != null)
                {
                    var shots = (ObservableCollection<LaunchMonitorShotData>)newValue;

                    if (shots != null)
                    {
                        shots.CollectionChanged += (s, e) =>
                        {
                            view.Invalidate();
                        };
                    }

                    view.ShotClusterDrawable.Shots = shots;
                }

                //Trigger a redraw.
                view.Invalidate();
            }
        }

        #endregion
    }

    public class ShotClusterDrawable : IDrawable
    {
        #region Private Properties

        private float padding = 20;
        private float distanceInterval = 10;

        #endregion

        #region Public Properties

        public string Metric { get; set; } = "Carry Distance";

        public ObservableCollection<LaunchMonitorShotData>? Shots { get; set; } = null;

        #endregion

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            //Clear canvas.
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(dirtyRect);

            canvas.Antialias = true;

            if (Shots != null && Shots.Count > 0)
            {
                float w = dirtyRect.Width;
                float h = dirtyRect.Height;

                //Set the font for the drawing.
                canvas.SetFont(new Microsoft.Maui.Graphics.Font("OpenSansRegular"), 14, Colors.Black);

                //Draw background.
                canvas.SetHatchPatternFill(Colors.ForestGreen, Colors.Green);
                canvas.FillRectangle(50, 0, w, h);

                //Get the max and min values for the selected property.

                //Extract the metrics needed for the drawing and calculate min and max distances.
                var distances = new float[Shots.Count];
                var lateralDistances = new float[Shots.Count];

                float maxDistance = 0;
                float minDistance = float.PositiveInfinity;
                float maxAbsLateral = 0;

                for (int i = 0; i < Shots.Count; i++)
                {
                    var shot = Shots[i];
                    float distance = GetSelectedPropertyValue(shot);
                    float lateralDistance = GetSelectedPropertyLateralValue(shot);

                    distances[i] = distance;
                    lateralDistances[i] = lateralDistance;

                    if(distance < minDistance)
                    {
                        minDistance = distance;
                    }

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }

                    float absLateralDistance = Math.Abs(lateralDistance);

                    if (absLateralDistance > maxAbsLateral)
                    {
                        maxAbsLateral = absLateralDistance;
                    }
                }

                //Multiple the max lateral distance by 2 to get the full lateral width range.
                float lateralWidth = maxAbsLateral * 2;

                //If min and max distances are the same, buffer them both by the distance interval to ensure there is enough space to draw the shots.
                if (maxDistance == minDistance)
                {
                    maxDistance += distanceInterval;
                    minDistance = Math.Max(minDistance - distanceInterval, 0);
                }

                //Draw center line.
                float centerLineX = w / 2 + 25;
                canvas.SetStrokeStyle(Colors.White, 3);
                canvas.DrawLine(centerLineX, 0, centerLineX, h);

                float baseY = h - padding;
                float baseDistance = (float)Math.Round(minDistance / distanceInterval) * distanceInterval;

                float distanceDiff = Math.Max(maxDistance - baseDistance, 1);

                float scale = (float)(w - padding * 2 - 50) / lateralWidth;
                float scaleY = (float)(h - padding * 2) / distanceDiff;

                scale = (float)Math.Min(scale, scaleY);

                //Draw lateral offset lines.
                float numLateralLines = (w - padding * 2 - 50) / distanceInterval / scale / 2;

                canvas.SetStrokeStyle(Colors.Black, 1);
                for (int i = 1; i < numLateralLines; i++)
                {
                    float xOffset = i * distanceInterval * scale;

                    float minX = centerLineX - xOffset;
                    float maxX = centerLineX + xOffset;

                    if (minX > padding + 50)
                    {
                        canvas.DrawLine(minX, padding, minX, h - padding);
                    }

                    if (maxX < h - padding)
                    {
                        canvas.DrawLine(maxX, padding, maxX, h - padding);
                    }
                }

                //Draw distance lines.
                int numDistanceLines = (int)Math.Ceiling((h - padding * 2) / distanceInterval / scale);

                //Center the data within the distance line grid.
                float verticalOffset = (float)Math.Floor((numDistanceLines * distanceInterval - distanceDiff) / distanceInterval / 2);
                if (verticalOffset > 0)
                {
                    baseDistance -= distanceInterval * verticalOffset;
                }

                float distanceMarker = baseDistance;
                for (int i = 0; i < numDistanceLines; i++)
                {
                    if (distanceMarker != 0)
                    {
                        float y = (float)(baseY - (distanceMarker - baseDistance) * scale);

                        canvas.DrawLine(padding + 50, y, h - padding, y);

                        //Make sure the distance is not too close to the edge.
                        if (y > 10 && y < h - 10)
                        {
                            canvas.DrawString(distanceMarker.ToString(), padding, y + 5, HorizontalAlignment.Center);
                        }
                    }

                    distanceMarker += distanceInterval;
                }

                //Draw shots.
                var points = new List<PointF>();

                canvas.SetStrokeStyle(Colors.Yellow, 5);
                for(int i = 0;i < distances.Length; i++)
                {
                    float x = (float)(centerLineX + lateralDistances[i] * scale);
                    float y = (float)(baseY - (distances[i] - baseDistance) * scale);

                    canvas.DrawCircle(x, y, 5);

                    points.Add(new PointF(x, y));
                }
              
                canvas.SetStrokeStyle(Colors.Red, 3);

                if (points.Count >= 2)
                {
                    var ellipsePath = Models.Ellipse.ToPath(Models.Ellipse.FromPoints(points));
                    canvas.DrawPath(ellipsePath);
                }
                else if (points.Count > 1)
                {
                    var c = Models.Circle.FromPoints(points);
                    canvas.DrawCircle(c.Center, c.Radius);
                }
            }
        }

        #region Private Methods

        private float GetSelectedPropertyValue(LaunchMonitorShotData shot)
        {
            if (Metric == "Carry Distance" && shot.CarryDistance != null)
            {
                return shot.CarryDistance.Value;
            }

            if (Metric == "Total Distance" && shot.TotalDistance != null)
            {
                return shot.TotalDistance.Value;
            }

            return 0;
        }

        private float GetSelectedPropertyLateralValue(LaunchMonitorShotData shot)
        {
            if (Metric == "Carry Distance" && shot.CarryDistanceOffline != null)
            {
                return shot.CarryDistanceOffline.Value;
            }

            if (Metric == "Total Distance" && shot.TotalDistanceOffline != null)
            {
                return shot.TotalDistanceOffline.Value;
            }

            return 0;
        }

        #endregion
    }
}
