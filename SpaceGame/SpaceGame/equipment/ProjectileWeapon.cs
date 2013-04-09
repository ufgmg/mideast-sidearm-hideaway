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
    class ProjectileWeapon : Weapon
    {
        #region constant
        #endregion

        #region static
        public static Dictionary<string, ProjectileWeaponData> DataDict;
        #endregion

        #region properties
        #endregion

        #region fields
        string _name;
        int _projectilesPerFire;
        ProjectileData _projectileInfo;
        ParticleEffect _fireParticleEffect;
        Projectile[] _projectiles;
        #endregion

        #region constructor
        public ProjectileWeapon(string name, PhysicalUnit owner)
            : this(DataDict[name], owner)
        { }

        protected ProjectileWeapon(ProjectileWeaponData data, PhysicalUnit owner)
            :base(TimeSpan.FromSeconds(1.0 / data.FireRate), 1, 0, owner)
        {
            _name = data.Name;
            _projectilesPerFire = data.ProjectilesPerFire;
            _projectileInfo = data.ProjectileInfo;
            _fireParticleEffect = new ParticleEffect(data.FireParticleEffectName);
            float maxProjectiles = 
                data.FireRate * data.ProjectileInfo.SecondsToLive * data.ProjectilesPerFire;
            _projectiles = new Projectile[(int)maxProjectiles + 1];
            for (int i = 0; i < _projectiles.Length; i++)
            {
                _projectiles[i] = new Projectile(data.ProjectileInfo.SpriteName);
            }
        }
        #endregion

        #region methods
        public override void CheckAndApplyCollision(PhysicalUnit unit)
        {
            if (!unit.Collides)
                return;

            foreach (Projectile p in _projectiles)
            {
                p.CheckAndApplyCollision(unit);
            }
        }

        protected override void UpdateWeapon(GameTime gameTime)
        {
            int projectilesToSpawn = _firing ? _projectilesPerFire : 0;

            foreach (Projectile p in _projectiles)
            {
                p.Update(gameTime);

                if (p.ProjectileState == Projectile.State.Dormant
                    && projectilesToSpawn > 0)
                {
                    p.Initialize(_owner.Position, _fireDirection,
                        _projectileInfo, _targetDestination);
                    projectilesToSpawn--;
                }
            }

            if (_fireParticleEffect != null)
            {
                if (_firing)
                {
                    _fireParticleEffect.Spawn(
                        _owner.Position, XnaHelper.DegreesFromVector(_fireDirection),
                        gameTime.ElapsedGameTime, _owner.Velocity);
                }
                _fireParticleEffect.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            foreach (Projectile p in _projectiles)
            {
                p.Draw(sb);
            }
            if (_fireParticleEffect != null)
                _fireParticleEffect.Draw(sb);
        }
        #endregion
    }

    class ProjectileWeaponData
    {
        public string Name;
        public float FireRate;
        public int ProjectilesPerFire;
        public ProjectileData ProjectileInfo;
        public string FireParticleEffectName;
    }
}
