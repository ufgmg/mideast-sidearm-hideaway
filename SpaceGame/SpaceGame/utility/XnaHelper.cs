using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace SpaceGame.utility
{
    static class XnaHelper
    {
        public static Texture2D PixelTexture;

        static Random rand = new Random();
        static Vector2 tempVec1, tempVec2;
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
        /// <returns></returns>
        public static Vector2 VectorFromAngle(float angle)
        {
            Matrix rotMatrix = Matrix.CreateRotationZ(angle);
            return Vector2.Transform(-Vector2.UnitY, rotMatrix);
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

        public static int RandomInt(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static void DrawRect(Color color, Rectangle rect, SpriteBatch sb)
        {
            sb.Draw(PixelTexture, rect, color);
        }
    }
}
