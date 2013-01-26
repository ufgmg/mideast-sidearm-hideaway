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

        public static void DrawRect(Color color, Rectangle rect, SpriteBatch sb)
        {
            sb.Draw(PixelTexture, rect, color);
        }

    }
}
