using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utility;

namespace SpaceGame.equipment
{
    class MeleeWeapon : Weapon
    {
        #region static
        public class MeleeWeaponData
        {
            public string Name;
            public float FireRate;  //attacks/second
            public int Damage;
            public int Impact;
            public float Range;
            public float Recoil;
            public float HitArc;

            public string AttackParticleEffect;
            public string HitParticleEffect;
        }
        public static Dictionary<string, MeleeWeaponData> MeleeWeaponDataDict;
        #endregion

        #region fields
        int _damage;
        int _force;
        float _range;
        float _recoil;
        float _hitArc;  //in radians
        ParticleEffect _attackParticleEffect;
        ParticleEffect _hitParticleEffect;
        Vector2 _tempVector;
        #endregion

        #region properties
        public float Range { get { return _range; } }
        #endregion

        #region constructor
        public MeleeWeapon(string weaponName, PhysicalUnit owner)
            : this(MeleeWeaponDataDict[weaponName], owner)
        { }

        protected MeleeWeapon(MeleeWeaponData data, PhysicalUnit owner)
            :base(TimeSpan.FromSeconds(1.0 / data.FireRate), owner)
        {
            _damage = data.Damage;
            _force = data.Impact;
            _recoil = data.Recoil;
            _hitArc = data.HitArc;
            _range = data.Range;
            _attackParticleEffect = (data.AttackParticleEffect == null) ?
                null : new ParticleEffect(data.AttackParticleEffect);
            _hitParticleEffect = (data.HitParticleEffect == null) ?
                null : new ParticleEffect(data.HitParticleEffect);
        }
        #endregion

        #region methods
        public override void CheckAndApplyCollision(PhysicalUnit unit, TimeSpan time)
        {
            if (!_firing || !unit.Collides)
                return;     //don't check collisions if not firing

            float fireAngle = XnaHelper.RadiansFromVector(_fireDirection);
            if (XnaHelper.RectangleIntersectsArc(unit.HitRect, _owner.Center, _range, fireAngle, _hitArc))
            { 
                _tempVector = unit.Center - _owner.Center;
                _tempVector.Normalize();
                unit.ApplyImpact(_force * _tempVector, 1);
                unit.ApplyDamage(_damage);
            }
        }
        protected override void UpdateWeapon(GameTime gameTime)
        {
            if (_firing)
            {
                _attackParticleEffect.Spawn(_owner.Center, XnaHelper.DegreesFromVector(_fireDirection),
                    gameTime.ElapsedGameTime, _owner.Velocity);
                //recoil
                _owner.ApplyImpact(-_recoil * _fireDirection, 1);
            }

            _attackParticleEffect.Update(gameTime);
        }
        public override void Draw(SpriteBatch sb)
        {
            _attackParticleEffect.Draw(sb);
        }
        #endregion
    }
}
