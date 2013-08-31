using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceGame.utility
{
    /// <summary>
    /// Represents a gravitational pull
    /// </summary>
    class Gravity
    {
        Vector2 _position;
        float _magnitude;
        readonly float _baseMagnitude;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float Magnitude
        {
            get { return _magnitude; }
        }

        public float MagnitudeFactor
        {
            get { return _magnitude / _baseMagnitude; }
            set { _magnitude = _baseMagnitude * value; }
        }

        public Gravity(Vector2 position, float magnitude)
        {
            _position = position;
            _baseMagnitude = magnitude;
            _magnitude = magnitude;
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            _position = newPosition;
        }

    }
}