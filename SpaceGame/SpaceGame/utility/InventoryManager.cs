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
    class InventoryManager
    {
        #region members
        //There are 6 Item slots
        private Item[] slots = new Item[6];
        private Item item;
        private Weapon primaryWeapon;
        private Weapon secondaryWeapon;
        private Gadget primaryGadget;
        private Gadget secondaryGadget;
        private int currentSlot;
        #endregion

        #region properties
        //Set Item slot, slots can be from 1-6
        public void setSlot(int slot, Item passed)
        {
            slots[slot - 1] = passed;
        }

        //Get secondary Item
        public Item getItem(int slot)
        {
            //Use when first starting
            if (item == null)
            {
                item = slots[slot];
                currentSlot = slot;
            }
            return item;
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

        //Set Primary Weapon
        public void setPrimaryWeapon(Weapon passed)
        {
            primaryWeapon = passed;
        }

        //Set Secondary Weapon
        public void setSecondaryWeapon(Weapon passed)
        {
            secondaryWeapon = passed;
        }

        //Set Primary Gadget
        public void setPrimaryGadget(Gadget passed)
        {
            primaryGadget = passed;
        }

        //Set Secondary Gadget
        public void setSecondaryGadget(Gadget passed)
        {
            secondaryGadget = passed;
        }

        //Get Primary Weapon
        public Weapon getPrimaryWeapon()
        {
            return primaryWeapon;
        }

        //Get Seconary Weapon
        public Weapon getSecondaryWeapon()
        {
            return secondaryWeapon;
        }

        //Get Primary Gadget
        public Gadget getPrimaryGadget()
        {
            return primaryGadget;
        }

        //Get Secondary Gadget
        public Gadget getSecondaryGadget()
        {
            return secondaryGadget;
        }
        #endregion;
        #region methods
        public InventoryManager()
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
            if(input.Item1)
            {
                //Change Primary to slot 0
                item = slots[0];
                currentSlot = 0;
            }
            else if (input.Item2)
            {
                //Change Primary to slot 1
                item = slots[1];
                currentSlot = 1;
            }
            else if (input.Item3)
            {
                //Change Primary to slot 2
                item = slots[2];
                currentSlot = 2;
            }

            //Check for Secondary Swap
            if (input.Item4)
            {
                //Change Secondary to slot 3
                item = slots[3];
                currentSlot = 3;
            }
            else if (input.Item5)
            {
                //Change Primary to slot 4
                item = slots[4];
                currentSlot = 4;
            }
            else if (input.Item6)
            {
                //Change Primary to slot 5
                item = slots[5];
                currentSlot = 5;
            }

            if (input.fCycle)
            {
                //Cycle items forward
                if (currentSlot == 5)
                {
                    currentSlot = 0;
                    item = slots[0];
                }
                currentSlot = currentSlot + 1;
                item = slots[currentSlot];
            }

            if (input.bCycle)
            {
                //Cycle items backwards
                if (currentSlot == 0)
                {
                    currentSlot = 5;
                    item = slots[5];
                }
                currentSlot = currentSlot + 1;
                item = slots[currentSlot];
            }
            
        }
        #endregion
    }
}
        