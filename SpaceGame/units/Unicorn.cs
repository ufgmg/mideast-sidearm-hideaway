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
    class UnicornData
    {
        public float StartTime, EndTime, SpawnTime;
    }

    class Unicorn
    {
        #region constant
        const float APPEAR_TIME = 1;
        const float MAX_SCAN_TIME = 3.0f;
        const float LOCK_ON_TIME = 0.5f;
        const float CHARGE_TIME = 0.5f;
        const float GRAVITY_FIELD = -3000;
        const float EAT_TIME = 2.0f;
        const int IMPACT_DAMAGE = 100;
        const int IMPACT_IMPULSE = 10000;
        const float MOVE_SPEED = 5000;
        const float MIN_BLACKHOLE_SPAWN_DISTANCE = 200;
        const float MIN_PLAYER_SPAWN_DISTANCE = 200;
        public const float UNICORN_MASS = 50.0f;
        const int OUT_OF_BOUNDS_BUFFER = 200;
        const float UNICORN_GRAVITY = -4000;
        const int PARTICLE_SPAWN_GRANULARITY = 20;
        const string SPRITE_NAME = "Unicorn";
        const string STAND_PARTICLE_EFFECT = "UnicornStand";
        const string MOVE_PARTICLE_EFFECT = "UnicornCharge";
        const string EXPLODE_PARTICLE_EFFECT = "UnicornExplode";
        #endregion

        #region static
        public enum State
        {
            Dormant,
            Appearing,
            Scanning,
            Locked,
            Charging,
            BeingEaten
        }
        static Random rand = new Random();
        #endregion

        #region properties
        public Gravity Gravity { get { return _gravity; } }
        public bool SpawnEnable 
        {
            get {return _spawnEnable; }
            set
            {
                _state = State.Dormant;
            }
        }
        #endregion

        #region fields
        TimeSpan _startTime, _endTime, _spawnTime;
        TimeSpan _timer, _lockOnTimer;
        ParticleEffect _standingEffect, _chargeEffect, _explodeEffect;
        Vector2 _position, _direction, _velocity;
        State _state;
        Sprite _sprite;
        Gravity _gravity;
        Rectangle _hitRect;
        bool _spawnEnable;
        #endregion

        #region constructor
        public Unicorn(UnicornData data)
        {
            _startTime = TimeSpan.FromSeconds(data.StartTime);
            _endTime = TimeSpan.FromSeconds(data.EndTime);
            _spawnTime = TimeSpan.FromSeconds(data.SpawnTime);
            _timer = _spawnTime;
            _standingEffect = new ParticleEffect(STAND_PARTICLE_EFFECT);
            _chargeEffect = new ParticleEffect(MOVE_PARTICLE_EFFECT);
            _explodeEffect = new ParticleEffect(EXPLODE_PARTICLE_EFFECT);
            _sprite = new Sprite(SPRITE_NAME, Sprite.SpriteType.Unit);
            _state = State.Dormant;
            _gravity = new Gravity(_position, UNICORN_GRAVITY);
            _hitRect = new Rectangle(0, 0, (int)_sprite.Width, (int)_sprite.Height);
            _spawnEnable = true;
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime, Rectangle levelBounds, Vector2 blackHolePos, Vector2 targetPos, Rectangle playerRect)
        {
            _standingEffect.Update(gameTime);
            _chargeEffect.Update(gameTime);
            _explodeEffect.Update(gameTime);
            _sprite.Update(gameTime);

            switch (_state)
            {
                case State.Dormant:
                    if (SpawnEnable)
                    {
                        _timer -= gameTime.ElapsedGameTime;
                    }

                    if (_timer <= TimeSpan.Zero)
                    {
                        setPosition(blackHolePos, targetPos, levelBounds.Width, levelBounds.Height);
                        _gravity.Position = _position;
                        _state = State.Appearing;
                        _timer = TimeSpan.FromSeconds(APPEAR_TIME);
                    }
                    break;

                case State.Appearing:
                    _timer -= gameTime.ElapsedGameTime;
                    if (_timer <= TimeSpan.Zero)
                    {
                        _timer = TimeSpan.FromSeconds(MAX_SCAN_TIME);
                        _lockOnTimer = TimeSpan.FromSeconds(LOCK_ON_TIME);
                        _state = State.Scanning;
                        _sprite.Shade = Color.White;
                    }
                    else
                    {
                        _standingEffect.Spawn(_position, XnaHelper.DegreesFromVector(_direction), gameTime.ElapsedGameTime, Vector2.Zero);
                        _sprite.Shade = Color.Lerp(Color.Transparent, Color.White,
                            (APPEAR_TIME - (float)_timer.TotalSeconds) / APPEAR_TIME);
                    }
                    break;

                case State.Scanning:
                    _timer -= gameTime.ElapsedGameTime;
                    _standingEffect.Spawn(_position, XnaHelper.DegreesFromVector(_direction), gameTime.ElapsedGameTime, Vector2.Zero);
                    _position.Y += scanVelocity(targetPos.Y - _position.Y) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    _hitRect.Y = (int)_position.Y - _hitRect.Height / 2;
                    //scan for player
                    //check if player found or scan time up
                    if (_hitRect.Top < targetPos.Y && targetPos.Y < _hitRect.Bottom)
                    {
                        _lockOnTimer -= gameTime.ElapsedGameTime;
                    }
                    if (_lockOnTimer < TimeSpan.Zero || _timer < TimeSpan.Zero)
                    {
                        _timer = TimeSpan.FromSeconds(CHARGE_TIME);
                        _state = State.Locked;
                        _direction = _sprite.FlipH ? -Vector2.UnitX : Vector2.UnitX;
                    }
                    break;

                case State.Locked:
                    _timer -= gameTime.ElapsedGameTime;
                    if (_timer < TimeSpan.Zero)
                    {
                        _state = State.Charging;
                        Vector2.Multiply(ref _direction, MOVE_SPEED, out _velocity);
                    }
                    break;

                case State.Charging:
                    //trace movement path
                    for (int i = 0 ; i < PARTICLE_SPAWN_GRANULARITY ; i++)
                    {
                        _position.X += _velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds / PARTICLE_SPAWN_GRANULARITY;
                        float angle = (_sprite.FlipH) ? 90 : -90;
                        _chargeEffect.Spawn(_position, angle, gameTime.ElapsedGameTime, Vector2.Zero);
                    }
                    _hitRect.X = (int)_position.X - _hitRect.Width;

                    _gravity.Position = _position;
                    if (outOfBounds(levelBounds.Width, levelBounds.Height))
                    {
                        _sprite.Reset();
                        _timer = _spawnTime;
                        _state = State.Dormant;
                    }
                    break;

                case State.BeingEaten:
                    _timer -= gameTime.ElapsedGameTime;
                    //_explodeEffect.Spawn(_position, 0.0f, gameTime.ElapsedGameTime, Vector2.Zero);
                    if (_timer <= TimeSpan.Zero)
                    {
                        _state = State.Dormant;
                        _timer = _spawnTime;
                    }
                    break;
            }
                    
        }

        public void CheckAndApplyCollision(PhysicalUnit unit, GameTime gameTime)
        {
            switch (_state)
            {
                case State.Dormant:
                    break;
                case State.Appearing:
                case State.Scanning:
                    unit.ApplyGravity(_gravity, gameTime);
                    break;
                case State.Charging:
                    unit.ApplyGravity(_gravity, gameTime);
                    if (willCollide(unit.Top, unit.Bottom, unit.Left, unit.Right, gameTime.ElapsedGameTime))
                    {
                        unit.ApplyImpact(_velocity, IMPACT_IMPULSE);
                        unit.ApplyDamage(IMPACT_DAMAGE);
                        break;
                    }
                    break;
            }
        }

        private bool willCollide(float targetTop, float targetBottom, float targetLeft, float targetRight, TimeSpan time)
        {
                    float leftPoint = _velocity.X > 0 ? _hitRect.Left
                         : _hitRect.Left + (int)((float)time.TotalSeconds * _velocity.X);
                    float rightPoint = _velocity.X > 0 ? _hitRect.Right + (int)((float)time.TotalSeconds * _velocity.X)
                        : _hitRect.Right;
                    return (targetTop < _hitRect.Bottom && targetBottom > _hitRect.Top
                        && leftPoint < targetRight && targetLeft < rightPoint);
        }

        private bool outOfBounds(int levelWidth, int levelHeight)
        {
            return (_position.X < -OUT_OF_BOUNDS_BUFFER || _position.X + _sprite.Width >= levelWidth + OUT_OF_BOUNDS_BUFFER||
                    _position.Y < -OUT_OF_BOUNDS_BUFFER || _position.Y + _sprite.Height >= levelHeight + OUT_OF_BOUNDS_BUFFER);
        }

        public bool EatByBlackHole(Vector2 blackHolePos, GameTime gameTime)
        {
            if (_state == State.Charging && willCollide(blackHolePos.Y, blackHolePos.Y, blackHolePos.X, blackHolePos.X, gameTime.ElapsedGameTime))
            {
                _state = State.BeingEaten;
                _sprite.Reset();
                _position = blackHolePos;
                _timer = TimeSpan.FromSeconds(EAT_TIME);
                _explodeEffect.Spawn(blackHolePos, 0, gameTime.ElapsedGameTime, Vector2.Zero);
                return true;
            }
            return false;
        }

        private void setPosition(Vector2 blackHolePosition, Vector2 playerPosition, int levelWidth, int levelHeight)
        {   //set bounds on new spawn location
            bool leftSide = (XnaHelper.RandomInt(0, 1) == 0);
            _position.X = leftSide ? 0 : levelWidth;
            _hitRect.X = leftSide ? 0 : levelWidth - _hitRect.Width;
            _sprite.FlipH = !leftSide;
            _position.Y = XnaHelper.RandomInt(0, levelHeight); 
            _hitRect.Y = (int)_position.Y;
        }

        /// <summary>
        /// get the move speed (px/second) as a function of distance from player
        /// </summary>
        /// <param name="distanceFromPlayer">Vertical displacement (signed) from target</param>
        /// <returns></returns>
        private float scanVelocity(float verticalDisplacement)
        {
            return verticalDisplacement + 100 * (verticalDisplacement < 0 ? -1 : 1);
        }

        public void Draw(SpriteBatch sb)
        {
            _standingEffect.Draw(sb);
            _chargeEffect.Draw(sb);
            _explodeEffect.Draw(sb);
            if (_state != State.Dormant && _state != State.BeingEaten)
                _sprite.Draw(sb, _position);
        }
        #endregion

    }
}
