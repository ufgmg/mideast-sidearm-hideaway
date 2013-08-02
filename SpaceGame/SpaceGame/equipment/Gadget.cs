using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using SpaceGame.units;
using SpaceGame.graphics;
using SpaceGame.states;

namespace SpaceGame.equipment
{
    /// <summary>
    /// Gadget the player can equip and turn on to create special effects
    /// consumes energy while turned on
    /// </summary>
    class Gadget
    {
        #region fields
        //While active, a gadget consumes 1 energy/millisecond
        public float Energy { get; private set; }
        public float MaxEnergy { get; private set; }
        public bool Active { get; private set; }
        ParticleEffect _activeParticleEffect;

        GadgetAction _gadgetAction;
        #endregion

        #region constructor
        public Gadget(float maxEnergy, GadgetAction action, string particleEffectName)
        {
            MaxEnergy = maxEnergy;
            Energy = MaxEnergy;
            _gadgetAction = action;
            if (particleEffectName != null)
            {
                _activeParticleEffect = new ParticleEffect(particleEffectName);
            }
        }
        #endregion

        #region methods
        public virtual void Trigger()
        {
            Active = Energy > 0 ? !Active : false;
            _gadgetAction();
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
