using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.equipment;
using SpaceGame.utility;

namespace SpaceGame.units
{
    /// <summary>
    /// A wave of enemies
    /// </summary>
    class Wave
    {
        #region classes
        public class WaveData
        {
            public EnemyData[] Enemies;
        }

        public class EnemyData
        {
            public Vector2 Position;
            public string Name;
        }
        #endregion

        #region fields
        int _numEnemies;        //total number of enemies in wave
        int _spawnedSoFar;      //number of enemies already spawned
        Enemy[] _enemies;
        //enemy spawning info
        TimeSpan _tillNextSpawn;    //how long till spawning a new enemy
        TimeSpan _spawnInterval;    //how long between enemy spawns
        int _enemiesToSpawn;        //how many enemies to spawn this update cycle
        bool _isTrickleWave;        //constant trickle of enemies through level
        #endregion

        #region properties
        //set when every enemy in mob has been spawned
        //does not apply to trickle waves
        public bool WaveComplete { get; private set; }
        #endregion

        #region contructor
        public Wave(Enemy[] enemies, TimeSpan spawnInterval, bool trickleWave)
        {
            _enemies = enemies;
            _numEnemies = enemies.Length;
            _spawnedSoFar = 0;
            _tillNextSpawn = spawnInterval;
            _spawnInterval = spawnInterval;
        }
        #endregion

        #region methods
        public void Spawn(GameTime gameTime, Vector2 position)
        {
            _tillNextSpawn -= gameTime.ElapsedGameTime;
            if (_tillNextSpawn <= TimeSpan.Zero && !WaveComplete)
            {
                Enemy enemy = _enemies[_spawnedSoFar % _numEnemies];
                if (enemy.UnitLifeState == PhysicalUnit.LifeState.Dormant ||
                    enemy.UnitLifeState == PhysicalUnit.LifeState.Destroyed)
                {
                    _enemies[_spawnedSoFar % _numEnemies].Respawn(position);
                    _tillNextSpawn = _spawnInterval;
                    //if its not a trickle wave and all enemies have been spawned, the wave is complete
                    WaveComplete = (!_isTrickleWave && _spawnedSoFar == _numEnemies);
                }
                //if slot not ready to be respawned, cycle through slots each update
                _spawnedSoFar++;    
            }
        }

        public void UpdateEnemies(GameTime gameTime, Spaceman player, 
            BlackHole blackHole, Weapon weapon1, Weapon weapon2)
        { 
            bool allDestroyed = true;
            for (int i = _enemies.Length - 1; i >= 0; i--)
            {
                if (_enemies[i].UnitLifeState == PhysicalUnit.LifeState.Destroyed)
                    continue;   //don't update destroyed units

                allDestroyed = false;
                for (int j = i - 1; j >= 0; j--)
                {
                    //check collision against other enemies 
                    _enemies[i].CheckAndApplyUnitCollision(_enemies[j]);
                }
                _enemies[i].CheckAndApplyUnitCollision(player);

                _enemies[i].Update(gameTime, player.Position, Vector2.Zero);
                blackHole.ApplyToUnit(_enemies[i], gameTime);
                weapon1.CheckAndApplyCollision(_enemies[i]);
                weapon2.CheckAndApplyCollision(_enemies[i]);
            }
            WaveComplete = allDestroyed;
        }

        public void DrawEnemies(SpriteBatch sb)
        {
            foreach (Enemy e in _enemies)
            {
                e.Draw(sb);
            }
        }
        #endregion
    }
}
