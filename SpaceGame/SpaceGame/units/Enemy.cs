using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using SpaceGame.utility;

namespace SpaceGame.units
{
    class Enemy : PhysicalUnit
    {
        public Enemy(string unitName)
            :base(unitName)
        {
        }

        public Enemy(string unitName, Vector2 startPosition)
            :base(unitName, startPosition)
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
