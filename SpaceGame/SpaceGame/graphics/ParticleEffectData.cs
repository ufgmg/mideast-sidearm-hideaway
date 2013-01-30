using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.graphics
{
    //for storing parameters to construct a particle effect retrieved from XML
    public class ParticleEffectData
    {
        /// <summary>
        /// Key to identify effect data
        /// </summary>
        public string Name;
        /// <summary>
        ///base particle speed, percent variance in speed, and percent reduction in speed per second
        /// </summary>
        public float Speed, SpeedVariance, DecelerationFactor;
        /// <summary>
        ///base particle scale, percent variance in scale, and scale at end of average particle life
        /// </summary>
        public float StartScale, ScaleVariance, EndScale;
        /// <summary>
        /// angle(degrees) through which effect spawns particles
        /// </summary>
        public float SpawnArc;
        /// <summary>
        /// Average lifespan of particle
        /// </summary>
        public TimeSpan ParticleLife;
        /// <summary>
        /// percent variance in particle life
        /// </summary>
        public float ParticleLifeVariance;
        /// <summary>
        /// angle (degrees) through which to rotate particle during average life 
        /// </summary>
        public float ParticleRotation;
        /// <summary>
        /// Colors through which particles transition during their life
        /// </summary>
        public Color StartColor, EndColor;
        /// <summary>
        /// If reversed, particles start at endpoint and move towards spawn point
        /// </summary>
        public bool Reversed;
        /// <summary>
        /// number of particles to spawn per second
        /// </summary>
        public int SpawnRate;

        /// <summary>
        /// Reference to unique particle texture stored in Content/particles
        /// If ommitted, uses blank pixel
        /// </summary>
        public Texture2D UniqueParticle;
    }
}
