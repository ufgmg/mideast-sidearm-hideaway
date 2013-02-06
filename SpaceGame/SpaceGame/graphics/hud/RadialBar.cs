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

        public static GraphicsDevice GameGraphicsDevice;

        public enum DecrementSide
        {
            Right,
            Left,
            Both
        }
        #endregion

        #region fields
        //single small white rect rotated and colored to form bar
        Texture2D _barPipTexture;
        //centerpoint of arc on texture (center of circle formed by arc)
        //BarPipTexture is rotated around this point to draw
        Vector2 _origin;
        //angle at which to start drawing of arc (radians, 0 is straight up)
        float _arcStart;
        //angle to which to draw when arc is full (radians)
        float _arcEnd;
        //color to draw bar with
        Color _barColor;
        //where top of pip texture should be drawn on screen (for angle 0)
        Vector2 _drawPoint;
        #endregion

        #region constructor
        public RadialBar(Vector2 arcCenter, float radius, int thickness, float arcStart, float arcEnd, Color barColor)
        {
            _arcStart = arcStart;
            _arcEnd = arcEnd;
            _barColor = barColor;

            _drawPoint = new Vector2(arcCenter.X, arcCenter.Y - radius);
            _origin = new Vector2(0, radius);
            _barPipTexture = new Texture2D(GameGraphicsDevice, 1, thickness);
            Color[] colorData = new Color[1 * thickness];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = Color.White;
            }
            _barPipTexture.SetData<Color>(colorData);
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
                sb.Draw(_barPipTexture, _drawPoint, null, _barColor, angle, _origin, 1.0f, SpriteEffects.None, 0);
            }
        }
        #endregion
    }
}
