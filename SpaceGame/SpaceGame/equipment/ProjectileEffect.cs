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
    class ProjectileEffectData
    {
        public int Radius;
        public int Damage;
        public string ParticleEffectName;
        public float Force;
    }

    class ProjectileEffect
    {
        public static Vector2 tempVec;

        public int Radius;
        public int Damage;
        public ParticleEffect ParticleEffect;
        public float Force;
        public ProjectileEffect(ProjectileEffectData data)
        {
            Radius = data.Radius;
            Damage = data.Damage;
            ParticleEffect = new ParticleEffect(data.ParticleEffectName);
            Force = data.Force;
        }

        /// <summary>
        /// Check if target unit is in range. If so, apply force and damage
        /// </summary>
        /// <param name="effectPos">origin of effect</param>
        /// <param name="target">target unit</param>
        public void TryApply(Vector2 effectPos, PhysicalUnit target)
        {
            tempVec.X = target.Position.X - effectPos.X;
            tempVec.Y = target.Position.Y - effectPos.Y;
            if (tempVec.Length() < Radius)
            {
                Vector2.Normalize(tempVec);
                target.ApplyForce(Force * tempVec);
                target.ApplyDamage(Damage);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (ParticleEffect != null)
                ParticleEffect.Update(gameTime);
        }

        public void SpawnParticles(TimeSpan time, Vector2 pos)
        {
            if (ParticleEffect != null)
                ParticleEffect.Spawn(pos, 0.0f, time, Vector2.Zero);
        }

        public void Draw(SpriteBatch sb)
        {
            if (ParticleEffect != null)
                ParticleEffect.Draw(sb);
        }

    }
}
