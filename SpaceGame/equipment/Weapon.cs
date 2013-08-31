using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utility;

namespace SpaceGame.equipment
{
    abstract class Weapon
    {
        #region static
        #endregion

        #region properties
        #endregion

        #region fields
        //minimum time between shots and till next shot
        TimeSpan _fireDelay, _tillNextFire;

        //whether the weapon is firing, and if so, what direction
        //set during Weapon.Trigger
        //check and apply during weapon.Update()
        protected bool _firing;
        protected Vector2 _fireDirection;
        protected Vector2 _targetDestination;

        protected PhysicalUnit _owner;

        #endregion

        #region constructor
        /// <summary>
        /// Create a new weapon
        /// </summary>
        /// <param name="fireDelay">Time between successive shots</param>
        /// <param name="maxAmmo">Total ammo capacity. Set to 1 for infinite ammo.</param>
        /// <param name="ammoConsumption">Ammo used per shot. Set to 0 for infinite ammo.</param>
        /// <param name="levelWidth">Width of the level in which this weapon is instantiated.</param>
        /// <param name="levelHeight">Height of the level in which this weapon is instantiated.</param>
        public Weapon(TimeSpan fireDelay, PhysicalUnit owner)
        {
            _fireDelay = fireDelay;
            _owner = owner;
        }
        #endregion

        #region concrete methods
        /// <summary>
        /// Attempt to fire a weapon. Return true if successfull
        /// Only fires if enough time has passed since the last fire 
        /// </summary>
        /// <param name="firePosition"></param>
        /// <param name="targetPosition"></param>
        public bool Trigger(Vector2 firePosition, Vector2 targetPosition)
        {
            if (_tillNextFire.TotalSeconds <= 0)
            {
                _firing = true;
                _fireDirection = XnaHelper.DirectionBetween(firePosition, targetPosition);
                _targetDestination = targetPosition;

                _tillNextFire = _fireDelay;
                return true;
            }
            return false;
        }

        public void Update(GameTime gameTime)
        {
            _tillNextFire -= gameTime.ElapsedGameTime;
            UpdateWeapon(gameTime);
            _firing = false;
        }
        #endregion

        #region abstract methods
        /// <summary>
        /// Check if weapon is hitting a target, and apply its affects if so
        /// Call during the update loop on each unit
        /// </summary>
        /// <param name="unit"></param>
        public abstract void CheckAndApplyCollision(PhysicalUnit unit, TimeSpan time);
        //update projectiles and add new projectiles if _firing
        protected abstract void UpdateWeapon(GameTime gameTime);
        public abstract void Draw(SpriteBatch sb);
        #endregion
    }
}
