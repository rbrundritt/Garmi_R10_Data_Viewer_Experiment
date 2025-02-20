using SkiaSharp;

namespace GarminR10DataViewer.Models
{
    public class Circle
    {
        #region Constructor 

        public Circle(PointF center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The center of the circle.
        /// </summary>
        public PointF Center { get; set; }

        /// <summary>
        /// The radius of the circle.
        /// </summary>
        public float Radius { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Calculate the area of a circle.
        /// </summary>
        /// <param name="circle"></param>
        /// <returns></returns>
        public static float Area(Circle circle)
        {
            return (float)(Math.PI * Math.Pow(circle.Radius, 2));
        }

        /// <summary>
        /// Calculate the bounding box of a circle.
        /// </summary>
        /// <param name="circle"></param>
        /// <returns></returns>
        public static RectF Bounds(Circle circle)
        {
            return new RectF(circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius, circle.Radius * 2, circle.Radius * 2);
        }

        /// <summary>
        /// Determines if a point is within a circle.
        /// </summary>
        /// <param name="circle">The circle</param>
        /// <param name="point">A point</param>
        /// <returns>If the circle contains the point</returns>
        public static bool Contains(Circle circle, PointF point)
        {
            if (circle != null)
            {
                if (point.Distance(circle.Center) <= circle.Radius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the smallest enclosing circle of a set of points using Welzl's algorithm.
        /// </summary>
        /// <param name="points"></param>
        public static Circle? FromPoints(List<PointF> points)
        {
            // Check the input
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Invalid input PointFs");
            }

            if(points.Count == 2)
            {
                return Circle2Point(points[0], points[1]);
            } 
            else if(points.Count == 3)
            {
                return Circle3Point(points[0], points[1], points[2]);
            }

            var r = new List<PointF>();
            return Welzl(points, r);
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Welzl Algorithm for computing the smallest circle containing a set of points in 2D.
        /// </summary>
        /// <param name="inputPoints">Set of input points</param>
        /// <param name="smallestCirclePoints">Set of points on the smallest circle</param>
        /// <returns>The smallest circle by Welzl algorithm</returns>
        private static Circle? Welzl(List<PointF> inputPoints, List<PointF> smallestCirclePoints)
        {
            //Create a copy of the input points for manipulation.
            var setPoints = new List<PointF>(inputPoints);
            var rand = new Random(); 

            if (setPoints.Count == 0 || smallestCirclePoints.Count == 3)
            {
                //If there are 3 or less points, it's a trivial case.

                //If less than 2 points, return null as a circle can't be created.
                if (smallestCirclePoints.Count < 2)
                {
                    return new Circle(new PointF(0, 0), 0);
                }

                //There there are two points, use the distance between them to caculate the center and radius.
                if (smallestCirclePoints.Count == 2)
                {
                    return Circle2Point(smallestCirclePoints[0], smallestCirclePoints[1]);
                }

                //There must be 3 or more points. This method is only means to take in 3 points at max, so ignore all other points.
                //Calculate the circumscribed circle of the three points.
                return Circle3Point(smallestCirclePoints[0], smallestCirclePoints[1], smallestCirclePoints[2]);
            }

            var pt = setPoints[rand.Next(setPoints.Count)];
            setPoints.Remove(pt); //Remove the choosen point from the set.
            Circle? d = Welzl(setPoints, smallestCirclePoints); //Recursive call on P1 and R , with P1 = P-{p}

            if (d != null && !Contains(d, pt))
            { 
                //If d is defined and it does not contain p
                smallestCirclePoints.Add(pt); //Add p to R
                d = Welzl(setPoints, smallestCirclePoints);
                smallestCirclePoints.Remove(pt);
            }

            return d;
        }
        
        /// <summary>
        /// Calculate a circle from 2 points.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static Circle? Circle2Point(PointF a, PointF b)
        {
            float cx = (a.X + b.X) / 2;
            float cy = (a.Y + b.Y) / 2;
            float r = a.Distance(b) / 2;

            return new Circle(new PointF(cx, cy), r);
        }

        /// <summary>
        /// Calculate a circle circumscribed by three Points.
        /// </summary>
        /// <param name="a">Point a</param>
        /// <param name="b">Point b</param>
        /// <param name="c">Point c</param>
        /// <returns>The circumscribed circle of a, b and c</returns>
        private static Circle? Circle3Point(PointF a, PointF b, PointF c)
        {
            //Calculation of the circumscribed circle of the three points.
            float d = (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) * 2;

            if (d == 0)
            {
                return null;
            }

            float x = ((Normal(a) * (b.Y - c.Y)) + (Normal(b) * (c.Y - a.Y)) + (Normal(c) * (a.Y - b.Y))) / d;
            float y = ((Normal(a) * (c.X - b.X)) + (Normal(b) * (a.X - c.X)) + (Normal(c) * (b.X - a.X))) / d;
            var center = new PointF(x, y);

            return new Circle(center, center.Distance(a));
        }

        /// <summary>
        /// Helper function to compute the norm of a vector.
        /// </summary>
        /// <param name="point">A vector point</param>
        /// <returns>The norm of the vector point</returns>
        private static float Normal(PointF point)
        {
            return (point.X * point.X) + (point.Y * point.Y);
        }

        #endregion
    }
}
