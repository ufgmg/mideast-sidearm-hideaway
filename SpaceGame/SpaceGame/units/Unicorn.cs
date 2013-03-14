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
        const float CHARGE_TIME = 0.5f;
        const float GRAVITY_FIELD = -3000;
        const int IMPACT_DAMAGE = 100;
        const int IMPACT_IMPULSE = 10000;
        const float MOVE_SPEED = 5000;
        const float MAX_SCAN_SPEED = MathHelper.TwoPi;
        const float SCAN_ARC_RADIUS = 2000;
        const float SCAN_ARC_ANGLE = 0.349f;     //20 degrees, in radians
        const float MIN_BLACKHOLE_SPAWN_DISTANCE = 200;
        const float MIN_PLAYER_SPAWN_DISTANCE = 200;
        const int OUT_OF_BOUNDS_BUFFER = 200;
        const float UNICORN_GRAVITY = -40000;
        const int COLLISION_GRANULARITY = 30;
        const string SPRITE_NAME = "Unicorn";
        const string STAND_PARTICLE_EFFECT = "UnicornStand";
        const string MOVE_PARTICLE_EFFECT = "UnicornCharge";
        #endregion

        #region static
        public enum State
        {
            Dormant,
            Appearing,
            Scanning,
            Locked,
            Charging
        }
        static Random rand = new Random();
        #endregion

        #region properties
        public Gravity Gravity { get { return _gravity; } }
        #endregion

        #region fields
        TimeSpan _startTime, _endTime, _spawnTime;
        TimeSpan _timer;
        ParticleEffect _standingEffect, _chargeEffect;
        Vector2 _position, _direction, _velocity;
        State _state;
        Sprite _sprite;
        Gravity _gravity;
        Rectangle[] _hitRects;
        float _turnSpeed;
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
            _sprite = new Sprite(SPRITE_NAME);
            _state = State.Dormant;
            _gravity = new Gravity(_position, UNICORN_GRAVITY);
            _hitRects = new Rectangle[COLLISION_GRANULARITY];
            for (int i = 0 ; i < COLLISION_GRANULARITY ; i++)
            {
                _hitRects[i] = new Rectangle(0, 0, (int)_sprite.Width, (int)_sprite.Height);
            }
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime, Rectangle levelBounds, Vector2 blackHolePos, Vector2 playerPos, Rectangle playerRect)
        {
            _standingEffect.Update(gameTime);
            _chargeEffect.Update(gameTime);
            _sprite.Update(gameTime);

            switch (_state)
            {
                case State.Dormant:
                    _timer -= gameTime.ElapsedGameTime;
                    if (_timer <= TimeSpan.Zero)
                    {
                        setPosition(blackHolePos, playerPos, levelBounds.Width, levelBounds.Height);
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
                        _state = State.Scanning;
                        _turnSpeed = 2 * MAX_SCAN_SPEED * (float)(0.5 - rand.NextDouble());
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
                    //scan for player
                    _sprite.Angle += _turnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    _sprite.FlipH = (MathHelper.WrapAngle(_sprite.Angle) > 0);
                    //check if player found or scan time up
                    if (XnaHelper.RectangleIntersectsArc(playerRect, _position, SCAN_ARC_RADIUS, _sprite.Angle, SCAN_ARC_ANGLE)
                        || _timer < TimeSpan.Zero)
                    {
                        _timer = TimeSpan.FromSeconds(CHARGE_TIME);
                        _state = State.Locked;
                        XnaHelper.VectorFromAngle(_sprite.Angle, out _direction); 
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
                    for (int i = 0; i < _hitRects.Length; i++)
                    {
                        _hitRects[i].X = (int)(_position.X - _hitRects[i].Width / 2  + i * _velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds / COLLISION_GRANULARITY);
                        _hitRects[i].Y = (int)(_position.Y - _hitRects[i].Height / 2 + i * _velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds / COLLISION_GRANULARITY);
                    }

                    foreach (Rectangle rect in _hitRects)
                    {
                        _chargeEffect.Spawn(new Vector2(rect.Center.X, rect.Center.Y), 0.0f, gameTime.ElapsedGameTime, Vector2.Zero);
                    }

                    _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    _gravity.Position = _position;
                    if (outOfBounds(levelBounds.Width, levelBounds.Height))
                    {
                        _sprite.Reset();
                        _timer = _spawnTime;
                        _state = State.Dormant;
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
                    for (int i = 0; i < _hitRects.Length; i++)
                    {
                        if (_hitRects[i].Intersects(unit.HitRect))
                        {
                            unit.ApplyImpact(_velocity, IMPACT_IMPULSE);
                            unit.ApplyDamage(IMPACT_DAMAGE);
                            break;
                        }
                    }
                    break;
            }
        }

        private bool outOfBounds(int levelWidth, int levelHeight)
        {
            return (_position.X < -OUT_OF_BOUNDS_BUFFER || _position.X + _sprite.Width >= levelWidth + OUT_OF_BOUNDS_BUFFER||
                    _position.Y < -OUT_OF_BOUNDS_BUFFER || _position.Y + _sprite.Height >= levelHeight + OUT_OF_BOUNDS_BUFFER);
        }

        private void EatByBlackHole()
        {
        }

        private void setPosition(Vector2 blackHolePosition, Vector2 playerPosition, int levelWidth, int levelHeight)
        {   //set bounds on new spawn location
            int minX, maxX, minY, maxY;

            //spawn in bounds 
            minX = 0;
            maxX = levelWidth;
            minY = 0;
            maxY = levelHeight;

            //keep reselecting position until find a position far enough from black hole
            do { XnaHelper.RandomizeVector(ref _position, minX, maxX, minY, maxY); }
            while (Vector2.Distance(blackHolePosition, _position) < MIN_BLACKHOLE_SPAWN_DISTANCE ||
                   Vector2.Distance(playerPosition, _position) < MIN_PLAYER_SPAWN_DISTANCE);

            _sprite.Angle = MathHelper.ToRadians(XnaHelper.RandomAngle(0.0f, 180.0f));
        }

        public void Draw(SpriteBatch sb)
        {
            _standingEffect.Draw(sb);
            _chargeEffect.Draw(sb);
            if (_state != State.Dormant)
                _sprite.Draw(sb, _position);
        }
        #endregion

    }
}
