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
    public class ParticleGeneratorData
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
        /// angle(degrees) offset from spawn angle (only used in generators that are part of an effect)
        /// </summary>
        public float Offset;
        /// <summary>
        /// Average lifespan of particle
        /// </summary>
        public float ParticleLife;
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
        /// Represented as a string containing 4 comma-separated values
        /// </summary>
        public string StartColor, EndColor;
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
        public string UniqueParticle;
    }

    public class ParticleGenerator
    {
        #region constant
        public const string PARTICLE_TEXTURE_DIRECTORY = "particles/";
        #endregion

        #region static
        //stores all data for particle effects
        public static Dictionary<string, ParticleGeneratorData> Data;

        public static ContentManager Content;
        static Texture2D particleTexture;
        //default texture to draw particles with. Hardcoded single pixel assigned in Game.LoadContent
        public static Texture2D ParticleTexture
        {
            get { return particleTexture; }
            set
            { 
                particleTexture = value;
            }
        }

        static Random rand = new Random();
        #endregion

        #region fields
        //range of angles through which particles can be spawned, in degrees
        float _arc;
        //angle offset for spawning
        float _offset;
        //speed with which particles are spawned, and random variance factor
        float _particleSpeed, _speedVariance;
        //fraction of speed that is reduced each second
        float _particleDecelerationFactor;
        //time particle exists
        TimeSpan _particleLife;
        //percent variance in life of particles
        float _particleLifeVariance;
        //how many particles to spawn per second 
        int _spawnRate;
        //time till spawning another particle
        TimeSpan _tillNextParticleSpawn;
        //starting scale of particles, percent variance in starting scale, and increase in scale per second
        float _particleScale, _scaleVariance, _scaleRate;
        //rotation, in radians per second
        float _particleRotationSpeed;
        //starting color, andchange in color per second, represented as 4-vectors
        Color _startColor, _endColor;
        List<Particle> _particles;

        Vector2 _textureCenter;
        Texture2D _particleTexture;

        ParticleGeneratorData _particleEffectData;

        float _speedFactor;
        #endregion

        #region properties
        public bool Reversed { get; set; }
        public float IntensityFactor 
        {
            get { return _speedFactor; }
            set
            {
                _speedFactor = value;
                _particleSpeed = _particleEffectData.Speed * IntensityFactor;
                //_particleLife = TimeSpan.FromSeconds((float)_particleEffectData.ParticleLife.TotalSeconds / IntensityFactor);
                _scaleRate =
                    (((_particleEffectData.EndScale - _particleEffectData.StartScale) / _particleTexture.Width)
                    / ((float)_particleLife.TotalSeconds));
                _spawnRate = (int)((float)_particleEffectData.SpawnRate * IntensityFactor);
            }
        }
        #endregion

        class Particle
        {
            public Vector2 Position, Velocity;
            public float Scale, Angle;     //size and rotation(radians)
            public TimeSpan LifeTime, TimeAlive;        //How many seconds the particle should exist and has existed
        }

        /// <summary>
        /// Create a new particle effect, based on parameters stored in ParticleEffectData.xml
        /// </summary>
        /// <param name="effectKey">string identifier used to fetch parameters. Must match Name attribute in XML</param>
        public ParticleGenerator(string effectKey)
            :this(Data[effectKey])
        {
        }

        public ParticleGenerator(ParticleGeneratorData data)
        {
            _particleEffectData = data;
            _particleSpeed = _particleEffectData.Speed;
            _speedVariance = _particleEffectData.SpeedVariance;
            _particleDecelerationFactor = _particleEffectData.DecelerationFactor;

            _particleTexture = (_particleEffectData.UniqueParticle == null) ? 
                particleTexture : Content.Load<Texture2D>(PARTICLE_TEXTURE_DIRECTORY + _particleEffectData.UniqueParticle);
            _textureCenter = new Vector2(_particleTexture.Width / 2.0f, particleTexture.Height / 2.0f); 

            _particleScale = _particleEffectData.StartScale / _particleTexture.Width;

            _particleLife = TimeSpan.FromSeconds(_particleEffectData.ParticleLife);
            _particleLifeVariance = _particleEffectData.ParticleLifeVariance;

            _scaleRate = 
                (((_particleEffectData.EndScale - _particleEffectData.StartScale) / _particleTexture.Width) 
                / ((float)_particleLife.TotalSeconds));
            _scaleVariance = _particleEffectData.ScaleVariance;
            _arc = _particleEffectData.SpawnArc;
            _offset = _particleEffectData.Offset;
            _particleRotationSpeed = MathHelper.ToRadians(
                _particleEffectData.ParticleRotation / (float)_particleLife.TotalSeconds);

            byte[] scv = _particleEffectData.StartColor.Split(',')
                .Select(n => Convert.ToByte(n)).ToArray();
            byte[] ecv = _particleEffectData.EndColor.Split(',')
                .Select(n => Convert.ToByte(n)).ToArray();

            if (_particleEffectData.Reversed)
            {
                _startColor = new Color(ecv[1], ecv[2], ecv[3], ecv[0]); 
                _endColor = new Color(scv[1], scv[2], scv[3], scv[0]);
                Reversed = true;
            }
            else
            {
                _startColor = new Color(scv[1], scv[2], scv[3], scv[0]); 
                _endColor = new Color(ecv[1], ecv[2], ecv[3], ecv[0]);
                Reversed = false;
            }

            _spawnRate = _particleEffectData.SpawnRate;
            _tillNextParticleSpawn = TimeSpan.FromSeconds(1.0f / (float)_spawnRate);
            _particles = new List<Particle>();
            IntensityFactor = 1.0f;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = _particles.Count - 1 ; i >= 0 ; i--)
            {
                Particle particle = _particles[i];
                if (particle.LifeTime < particle.TimeAlive)
                    _particles.RemoveAt(i);
                else
                {
                    //reduce life
                    particle.TimeAlive += gameTime.ElapsedGameTime;
                    //move particle
                    particle.Position += particle.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //scale down velocity
                    particle.Velocity -= particle.Velocity * _particleDecelerationFactor * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (Reversed)
                    {
                        //rotate particle
                        particle.Angle -= _particleRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        //adjust scale
                        particle.Scale -= _scaleRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        //rotate particle
                        particle.Angle += _particleRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        //adjust scale
                        particle.Scale += _scaleRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
            }
        }

        private Particle newParticle(Vector2 pos, float angle, Vector2 sourceVelocity)
        {
            Particle particle = new Particle();
            particle.Position = pos;
            float directionAngle = (float)MathHelper.ToRadians((XnaHelper.RandomAngle(angle, _arc)));
            float speed = applyVariance(_particleSpeed, _speedVariance);
            particle.Velocity = speed * XnaHelper.VectorFromAngle(directionAngle);
            particle.Scale = applyVariance(_particleScale, _scaleVariance);
            particle.Angle = angle;
            particle.LifeTime = TimeSpan.FromSeconds(applyVariance((float)_particleLife.TotalSeconds, _particleLifeVariance));

            if (Reversed)
            {
                float secondsAlive = (float)particle.LifeTime.TotalSeconds;
                //start at the end
                particle.Position = particle.Position + particle.Velocity * secondsAlive; 
                //comment above and uncomment below for a cool effect (unintentional side effect while working on particles.
                //not sure why it looks so awesome, but it does)
                //particle.Position = particle.Position + particle.Velocity * secondsAlive * (1 - _particleDecelerationFactor);

                //movce in reverse
                particle.Velocity = Vector2.Negate(particle.Velocity);
                //start at end scale
                particle.Scale = _particleScale + _scaleRate * secondsAlive;
                //start at end rotation
                particle.Angle = _particleRotationSpeed * secondsAlive;
            }

            particle.Velocity += sourceVelocity;
            return particle;
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
            //fractional number of particles to spawn
            float particlesToSpawn = multiplier * (float)(_spawnRate * (float)time.TotalSeconds);            
            //spawn integer number of particles
            for(int i = 0 ; i < (int)particlesToSpawn ; i++)
            {
                _particles.Add(newParticle(position, angle + _offset, sourceVelocity));
            }
            //now deal with fractional part
            _tillNextParticleSpawn -= TimeSpan.FromSeconds((double)(particlesToSpawn - (int)particlesToSpawn));
            if (_tillNextParticleSpawn < TimeSpan.Zero)
            {
                _particles.Add(newParticle(position, angle + _offset, sourceVelocity));
                _tillNextParticleSpawn = TimeSpan.FromSeconds(1.0f / (float)_spawnRate);
            }
        }

        private float applyVariance(float baseFloat, float variance)
        {
            return baseFloat + baseFloat * variance * (1.0f - 2 * (float)rand.NextDouble());
        }

        public void Draw(SpriteBatch sb)
        {

            foreach (Particle p in _particles)
            {
                Color drawColor;
                if (Reversed)
                    drawColor = Color.Lerp(_endColor, _startColor, (float)p.TimeAlive.TotalSeconds / (float)p.LifeTime.TotalSeconds);
                else
                    drawColor = Color.Lerp(_startColor, _endColor, (float)p.TimeAlive.TotalSeconds / (float)p.LifeTime.TotalSeconds);

                sb.Draw(_particleTexture, p.Position, null, drawColor, p.Angle, _textureCenter, p.Scale, SpriteEffects.None, 0 );
            }
        }

        public void Draw(SpriteBatch sb, Vector2 origin)
        {

            foreach (Particle p in _particles)
            {
                Color drawColor;
                if (Reversed)
                    drawColor = Color.Lerp(_endColor, _startColor, (float)p.TimeAlive.TotalSeconds / (float)p.LifeTime.TotalSeconds);
                else
                    drawColor = Color.Lerp(_startColor, _endColor, (float)p.TimeAlive.TotalSeconds / (float)p.LifeTime.TotalSeconds);

                sb.Draw(_particleTexture, p.Position, null, drawColor, p.Angle, origin - p.Position + _textureCenter, p.Scale, SpriteEffects.None, 0 );
            }
        }

    }
}