using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using SpaceGame.utility;
using SpaceGame.equipment;
using Microsoft.Xna.Framework.Graphics;
using SpaceGame.units;

namespace SpaceGame.utility
{
    class InventoryManager
    {
        #region members
        //There are 6 Item slots
        private IConsumable[] _slots = new IConsumable[6];
        private IConsumable _item;
        private Weapon primaryWeapon;
        private Weapon secondaryWeapon;
        private Gadget primaryGadget;
        private Gadget secondaryGadget;
        private int _currentSlot;
        #endregion

        #region properties
        public IConsumable CurrentItem
        {
            get 
            {
                _item = _item ?? _slots[0];
                return _item; 
            }
        }

        //Set Item slot, slots can be from 1-6
        public void setSlot(int slot, IConsumable passed) 
        {
            _slots[slot - 1] = passed;
        }

        //Get secondary Item
        public IConsumable getItem(int slot)
        {
            //Use when first starting
            if (_item == null)
            {
                _item = _slots[slot];
                _currentSlot = slot;
            }
            return _item;
        }

        //See if slots is empty
        public bool isEmpty()
        {
            if (_slots.Length == 0)
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
        public void Update(GameTime gameTime, InputManager input)
        {
            handleSwap(input);
            foreach (IConsumable item in _slots)
            {
                if (item is ProjectileWeapon)
                {
                    (item as ProjectileWeapon).Update(gameTime);
                }
            }
        }

        public void CheckCollisions(GameTime gameTime, PhysicalUnit unit)
        {
            foreach (IConsumable item in _slots)
            {
                if (item is ProjectileWeapon)
                {
                    (item as ProjectileWeapon).CheckAndApplyCollision(unit, gameTime.ElapsedGameTime);
                }
            }
        }

        private void handleSwap(InputManager input)
        {
            int slotSelected = input.SelectItemNum;
            if (slotSelected >= 0)
            {
                _item = _slots[slotSelected];
                _currentSlot = slotSelected;
            }
            else if (input.fCycle)
            {
                //Cycle items forward
                if (_currentSlot == 5)
                {
                    _currentSlot = 0;
                    _item = _slots[0];
                }
                _currentSlot = _currentSlot + 1;
                _item = _slots[_currentSlot];
            }
            else if (input.bCycle)
            {
                //Cycle items backwards
                if (_currentSlot == 0)
                {
                    _currentSlot = 5;
                    _item = _slots[5];
                }
                _currentSlot = _currentSlot + 1;
                _item = _slots[_currentSlot];
            }
            
        }
        #endregion
    }
}
        