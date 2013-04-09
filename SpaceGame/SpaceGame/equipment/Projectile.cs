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
        public Vector2 Acceleration { get { return _acceleration; }  }
        public TimeSpan LifeTime { get { return _lifeTime; }  }
        public Sprite Sprite { get { return _sprite; }   }
        public int Penetration { get { return _penetration; } }     
        public int Mass { get { return _mass; } }
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
        int _mass;            //affects force applied to impacted unit
        ProjectileEffect _contactEffect;          //effect upon hitting a unit
        ProjectileEffect _proximityEffect;        //effect upon moving/existing
        ProjectileEffect _destinationEffect;      //effect upon reaching click location
        State _state;
        Rectangle _hitRect;
        #endregion

        #region contructor
        public Projectile(string spriteName)
        {
            _sprite = new Sprite(spriteName);
            _hitRect = new Rectangle(0, 0, (int)_sprite.Width, (int)_sprite.Height);
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
            Vector2 targetDestination)
        {
            Vector2.Multiply(ref direction, data.Speed, out _velocity);
            Vector2.Multiply(ref direction, data.Acceleration, out _acceleration);
            _lifeTime = TimeSpan.FromSeconds(data.SecondsToLive);
            _sprite = new Sprite(data.SpriteName);
            _penetration = data.Penetration;
            _mass = data.Mass;
            _contactEffect = data.ContactEffect;          
            _proximityEffect = data.ProximityEffect;       
            _destinationEffect = data.DestinationEffect;
            _distanceLeft = Vector2.Distance(pos, targetDestination);
        }

        public void Update(GameTime gameTime)
        {

            TimeSpan time = gameTime.ElapsedGameTime;

            switch (_state)
            {
                case State.Dormant:
                    break;

                case State.Moving:
                    _proximityEffect.SpawnParticles(time, _position);
                    _velocity += _acceleration * (float)time.TotalSeconds;
                    _position += _velocity * (float)time.TotalSeconds;
                    _lifeTime -= time;
                    if (_lifeTime < TimeSpan.Zero)
                    {
                        _state = State.Dormant;
                    }
                    _distanceLeft -= _velocity.Length() * (float)time.TotalSeconds;
                    if (_distanceLeft < 0 && _destinationEffect != null)
                    {
                        _state = State.ReachedDestination;
                    }
                    break;

                case State.JustHit:
                    _state = State.ApplyContactEffect;
                    break;

                case State.ApplyContactEffect:
                    _state = State.Dormant;
                    _contactEffect.SpawnParticles(time, _position);
                    break;

                case State.ReachedDestination:
                    _state = State.Dormant;
                    _destinationEffect.SpawnParticles(time, _position);
                    break;
            }
        }

        public void CheckAndApplyCollision(PhysicalUnit u)
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
                        if (_penetration > 0)
                        {
                            _penetration -= 1;
                            if (_penetration == 0)
                                _state = State.JustHit;
                        }
                        u.ApplyImpact(_velocity, _mass);
                    }
                    _proximityEffect.TryApply(_position, u);
                    break;
                case State.JustHit:
                    break;
                case State.ApplyContactEffect:
                    _contactEffect.TryApply(_position, u);
                    break;
                case State.ReachedDestination:
                    _destinationEffect.TryApply(_position, u);
                    break;
                default:
                    break;
            }
        }


        public void Draw(SpriteBatch sb)
        {
            if (_state != State.Dormant)
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
        public int MaxInstances;
        public int Penetration;     //number of hits before dissipating. Set as -1 for infinite
        public int Mass;            //affects force applied to impacted unit
        public ProjectileEffect ContactEffect;          //effect upon hitting a unit
        public ProjectileEffect ProximityEffect;        //effect upon moving/existing
        public ProjectileEffect DestinationEffect;        //effect upon reaching destination
    }
