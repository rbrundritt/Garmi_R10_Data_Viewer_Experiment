namespace GarminR10DataViewer.Models
{
    public class MathHelper
    {
        #region Public Methods

        /// <summary>
        /// Calculate the centroid (center of mass) of points that make up a polygon.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static PointF GetCentroid(List<PointF> points)
        {
            float totalArea = 0;
            float centroidX = 0;
            float centroidY = 0;

            for (int i = 0; i < points.Count; i++)
            {
                int nextIndex = (i + 1) % points.Count;
                float triangleArea = TriangleArea(points[i], points[nextIndex], points[0]);

                totalArea += triangleArea;
                centroidX += (points[i].X + points[nextIndex].X + points[0].X) * triangleArea;
                centroidY += (points[i].Y + points[nextIndex].Y + points[0].Y) * triangleArea;
            }

            centroidX /= (3 * totalArea);
            centroidY /= (3 * totalArea);

            return new PointF { X = centroidX, Y = centroidY };
        }

        /// <summary>
        /// Calculates the path of a cardinal spline through a collection of points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="nodeSize"></param>
        /// <param name="tension"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static PathF CardinalSpline(List<PointF> points, int nodeSize = 20, float tension = 0.5f)
        {
            if (points.Count < 3)
            {
                throw new ArgumentException("At least three points are required for interpolation");
            }

            //In this case the spline is not closed, so tanget of end points will be 0.
            //Buffer the end-points so that tanget calculations can be performed.
            points.Insert(0, points[0]);
            points.Add(points[points.Count - 1]);

            //Precalculate the hermite basis function steps along the spline. 
            var hermiteSteps = new List<float[]>()
            {
                //Force the first step between two locations to be the first location.
                new float[] { 1, 0, 0, 0 }
            };

            //Calculate the steps along the spline between two locations.
            for (int i = 1; i < nodeSize - 1; i++)
            {
                float step = (float)i / (float)nodeSize;            //Scale step to go from 0 to 1.

                float step2 = step * step;            //s^2
                float step3 = step * step2;           //s^3

                hermiteSteps.Add([
                    2 * step3 - 3 * step2 + 1,  //Calculate hermite basis function 1.
                    -2 * step3 + 3 * step2,     //Calculate hermite basis function 2.
                    step3 - 2 * step2 + step,   //Calculate hermite basis function 3.
                    step3 - step2]);            //Calculate hermite basis function 4.
            }

            //Force the last step between two locations to be the last location.
            hermiteSteps.Add([0, 1, 0, 0]);

            var path = new PathF();
            path.MoveTo(points[0]);

            //Loop through and calculate the spline path between each point. 
            for (int i = 1; i < points.Count - 2; i++)
            {
                //Tangents
                float t1x = tension * (points[i + 1].X - points[i - 1].X);
                float t1y = tension * (points[i + 1].Y - points[i - 1].Y);
                float t2x = tension * (points[i + 2].X - points[i].X);
                float t2y = tension * (points[i + 2].Y - points[i].Y);

                for (int j = 0; j < nodeSize; j++)
                {
                    var hermiteStep = hermiteSteps[j];

                    float x = hermiteStep[0] * points[i].X + hermiteStep[1] * points[i + 1].X + hermiteStep[2] * t1x + hermiteStep[3] * t2x;
                    float y = hermiteStep[0] * points[i].Y + hermiteStep[1] * points[i + 1].Y + hermiteStep[2] * t1y + hermiteStep[3] * t2y;

                    path.LineTo(new PointF(x, y));
                }
            }

            return path;
        }
 
        /// <summary>
        /// A method to find the convex hull of a set of points using the Jarvis march algorithm.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<PointF> ConvexHull(List<PointF> points)
        {
            // Check the input
            if (points == null || points.Count < 3)
            {
                throw new ArgumentException("Invalid input points");
            }

            // Find the leftmost point
            int left = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].X < points[left].X)
                {
                    left = i;
                }
            }

            // Start the march
            var hull = new List<PointF>(); // indices of the hull points
            int p = left; // current point
            do
            {
                // Add the current point to the hull
                hull.Add(points[p]);

                // Find the next point
                int q = (p + 1) % points.Count; // candidate point
                for (int i = 0; i < points.Count; i++)
                {
                    // Check if i is more counterclockwise than q
                    if (Orientation(points[p], points[i], points[q]) == 2)
                    {
                        q = i;
                    }
                }

                // Set the next point
                p = q;

            } while (p != left); // loop until reaching the first point

            // Return the hull
            return hull;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculate the area of a triangle formed by three points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        private static float TriangleArea(PointF p1, PointF p2, PointF p3)
        {
            return 0.5f * Math.Abs((p1.X * (p2.Y - p3.Y)) + (p2.X * (p3.Y - p1.Y)) + (p3.X * (p1.Y - p2.Y)));
        }


        // A method to find the orientation of the ordered triplet (p, q, r)
        // 0 = collinear, 1 = clockwise, 2 = counterclockwise
        private static int Orientation(PointF p, PointF q, PointF r)
        {
            // Use the cross product of vectors (p - q) and (r - q)
            float val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);

            if (val == 0)
            {
                return 0; // collinear
            }

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        #endregion
    }
}
