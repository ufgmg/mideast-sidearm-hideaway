using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.utility;

namespace SpaceGame.units
{
    /// <summary>
    /// A unit with physics properties
    /// superclass for player and most enemies (not including unicorns)
    /// </summary>
    class PhysicalUnit
    {
        #region static members
        //store data for all physical units
        public static Dictionary<string, PhysicalData> Data;
        //store screen dimensions for keeping sprites in bounds
        public static int ScreenWidth, ScreenHeight;

        //factor of force applied based on distance out of bounds
        const float OUT_OF_BOUNDS_ACCEL_FACTOR = 30;
        const float BOUND_BUFFER = 20;
        //factor of force applied in unit collisions
        const float COLLISION_FORCE_FACTOR = 10.0f;
        #endregion
        #region physical properties
        //Physical properties--------------------
        public Vector2 Position;
        Vector2 _velocity;
        Vector2 _acceleration;
        float _angularVelocity = 0;
        float _mass;
        float _additionalMass;
        float _health;
        float _maxSpeed;
        //force applied for movement
        float _moveForce;
        //fractional speed reduction each frame
        float _decelerationFactor;
        //status effects and recovery rates (per second)
        float _stunEffect, _stunRecovery;
        #endregion

        #region properties
        public float Mass { get { return _mass + _additionalMass; } }
        Rectangle _hitRect;
        public Vector2 Center {get {return new Vector2(_hitRect.Center.X, _hitRect.Center.Y);}}
        public float Bottom { get { return Position.Y + _hitRect.Height; } }
        public float Top { get { return Position.Y; } }
        public float Left { get { return Position.X; } }
        public Rectangle HitRect { get { return _hitRect; } }
        public float Right { get { return Position.X + _hitRect.Width; } }
        public LifeState UnitLifeState { get { return _lifeState; } }

        //determine behavior for next update
        public Vector2 MoveDirection { get; set; }
        public Vector2 LookDirection { get; set; }
        #endregion

        #region other members
        Sprite _sprite;
        ParticleEffect _movementParticleEffect;
        #endregion

        #region states
        enum SpriteState
        {
            FaceLeft,
            FaceDown,
            FaceRight,
            FaceUp
        }

        public enum LifeState
        {
            Living,
            Stunned,
            Disabled,       //health <= 0 , float aimlessly, no attempt to move
            BeingEaten,     //being consumed by black hole
            Destroyed,      //no longer Update or Draw
        }

        protected LifeState _lifeState;
        #endregion

        #region constructor
        /// <summary>
        /// Create a new physical sprite
        /// </summary>
        /// <param name="unitName">key to find SpriteData and PhysicalData</param>
        public PhysicalUnit(string unitName)
        {
            _sprite = new Sprite(unitName);
            PhysicalData pd = Data[unitName];

            if (pd.MovementParticleEffectName != null)
                _movementParticleEffect = new ParticleEffect(pd.MovementParticleEffectName);

            _mass = pd.Mass;
            _moveForce = pd.MoveForce;
            _maxSpeed = pd.MaxSpeed;
            _health = pd.Health;
            _decelerationFactor = pd.DecelerationFactor;

            _lifeState = LifeState.Living;
            _hitRect = new Rectangle(0, 0, (int)_sprite.Width, (int)_sprite.Height);

            MoveDirection = Vector2.Zero;
            LookDirection = Vector2.Zero;
        }

        public PhysicalUnit(string unitName, Vector2 startPosition)
            :this(unitName)
        {
            Position = startPosition;
        }

        #endregion

        #region methods
        public void ApplyForce(Vector2 theForce)
        {
            _acceleration += theForce / Mass;
        }

        public void ApplyDamage(int Damage)
        {
            if (_lifeState == LifeState.Destroyed || _lifeState == LifeState.BeingEaten)
                return;

            _health -= Damage;
            if (_health <= 0)
            {
                _lifeState = LifeState.Disabled;
                _sprite.Shade = Color.Gray;
                _angularVelocity = (float)MathHelper.PiOver4 * _health;
            }
            else
                _sprite.Flash(Color.Orange, TimeSpan.FromSeconds(0.1), 3);
        }

        public void EatByBlackHole()
        {
            if (_lifeState != LifeState.BeingEaten && _lifeState != LifeState.Destroyed)
            {
                _lifeState = LifeState.BeingEaten;
                _angularVelocity = 4 * MathHelper.TwoPi;
            }
        }

        #region Update Logic
        public virtual void Update(GameTime gameTime)
        {

            switch(_lifeState)
            {
                case (LifeState.Living):
                    {
                        lookThisWay(LookDirection);
                        if (MoveDirection.Length() > 0)
                            moveThisWay(MoveDirection, gameTime);

                        break;
                    }

                case LifeState.Disabled:
                    {
                        break;
                    }
                case LifeState.BeingEaten:
                    {
                        _sprite.ScaleFactor -= 0.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (_sprite.ScaleFactor <= 0)
                            _lifeState = LifeState.Destroyed;
                        break;
                    }
                case LifeState.Destroyed:
                    {
                        return;
                    }
            }

            stayInBounds(ScreenWidth, ScreenHeight);
            _velocity += _acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            controlVelocity(_maxSpeed, gameTime.ElapsedGameTime);
            _sprite.Angle += _angularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            LookDirection = Vector2.Zero;
            MoveDirection = Vector2.Zero;
            _acceleration = Vector2.Zero;

            if (_movementParticleEffect != null)
                _movementParticleEffect.Update(gameTime);
            _hitRect.X = (int)Position.X - _hitRect.Width / 2;
            _hitRect.Y = (int)Position.Y - _hitRect.Height / 2;

            _sprite.Update(gameTime);
        }

        //more dynamic stayInBounds
        private void stayInBounds(float screenWidth, float screenHeight)
        {
            if (Position.X < BOUND_BUFFER)
            {
                _acceleration.X += OUT_OF_BOUNDS_ACCEL_FACTOR * (BOUND_BUFFER - Position.X);
            }
            else if (Position.X + _sprite.Width >= screenWidth - BOUND_BUFFER)
            {
                _acceleration.X += OUT_OF_BOUNDS_ACCEL_FACTOR * (screenWidth - BOUND_BUFFER - Position.X - _hitRect.Width);
            }

            if (Position.Y < 0)
            {
                _acceleration.Y += OUT_OF_BOUNDS_ACCEL_FACTOR * (BOUND_BUFFER - Position.Y);
            }

            else if (Position.Y + _sprite.Height >= screenHeight)
            {
                _acceleration.Y += OUT_OF_BOUNDS_ACCEL_FACTOR * (screenHeight - BOUND_BUFFER - Position.Y - _hitRect.Height);
            }
        }

        /*
        //original static version of stayInBounds
        private void stayInBounds(float screenWidth, float screenHeight)
        {
            if (Position.X < 0)
            {
                Position.X = 0;
                _acceleration.X = 0;
            }
            else if (Position.X + _sprite.Width >= screenWidth)
            {
                Position.X = screenWidth - _sprite.Width;
                _acceleration.X = 0;
            }

            if (Position.Y < 0)
            {
                Position.Y = 0;
                _acceleration.Y = 0;
            }
            else if (Position.Y + _sprite.Height >= screenHeight)
            {
                Position.Y = screenHeight - _sprite.Height;
                _acceleration.Y = 0;
            }
        }
        */

        /// <summary>
        /// Move the sprite in the given direction based on its moveForce property
        /// </summary>
        /// <param name="direction">Direction to move. Should be normalized for normal movement.</param>
        private void moveThisWay(Vector2 direction, GameTime gameTime)
        {
            ApplyForce(_moveForce * direction);
            if (_movementParticleEffect != null)
                _movementParticleEffect.Spawn(Center, XnaHelper.DegreesFromVector(-direction), gameTime.ElapsedGameTime, _velocity); 
        }

        public void ApplyGravity(Gravity gravity, GameTime theGameTime)
        {
            Vector2 direction = gravity.Position - Position;
            //float distance = direction.Length();
            direction.Normalize();
            //_acceleration += gravity.Magnitude * direction * (float)theGameTime.ElapsedGameTime.TotalSeconds / (distance * 0.01f);
            _acceleration += gravity.Magnitude * direction * (float)theGameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Reset()
        {
            _velocity = Vector2.Zero;
            _acceleration = Vector2.Zero;
            _sprite.Reset();
        }

        private void controlVelocity(float maximumSpeed, TimeSpan time)
        {
            float speed = _velocity.Length();
            if (speed > 0)  //scale down velocity
            {
                _velocity -= _velocity * _decelerationFactor * (float)time.TotalSeconds;
                if (speed > maximumSpeed)   //keep below maximum
                    _velocity = _velocity * maximumSpeed / speed;
            }
        }

        protected void lookThisWay(Vector2 direction)
        {
            SpriteState spriteDirection;

            float angle = XnaHelper.RadiansFromVector(direction);

            if (angle > -Math.PI / 4 && angle < Math.PI / 4)
                spriteDirection = SpriteState.FaceUp;
            else if (angle >= Math.PI / 4 && angle < 3 * Math.PI / 4)
                spriteDirection = SpriteState.FaceRight;
            else if (angle > 3 * Math.PI / 4 || angle < -3 * Math.PI / 4)
                spriteDirection =  SpriteState.FaceDown;
            else
                spriteDirection =  SpriteState.FaceLeft;

            _sprite.AnimationState = (int)spriteDirection;
        }

        public void CheckAndApplyUnitCollision(PhysicalUnit other)
        {
            Vector2 temp = other._velocity;
            if (XnaHelper.RectsCollide(HitRect, other.HitRect))
            {
                other._velocity = this._velocity * other.Mass / this.Mass;
                this._velocity = temp * other.Mass / this.Mass;
            }
        }
        #endregion

        #region Draw Logic
        public void Draw(SpriteBatch sb)
        {
            if (_lifeState == LifeState.Destroyed)
                return;     //dont draw destroyed sprites

            if (_movementParticleEffect != null)
                _movementParticleEffect.Draw(sb);

            _sprite.Draw(sb, Position);
        }
        #endregion
        #endregion
    }
}
