using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SpaceGame.utility
{
    class InputManager
    {
        #region constants
        const int WHEEL_UNITS_PER_SCROLL = 10;
        #endregion
        #region fields
        #region members
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;
        MouseState previousMouseState;
        MouseState currentMouseState;
        //current scrolls toward next scroll event
        //- for scroll down, + for scroll up
        int _scrollCounter;
        bool _scrollUp, _scrollDown;
        #endregion

        #region properties

        //Input Requests
        public bool MoveLeft
        {
            get { return currentKeyboardState.IsKeyDown(Keys.A); }
        }
        public bool MoveRight 
        { 
            get {return currentKeyboardState.IsKeyDown(Keys.D);} 
        }
        public bool MoveDown 
        { 
            get {return currentKeyboardState.IsKeyDown(Keys.S);}
        }
        public bool MoveUp 
        { 
            get {return currentKeyboardState.IsKeyDown(Keys.W);}
        }

        public bool ScrollUp { get { return _scrollUp; } }
        public bool ScrollDown { get { return _scrollDown; } }

        /// <summary>
        /// Request to move selector left (use for menus)
        /// </summary>
        public bool SelectLeft
        {
            get { return keyTapped(Keys.A) || keyTapped(Keys.Left)
                || keyTapped(Keys.H); }
        }
        /// <summary>
        /// Request to move selector right (use for menus)
        /// </summary>
        public bool SelectRight 
        { 
            get { return keyTapped(Keys.D) || keyTapped(Keys.Right)
                || keyTapped(Keys.L); }
        }
        /// <summary>
        /// Request to move selector down (use for menus)
        /// </summary>
        public bool SelectDown 
        { 
            get { return keyTapped(Keys.S) || keyTapped(Keys.Down)
                || keyTapped(Keys.J); }
        }
        /// <summary>
        /// Request to move selector up (use for menus)
        /// </summary>
        public bool SelectUp 
        { 
            get { return keyTapped(Keys.W) || keyTapped(Keys.Up)
                || keyTapped(Keys.K); }
        }

        /// <summary>
        /// Confirmation button pressed (use for menus)
        /// </summary>
        public bool Confirm 
        {
            get { return keyTapped(Keys.Enter) || keyTapped(Keys.Space)
                || keyTapped(Keys.I); }
        }
        /// <summary>
        /// Cancellation/back button pressed (use for menus)
        /// </summary>
        public bool Cancel 
        {
            get { return keyTapped(Keys.Escape) || keyTapped(Keys.Back); }
        }
        /// <summary>
        /// Get requested direction based on movement keys (normalized)
        /// </summary>
        public Vector2 MoveDirection
        {
            get 
            {
                Vector2 direction = Vector2.Zero;

                if (MoveDown)
                    direction.Y = 1;
                else if (MoveUp)
                    direction.Y = -1;
                if (MoveRight)
                    direction.X = 1;
                else if (MoveLeft)
                    direction.X = -1;

                if (direction.Length() > 0)
                    direction.Normalize();
                return direction;
            }
        }
        public bool FirePrimary 
        {
            get { return currentMouseState.LeftButton == ButtonState.Pressed; }
        }
        public bool FireSecondary
        {
            get { return currentMouseState.RightButton == ButtonState.Pressed; }
        }
        public bool TogglePrimary { get { return keyTapped(Keys.Q); } }
        public bool ToggleSecondary { get { return keyTapped(Keys.E); } }
        public bool TriggerGadget1
        { 
            get {return (currentKeyboardState.IsKeyDown(Keys.LeftShift)
                            && previousKeyboardState.IsKeyUp(Keys.LeftShift));}
        }
        public bool TriggerGadget2 
        { 
            get {return (currentKeyboardState.IsKeyDown(Keys.Space)
                            && previousKeyboardState.IsKeyUp(Keys.Space));}
        }
        public Vector2 MouseLocation
        {
            get { return new Vector2(currentMouseState.X, currentMouseState.Y); }
        }
        public bool Exit
        {
            get { return currentKeyboardState.IsKeyDown(Keys.Escape); }
        }
        //Change Weapon Request
        public bool Weapon1
        {
            get { return currentKeyboardState.IsKeyDown(Keys.NumPad1) ||
                         currentKeyboardState.IsKeyDown(Keys.D1);}
        }
        public bool Weapon2
        {
            get { return currentKeyboardState.IsKeyDown(Keys.NumPad2)||
                         currentKeyboardState.IsKeyDown(Keys.D2);}
        }
        public bool Weapon3
        {
            get { return currentKeyboardState.IsKeyDown(Keys.NumPad3)||
                         currentKeyboardState.IsKeyDown(Keys.D3);}
        }
        public bool Weapon4
        {
            get { return currentKeyboardState.IsKeyDown(Keys.NumPad4)||
                         currentKeyboardState.IsKeyDown(Keys.D4);}
        }
        public bool Weapon5
        {
            get { return currentKeyboardState.IsKeyDown(Keys.NumPad5)||
                         currentKeyboardState.IsKeyDown(Keys.D5);}
        }
        public bool Weapon6
        {
            get { return currentKeyboardState.IsKeyDown(Keys.NumPad6)||
                         currentKeyboardState.IsKeyDown(Keys.D6);}
        }

        //the magical all-purpose dubugging key. Who knows what surprises it holds?
        public bool DebugKey    
        {
            get { return currentKeyboardState.IsKeyDown(Keys.B); }
        }
        #endregion
        #endregion

        #region methods
        public InputManager()
        { }

        public void Update()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            _scrollCounter += (currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue);
            _scrollDown = false;
            _scrollUp = false;
            if (_scrollCounter > WHEEL_UNITS_PER_SCROLL)
            {
                _scrollUp = true;
                _scrollCounter -= WHEEL_UNITS_PER_SCROLL;
            }
            else if (_scrollCounter < -WHEEL_UNITS_PER_SCROLL)
            {
                _scrollDown = true;
                _scrollCounter += WHEEL_UNITS_PER_SCROLL;
            }
        }

        private bool keyTapped(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key)
                && previousKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Return the integer of the numkey between 1 and 6 pressed
        /// Return -1 if no numkey pressed
        /// </summary>
        /// <returns></returns>
        public int NumKey()
        {
            foreach (Keys key in currentKeyboardState.GetPressedKeys())
            {
                if (keyTapped(key) && Keys.NumPad0 <= key && key <= Keys.NumPad6)
                    return (int)(key - Keys.NumPad0);
                if (keyTapped(key) && Keys.D0 <= key && key <= Keys.D6)
                    return (int)(key - Keys.D0);
            }
            return -1;
        }
        #endregion
    }
}
