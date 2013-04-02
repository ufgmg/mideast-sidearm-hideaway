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
        #region constants
        const float SECONDS_BEFORE_EXPLODE = 1.0f;
        const float SECONDS_DURING_EXPLODE = 5.0f;
        #endregion

        enum BlackHoleState
        {
            Pulling,
            PreExplosion,
            Explosion,
            Exhausted
        }

        #region fields
        public Vector2 Position;
        public Gravity Gravity { get; private set; }

        float _radius;
        float _capacityUsed;
        float _totalCapacity;
        
        ParticleEffect _particleEffect;

        BlackHoleState _state;    //state of black hole
        bool Exhausted;      //explosion complete

        TimeSpan _explosionTimer;

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
            _particleEffect = new ParticleEffect("BlackHoleEffect");
            _totalCapacity = capacity;
            _capacityUsed = 0.0f;
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime)
        {
            _particleEffect.Update(gameTime);
            //only spawn particles when pulling or pushing
            if (_state == BlackHoleState.Pulling || _state == BlackHoleState.Explosion)
            {
                _particleEffect.Spawn(Position, 0.0f, gameTime.ElapsedGameTime, Vector2.Zero,
                    1.0f + _capacityUsed / _totalCapacity);   //spawn rate increases as capacity fills 
            }

            if (_state == BlackHoleState.PreExplosion || _state == BlackHoleState.Explosion)
            {
                _explosionTimer -= gameTime.ElapsedGameTime;
                if (_explosionTimer <= TimeSpan.Zero)
                {
                    if (_state == BlackHoleState.PreExplosion)
                    {
                        _state = BlackHoleState.Explosion;  //start exploding
                        Gravity.MagnitudeFactor = -3;   //go from suck to blow 
                        _particleEffect.Reversed = false;     //cause particle effects to push out
                        _explosionTimer = TimeSpan.FromSeconds(SECONDS_DURING_EXPLODE);
                    }
                    else
                        _state = BlackHoleState.Exhausted;      //stop affecting anything
                }
                        
            }
        }

        /// <summary>
        /// Apply Gravity on a unit and eat it if close enough
        /// call on each unit during unit update loop
        /// </summary>
        /// <param name="unit">unit to affect. Should be called after updating unit</param>
        public void ApplyToUnit(PhysicalUnit unit, GameTime gameTime)
        {
            if (_state != BlackHoleState.Pulling)
                return;

            unit.ApplyGravity(Gravity, gameTime);
            if ((Position - unit.Center).Length() <= _radius && _state == BlackHoleState.Pulling)
            {   //try to eat unit
                if (unit.EatByBlackHole())
                {
                    _capacityUsed += unit.Mass;
                    _particleEffect.IntensityFactor = 1.0f + _capacityUsed / _totalCapacity;
                    Gravity.MagnitudeFactor = (1.0f + _capacityUsed / _totalCapacity);
                    if (_capacityUsed > _totalCapacity)
                    {
                        _state = BlackHoleState.PreExplosion;
                        _explosionTimer = TimeSpan.FromSeconds(SECONDS_BEFORE_EXPLODE);
                    }
                }
            }

        }

        /// <summary>
        /// Try to eat a passing unicorn
        /// </summary>
        /// <param name="unicorn">unicorn</param>
        public void TryEatUnicorn(Unicorn uni, GameTime gameTime)
        {
            if (_state != BlackHoleState.Pulling)
                return;

            if (uni.EatByBlackHole(Position, gameTime) && _state == BlackHoleState.Pulling)
            {   //try to eat unit
                _capacityUsed += Unicorn.UNICORN_MASS;
                _particleEffect.IntensityFactor = 1.0f + _capacityUsed / _totalCapacity;
                Gravity.MagnitudeFactor = (1.0f + _capacityUsed / _totalCapacity);
                if (_capacityUsed > _totalCapacity)
                {
                    _state = BlackHoleState.PreExplosion;
                    _explosionTimer = TimeSpan.FromSeconds(SECONDS_BEFORE_EXPLODE);
                }
            }

        }

        public void Draw(SpriteBatch sb)
        {
            _particleEffect.Draw(sb);
        }
        #endregion
    }
}
