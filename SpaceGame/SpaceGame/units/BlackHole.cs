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
        const float SECONDS_FOR_OVERDRIVE = 3.0f;
        const float SECONDS_BEFORE_EXPLODE = 1.0f;
        const float SECONDS_DURING_EXPLODE = 5.0f;
        #endregion

        public enum BlackHoleState
        {
            Pulling,
            Overdrive,
            PreExplosion,
            Explosion,
            Exhausted
        }

        #region properties
        public Vector2 Position;
        public float Radius { get { return _radius; } }
        public Gravity Gravity { get; private set; }
        public BlackHoleState State { get { return _state; } }
        #endregion
        
        #region fields

        float _radius;
        float _capacityUsed;
        float _totalCapacity;
        
        ParticleEffect _particleEffect;

        BlackHoleState _state;    //state of black hole
        bool Exhausted;      //explosion complete

        TimeSpan _explosionTimer;
        TimeSpan _overdriveTimer;

        #endregion

        #region properties
        public float capacityUsed { get { return _capacityUsed; } }
        public float totalCapacity { get { return _totalCapacity; } }
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
            _overdriveTimer = TimeSpan.FromSeconds(SECONDS_FOR_OVERDRIVE);
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime)
        {
            _particleEffect.Update(gameTime);
            //only spawn particles when pulling or pushing
            if (_state == BlackHoleState.Pulling || _state == BlackHoleState.Explosion || _state == BlackHoleState.Overdrive)
            {
                _particleEffect.Spawn(Position, 0.0f, gameTime.ElapsedGameTime, Vector2.Zero,
                    1.0f + _capacityUsed / _totalCapacity);   //spawn rate increases as capacity fills 
            }

            if (_state == BlackHoleState.Overdrive)
            {
                _overdriveTimer -= gameTime.ElapsedGameTime;
                if (_overdriveTimer < TimeSpan.Zero)
                {
                    _state = BlackHoleState.PreExplosion;
                }
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
            float massEaten;
            if (_state != BlackHoleState.Pulling && _state != BlackHoleState.Overdrive)
                return;
            if (_state == BlackHoleState.Pulling)
            {
                unit.ApplyGravity(Gravity, gameTime);
            }
            else if (_state == BlackHoleState.Overdrive)
            {
                unit.FlyToPoint(Position, _overdriveTimer, 2.0f);
            }

            if ((massEaten = unit.EatByBlackHole(Position, _radius)) > 0)
            {
                _capacityUsed += massEaten;
                _particleEffect.IntensityFactor = 1.0f + _capacityUsed / _totalCapacity;
                Gravity.MagnitudeFactor = (1.0f + _capacityUsed / _totalCapacity);
                if (_capacityUsed > _totalCapacity)
                {
                    goOverdrive();
                }
            }
        }

        /// <summary>
        /// Try to eat a passing unicorn
        /// </summary>
        /// <param name="unicorn">unicorn</param>
        public void TryEatUnicorn(Unicorn uni, GameTime gameTime)
        {
            if (_state != BlackHoleState.Pulling && _state != BlackHoleState.Overdrive)
                return;

            if (uni.EatByBlackHole(Position, gameTime)) 
            {   //try to eat unit
                _capacityUsed += Unicorn.UNICORN_MASS;
                _particleEffect.IntensityFactor = 1.0f + _capacityUsed / _totalCapacity;
                Gravity.MagnitudeFactor = (1.0f + _capacityUsed / _totalCapacity);
                if (_capacityUsed > _totalCapacity)
                {
                    goOverdrive();
                }
            }
        }

        private void goOverdrive()
        {
            _state = BlackHoleState.Overdrive;
            _explosionTimer = TimeSpan.FromSeconds(SECONDS_BEFORE_EXPLODE);
            _particleEffect.IntensityFactor = 3.0f;
            Gravity = new Gravity(Position, Gravity.Magnitude * 1.4f);
        }

        /// <summary>
        /// automatically explode black hole
        /// </summary>
        public void Explode()
        {
            goOverdrive();
        }

        public void Draw(SpriteBatch sb)
        {
            _particleEffect.Draw(sb);
        }
        #endregion
    }
}
