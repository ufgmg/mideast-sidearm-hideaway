using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.equipment
{
    class ProjectileWeaponData
    {
        public string Name;
        public string ProjectileSpriteName;
        public float FireRate;  //rounds/second
        public int MaxAmmo;
        public int AmmoConsumption;
        public int ProjectilesPerFire;
        public int Damage;
        public float ProjectileForce;
        public float Recoil;
        public float ProjectileSpeed;
        public float ProjectileAcceleration;
        public float ProjectileSpread;  //spread of fired projectiles in degrees
        public bool DissipateOnHit;
        public int MaxProjectiles;
        public TimeSpan ProjectileLife;

        public float SplashRadius;  //in pixels
        public int SplashDamage;  //damage per second while splashing
        public float SplashForce;

        public string FireParticleEffect;
        public string MovementParticleEffect;
        public string SplashParticleEffect;
    }
}
