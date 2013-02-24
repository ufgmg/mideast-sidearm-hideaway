using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using SpaceGame.utility;

namespace SpaceGame.units
{
    public class EnemyData
    {
        public string Name;
        public string MeleeWeaponName;
        public PhysicalData PhysicalData;
    }

    class Enemy : PhysicalUnit
    {
        public static Dictionary<string, EnemyData> EnemyDataDict;

        public Enemy(string unitName)
            :this(EnemyDataDict[unitName])
        {
        }

        protected Enemy(EnemyData data)
            : base(data.PhysicalData)
        { 
        }

        public virtual void Update(GameTime gameTime, Vector2 playerPosition, Vector2 blackHolePosition)
        {
            Vector2 directionToPlayer = XnaHelper.DirectionBetween(Position, playerPosition);
            MoveDirection = directionToPlayer;
            LookDirection = directionToPlayer;
            base.Update(gameTime);
        }
    }
}
