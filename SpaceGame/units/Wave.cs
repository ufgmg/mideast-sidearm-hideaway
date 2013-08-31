using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.equipment;
using SpaceGame.graphics;
using SpaceGame.utility;

namespace SpaceGame.units
{
    /// <summary>
    /// A wave of enemies
    /// </summary>
    class Wave
    {
        #region constant
        //how far out of bounds trickle waves can spawn enemies
        const int OUT_OF_BOUNDS_SPAWN_BUFFER = 30;
        //how long between starting to show effect and spawning enemies
        const float ACTIVATION_DELAY_SECONDS = 3;
        //how fast portal effect rotates (degrees/sec)
        const float PORTAL_ROTATION_RATE = 720;
        const string PORTAL_EFFECT_NAME1 = "SpawnPortal1";
        //minimum distance allowable between spawn location and black hole
        const float MIN_BLACKHOLE_SPAWN_DISTANCE = 200; 
        #endregion

        #region classes
        public class WaveData
        {
            public string[] EnemyNames;
            public TimeSpan SpawnInterval;
            public TimeSpan StartTime;
        }
        #endregion

        #region fields
        int _numEnemies;        //total number of enemies in wave
        int _spawnedSoFar;      //number of enemies already spawned
        Enemy[] _enemies;
        //enemy spawning info
        TimeSpan _tillNextSpawn;    //how long till spawning a new enemy
        TimeSpan _spawnInterval;    //how long between enemy spawns
        bool _isTrickleWave;        //constant trickle of enemies through level
        Vector2 _spawnLocation;     //where in level to spawn enemies
        TimeSpan _startTimer;       //when to start spawning enemies

        //these three should only apply to burst waves
        TimeSpan _activationDelay;  //how long to wait after activation before spawning
        ParticleEffect _portalEffect;   //particle effect to play once spawning begins
        float _portalAngle;         //so portal effect can rotate

        Rectangle _levelBounds;
        #endregion

        #region properties
        //set when every enemy in mob has been spawned
        //does not apply to trickle waves
        public bool Active { get; private set; }
        /// <summary>
        /// Set to false when level ends to prevent spawning
        /// </summary>
        public bool SpawnEnable { get; set; }
        #endregion

        #region constructor
        public Wave(WaveData data, bool trickleWave, Rectangle levelBounds)
        {
            string[] enemyNames = data.EnemyNames;
            _enemies = new Enemy[enemyNames.Length];
            for (int j = 0; j < _enemies.Length; j++)
            {
                _enemies[j] = new Enemy(enemyNames[j], levelBounds);
            }
            _numEnemies = _enemies.Length;
            _spawnedSoFar = 0;
            _tillNextSpawn = data.SpawnInterval;
            _spawnInterval = data.SpawnInterval;
            _startTimer = data.StartTime;
            _isTrickleWave = trickleWave;
            _spawnLocation = Vector2.Zero;
            //activation delay is zero for trickle waves
            _activationDelay = _isTrickleWave ? TimeSpan.Zero : TimeSpan.FromSeconds((double)ACTIVATION_DELAY_SECONDS);
            //assign a portal particle effect if it is a burst wave
            _portalEffect = (trickleWave) ? null : new ParticleEffect(PORTAL_EFFECT_NAME1);
            SpawnEnable = true;
            _levelBounds = levelBounds;
        }
        #endregion

        #region methods
        private void spawn(GameTime gameTime, Vector2 position, Vector2 blackHolePosition)
        {   //count down towards next spawn, spawn if count <= 0
            if (!SpawnEnable)
                return;
            if (_spawnedSoFar == _numEnemies && !_isTrickleWave)
                return;     //non-trickle waves should not respawn enemies

            _tillNextSpawn -= gameTime.ElapsedGameTime;
            if (_tillNextSpawn <= TimeSpan.Zero && Active)
            {   //time to spawn
                Enemy enemy = _enemies[_spawnedSoFar % _numEnemies];
                if (enemy.UnitLifeState == PhysicalUnit.LifeState.Dormant ||
                    enemy.UnitLifeState == PhysicalUnit.LifeState.Destroyed)
                {
                    _enemies[_spawnedSoFar % _numEnemies].Respawn(position);
                    _tillNextSpawn = _spawnInterval;

                    //trickle waves should reposition after every spawn
                    //note: this is inside this block so it only runs when an enemy is Sucessfully spawned
                    //if the enemy in the current slot is already alive, it will not reposition
                    if (_isTrickleWave)
                        setPosition(blackHolePosition, _levelBounds.Width, _levelBounds.Height);     
                }
                //if slot not ready to be respawned, cycle through slots each update
                _spawnedSoFar++;    
            }
        }

        /// <summary>
        /// Update wave, updating behavior of all enemies.
        /// Check collisions against player and self, but not other waves
        /// Check weapon collisions against player
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="player"></param>
        /// <param name="blackHole"></param>
        /// <param name="weapon1"></param>
        /// <param name="weapon2"></param>
        public void Update(GameTime gameTime, Spaceman player, 
            BlackHole blackHole, Weapon weapon1, Weapon weapon2, InventoryManager inventory, Unicorn[] unicorns)
        {
            if (_startTimer >= TimeSpan.Zero)        //not started yet
            {
                _startTimer -= gameTime.ElapsedGameTime;
                if (_startTimer < TimeSpan.Zero)
                {
                    Active = true;      //activate if start timer complete
                    setPosition(blackHole.Position, _levelBounds.Width, _levelBounds.Height);        //set first spawn position
                }
            }

            if (_portalEffect != null)
                _portalEffect.Update(gameTime);

            if (!Active)
                return;     //don't update if not active

            //play particle effect if existant 
            if (_portalEffect != null)
            {
                //spawn particles if still spawning enemies
                if (_spawnedSoFar < _numEnemies && SpawnEnable)
                {
                    _portalEffect.Spawn(_spawnLocation, 90.0f + _portalAngle, gameTime.ElapsedGameTime, Vector2.Zero);
                    _portalEffect.Spawn(_spawnLocation, -90.0f + _portalAngle, gameTime.ElapsedGameTime, Vector2.Zero);
                }
                _portalAngle += (float)gameTime.ElapsedGameTime.TotalSeconds * PORTAL_ROTATION_RATE;
                if (_activationDelay >= TimeSpan.Zero)      //gradually increase particle intensity
                {
                    _portalEffect.IntensityFactor = 1.0f - (float)_activationDelay.TotalSeconds / ACTIVATION_DELAY_SECONDS;
                }
            }

            //only start spawning if activation delay is elapsed. 
            //Otherwise, just start particleeffect and don't spawn enemies yet
            if (_activationDelay > TimeSpan.Zero)
            {
                _activationDelay -= gameTime.ElapsedGameTime;
                return;
            }

            //run spawning logic
            spawn(gameTime, _spawnLocation, blackHole.Position);

            //update all enemies in wave
            bool allDestroyed = true;   //check if all enemies destroyed
            for (int i = _enemies.Length - 1; i >= 0; i--)
            {
                if (!_enemies[i].Updates)
                    continue;   //don't update units that shouldn't be updated

                allDestroyed = false;       //found one that isn't destroyed

                for (int j = i - 1; j >= 0; j--)
                {
                    //check collision against other enemies in same wave
                    _enemies[i].CheckAndApplyUnitCollision(_enemies[j]);
                }

                for (int j = 0 ; j < unicorns.Length ; j++)
                {
                    //check collision against unicorns
                    unicorns[j].CheckAndApplyCollision(_enemies[i], gameTime);
                }
                _enemies[i].CheckAndApplyUnitCollision(player);
                _enemies[i].CheckAndApplyWeaponCollision(player, gameTime.ElapsedGameTime);

                _enemies[i].Update(gameTime, player.Position, Vector2.Zero, _levelBounds);
                blackHole.ApplyToUnit(_enemies[i], gameTime);
                weapon1.CheckAndApplyCollision(_enemies[i], gameTime.ElapsedGameTime);
                weapon2.CheckAndApplyCollision(_enemies[i], gameTime.ElapsedGameTime);
                inventory.CheckCollisions(gameTime, _enemies[i]);
            }
            //stay active unless it is not a trickle wave and all enemies are destroyed
            Active = Active && (_isTrickleWave || !allDestroyed);
        }

        public void CheckAndApplyCollisions(Wave otherWave)
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (!_enemies[i].Collides)
                    continue;   //don't check enemies that shouldn't collide

                for (int j = 0; j < otherWave._enemies.Length; j++)
                {
                    //check collision against other enemies 
                    _enemies[i].CheckAndApplyUnitCollision(otherWave._enemies[j]);
                }
            }
        }

        private void setPosition(Vector2 blackHolePosition, int levelWidth, int levelHeight)
        {   //set bounds on new spawn location
            int minX, maxX, minY, maxY;

            //spawn in bounds -- default for burst wave
            minX = 0;
            maxX = levelWidth;
            minY = 0;
            maxY = levelHeight;

            if (_isTrickleWave)     //spawn out of bounds
            { 
                switch (XnaHelper.RandomInt(0,3))
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
            }

            XnaHelper.RandomizeVector(ref _spawnLocation, minX, maxX, minY, maxY);

            //if spawned too close to black hole, try again
            if ((_spawnLocation - blackHolePosition).Length() < MIN_BLACKHOLE_SPAWN_DISTANCE)
                setPosition(blackHolePosition, _levelBounds.Width, _levelBounds.Height);
        }

        public void Draw(SpriteBatch sb)
        {
            if (_portalEffect != null)
                _portalEffect.Draw(sb);

            foreach (Enemy e in _enemies)
            {
                e.Draw(sb);
            }
        }
        #endregion
    }
}
