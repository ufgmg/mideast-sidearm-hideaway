using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.units;

namespace SpaceGame.equipment
{
    class Projectile
    {
        #region static
        public enum State
        {
            Dormant,
            Moving,
            JustHit,
            ApplyContactEffect,
            ReachedDestination
        }
        Vector2 tempVec;
        #endregion

        #region properties
        public Vector2 Position { get { return _position; } }
        public Vector2 Velocity { get { return _velocity; } }
        public Vector2 Acceleration { get { return _acceleration; } }
        public TimeSpan LifeTime { get { return _lifeTime; } }
        public Sprite Sprite { get { return _sprite; } }
        public int Penetration { get { return _penetration; } }
        public float Mass { get { return _mass; } }
        public State ProjectileState { get { return _state; } }
        public Rectangle HitRect { get { return _hitRect; } }
        #endregion

        #region fields
        Vector2 _position;
        Vector2 _velocity;
        Vector2 _acceleration;
        float _distanceLeft;
        TimeSpan _lifeTime;
        Sprite _sprite;
        int _penetration;     //number of hits before dissipating. Set as -1 for infinite
        float _mass;            //affects force applied to impacted unit
        float _angularVelocity;            //radians per second
        ProjectileEffect _contactEffect;          //effect upon hitting a unit
        ProjectileEffect _proximityEffect;        //effect upon moving/existing
        ProjectileEffect _destinationEffect;      //effect upon reaching click location
        State _state;
        Rectangle _hitRect;
        TimeSpan _timer;
        #endregion

        #region contructor
        public Projectile(string spriteName)
        {
            _sprite = new Sprite(spriteName, graphics.Sprite.SpriteType.Projectile);
            _hitRect = new Rectangle(0, 0, (int)_sprite.Width, (int)_sprite.Height);
            _contactEffect = ProjectileEffect.NullEffect;
            _destinationEffect = ProjectileEffect.NullEffect;
            _proximityEffect = ProjectileEffect.NullEffect;
        }
        #endregion

        #region public methods
        /// <summary>
        /// initialize a new projectile
        /// </summary>
        /// <param name="pos">start position</param>
        /// <param name="direction">fire direction</param>
        /// <param name="data">data to initialize projectile with</param>
        public void Initialize(Vector2 pos, Vector2 direction, ProjectileData data,
            Vector2 targetDestination, Vector2 sourceVelocity,
            ProjectileEffect contactEffect, ProjectileEffect destinationEffect,
            ProjectileEffect proximityEffect)
        {
            _position = pos;
            Vector2.Multiply(ref direction, data.Speed, out _velocity);
            Vector2.Add(ref _velocity, ref sourceVelocity, out _velocity);
            Vector2.Multiply(ref direction, data.Acceleration, out _acceleration);
            _lifeTime = TimeSpan.FromSeconds(data.SecondsToLive);
            _sprite.Reset();
            //_sprite = new Sprite(data.SpriteName);
            _penetration = data.Penetration;
            _mass = data.Mass;
            _contactEffect = contactEffect;
            _proximityEffect = proximityEffect;
            _destinationEffect = destinationEffect;
            _distanceLeft = Vector2.Distance(pos, targetDestination);
            _state = State.Moving;
            _sprite.Angle = utility.XnaHelper.RadiansFromVector(direction);
            _angularVelocity = MathHelper.ToRadians(data.Rotation);
        }

        public void Update(GameTime gameTime)
        {

            TimeSpan time = gameTime.ElapsedGameTime;

            switch (_state)
            {
                case State.Dormant:
                    break;

                case State.Moving:
                    _proximityEffect.SpawnParticles(time, _position, MathHelper.ToDegrees(Sprite.Angle - MathHelper.Pi), _velocity);
                    _velocity += _acceleration * (float)time.TotalSeconds;
                    _position += _velocity * (float)time.TotalSeconds;
                    _lifeTime -= time;
                    _hitRect.X = (int)_position.X;
                    _hitRect.Y = (int)_position.Y;
                    Sprite.Angle += _angularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_lifeTime < TimeSpan.Zero)
                    {
                        if (_destinationEffect != ProjectileEffect.NullEffect)
                        {
                            _state = State.ReachedDestination;
                            _timer = _destinationEffect.Duration;
                        }
                        else
                        {
                            _state = State.Dormant;
                        }
                    }
                    _distanceLeft -= _velocity.Length() * (float)time.TotalSeconds;
                    if (_distanceLeft < 0 && _destinationEffect != ProjectileEffect.NullEffect)
                    {
                        _state = State.ReachedDestination;
                        _timer = _destinationEffect.Duration;
                    }
                    break;

                case State.JustHit:
                    if (_contactEffect != ProjectileEffect.NullEffect)
                    {
                        _state = State.ApplyContactEffect;
                        _timer = _contactEffect.Duration;
                    }
                    else
                    {
                        _state = State.Dormant;
                    }
                    break;

                case State.ApplyContactEffect:
                    _timer -= time;
                    if (_timer < TimeSpan.Zero)
                    {
                        _state = State.Dormant;
                    }
                    _contactEffect.SpawnParticles(time, _position, 0.0f, Vector2.Zero);
                    break;

                case State.ReachedDestination:
                    _timer -= time;
                    if (_timer < TimeSpan.Zero)
                    {
                        _state = State.Dormant;
                    }
                    _destinationEffect.SpawnParticles(time, _position, 0.0f, Vector2.Zero);
                    break;
            }
        }

        public void CheckAndApplyCollision(PhysicalUnit u, TimeSpan time)
        {
            if (!u.Collides)
                return;

            switch (_state)
            {
                case State.Dormant:
                    break;
                case State.Moving:
                    if (u.HitRect.Intersects(_hitRect))
                    {
                        if (_penetration != -1)
                        {
                            _penetration -= 1;
                            if (_penetration <= 0)
                                _state = State.JustHit;
                        }
                        u.ApplyImpact(_velocity, _mass);
                    }
                    _proximityEffect.TryApply(_position, u, time);
                    break;
                case State.JustHit:
                    break;
                case State.ApplyContactEffect:
                    _contactEffect.TryApply(_position, u, time);
                    break;
                case State.ReachedDestination:
                    _destinationEffect.TryApply(_position, u, time);
                    break;
                default:
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (_state == State.Dormant)
            {
                return;
            }

            if (_state == State.Moving)
            {
                _sprite.Draw(sb, _position);
            }
        }
        #endregion
    }

    class ProjectileData
    {
        public float Speed;
        public float Acceleration;
        public float SecondsToLive;
        public string SpriteName;
        public int Penetration;     //number of hits before dissipating. Set as -1 for infinite
        public float Mass;            //affects force applied to impacted unit
        public float Rotation;            //angular velocity (Degrees per second)
        public ProjectileEffectData ContactEffect;          //effect upon hitting a unit
        public ProjectileEffectData ProximityEffect;        //effect upon moving/existing
        public ProjectileEffectData DestinationEffect;        //effect upon reaching destination
    }
}
