using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using SpaceGame.utility;

namespace SpaceGame.graphics
{
    public class ParticleEffectData
    {
        public string Name;
        public ParticleGeneratorData[] ParticleGenerators;
    }

    public class ParticleEffect
    {
        #region static
        //stores all data for particle effects
        public static Dictionary<string, ParticleEffectData> Data;
        #endregion

        #region fields
        float _intensityFactor;
        bool _reversed;
        ParticleGenerator[] _generators;
        #endregion

        #region properties
        public bool Reversed 
        {
            get { return _reversed; }
            set
            {
                _reversed = value;
                foreach (ParticleGenerator gen in _generators)
                    gen.Reversed = value;
            }
        }
        public float IntensityFactor 
        {
            get { return _intensityFactor; }
            set
            {
                _intensityFactor = value;
                for (int i = 0; i < _generators.Length; i++)
                {
                    _generators[i].IntensityFactor = value;
                }
            }
        }
        #endregion


        /// <summary>
        /// Create a new particle effect, based on parameters stored in ParticleEffectData.xml
        /// </summary>
        /// <param name="effectKey">string identifier used to fetch parameters. Must match Name attribute in XML</param>
        public ParticleEffect(string effectKey)
        {
            if (Data.ContainsKey(effectKey))
            {
                ParticleEffectData data = Data[effectKey];
                _generators = new ParticleGenerator[data.ParticleGenerators.Length];
                for (int i = 0; i < _generators.Length; i++)
                {
                    _generators[i] = new ParticleGenerator(data.ParticleGenerators[i]);
                }
            }
            else
            {
                _generators = new ParticleGenerator[1];
                _generators[0] = new ParticleGenerator(effectKey);
            }

            IntensityFactor = 1.0f;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < _generators.Length; i++)
            {
                _generators[i].Update(gameTime);
            }
        }

        /// <summary>
        /// Spawn new particles
        /// </summary>
        /// <param name="position">Location at which to spawn particles</param>
        /// <param name="angle">direction at which to spawn particles (degrees)</param>
        /// <param name="sourceVeloctiy">Velocity of particle source, added to all particles</param>
        /// <param name="time"></param>
        public void Spawn(Vector2 position, float angle, TimeSpan time, Vector2 sourceVelocity)
        {
            Spawn(position, angle, time, sourceVelocity, 1.0f);
        }

        /// <summary>
        /// Spawn new particles
        /// </summary>
        /// <param name="position">Location at which to spawn particles</param>
        /// <param name="angle">direction at which to spawn particles (degrees)</param>
        /// <param name="sourceVeloctiy">Velocity of particle source, added to all particles</param>
        /// <param name="time"></param>
        /// <param name="multiplier">Multiplier to apply to default spawn rate </param>
        public void Spawn(Vector2 position, float angle, TimeSpan time, Vector2 sourceVelocity, float multiplier)
        {
            foreach (ParticleGenerator gen in _generators)
            {
                gen.Spawn(position, angle, time, sourceVelocity, multiplier);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (ParticleGenerator gen in _generators)
            {
                gen.Draw(sb);
            }
        }

        public void Draw(SpriteBatch sb, Vector2 origin)
        {
            foreach (ParticleGenerator gen in _generators)
            {
                gen.Draw(sb, origin);
            }
        }

    }
}
