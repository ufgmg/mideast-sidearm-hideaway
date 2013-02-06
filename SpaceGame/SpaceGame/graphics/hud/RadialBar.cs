using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.graphics.hud
{
    //radial progress bar used for player health and black hole fullness
    class RadialBar
    {
        #region static
        //how many radians between successive pips
        //lower value -> higher granularity -> smoother appearance
        float DRAW_FREQUENCY = 0.01f;

        //single small rect with transparent background
        //rotated to form radial arc
        public static Texture2D BarPipTexture;
        //centerpoint of arc on texture (center of circle formed by arc)
        //BarPipTexture is rotated around this point to draw
        public static Vector2 PipTextureOrigin = new Vector2(96,105);   //TODO--don't hardcode
        public enum DecrementSide
        {
            Right,
            Left,
            Both
        }
        #endregion

        #region fields
        //angle at which to start drawing of arc (radians, 0 is straight up)
        float _arcStart;
        //angle to which to draw when arc is full (radians)
        float _arcEnd;
        //color to draw bar with
        Color _barColor;
        //where center of drawn arc should be on screen
        Vector2 _drawPoint;
        #endregion

        #region constructor
        public RadialBar(Vector2 arcCenter, float arcStart, float arcEnd, Color barColor)
        {
            _arcStart = arcStart;
            _arcEnd = arcEnd;
            _barColor = barColor;
            //get the top left point of the image
            _drawPoint = arcCenter - PipTextureOrigin;
        }
        #endregion

        #region methods
        public void Draw(SpriteBatch sb, float currentVal, float maxVal)
        {
            //angle of rotation to currently draw bar pip at
            float stopAngle = _arcStart + currentVal / maxVal * (_arcEnd - _arcStart);
            //increment through angle, drawing pip along way
            for (float angle = _arcStart; angle < stopAngle; angle += DRAW_FREQUENCY)
            {
                sb.Draw(BarPipTexture, _drawPoint, null, _barColor, angle, PipTextureOrigin, 1.0f, SpriteEffects.None, 0);
            }
            //DEBUG
            SpaceGame.utility.XnaHelper.DrawRect(Color.Green,
                new Rectangle((int)(_drawPoint.X + PipTextureOrigin.X - 2), (int)(_drawPoint.Y + PipTextureOrigin.Y - 2), 2, 2), sb);
        }
        #endregion
    }
}
