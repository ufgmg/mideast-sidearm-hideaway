using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.utility;
using SpaceGame.equipment;

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
        #region static
        public static Dictionary<string, EnemyData> EnemyDataDict;
        #endregion

        #region fields
        MeleeWeapon _meleeWeapon;
        #endregion

        #region constructor
        public Enemy(string unitName, Rectangle levelBounds)
            :this(EnemyDataDict[unitName], levelBounds)
        {
        }


        protected Enemy(EnemyData data, Rectangle levelBounds)
            : base(data.PhysicalData)
        {
            if (data.MeleeWeaponName != null)
                _meleeWeapon = new MeleeWeapon(data.MeleeWeaponName, this);
        }
        #endregion

        #region methods
        public virtual void Update(GameTime gameTime, Vector2 playerPosition, Vector2 blackHolePosition, Rectangle levelBounds)
        {
            Vector2 directionToPlayer = XnaHelper.DirectionBetween(Position, playerPosition);
            MoveDirection = directionToPlayer;
            LookDirection = directionToPlayer;
            if (_meleeWeapon != null)
            {
                _meleeWeapon.Update(gameTime);

                if ((playerPosition - Position).Length() <= _meleeWeapon.Range && _lifeState == LifeState.Living)
                    _meleeWeapon.Trigger(Center, playerPosition);
            }
            base.Update(gameTime, levelBounds);
        }

        public void CheckAndApplyWeaponCollision(PhysicalUnit unit, TimeSpan time)
        {
            if (_meleeWeapon != null)
                _meleeWeapon.CheckAndApplyCollision(unit, time);
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            _meleeWeapon.Draw(sb);
        }
        #endregion
    }
}
