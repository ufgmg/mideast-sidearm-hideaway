using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using SpaceGame.units;
using SpaceGame.graphics;

namespace SpaceGame.equipment
{
    /// <summary>
    /// Gadget the player can equip and turn on to create special effects
    /// consumes energy while turned on
    /// </summary>
    class Gadget
    {
        #region classes
        public class GadgetData
        {
            public float MaxEnergy;
            public string ParticleEffectName;
        }

        #endregion

        #region fields
        //While active, a gadget consumes 1 energy/millisecond
        public float Energy { get; private set; }
        public float MaxEnergy { get; private set; }
        public bool Active { get; private set; }
        ParticleEffect _activeParticleEffect;
        #endregion

        #region constructor
        public Gadget(GadgetData data)
        {
            MaxEnergy = data.MaxEnergy;
            Energy = MaxEnergy;
            if (data.ParticleEffectName != null)
                _activeParticleEffect = new ParticleEffect(data.ParticleEffectName);
        }
        #endregion

        #region methods
        public virtual void Trigger()
        {
            Active = !Active;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Active)
            {
                Energy -= (float)gameTime.TotalGameTime.Seconds;
            }
            if (Energy <= 0)
            {
                Active = false;
            }
            if (_activeParticleEffect != null)
            {
                _activeParticleEffect.Update(gameTime);
            }
        }
        #endregion
    }
}
