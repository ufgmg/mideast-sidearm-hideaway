using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using SpaceGame.utility;
using SpaceGame.equipment;

namespace SpaceGame.utility
{
    class WeaponManager
    {
        #region members
        //There are 6 weapon slots
        private Weapon[] slots = new Weapon[6];
        private Weapon primary;
        private Weapon secondary;
        #endregion

        #region properties
        //Set weapon slot, slots can be from 1-6
        public void setSlot(int slot, Weapon passed)
        {
            slots[slot - 1] = passed;
        }

        //Get Primary weapon
        public Weapon getPrimary()
        {
            //Used when first starting
            if (primary == null)
            {
                primary = slots[0];
            }
            return primary;
        }

        //Get secondary weapon
        public Weapon getSecondary()
        {
            //Use when first starting
            if (secondary == null)
            {
                secondary = slots[3];
            }
            return secondary;
        }

        //See if slots is empty
        public bool isEmpty()
        {
            if (slots.Length == 0)
            {
                return true;
            }
            return false;
        }
        #endregion;
        #region methods
        public WeaponManager()
        { 
        }

        //Runs often
        public void Update(InputManager input)
        {
            handleSwap(input);
        }

        private void handleSwap(InputManager input)
        {
            //check for Primary Swap
            if(input.Weapon1)
            {
                //Change Primary to slot 0
                primary = slots[0];
            }
            else if (input.Weapon2)
            {
                //Change Primary to slot 1
                primary = slots[1];
            }
            else if (input.Weapon3)
            {
                //Change Primary to slot 2
                primary = slots[2];
            }

            //Check for Secondary Swap
            if (input.Weapon4)
            {
                //Change Secondary to slot 3
                secondary = slots[3];
            }
            else if (input.Weapon5)
            {
                //Change Primary to slot 4
                secondary = slots[4];
            }
            else if (input.Weapon6)
            {
                //Change Primary to slot 5
                secondary = slots[5];
            }
            
        }
        #endregion
    }
}
        