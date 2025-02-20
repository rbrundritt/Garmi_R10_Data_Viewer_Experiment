using System.Diagnostics;

namespace GarminR10DataViewer.Models
{
    public class Ellipse
    {
        #region Constructor

        /// <summary>
        /// Creates a new ellipse.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="majorAxis"></param>
        /// <param name="minorAxis"></param>
        /// <param name="rotationRadians"></param>
        public Ellipse(PointF center, float majorAxis, float minorAxis, float rotationRadians = 0)
        {
            Center = center;
            MajorAxis = majorAxis;
            MinorAxis = minorAxis;
            RotationRadians = rotationRadians;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The center of the ellipse.
        /// </summary>
        public PointF Center { get; set; }

        /// <summary>
        /// The major axis of the ellipse.
        /// </summary>
        public float MajorAxis { get; set; }

        /// <summary>
        /// The minor axis of the ellipse.
        /// </summary>
        public float MinorAxis { get; set; }

        /// <summary>
        /// The rotation of the ellipse in radians.
        /// </summary>
        public float RotationRadians { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Creates an ellipse that surrounds a collection of points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance">A percentage tolerance allowed based on the scale of the major axis. Values should be between 0 and 1.</param>
        /// <returns></returns>
        public static Ellipse FromPoints(List<PointF> points, float tolerance = 0.05f)
        {
            if(points.Count < 2)
            {
                throw new ArgumentException("At least three points are required to calculate an ellipse.");
            }

            //Scale factor of tolerance.
            float scale = 1f + tolerance;

            //When only two points, create an ellipse with the major axis as the distance between the center and the points, and the minor axis as the tolerance.
            if (points.Count == 2)
            {
                //Calculate the center of the two points.
                var center = new PointF((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2);

                //Calculate the major axis as the distance between the center and one of the points.
                float mA = points[0].Distance(center) * scale;

                //Calculate the rotation of the major axis. 
                float r = (float)Math.Atan2(points[0].Y - center.Y, points[0].X - center.X);

                //Create an initial ellipse and set the minor axis to a small distance.
                return new Ellipse(center, mA, mA * tolerance, r);
            }

            //Calculate a convex hull for the points.
            var hullPoints = MathHelper.ConvexHull(points);

            //Calculate the center of the convex hull.
            PointF hullCenter = MathHelper.GetCentroid(hullPoints);

            //Sort the points of the hull from furthest to closest to the center.
            var distanceSortedHullPoints = hullPoints.OrderByDescending(p => p.Distance(hullCenter)).ToArray();

            //Set the major axis to the distance from the center to the furthest point. 
            float majorAxis = distanceSortedHullPoints[0].Distance(hullCenter); 

            //Calculate the rotation of the major axis. 
            float rotation = (float)Math.Atan2(distanceSortedHullPoints[0].Y - hullCenter.Y, distanceSortedHullPoints[0].X - hullCenter.X);

            //Create an initial ellipse and set the minor axis to a small distance. 
            var ellipse = new Ellipse(hullCenter, majorAxis, majorAxis * tolerance, rotation);

            //Loop through the hull points and scale the minor axis until the ellipse contains all the points.
            bool flag = false;
            int iterations = 0;
            while (!flag)
            {
                flag = true;

                for (int i = 0; i < hullPoints.Count; i++)
                {
                    var p = hullPoints[i];
                    iterations++;
                    //Determine if the point is not inside the ellipse.
                    //If it doesn't break the loop and scale the minor axis, and try again.
                    if (!Contains(ellipse, p))
                    {
                        //Scale the minor axis.
                        ellipse.MinorAxis *= scale;

                        flag = false;
                        break;
                    }
                }

                //Check to see if the minor axis is greater than the major axis.
                if (ellipse.MinorAxis >= ellipse.MajorAxis)
                {
                    //If it is, scale the major axis and reset the minor axis.
                    ellipse.MajorAxis *= scale;
                    ellipse.MinorAxis = tolerance;
                }
            }

            Debug.WriteLine("Iterations: " + iterations);

            return ellipse;
        }

        /// <summary>
        /// Calculates the area of the ellipse.
        /// </summary>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        public static float Area(Ellipse ellipse)
        {
            return (float)(Math.PI * ellipse.MajorAxis * ellipse.MinorAxis);
        }

        /// <summary>
        /// Calculates the unrotated bounding box of the ellipse.
        /// </summary>
        /// <param name="ellipse"></param>
        /// <returns></returns>
        public static RectF Bounds(Ellipse ellipse)
        {
            float radians90 = ellipse.RotationRadians + (float)Math.PI / 2;
            float ux = ellipse.MajorAxis * (float)Math.Cos(ellipse.RotationRadians);
            float uy = ellipse.MajorAxis * (float)Math.Sin(ellipse.RotationRadians);
            float vx = ellipse.MinorAxis * (float)Math.Cos(radians90);
            float vy = ellipse.MinorAxis * (float)Math.Sin(radians90);

            float width = (float)Math.Sqrt(ux * ux + vx * vx) * 2;
            float height = (float)Math.Sqrt(uy * uy + vy * vy) * 2;
            float x = ellipse.Center.X - (width / 2);
            float y = ellipse.Center.Y - (height / 2);

            return new RectF(x, y, width, height);
        }

        /// <summary>
        /// Determines if the given point is contained within the ellipse.
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool Contains(Ellipse ellipse, PointF point)
        {
            float cosRotation = (float)Math.Cos(ellipse.RotationRadians);
            float sinRotation = (float)Math.Sin(ellipse.RotationRadians);

            float a = ellipse.MajorAxis;
            float b = ellipse.MinorAxis;

            float xPrime = (float)((point.X - ellipse.Center.X) * cosRotation + (point.Y - ellipse.Center.Y) * sinRotation);
            float yPrime = (float)((point.Y - ellipse.Center.Y) * cosRotation - (point.X - ellipse.Center.X) * sinRotation);

            return (xPrime * xPrime) / (a * a) + (yPrime * yPrime) / (b * b) <= 1;
        }

        /// <summary>
        /// Converts the ellipse to a path.
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="numPoints">The number of sample points to use to interpolate the ellipse's path.</param>
        /// <returns></returns>
        public static PathF ToPath(Ellipse ellipse, int numPoints = 100)
        {
            var path = new PathF();

            float angleIncrement = (float)(2 * Math.PI / numPoints);
            float cosRotation = (float)Math.Cos(ellipse.RotationRadians);
            float sinRotation = (float)Math.Sin(ellipse.RotationRadians);

            float a = (float)ellipse.MajorAxis;
            float b = (float)ellipse.MinorAxis;

            float x0 = ellipse.Center.X;
            float y0 = ellipse.Center.Y;

            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * angleIncrement;
                float x = (float)(x0 + a * Math.Cos(angle) * cosRotation - b * Math.Sin(angle) * sinRotation);
                float y = (float)(y0 + a * Math.Cos(angle) * sinRotation + b * Math.Sin(angle) * cosRotation);

                if (i == 0)
                {
                    path.MoveTo(new PointF(x, y));
                }
                else
                {
                    path.LineTo(new PointF(x, y));
                }
            }

            path.Close();

            return path;
        }

        #endregion
    }
}
