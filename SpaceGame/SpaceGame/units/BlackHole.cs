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
        float _capacityLeft;
        
        ParticleEffect _particleEffect;
        #endregion

        #region constructors
        /// <summary>
        /// Construct a new black hole
        /// </summary>
        /// <param name="position">location of black hole center</param>
        /// <param name="gravMagnitude">strength of gravitational force</param>
        /// <param name="radius">max distance that black hole will consume units</param>
        public BlackHole(Vector2 position, float gravMagnitude, float radius)
        {
            Position = position;
            Gravity = new Gravity(position, gravMagnitude);
            _radius = radius;
            _particleEffect = new ParticleEffect("BlackHoleEffect");
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime)
        {
            _particleEffect.Update(gameTime);
            _particleEffect.Spawn(Position, 0.0f, gameTime.ElapsedGameTime, Vector2.Zero);
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
            {
                _capacityLeft -= unit.Mass;
                unit.EatByBlackHole();
            }

        }

        public void Draw(SpriteBatch sb)
        {
            _particleEffect.Draw(sb);
        }
        #endregion
    }
}
