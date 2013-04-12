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
        float DRAW_FREQUENCY = 0.001f;

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
        /// <summary>
        /// Progress bar that displays information radially
        /// </summary>
        /// <param name="arcCenter">Origin of the circle</param>
        /// <param name="radius">distance from origin to outside of bar</param>
        /// <param name="thickness">number of pixels in bar thickness</param>
        /// <param name="arcStart">angle (radians) which forms bar base (0 is vertical upward)</param>
        /// <param name="arcEnd">angle (radians) which forms bar endpoint when full (0 is vertical upward)</param>
        /// <param name="barColor">color with which to draw bar</param>
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
        /// <summary>
        /// Draw the radial bar to the screen
        /// </summary>
        /// <param name="sb">Sprite batch to use for drawing</param>
        /// <param name="currentVal">Current value of the value being displayed</param>
        /// <param name="maxVal">Current value of the value being displayed</param>
        public void Draw(SpriteBatch sb, float currentVal, float maxVal)
        {
            if (_arcStart < _arcEnd)
            {
                //angle of rotation to currently draw bar pip at
                float stopAngle = _arcStart + currentVal / maxVal * (_arcEnd - _arcStart);
                //increment through angle, drawing pip along way
                for (float angle = _arcStart; angle < stopAngle; angle += DRAW_FREQUENCY)
                {
                    sb.Draw(_barPipTexture, _drawPoint, null, _barColor, angle, _origin, 1.0f, SpriteEffects.None, 0);
                }
            }
            else if (_arcStart > _arcEnd)
            {
                //angle of rotation to currently draw bar pip at
                float stopAngle = _arcStart + currentVal / maxVal * (_arcEnd - _arcStart);
                //increment through angle, drawing pip along way
                for (float angle = _arcStart; angle > stopAngle; angle -= DRAW_FREQUENCY)
                {
                    sb.Draw(_barPipTexture, _drawPoint, null, _barColor, angle, _origin, 1.0f, SpriteEffects.None, 0);
                }
            }
        }
        #endregion
    }
}
