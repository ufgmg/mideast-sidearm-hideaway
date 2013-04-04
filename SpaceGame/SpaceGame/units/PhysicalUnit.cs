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
        //factor of force applied based on distance out of bounds
        const float OUT_OF_BOUNDS_ACCEL_FACTOR = 30;
        const float BOUND_BUFFER = 20;
        //factor of force applied in unit collisions
        const float COLLISION_FORCE_FACTOR = 10.0f;

        //reusable Vector2 for calculations
        static Vector2 temp;
        #endregion
        #region fields
        string _unitName;
        protected Vector2 _position;
        //Physical properties--------------------
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _hitRect.X = (int)value.X;
                _hitRect.Y = (int)value.Y;
            }
        }
        Vector2 _velocity;
        public Vector2 Velocity { get { return _velocity; } }
        Vector2 _acceleration;
        float _angularVelocity = 0;
        float _mass;
        float _additionalMass;
        float _health, _maxHealth;
        float _maxSpeed;
        //force applied for movement
        float _moveForce;
        //fractional speed reduction each frame
        float _decelerationFactor;
        #endregion

        #region properties
        public float health { get { return _health; } }
        public float maxHealth { get { return _maxHealth; } }

        public float Mass { get { return _mass + _additionalMass; } }
        Rectangle _hitRect;

        public Rectangle HitRect { get { return _hitRect; } }

        public Vector2 Center 
        {
            get {return new Vector2(_hitRect.Center.X, _hitRect.Center.Y);}
            set
            {
                _position.X = value.X - HitRect.Width / 2.0f;
                _position.Y = value.Y - HitRect.Height / 2.0f;
            }
        }

        public int Bottom 
        {
            get { return HitRect.Bottom; }
            set
            {
                _position.Y = value - HitRect.Height;
                _hitRect.Y = (int)_position.Y;
            }
        }

        public int Top 
        {
            get { return HitRect.Top; }
            set
            {
                _position.Y = value;
                _hitRect.Y = (int)_position.Y;
            }
        }

        public int Left 
        {
            get { return HitRect.Left; }
            set
            {
                _position.X = value;
                _hitRect.X = (int)_position.X;
            }
        }

        public int Right 
        {
            get { return HitRect.Right; }
            set
            {
                _position.X = value - HitRect.Width;
                _hitRect.X = (int)_position.X;
            }
        }

        public LifeState UnitLifeState { get { return _lifeState; } }

        //determine behavior for next update
        public Vector2 MoveDirection { get; set; }
        public Vector2 LookDirection { get; set; }

        //behavioral properties
        public bool Collides
        {
            get { return _lifeState == LifeState.Living || _lifeState == LifeState.Disabled; }
        }
        public bool Updates
        {
            get { return !(_lifeState == LifeState.Dormant || _lifeState == LifeState.Destroyed); }
        }
        #endregion

        #region other members
        protected Sprite _sprite;
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
            Dormant,        //never been spawned
            Living,
            Stunned,
            Disabled,       //health <= 0 , float aimlessly, no attempt to move
            BeingEaten,     //being consumed by black hole
            Destroyed,      //no longer Update or Draw
            Ghost           //keep updating and drawing, but don't collide or apply gravity
        }

        protected LifeState _lifeState;
        #endregion

        #region constructor
        /// <summary>
        /// Create a new physical sprite from data
        /// </summary>
        /// <param name="pd">data from which to construct unit</param>
        protected PhysicalUnit(PhysicalData pd)
        {
            _unitName = pd.Name;
            _sprite = new Sprite(_unitName);

            if (pd.MovementParticleEffectName != null)
                _movementParticleEffect = new ParticleEffect(pd.MovementParticleEffectName);

            _mass = pd.Mass;
            _moveForce = pd.MoveForce;
            _maxSpeed = pd.MaxSpeed;
            _maxHealth = pd.Health;
            _health = _maxHealth;
            _decelerationFactor = pd.DecelerationFactor;

            _lifeState = LifeState.Dormant;     //not yet spawned
            _hitRect = new Rectangle(0, 0, (int)_sprite.Width, (int)_sprite.Height);

            Position = Vector2.Zero;
            MoveDirection = Vector2.Zero;
            LookDirection = Vector2.Zero;
        }

        #endregion

        #region methods
        public void ApplyForce(Vector2 theForce)
        {
            _acceleration += theForce / Mass;
        }

        /// <summary>
        /// apply the effect of an object impacting this unit.
        /// Useful for single time impacts, like a melee weapon hit
        /// </summary>
        /// <param name="objectVelocity">Velocity of object hitting the unit</param>
        /// <param name="objectMass">mass of object hitting the unit</param>
        public void ApplyImpact(Vector2 objectVelocity, int objectMass)
        {
            _velocity = (_velocity * (this.Mass - objectMass) + 2 * objectMass * objectVelocity) /
                                (this.Mass + objectMass);
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

        /// <summary>
        /// Attempt to absorb unit into black hole. 
        /// </summary>
        /// <returns>Whether unit was successfully eaten</returns>
        public virtual bool EatByBlackHole()
        {
            if (_lifeState != LifeState.BeingEaten && _lifeState != LifeState.Destroyed 
                && _lifeState != LifeState.Ghost)
            {
                _lifeState = LifeState.BeingEaten;
                _angularVelocity = 4 * MathHelper.TwoPi;
                return true;
            }
            return false;
        }

        #region Update Logic
        public virtual void Update(GameTime gameTime, Rectangle levelBounds)
        {

            switch(_lifeState)
            {
                case LifeState.Living:
                case LifeState.Ghost:
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
                        _sprite.ScaleFactor -= 1.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (_sprite.ScaleFactor <= 0)
                            _lifeState = LifeState.Destroyed;
                        break;
                    }
                default:
                    {
                        return;     //don't update anything
                    }
            }

            stayInBounds(levelBounds.Width, levelBounds.Height);
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
        private void stayInBounds(float levelWidth, float levelHeight)
        {
            if (_lifeState == LifeState.Ghost)
                return;

            if (Position.X < BOUND_BUFFER)
            {
                _acceleration.X += OUT_OF_BOUNDS_ACCEL_FACTOR * (BOUND_BUFFER - Position.X);
            }
            else if (Position.X + _sprite.Width >= levelWidth - BOUND_BUFFER)
            {
                _acceleration.X += OUT_OF_BOUNDS_ACCEL_FACTOR * (levelWidth - BOUND_BUFFER - Position.X - _hitRect.Width);
            }

            if (Position.Y < 0)
            {
                _acceleration.Y += OUT_OF_BOUNDS_ACCEL_FACTOR * (BOUND_BUFFER - Position.Y);
            }

            else if (Position.Y + _sprite.Height >= levelHeight)
            {
                _acceleration.Y += OUT_OF_BOUNDS_ACCEL_FACTOR * (levelHeight - BOUND_BUFFER - Position.Y - _hitRect.Height);
            }
        }

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
            _acceleration += direction * gravity.Magnitude * (float)theGameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Respawn(Vector2 newPosition)
        {
            Position = newPosition;
            _lifeState = LifeState.Living;
            Reset();
        }

        public void Reset()
        {
            _velocity = Vector2.Zero;
            _acceleration = Vector2.Zero;
            _health = _maxHealth;
            _additionalMass = 0;
            _angularVelocity = 0;
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
            if (!Collides)
                return;     //don't check collision if unit shouldn't collide

            if (XnaHelper.RectsCollide(HitRect, other.HitRect))
            {
                temp = other._velocity; //temp is a static reusable vector

                other._velocity = (other._velocity * (other.Mass - this.Mass) + 2 * this.Mass * this._velocity) /
                                    (this.Mass + other.Mass);
                this._velocity = (this._velocity * (this.Mass - other.Mass) + 2 * other.Mass * temp) /
                                    (this.Mass + other.Mass);
            }
        }
        #endregion

        #region Draw Logic
        public virtual void Draw(SpriteBatch sb)
        {
            if (_lifeState == LifeState.Destroyed || _lifeState == LifeState.Dormant)
                return;     //dont draw destroyed or not yet spawned sprites

            if (_movementParticleEffect != null)
                _movementParticleEffect.Draw(sb);

            _sprite.Draw(sb, Position);
        }
        #endregion
        #endregion
    }
}
