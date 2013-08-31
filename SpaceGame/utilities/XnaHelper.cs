using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace SpaceGame.utility
{
    enum Direction {North, East, South, West}

    /// <summary>
    /// Represents a 2D line segment
    /// </summary>
    struct Segment
    {
        public float X1, X2, Y1, Y2;

        public Vector2 Start
        {
            get { return new Vector2(X1, Y1); }
            set { X1 = value.X; Y1 = value.Y; }
        }
        public Vector2 End
        {
            get { return new Vector2(X2, Y2); }
            set { X2 = value.X; Y2 = value.Y; }
        }

        public static Segment FromRectangle(Rectangle rect, Direction side)
        {
            Segment seg = new Segment();
            switch (side)
            {
                case Direction.North:
                    seg.X1 = rect.Left; seg.Y1 = rect.Top;
                    seg.X2 = rect.Right; seg.Y2 = rect.Top;
                    break;
                case Direction.East:
                    seg.X1 = rect.Right; seg.Y1 = rect.Top;
                    seg.X2 = rect.Right; seg.Y2 = rect.Bottom;
                    break;
                case Direction.South:
                    seg.X1 = rect.Right; seg.Y1 = rect.Bottom;
                    seg.X2 = rect.Left; seg.Y2 = rect.Bottom;
                    break;
                case Direction.West:
                    seg.X1 = rect.Left; seg.Y1 = rect.Top;
                    seg.X2 = rect.Left; seg.Y2 = rect.Bottom;
                    break;
            }
            return seg;
        }

        public static IEnumerable<Segment> RectangleSides(Rectangle rect)
        {
            return new List<Segment>() 
            {
                FromRectangle(rect, Direction.North), FromRectangle(rect, Direction.East),
                FromRectangle(rect, Direction.South), FromRectangle(rect, Direction.West),
            };
            
        }
    }

    static class XnaHelper
    {
        public static Texture2D PixelTexture;

        static Random rand = new Random();
        static Vector2 tempVec1;
        static Matrix tempMatrix;
        /// <summary>
        /// get a unit vector pointing from start to end
        /// </summary>
        /// <param name="start">direction vector origin</param>
        /// <param name="end">direction vector destination</param>
        /// <returns></returns>
        public static Vector2 DirectionBetween(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;

            if (direction.Length() > 0)
                direction.Normalize();

            return direction;
        }

        /// <summary>
        /// get the angle (in radians) that a vector is pointing
        /// </summary>
        /// <param name="direction">vector from which to compute angle</param>
        /// <returns></returns>
        public static float RadiansFromVector(Vector2 direction)
        {
            return (float)Math.Atan2(direction.X, -direction.Y);
        }

        /// <summary>
        /// get the angle (in degrees) that a vector is pointing
        /// </summary>
        /// <param name="direction">vector from which to compute angle</param>
        /// <returns></returns>
        public static float DegreesFromVector(Vector2 direction)
        {
            return MathHelper.ToDegrees((float)Math.Atan2(direction.X, -direction.Y));
        }

        /// <summary>
        /// get a unit vector pointing in the direction of the angle
        /// </summary>
        /// <param name="angle">angle in radians</param>
        /// <returns>unit vector pointing towards angle</returns>
        public static Vector2 VectorFromAngle(float angle)
        {
            Matrix.CreateRotationZ(angle, out tempMatrix);
            return Vector2.Transform(-Vector2.UnitY, tempMatrix);
        }

        /// <summary>
        /// get a unit vector pointing in the direction of the angle
        /// </summary>
        /// <param name="angle">angle in radians</param>
        /// <returns></returns>
        public static void VectorFromAngle(float angle, out Vector2 outVector)
        {
            Matrix.CreateRotationZ(angle, out tempMatrix);
            tempVec1.X = 0;
            tempVec1.Y = -1;
            Vector2.Transform(ref tempVec1, ref tempMatrix, out outVector);
        }

        public static void ClampVector(ref Vector2 inVec, float maxLen, out Vector2 outVec)
        {
            float factor = inVec.Length() / maxLen;
            if (factor > 1)
            {
                Vector2.Divide(ref inVec, factor, out outVec);
            }
            else
            {
                outVec = inVec;
            }
        }

        /// <summary>
        /// Get the absolute value of the inner angle (radians) between the two vectors
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float AngleBetween(Vector2 v1, Vector2 v2)
        {
            return Math.Min(
                Math.Abs(RadiansFromVector(v1) - RadiansFromVector(v2)),
                MathHelper.TwoPi - Math.Abs(RadiansFromVector(v1) - RadiansFromVector(v2)));
        }

        public static bool RectangleIntersectsCircle(Rectangle rect, Vector2 circCenter, float radius)
        {
            return RectangleIntersectsArc(rect, circCenter, radius, 0, MathHelper.TwoPi);
        }

        public static bool RectangleIntersectsArc(Rectangle rect, Vector2 arcCenterPoint,
            float arcRadius, float arcCenterAngle, float arcAngle)
        {
            //calcualte line in arc closest to pointing at rect center
            tempVec1.X = rect.Center.X - arcCenterPoint.X;
            tempVec1.Y = rect.Center.Y - arcCenterPoint.Y;
            float angle = MathHelper.Clamp(
                RadiansFromVector(tempVec1),
                arcCenterAngle - arcAngle / 2.0f,
                arcCenterAngle + arcAngle / 2.0f);

            float x1 = arcCenterPoint.X;
            float y1 = arcCenterPoint.Y;
            float x2 = x1 + (float)Math.Sin(angle) * arcRadius;
            float y2 = y1 - (float)Math.Cos(angle) * arcRadius;

            return ( SegmentsIntersect(x1, y1, x2, y2, rect.Left, rect.Top, rect.Left, rect.Bottom) //left
                || SegmentsIntersect(x1, y1, x2, y2, rect.Right, rect.Top, rect.Right, rect.Bottom)   //right
                || SegmentsIntersect(x1, y1, x2, y2, rect.Left, rect.Bottom, rect.Right, rect.Bottom)   //bottom
                || SegmentsIntersect(x1, y1, x2, y2, rect.Left, rect.Top, rect.Right, rect.Top)     //top
                || PointInRect(arcCenterPoint, rect));  
        }

        public static bool SegmentsIntersect(float x1, float y1, float x2, float y2, 
            float x3, float y3, float x4, float y4)
        {
            float u1 =  ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3))
                / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
            float u2 =  ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3))
                / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
            return (0.0f <= u1 && u1 <= 1.0f && 0.0f <= u2 && u2 <= 1.0f);
        }

        public static bool SegmentsIntersect(Segment s1, Segment s2)
        {
            return SegmentsIntersect(s1.X1, s1.Y1, s1.X2, s1.Y2, s2.X1, s2.Y1, s1.X2, s2.Y2);
        }

        /// <summary>
        /// return shortest vector between a point and a line segment
        /// </summary>
        /// <param name="v">point on one end of segment</param>
        /// <param name="w">point on other end of segment</param>
        /// <param name="p">point from which to calculate vector</param>
        /// <returns></returns>
        private static Vector2 shortestVectorToSegment(Vector2 v, Vector2 w, Vector2 p)
        {
            //compute length of segment squared ( |w-v|^2 )
            float l2 = (float)Math.Pow((w-v).Length(), 2);
            if (l2 == 0.0f)     //case where segment is just a point
                return p - v;
            //compute projection of p onto the line
            //t = [(p-v) . (w - v)] / |w-v|^2
            float t = Vector2.Dot(p - v, w - v) / l2;
            if (t < 0.0f)
                return p - v;    //beyond v end of segment
            else if (t > 1.0f)
                return p - w;    //beyond w end of segment
            tempVec1 = v + t * (w - v);     //projection onto segment
            return p - tempVec1;
        }

        public static float DistanceToSegment(Vector2 point, Segment segment)
        {
            return shortestVectorToSegment(segment.Start, segment.End, point).Length();
        }

        public static float DistanceBetweenSegments(Segment s1, Segment s2)
        {
            if (SegmentsIntersect(s1, s2))
            {
                return 0;
            }

            return Math.Min(DistanceToSegment(s1.Start, s2),
                   Math.Min(DistanceToSegment(s1.End, s2),
                   Math.Min(DistanceToSegment(s2.Start, s1), 
                            DistanceToSegment(s2.End, s1))));
        }

        public static float DistanceBetweenRects(Rectangle r1, Rectangle r2)
        {
            if (r1.Intersects(r2))
                return 0;

            float min = float.MaxValue;
            foreach (Segment s1 in Segment.RectangleSides(r1))
            {
                foreach (Segment s2 in Segment.RectangleSides(r2))
                {
                    min = Math.Min(min, DistanceBetweenSegments(s1, s2));
                }
            }

            return min;
        }

        private static void solveQuadratic(float a, float b, float c, out float result1, out float result2)
        {
            //sqrt = (b^2 - 4ac) ^ (1/2)
            float sqrt = (float)Math.Sqrt(b * b - 4 * a * c);
            //result1 = (-b + sqrt) / (2a)
            result1 = (-b + sqrt) / (2 * a);
            //result2 = (-b - sqrt) / (2a)
            result2 = (-b - sqrt) / (2 * a);
        }

        public static bool RectsCollide(Rectangle rect1, Rectangle rect2)
        {
            return (rect1.Right > rect2.Left && rect1.Left < rect2.Right &&
                    rect1.Bottom > rect2.Top && rect1.Top < rect2.Bottom);
        }

        public static bool PointInRect(Vector2 point, Rectangle rect)
        {
            return (rect.Left <= point.X && point.X <= rect.Right && rect.Top <= point.Y && point.Y <= rect.Bottom);
        }


        public static bool SegmentIntersectsRect(Point p1, Point p2, Rectangle r)
        {
            return SegmentIntersectsSegment(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
                   SegmentIntersectsSegment(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
                   SegmentIntersectsSegment(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
                   SegmentIntersectsSegment(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }

        private static bool SegmentIntersectsSegment(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generate a random angle(degrees) from within a given arc
        /// </summary>
        /// <param name="centerAngle">Angle of the center of the arc (degrees, clockwise from vertical)</param>
        /// <param name="arc">Width of arc (degrees, edge to edge)</param>
        /// <returns></returns>
        public static float RandomAngle(float centerAngle, float arc)
        {
            return centerAngle + arc * (0.5f - (float)rand.NextDouble());
        }

        public static void RandomizeVector(ref Vector2 refVector, float minX, float maxX, float minY, float maxY)
        {
            refVector.X = minX + (float)rand.NextDouble() * (maxX - minX);
            refVector.Y = minY + (float)rand.NextDouble() * (maxY - minY);
        }

        /// <summary>
        /// Get a random number in the range [min,max]
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInt(int min, int max)
        {
            return rand.Next(min, max + 1);
        }

        public static void DrawRect(Color color, Rectangle rect, SpriteBatch sb)
        {
            sb.Draw(PixelTexture, rect, color);
        }
    }
}
