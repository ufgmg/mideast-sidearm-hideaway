using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SpaceGame.graphics;
using SpaceGame.utility;
using SpaceGame.units;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.equipment
{
    class ProjectileEffectData
    {
        public int Radius;
        public int Damage;
        public string ParticleEffectName;
        public float Force;
    }

    class ProjectileData
    {
        public float Speed;
        public float Acceleration;
        public float SecondsToLive;
        public string SpriteName;
        public int MaxInstances;
        public int Penetration;
        public int Mass;
        public ProjectileEffectData ContactEffect;          //effect upon hitting a unit
        public ProjectileEffectData ProximityEffect;        //effect upon moving/existing
        public ProjectileEffectData DestinationEffect;      //effect upon reaching click location
    }

    class ProjectileWeaponData
    {
        public string Name;
        public float FireRate;  //rounds/second
        public int MaxAmmo;
        public int AmmoConsumption;
        public int ProjectilesPerFire;
        public float Recoil;
        public float ProjectileSpread;  //spread of fired projectiles in degrees
        public string FireParticleEffectName;
        public ProjectileData Projectile;
    }
    

    class ProjectileWeapon : Weapon
    {
        #region static
        public static Dictionary<string, ProjectileWeaponData> ProjectileWeaponData;

        protected class Projectile
        {
            public enum ProjectileState
            {
                Dormant,
                Active,
                JustHit,
                ApplyContactEffect,
                ReachedDestination,
            }
            public ProjectileState State;
            public Vector2 Position;
            public Vector2 Velocity;
            public Vector2 Acceleration;
            public Rectangle HitRect;
            public TimeSpan LifeLeft;
            public float Angle;     //in radians
            public Sprite ProjectileSprite;
        }

            
        #endregion

        #region fields
        int _maxProjectiles;    //total number of on-screen projectiles allowable
        protected Projectile[] _projectiles;
        int _projectilesPerFire;

        float _recoilForce; 
        float _projectileSpread;
        float _projectileAcceleration;

        Rectangle _hitDetectionRect;    //width and height based on texture, x and y change
        Rectangle _splashDetectionRect;    //width and height based on _splashRadius, x and y change

        ProjectileData _projData;

        #endregion

        #region constructor
        public ProjectileWeapon(string weaponName, PhysicalUnit owner, Rectangle levelBounds)
            : this(ProjectileWeaponData[weaponName], owner, levelBounds)
        { }

        protected ProjectileWeapon(ProjectileWeaponData data, PhysicalUnit owner, Rectangle levelBounds)
            :base(TimeSpan.FromSeconds(1.0 / data.FireRate), data.MaxAmmo,
                  data.AmmoConsumption, owner, levelBounds)
        {
            _projData = data.Projectile;
            _maxProjectiles = _projData.MaxInstances;
            _projectiles = new Projectile[_maxProjectiles];
            for (int i = 0; i < _maxProjectiles; i++)
            {
                _projectiles[i] = new Projectile();
                Sprite sprite = new Sprite(_projData.SpriteName);
                _projectiles[i].ProjectileSprite = sprite;
                _projectiles[i].HitRect = new Rectangle(0,0, (int)(sprite.Width), (int)sprite.Height); 
                _projectiles[i].State = Projectile.ProjectileState.Dormant;
            }
            _recoilForce = data.Recoil;
            _projectileSpread = MathHelper.ToRadians(data.ProjectileSpread);
            _projectilesPerFire = data.ProjectilesPerFire;
        }
        #endregion

        #region methods
        public override void CheckAndApplyCollision(units.PhysicalUnit unit)
        {
            if (!unit.Collides)
                return;

            Projectile p;
            for (int i = 0 ; i < _projectiles.Length ; i++)
            {
                p = _projectiles[i];
                if (_projectiles[i].Active)
                {
                    _hitDetectionRect.X = (int)_projectiles[i].Position.X;
                    _hitDetectionRect.Y = (int)_projectiles[i].Position.Y;
                    if (XnaHelper.RectsCollide(unit.HitRect, _hitDetectionRect))
                    {
                        applyProjectileHit(_projectiles[i], unit);
                    }
                }
                if (_projectiles[i].Splashing && _projectiles[i].ReadyToSplash)
                {
                    _splashDetectionRect.X = (int)_projectiles[i].Position.X;
                    _splashDetectionRect.Y = (int)_projectiles[i].Position.Y;
                    _splashDetectionRect.Width = (int)(_splashRadius * 2);
                    _splashDetectionRect.Height = (int)(_splashRadius * 2);
                    if (XnaHelper.RectsCollide(unit.HitRect, _splashDetectionRect))
                    {
                        unit.ApplyDamage(_splashDamage);
                        unit.ApplyForce(_splashForce * XnaHelper.DirectionBetween(_projectiles[i].Position, unit.Center));
                    }

                }
            }
        }

        protected virtual void applyProjectileHit(Projectile p, PhysicalUnit unit)
        {
            unit.ApplyForce(p.Velocity / p.Velocity.Length() * _projectileForce);
            unit.ApplyDamage(_projectileDamage);
            if (_dissipateOnHit)
                p.Active = false;
            if (_splashRadius > 0)
            {
                p.ReadyToSplash = true;
                p.Velocity = Vector2.Zero;
                if (_hasProjectileSprite)
                    p.ProjectileSprite.PlayAnimation(1);
            }
        }

        protected override void UpdateWeapon(GameTime gameTime)
        {
            int projectilesToSpawn = _firing ? _projectilesPerFire : 0;
            Projectile p;
            for (int i = 0; i < _projectiles.Length; i++)
            {
                p = _projectiles[i];
                TimeSpan time = gameTime.ElapsedGameTime;

                if (p.ReadyToSplash && p.Splashing)
                    p.ReadyToSplash = false;

                p.Splashing = p.ReadyToSplash || p.Splashing;

                if (p.Active)
                {
                    //updateProjectile(p, gameTime.ElapsedGameTime);
                    if (p.Velocity.Length() != 0)
                    {
                        p.Velocity += p.Acceleration;
                    }
                    p.Position += p.Velocity * (float)time.TotalSeconds;
                    p.LifeLeft -= time;
                    if (_hasProjectileSprite)
                        p.ProjectileSprite.Update(gameTime);
                    if (_movementParticleEffect != null)
                        _movementParticleEffect.Spawn(p.Position, MathHelper.ToDegrees(MathHelper.Pi + p.Angle), gameTime.ElapsedGameTime, Vector2.Zero);
                    if (p.LifeLeft <= TimeSpan.Zero || !(XnaHelper.PointInRect(p.Position, _levelBounds)))
                        p.Active = false;
                }

                if (p.Splashing)
                {
                    p.ProjectileSprite.Update(gameTime);
                    p.Splashing = !(p.ProjectileSprite.AnimationOver);
                    p.ProjectileSprite.ScaleFactor += _splashScaleRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_splashParticleEffect != null)
                        _splashParticleEffect.Spawn(p.Position, MathHelper.ToDegrees(p.Angle), gameTime.ElapsedGameTime, Vector2.Zero);
                }
                else if (!p.Active)
                {
                    if (projectilesToSpawn > 0)
                    {
                        //initializeProjectile(p, _fireDirection);

                        p.Active = true;
                        p.Splashing = false;
                        p.ReadyToSplash = false;
                        p.Angle = XnaHelper.RadiansFromVector(_fireDirection);
                        p.Angle = XnaHelper.RandomAngle(p.Angle, _projectileSpread);
                        p.LifeLeft = _projectileLife;
                        p.Position = _owner.Center;
//                        p.Velocity = XnaHelper.VectorFromAngle(p.Angle) * _projectileSpeed + _owner.Velocity;
                        p.Velocity = _fireDirection * _projectileSpeed ;
                        p.Acceleration = _fireDirection * _projectileAcceleration;
                        projectilesToSpawn -= 1;
                        _owner.ApplyForce(-_recoilForce * _fireDirection);
                        if (_hasProjectileSprite)
                            p.ProjectileSprite.Reset();
                        if (_fireParticleEffect != null)
                            _fireParticleEffect.Spawn(p.Position, MathHelper.ToDegrees(p.Angle), gameTime.ElapsedGameTime, _owner.Velocity);
                    }
                }

            }

            if (_fireParticleEffect != null)
                _fireParticleEffect.Update(gameTime);
            if (_movementParticleEffect != null)
                _movementParticleEffect.Update(gameTime);
            if (_splashParticleEffect != null)
                _splashParticleEffect.Update(gameTime);
        }

        protected virtual void initializeProjectile(Projectile p, Vector2 fireDirection)
        {
            p.Active = true;
            p.Angle = XnaHelper.RadiansFromVector(fireDirection);
            p.LifeLeft = _projectileLife;
            p.Position = _owner.Center;
            p.Velocity = fireDirection * _projectileSpeed;
            p.ProjectileSprite.Reset();
        }

        public override void Draw(SpriteBatch sb)
        {
            foreach (Projectile p in _projectiles)
            {
                if (p.Active || p.Splashing)
                {
                    if (_hasProjectileSprite)
                        p.ProjectileSprite.Draw(sb, p.Position, p.Angle);
                }
            }
            //draw particle effects
            if (_fireParticleEffect != null)
                _fireParticleEffect.Draw(sb);
            if (_movementParticleEffect != null)
                _movementParticleEffect.Draw(sb);
            if (_splashParticleEffect != null)
                _splashParticleEffect.Draw(sb);

        }
        #endregion
    }
}
