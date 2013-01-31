using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.utility;
using SpaceGame.graphics;

namespace SpaceGame.units
{
    /// <summary>
    /// Black hole that applies gravity on all active units
    /// Units too close will be sucked in
    /// </summary>
    class BlackHole
    {
        #region fields
        public Vector2 Position;
        public Gravity Gravity { get; private set; }

        float _radius;
        float _capacityUsed;
        float _totalCapacity;
        
        ParticleEffect[] _particleEffects;
        #endregion

        #region constructors
        /// <summary>
        /// Construct a new black hole
        /// </summary>
        /// <param name="position">location of black hole center</param>
        /// <param name="gravMagnitude">strength of gravitational force</param>
        /// <param name="radius">max distance that black hole will consume units</param>
        public BlackHole(Vector2 position, float gravMagnitude, float radius, float capacity)
        {
            Position = position;
            Gravity = new Gravity(position, gravMagnitude);
            _radius = radius;
            _particleEffects = new ParticleEffect[] {
                new ParticleEffect("BlackHoleEffect1"),
                new ParticleEffect("BlackHoleEffect2")};
            _totalCapacity = capacity;
            _capacityUsed = 0.0f;
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime)
        {
            foreach (ParticleEffect p in _particleEffects)
            {
                p.Update(gameTime);
                p.Spawn(Position, 0.0f, gameTime.ElapsedGameTime, Vector2.Zero);
            }
        }

        /// <summary>
        /// Apply Gravity on a unit and eat it if close enough
        /// call on each unit during unit update loop
        /// </summary>
        /// <param name="unit">unit to affect. Should be called after updating unit</param>
        public void PullUnit(PhysicalUnit unit, GameTime gameTime)
        {
            unit.ApplyGravity(Gravity, gameTime);
            if ((Position - unit.Center).Length() <= _radius)
            {   //try to eat unit
                if (unit.EatByBlackHole())
                {
                    _capacityUsed += unit.Mass;
                    foreach (ParticleEffect p in _particleEffects)
                    {
                        p.SpeedFactor = 1 + _capacityUsed / _totalCapacity;
                    }
                    Gravity.MagnitudeFactor = (1 + _capacityUsed / _totalCapacity);
                }
            }

        }

        public void Draw(SpriteBatch sb)
        {
            foreach (ParticleEffect p in _particleEffects)
                p.Draw(sb, Position);
        }
        #endregion
    }
}
