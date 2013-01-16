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
        readonly float _magnitude;

        public Vector2 Position
        {
            get { return _position; }
        }

        public float Magnitude
        {
            get { return _magnitude; }
        }

        public Gravity(Vector2 position, float magnitude)
        {
            _position = position;
            _magnitude = magnitude;
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            _position = newPosition;
        }

        //TODO -- add support for adding Gravities (for units like ogres)
    }
}