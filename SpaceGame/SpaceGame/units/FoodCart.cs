using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using SpaceGame.graphics;
using SpaceGame.utility;

namespace SpaceGame.units
{
    class FoodCart : PhysicalUnit
    {
        #region constant
        const float MIN_BLACKHOLE_DISTANCE = 200;
        const float MIN_WAYPOINT_DISTANCE = 20;
        const float LEAVE_LEVEL_TIME = 3;
        //how far out of bounds trickle waves can spawn enemies
        const int OUT_OF_BOUNDS_SPAWN_BUFFER = 30;
        #endregion

        #region static
        public static PhysicalData Data;
        #endregion

        #region properties
        /// <summary>
        /// Whether the player can enter the shop
        /// </summary>
        public bool CanInteract;
        #endregion

        #region fields
        TimeSpan _startTime, _duration, _leaveTimer;
        Vector2 _nextWaypoint;
        #endregion

        #region constructor
        public FoodCart(TimeSpan startTime, TimeSpan duration)
            :base(Data)
        {
            _startTime = startTime;
            _duration = duration;
            _lifeState = LifeState.Dormant;
            _leaveTimer = TimeSpan.FromSeconds(LEAVE_LEVEL_TIME);
        }
        #endregion

        #region methods
        public void Update(GameTime gameTime, Rectangle levelBounds, Vector2 blackHolePosition)
        {
            if (_lifeState == LifeState.Dormant)
            {
                _startTime -= gameTime.ElapsedGameTime;
                if (_startTime <= TimeSpan.Zero)
                {
                    _lifeState = LifeState.Living;
                    CanInteract = true;
                    setWaypoint(blackHolePosition, levelBounds.Width, levelBounds.Height);
                    setPosition(blackHolePosition, levelBounds.Width, levelBounds.Height);
                }
            }
            else if (_lifeState == LifeState.Living)
            {
                _duration -= gameTime.ElapsedGameTime;
                if (_duration < TimeSpan.Zero)
                {
                    _lifeState = LifeState.Ghost;
                    CanInteract = false;
                }
                if (Vector2.Distance(_nextWaypoint, Position) < MIN_WAYPOINT_DISTANCE)
                    setWaypoint(blackHolePosition, levelBounds.Width, levelBounds.Height);

                MoveDirection = XnaHelper.DirectionBetween(Position, _nextWaypoint);
            }
            else if (_lifeState == LifeState.Ghost)
            {
                MoveDirection = XnaHelper.DirectionBetween(Position, _nextWaypoint);
                _leaveTimer -= gameTime.ElapsedGameTime;
                _sprite.ScaleFactor = MathHelper.Lerp(0.0f, 1.0f, (float)_leaveTimer.TotalSeconds / LEAVE_LEVEL_TIME);
                _sprite.Shade = Color.Lerp(Color.Transparent, Color.White, (float)_leaveTimer.TotalSeconds / LEAVE_LEVEL_TIME);
                if (_leaveTimer < TimeSpan.Zero)
                {
                    _lifeState = LifeState.Destroyed;
                }
            }
            

            if (Vector2.Distance(_nextWaypoint, Position) < MIN_WAYPOINT_DISTANCE)
                setWaypoint(blackHolePosition, levelBounds.Width, levelBounds.Height);

            base.Update(gameTime, levelBounds);
        }

        private void setWaypoint(Vector2 blackHolePosition, int levelWidth, int levelHeight)
        {   //set bounds on new spawn location
            int minX, maxX, minY, maxY;

            //spawn in bounds 
            minX = 0;
            maxX = levelWidth;
            minY = 0;
            maxY = levelHeight;

            //keep reselecting position until find a position far enough from black hole
            do { XnaHelper.RandomizeVector(ref _nextWaypoint, minX, maxX, minY, maxY); }
            while (Vector2.Distance(blackHolePosition, _nextWaypoint) < MIN_BLACKHOLE_DISTANCE);

        }

        private void setPosition(Vector2 blackHolePosition, int levelWidth, int levelHeight)
        {   //set bounds on new spawn location
            int minX, maxX, minY, maxY;

            //spawn in bounds -- default for burst wave
            minX = 0;
            maxX = levelWidth;
            minY = 0;
            maxY = levelHeight;

            switch (XnaHelper.RandomInt(0, 3))
            {
                case 0:     //top
                    minX = -OUT_OF_BOUNDS_SPAWN_BUFFER;
                    maxX = levelWidth + OUT_OF_BOUNDS_SPAWN_BUFFER;
                    minY = -OUT_OF_BOUNDS_SPAWN_BUFFER;
                    maxY = 0;
                    break;
                case 1:     //right
                    minX = levelWidth;
                    maxX = levelWidth + OUT_OF_BOUNDS_SPAWN_BUFFER;
                    minY = -OUT_OF_BOUNDS_SPAWN_BUFFER;
                    maxY = levelHeight + OUT_OF_BOUNDS_SPAWN_BUFFER;
                    break;
                case 2:     //bottom
                    minX = -OUT_OF_BOUNDS_SPAWN_BUFFER;
                    maxX = levelWidth + OUT_OF_BOUNDS_SPAWN_BUFFER;
                    minY = levelHeight;
                    maxY = levelHeight + OUT_OF_BOUNDS_SPAWN_BUFFER;
                    break;
                case 3:     //left
                    minX = -OUT_OF_BOUNDS_SPAWN_BUFFER;
                    maxX = 0;
                    minY = -OUT_OF_BOUNDS_SPAWN_BUFFER;
                    maxY = levelHeight + OUT_OF_BOUNDS_SPAWN_BUFFER;
                    break;
            }

            do { XnaHelper.RandomizeVector(ref _position, minX, maxX, minY, maxY); }
            while (Vector2.Distance(blackHolePosition, _position) < MIN_BLACKHOLE_DISTANCE);
        }

        #endregion
    }
}
