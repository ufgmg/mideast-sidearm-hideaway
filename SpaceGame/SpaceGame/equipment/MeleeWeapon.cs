using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SpaceGame.graphics;
using SpaceGame.units;

namespace SpaceGame.equipment
{
    class MeleeWeapon : Weapon
    {
        #region static
        public struct MeleeWeaponData
        {
            public string Name;
            public float FireRate;  //attacks/second
            public int MaxAmmo;
            public int AmmoConsumption;
            public int Damage;
            public int Force;
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
        #endregion

        #region properties
        #endregion

        #region constructor
        public MeleeWeapon(string weaponName, PhysicalUnit owner)
            : this(MeleeWeaponDataDict[weaponName], owner)
        { }

        protected MeleeWeapon(MeleeWeaponData data, PhysicalUnit owner)
            :base(TimeSpan.FromSeconds(1.0 / data.FireRate), 
                  data.MaxAmmo,
                  data.AmmoConsumption,
                  owner)
        {
            _damage = data.Damage;
            _force = data.Force;
            _recoil = data.Force;
            _hitArc = data.HitArc;
            _attackParticleEffect = (data.AttackParticleEffect == null) ?
                null : new ParticleEffect(data.AttackParticleEffect);
            _hitParticleEffect = (data.HitParticleEffect == null) ?
                null : new ParticleEffect(data.HitParticleEffect);
        }
        #endregion

        #region methods
        #endregion
    }
}
