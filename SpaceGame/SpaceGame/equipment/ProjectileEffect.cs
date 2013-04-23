using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.units;

namespace SpaceGame.equipment
{
    struct StatEffect
    {
        public float Fire, Cryo, Shock;
        public StatEffect(float fire, float cryo, float shock)
        {
            Fire = fire;
            Cryo = cryo;
            Shock = shock;
        }

        public static StatEffect operator + (StatEffect s1, StatEffect s2)
        {
            return new StatEffect(s1.Fire + s2.Fire,
                                  s1.Cryo + s2.Cryo,
                                  s1.Shock + s2.Shock);
        }

        public static StatEffect operator - (StatEffect s1, StatEffect s2)
        {
            return new StatEffect(s1.Fire - s2.Fire,
                                  s1.Cryo - s2.Cryo,
                                  s1.Shock - s2.Shock);
        }

        public static StatEffect operator * (StatEffect s, float f)
        {
            return new StatEffect(s.Fire * f,
                                  s.Cryo * f,
                                  s.Shock * f);
        }

        public void Clamp(float min, float max)
        {
            Fire = MathHelper.Clamp(Fire, min, max);
            Cryo = MathHelper.Clamp(Cryo, min, max);
            Shock = MathHelper.Clamp(Shock, min, max);
        }
    }

    class ProjectileEffectData
    {
        public int Radius;
        public float Damage;
        public string ParticleEffectName;
        public float Force;
        public float Duration;
        public float FireEffect;
        public float ShockEffect;
        public float CryoEffect;
    }

    class ProjectileEffect
    {
        #region static
        public static Vector2 tempVec;
        /// <summary>
        /// Effect that does nothing
        /// </summary>
        public static ProjectileEffect NullEffect =
            new ProjectileEffect
            {
                _force = 0,
                _damage = 0,
                _particleEffect = null,
                _statEffects = new StatEffect(),
                _radius = 0,
                Duration = TimeSpan.Zero
            };
        #endregion

        #region properties
        public TimeSpan Duration { get; private set; }
        #endregion

        #region fields
        int _radius;
        float _damage;
        ParticleEffect _particleEffect;
        float _force;
        StatEffect _statEffects;
        #endregion

        public ProjectileEffect(ProjectileEffectData data)
        {
            _radius = data.Radius;
            _damage = data.Damage;
            _particleEffect = data.ParticleEffectName == null ?
                null : new ParticleEffect(data.ParticleEffectName);
            _force = data.Force;
            _statEffects = new StatEffect(data.FireEffect, data.CryoEffect, data.ShockEffect);
            Duration = TimeSpan.FromSeconds(data.Duration);
        }

        /// <summary>
        /// Null effect
        /// </summary>
        private ProjectileEffect()
        { }

        #region methods
        /// <summary>
        /// Check if target unit is in range. If so, apply force and damage
        /// </summary>
        /// <param name="effectPos">origin of effect</param>
        /// <param name="target">target unit</param>
        public void TryApply(Vector2 effectPos, PhysicalUnit target, TimeSpan time)
        {
            if (utility.XnaHelper.RectangleIntersectsCircle(target.HitRect, effectPos, _radius))
            {
                tempVec.X = target.Position.X - effectPos.X;
                tempVec.Y = target.Position.Y - effectPos.Y;
                Vector2.Normalize(tempVec);
                float factor = Duration == TimeSpan.Zero ? 1 : (float)time.TotalSeconds / (float)Duration.TotalSeconds;
                target.ApplyForce(_force * factor * tempVec);
                target.ApplyDamage((_damage * factor));
                target.ApplyStatus(_statEffects);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_particleEffect != null)
                _particleEffect.Update(gameTime);
        }

        public void SpawnParticles(TimeSpan time, Vector2 pos, float angle, Vector2 sourceVelocity)
        {
            if (_particleEffect != null)
                _particleEffect.Spawn(pos, angle, time, sourceVelocity);
        }

        public void Draw(SpriteBatch sb)
        {
            if (_particleEffect != null)
                _particleEffect.Draw(sb);
        }
        #endregion

    }
}
