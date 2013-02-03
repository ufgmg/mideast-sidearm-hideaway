using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        Enemy[] _enemies;
        #endregion

        #region properties
        //set when every enemy in mob is destroyed
        public bool AllDestroyed { get; private set; }
        #endregion

        #region contructor
        public Wave(Enemy[] enemies)
        {
            _enemies = enemies;
        }
        #endregion

        #region methods
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
            AllDestroyed = allDestroyed;
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
