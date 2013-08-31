using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SpaceGame.utility
{
    public class InputManager
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
        Vector2 _cameraOffset;  //use to calculate absolute mouse position
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
        public bool UseItem { get { return keyTapped(Keys.Q); } }
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
            get 
            { 
                return new Vector2(currentMouseState.X + _cameraOffset.X, currentMouseState.Y + _cameraOffset.Y); 
            }
        }
        public bool Exit
        {
            get { return currentKeyboardState.IsKeyDown(Keys.Escape); }
        }
        //Change Item Request
        public int SelectItemNum
        {
            get
            {
                if (keyTapped(Keys.NumPad1) || keyTapped(Keys.D1))
                {
                    return 1;
                }
                else if (keyTapped(Keys.NumPad2) || keyTapped(Keys.D2))
                {
                    return 2;
                }
                else if (keyTapped(Keys.NumPad3) || keyTapped(Keys.D3))
                {
                    return 3;
                }
                else if (keyTapped(Keys.NumPad4) || keyTapped(Keys.D4))
                {
                    return 4;
                }
                else if (keyTapped(Keys.NumPad5) || keyTapped(Keys.D5))
                {
                    return 5;
                }
                else if (keyTapped(Keys.NumPad6) || keyTapped(Keys.D6))
                {
                    return 6;
                }
                else
                {
                    return -1;
                }
            }
        }
                         

        public bool fCycle
        {
            get { return keyTapped(Keys.Q); }
        }

        public bool bCycle
        {
            get { return keyTapped(Keys.E); }
        }
        /// <summary>
        /// return true if debug key (B) is pressed
        /// </summary>
        public bool DebugKey    
        {
            get { return currentKeyboardState.IsKeyDown(Keys.B); }
        }
        #endregion
        #endregion

        #region methods
        public InputManager()
        { }

        public void SetCameraOffset(Vector2 offset)
        {
            _cameraOffset = offset;
        }

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
