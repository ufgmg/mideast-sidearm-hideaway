using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;

using SpaceGame.units;
using SpaceGame.graphics;
using SpaceGame.states;

namespace SpaceGame.equipment
{
    class GadgetData
    {
        public string Name;
        public bool Immediate;
        public float EnergyConsumption;
        public string Action;
    }

    /// <summary>
    /// Gadget the player can equip and turn on to create special effects
    /// consumes energy while turned on
    /// </summary>
    class Gadget
    {
        #region const
        const float c_maxGadgetEnergy = 100.0f;
        #endregion

        #region static
        public static Dictionary<string, GadgetData> GadgetDataDict;
        #endregion

        #region properties
        public float Energy
        {
            get { return _energy; }
            set { _energy = MathHelper.Clamp(value, 0.0f, c_maxGadgetEnergy); }
        }
        public bool Active { get; private set; }
        #endregion

        #region fields
        public string Name;
        float _energy, _energyConsumption;
        //if immediate, triggering causes an instant effect
        //otherwise, triggering turns a persistent effect on/off
        bool _immediate;
        GadgetAction _gadgetAction;
        #endregion

        #region constructor
        public Gadget(string name, Level level)
            : this(GadgetDataDict[name], level)
        { }

        protected Gadget(GadgetData data, Level level)
        {
            _energy = c_maxGadgetEnergy;
            _energyConsumption = data.EnergyConsumption;
            _gadgetAction = Delegate.CreateDelegate(typeof(GadgetAction), level, data.Action) as GadgetAction;
            _immediate = data.Immediate;
        }

        #endregion

        #region methods
        public virtual void Trigger()
        {
            if (_immediate && _energy >= _energyConsumption)
            {
                _gadgetAction(true);
                _energy -= _energyConsumption;
            }
            else if (!_immediate && _energy > 0)
            {
                Active = !Active;
                _gadgetAction(Active);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Active)
            {
                Energy -= _energyConsumption * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (Energy <= 0 && Active == true)
            {
                Active = false;
                _gadgetAction(false);
            }
        }
        #endregion
    }
}
