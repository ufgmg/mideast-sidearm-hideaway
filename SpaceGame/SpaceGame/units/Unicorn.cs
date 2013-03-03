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
        const float SECONDS_TILL_CHARGE = 3;
        const float GRAVITY_FIELD = -3000;
        const float IMPACT_DAMAGE = 100;
        const float IMPACT_IMPULSE = 10000;
        const float MOVE_SPEED = 10000;
        const float MIN_BLACKHOLE_SPAWN_DISTANCE = 200; 
        #endregion

        #region static
        public enum State
        {
            Dormant,
            Standing,
            Charging
        }
        #endregion

        #region properties
        #endregion

        #region fields
        TimeSpan _startTime, _endTime, _spawnTime;
        TimeSpan _timer;
        ParticleEffect _standingEffect, _chargeEffect;
        Vector2 _position, _direction;
        State _state;
        Sprite _sprite;
        #endregion

        #region constructor
        public Unicorn(UnicornData data)
        {
            _startTime = TimeSpan.FromSeconds(data.StartTime);
            _endTime = TimeSpan.FromSeconds(data.EndTime);
            _spawnTime = TimeSpan.FromSeconds(data.SpawnTime);
            _timer = _spawnTime;
            _standingEffect = new ParticleEffect("UnicornStand");
            _chargeEffect = new ParticleEffect("UnicornCharge");
            _sprite = new Sprite("Unicorn");
            _state = State.Dormant;
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime, Rectangle levelBounds, Vector2 blackHolePos)
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
                        setPosition(blackHolePos, levelBounds.Width, levelBounds.Height);
                        _state = State.Standing;
                        _timer = TimeSpan.FromSeconds(SECONDS_TILL_CHARGE);
                    }
                    break;

                case State.Standing:
                    _timer -= gameTime.ElapsedGameTime;
                    _standingEffect.Spawn(_position, 0.0f, gameTime.ElapsedGameTime, Vector2.Zero);
                    _sprite.Shade = Color.Lerp(Color.Transparent, Color.White, 
                        (SECONDS_TILL_CHARGE - (float)_timer.TotalSeconds) / SECONDS_TILL_CHARGE);
                    break;

                case State.Charging:
                    break;
            }
                    
        }

        private void EatByBlackHole()
        {
        }

        private void setPosition(Vector2 blackHolePosition, int levelWidth, int levelHeight)
        {   //set bounds on new spawn location
            int minX, maxX, minY, maxY;

            //spawn in bounds 
            minX = 0;
            maxX = levelWidth;
            minY = 0;
            maxY = levelHeight;

            do { XnaHelper.RandomizeVector(ref _position, minX, maxX, minY, maxY); }
            while (Vector2.Distance(blackHolePosition, _position) < MIN_BLACKHOLE_SPAWN_DISTANCE);

            _sprite.Angle = MathHelper.ToRadians(XnaHelper.RandomAngle(0, 360));
            _direction.X = (float)Math.Sin(_sprite.Angle);
            _direction.Y = -(float)Math.Cos(_sprite.Angle);
        }

        public void Draw(SpriteBatch sb)
        {
            if (_state != State.Dormant)
                _sprite.Draw(sb, _position);
            _standingEffect.Draw(sb);
            _chargeEffect.Draw(sb);
        }
        #endregion

    }
}
