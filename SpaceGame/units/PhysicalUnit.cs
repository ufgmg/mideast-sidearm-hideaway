using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.utility;
using SpaceGame.equipment;

namespace SpaceGame.units
{
    /// <summary>
    /// A unit with physics properties
    /// superclass for player and most enemies (not including unicorns)
    /// </summary>
    class PhysicalUnit
    {
        #region classes/structs
        struct IceFragment
        {
            public Vector2 Position, Velocity, Acceleration;
            public float Angle;
            public float AngularVelocity;
            public float Health;
            public float ScaleFactor;
            public bool BeingEaten;
            public bool Active;
        }
        #endregion

        #region constant
        //factor of force applied based on distance out of bounds
        const float OUT_OF_BOUNDS_ACCEL_FACTOR = 30;
        const float BOUND_BUFFER = 20;
        //factor of force applied in unit collisions
        const float COLLISION_FORCE_FACTOR = 10.0f;

        //how fast units scale down when being eaten by black hole
        const float BLACK_HOLE_EAT_SCALE_FACTOR = 1.5f;

        //status effect constants
        const float MAX_STAT_EFFECT = 100;
        const float FIRE_DPS = 0.2f;   //damage per second per point of fire effect 
        const int FIRE_SPREAD_DISTANCE = 80;   //how far away a unit must be to transfer fire
        //portion of own fire effect transfered to nearby units per second
        const float FIRE_SPREAD_FACTOR = 0.40f;   
        //portion of transfered fire deducted from transferer
        const float FIRE_SPREAD_LOSS = 0.0002f;   
        //how much fire effect causes panic (random movement)
        const float FIRE_PANIC_THRESHOLD = 20.0f;
        //how often to change direction while panicking
        const float PANIC_DIRECTION_CHANGE_FREQUENCY = 0.5f;
        //factor of max health used to represent frozen integrity
        //damage is dealt to this while frozen - shatter if < 0
        const float ICE_INTEGRITY_FACTOR = 0.5f;
        //number of vertical and horizontal divisions when shattering
        const int ICE_DIVISIONS = 3;
        //amount of health ice fragments have relative to unit
        const float FRAGMENT_HEALTH = 20;
        //max velocity of an ice fragment (px/second)
        const float FRAGMENT_MAX_VELOCITY = 400.0f;
        //max angular velocity of an ice fragment (radians/second)
        const float FRAGMENT_MAX_ANGULAR_VELOCITY = 6.0f;
        //how much of unit velocity to transfer to fragments on shatter
        const float FRAGMENT_VELOCITY_FACTOR = 0.3f;
        //How much integrity ice fragments lose per second (how fast ice fragments melt)
        const float FRAGMENT_MELT_RATE = 3;
        //How much integrity ice fragments lose per second while black hole is eating
        const float FRAGMENT_EAT_RATE = 90;
        const float FRAGMENT_SCALE_FACTOR = 1.8f;

        #endregion

        #region static members

        //reusable Vector2 and rect for calculations
        static Vector2 temp;
        static Rectangle tempRec;
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
        StatEffect _statusEffects, _statusResist;
        ParticleEffect _burningParticleEffect;
        float _iceIntegrity;
        IceFragment[,] _fragments;
        TimeSpan _panicTimer;   //time till next direction switch
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
        Vector2 _moveDirection;
        Vector2 _lookDirection;
        public Vector2 MoveDirection
        {
            get { return _moveDirection; }
            set { _moveDirection = Panicked ? _moveDirection : value; }
        }
        public Vector2 LookDirection
        {
            get { return _lookDirection; }
            set { _lookDirection = Panicked ? _lookDirection : value; }
        }

        //behavioral properties
        public bool Collides
        {
            get { return Updates && (_lifeState != LifeState.Ghost); }
        }
        public bool Updates
        {
            get { return !(_lifeState == LifeState.Dormant || _lifeState == LifeState.Destroyed); }
        }
        public bool Panicked
        {
            get { return _statusEffects.Fire > FIRE_PANIC_THRESHOLD; }
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
            Frozen,         //hit max cryo effect, cannot move
            Shattered,      //shattered into fragments after being frozen
            Disabled,       //health <= 0 , float aimlessly, no attempt to move
            BeingEaten,     //being consumed by black hole
            Destroyed,      //no longer Update or Draw
            Ghost,          //keep updating and drawing, but don't collide or apply gravity
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
            _sprite = new Sprite(_unitName, Sprite.SpriteType.Unit);

            if (pd.MovementParticleEffectName != null)
                _movementParticleEffect = new ParticleEffect(pd.MovementParticleEffectName);
            _burningParticleEffect = new ParticleEffect("Burning");

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

            _statusEffects = new StatEffect(0, 0, 0);
            _statusResist = new StatEffect(pd.FireResist, pd.CryoResist, pd.ShockResist);

            _fragments = new IceFragment[ICE_DIVISIONS, ICE_DIVISIONS];
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
        public void ApplyImpact(Vector2 objectVelocity, float objectMass)
        {
            _velocity = (_velocity * (this.Mass - objectMass) + 2 * objectMass * objectVelocity) /
                                (this.Mass + objectMass);
        }

        public void ApplyStatus(StatEffect effects)
        {
            _statusEffects += effects;
        }

        public void ApplyDamage(float Damage)
        {
            if (Damage == 0.0f || _lifeState == LifeState.Destroyed || _lifeState == LifeState.BeingEaten
                || _lifeState == LifeState.Shattered)
                return;

            if (_lifeState == LifeState.Frozen)
            {   //attempt to shatter ice
                _iceIntegrity -= Damage;
                if (_iceIntegrity < 0)
                {
                    shatter();
                }
                return;
            }

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

        private void shatter()
        {
            _lifeState = LifeState.Shattered;
            for (int row = 0; row < ICE_DIVISIONS; row++)
                for (int col = 0 ; col < ICE_DIVISIONS ; col++)
                {
                    _fragments[row, col].Health = FRAGMENT_HEALTH * _statusEffects.Cryo / MAX_STAT_EFFECT;
                    _fragments[row, col].Position.X = Position.X + (0.5f + _sprite.Width * (float)col / ICE_DIVISIONS);
                    _fragments[row, col].Position.Y = Position.Y + (0.5f + _sprite.Height * (float)row / ICE_DIVISIONS);
                    XnaHelper.RandomizeVector(ref _fragments[row,col].Velocity, -FRAGMENT_MAX_VELOCITY, FRAGMENT_MAX_VELOCITY, 
                                                -FRAGMENT_MAX_VELOCITY, FRAGMENT_MAX_VELOCITY);
                    Vector2.Add(ref _fragments[row, col].Velocity, ref _velocity, out _fragments[row, col].Velocity);
                    Vector2.Multiply(ref _fragments[row, col].Velocity, FRAGMENT_VELOCITY_FACTOR, out _fragments[row, col].Velocity);
                    _fragments[row, col].Angle = 0f;
                    _fragments[row, col].AngularVelocity = XnaHelper.RandomAngle(0.0f, FRAGMENT_MAX_ANGULAR_VELOCITY);
                    _fragments[row, col].ScaleFactor = 1f;
                    _fragments[row, col].Active = true;
                }
        }

        /// <summary>
        /// Attempt to absorb unit into black hole. 
        /// </summary>
        /// <returns>Amount of mass eaten
        public virtual float EatByBlackHole(Vector2 blackHolePos, float blackHoleRadius)
        {
            if (_lifeState == LifeState.Shattered)      //special handling
            {
                for (int row = 0; row < ICE_DIVISIONS; row++)
                    for (int col = 0; col < ICE_DIVISIONS; col++)
                    {
                        if (Vector2.Distance(_fragments[row, col].Position, blackHolePos) < blackHoleRadius)
                        {
                            _fragments[row, col].BeingEaten = true;
                        }
                    }
            }

            if (_lifeState != LifeState.BeingEaten && _lifeState != LifeState.Destroyed 
                && _lifeState != LifeState.Ghost && blackHoleRadius > (Center - blackHolePos).Length())
            {
                _lifeState = LifeState.BeingEaten;
                _angularVelocity = 4 * MathHelper.TwoPi;
                return Mass;
            }
            return 0;
        }

        public void FlyToPoint(Vector2 pos, TimeSpan time)
        {
            _velocity = (pos - _position) / (float)time.TotalSeconds;
        }

        public void FlyToPoint(Vector2 pos, TimeSpan time, float speedFactor)
        {
            _velocity = speedFactor * (pos - _position) / (float)time.TotalSeconds;
        }

        public void Teleport(Vector2 destination)
        {
            _sprite.PlayTeleportEffect(Position);
            Position = destination;
        }

        #region Update Logic
        public virtual void Update(GameTime gameTime, Rectangle levelBounds)
        {
            switch(_lifeState)
            {
                case LifeState.Living:
                case LifeState.Ghost:
                    {
                        if (Panicked)
                        {
                            _panicTimer -= gameTime.ElapsedGameTime;
                            if (_panicTimer <= TimeSpan.Zero)
                            {
                                _panicTimer = TimeSpan.FromSeconds(PANIC_DIRECTION_CHANGE_FREQUENCY);
                                XnaHelper.RandomizeVector(ref _moveDirection, -1, 1, -1, 1);
                                _lookDirection = _moveDirection;
                            }
                        }
                        lookThisWay(LookDirection);
                        if (MoveDirection.Length() > 0)
                            moveThisWay(MoveDirection, gameTime);

                        //handle burning
                        ApplyDamage(_statusEffects.Fire * (float)gameTime.ElapsedGameTime.TotalSeconds * FIRE_DPS);

                        break;
                    }
                case LifeState.Disabled:
                case LifeState.Frozen:
                    {
                        if (_statusEffects.Cryo <= 0)
                        {
                            _lifeState = LifeState.Living;
                            //still cold after defrosting
                            _statusEffects.Cryo = MAX_STAT_EFFECT / 2;
                        }
                        break;
                    }
                case LifeState.Shattered:
                    {
                        for (int y = 0; y < ICE_DIVISIONS; y++)
                            for (int x = 0; x < ICE_DIVISIONS; x++)
                            {
                                _fragments[x, y].Angle += _fragments[x, y].AngularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                _fragments[x, y].Position += _fragments[x, y].Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                _fragments[x, y].Velocity += _fragments[x, y].Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                _fragments[x, y].Acceleration = Vector2.Zero;
                                _fragments[x, y].Health -= FRAGMENT_MELT_RATE * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                _fragments[x, y].ScaleFactor = _fragments[x,y].Health / FRAGMENT_HEALTH * FRAGMENT_SCALE_FACTOR;
                                XnaHelper.ClampVector(ref _fragments[x, y].Velocity, FRAGMENT_MAX_VELOCITY, out _fragments[x, y].Velocity);
                                if (_fragments[x, y].BeingEaten)
                                {
                                    _fragments[x, y].Health -= FRAGMENT_EAT_RATE * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                }
                            }
                        return;
                    }
                case LifeState.BeingEaten:
                    {
                        _sprite.ScaleFactor -= BLACK_HOLE_EAT_SCALE_FACTOR * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (_sprite.ScaleFactor <= 0)
                            _lifeState = LifeState.Destroyed;
                        break;
                    }
                case LifeState.Destroyed:
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

            //burning visual effect
            _burningParticleEffect.Spawn(Position, 0.0f, gameTime.ElapsedGameTime, _velocity);
            _burningParticleEffect.IntensityFactor = _statusEffects.Fire / MAX_STAT_EFFECT;
            _burningParticleEffect.Update(gameTime);

            //cryo visual effect
            if (_statusEffects.Cryo > 0 && _lifeState != LifeState.Disabled)
            {
                _sprite.Shade = Color.Lerp(Color.White, Color.Blue, _statusEffects.Cryo / MAX_STAT_EFFECT);
            }

            _hitRect.X = (int)Position.X - _hitRect.Width / 2;
            _hitRect.Y = (int)Position.Y - _hitRect.Height / 2;

            //manage stat effects
            if (_statusEffects.Cryo >= MAX_STAT_EFFECT && _lifeState != LifeState.Frozen)
            {
                _lifeState = LifeState.Frozen;
                _iceIntegrity = maxHealth * ICE_INTEGRITY_FACTOR;
                _statusEffects.Fire = 0;    //stop burning if frozen
            }

            //decrement every stat effect based on status resist
            _statusEffects -= _statusResist * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _statusEffects.Clamp(0, MAX_STAT_EFFECT);

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
            //apply movement force, taking into account cryo effect (which slows)
            ApplyForce(_moveForce * direction * (1 - _statusEffects.Cryo / MAX_STAT_EFFECT) );
            if (_movementParticleEffect != null)
                _movementParticleEffect.Spawn(Center, XnaHelper.DegreesFromVector(-direction), gameTime.ElapsedGameTime, _velocity); 
        }

        public void ApplyGravity(Gravity gravity, GameTime theGameTime)
        {
            Vector2 direction;
            if (_lifeState == LifeState.Shattered)
            {
                for (int y = 0; y < ICE_DIVISIONS; y++)
                    for (int x = 0; x < ICE_DIVISIONS; x++)
                    {
                        direction = gravity.Position - _fragments[y,x].Position;
                        direction.Normalize();
                        _fragments[y, x].Acceleration += direction * gravity.Magnitude * (float)theGameTime.ElapsedGameTime.TotalSeconds;
                    }
            }
            direction = gravity.Position - Position;
            //float distance = direction.Length();
            direction.Normalize();
            //_acceleration += gravity.Magnitude * direction * (float)theGameTime.ElapsedGameTime.TotalSeconds / (distance * 0.01f);
            _acceleration += direction * gravity.Magnitude * (float)theGameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Respawn(Vector2 newPosition)
        {
            System.Diagnostics.Debug.Assert(_lifeState == LifeState.Destroyed || _lifeState == LifeState.Dormant,
                "Error: Tried to respawn an enemy that was not destroyed or dormant");
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
            //special shattered collision detection
            if (_lifeState == LifeState.Shattered)
            {
                tempRec.Width = _hitRect.Width / ICE_DIVISIONS;
                tempRec.Height = _hitRect.Height / ICE_DIVISIONS;
                for (int i = 0; i < ICE_DIVISIONS ; i++)
                    for (int j = 0; j < ICE_DIVISIONS; j++)
                    {
                        tempRec.X = (int)(_fragments[i,j].Position.X - tempRec.Width / 2);
                        tempRec.Y = (int)(_fragments[i,j].Position.Y - tempRec.Height / 2);
                        if (tempRec.Intersects(other.HitRect))
                        {
                            Vector2 fVel = _fragments[i, j].Velocity;
                            float fMass = (float)Mass / (ICE_DIVISIONS * ICE_DIVISIONS);
                            temp = other.Velocity;
                            other._velocity = (other._velocity * (other.Mass - fMass) + 2 * fMass * fVel) /
                                                (fMass + other.Mass);
                            _fragments[i,j].Velocity = (fVel * (fMass - other.Mass) + 2 * other.Mass * temp) /
                                                (fMass + other.Mass);
                        }
                    }
                return; //ignore normal collision detection
            }

            //check if fire should be transferred
            float dist = XnaHelper.DistanceBetweenRects(HitRect, other.HitRect);
            if (dist < FIRE_SPREAD_DISTANCE)
            {
                if (_statusEffects.Fire > other._statusEffects.Fire)
                {
                    StatEffect transfer = new StatEffect() { Fire = FIRE_SPREAD_FACTOR * dist / FIRE_SPREAD_DISTANCE * _statusEffects.Fire };
                    other.ApplyStatus(transfer);
                    ApplyStatus(transfer * -FIRE_SPREAD_LOSS);
                }
            }

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


            //special shattered drawing logic
            if (_lifeState == LifeState.Shattered)
            {
                float integrityFactor;
                for (int y = 0; y < ICE_DIVISIONS; y++)
                    for (int x = 0; x < ICE_DIVISIONS; x++)
                    {
                        integrityFactor = _fragments[y,x].Health / (FRAGMENT_HEALTH);
                        tempRec.X = (int)(_fragments[y,x].Position.X - tempRec.Width / 2);
                        tempRec.Y = (int)(_fragments[y,x].Position.Y - tempRec.Height / 2);
                        tempRec.Width = (int)(_hitRect.Width * _fragments[y,x].ScaleFactor / ICE_DIVISIONS);
                        tempRec.Height = (int)(_hitRect.Height  * _fragments[y,x].ScaleFactor / ICE_DIVISIONS);
                        _sprite.DrawFragment(sb, y, x, ICE_DIVISIONS, tempRec, _fragments[y, x].Angle, integrityFactor);
                        _sprite.DrawIce(sb, tempRec, _fragments[y, x].Angle, integrityFactor);
                    }
                return;
            }

            if (_movementParticleEffect != null)
                _movementParticleEffect.Draw(sb);
            if (_lifeState != LifeState.BeingEaten && _lifeState != LifeState.Destroyed
                && _lifeState != LifeState.Dormant)
            {
                _burningParticleEffect.Draw(sb);
            }

            _sprite.Draw(sb, Position);

            if (_lifeState == LifeState.Frozen)
            {
                tempRec = HitRect;
                tempRec.X += (int)(HitRect.Width / 2);
                tempRec.Y += (int)(HitRect.Height / 2);
                _sprite.DrawIce(sb, tempRec, _sprite.Angle, _statusEffects.Cryo / MAX_STAT_EFFECT);
            }
        }
        #endregion
        #endregion
    }
}
