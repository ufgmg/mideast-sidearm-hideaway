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
        #region fields
        #region members
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;
        MouseState previousMouseState;
        MouseState currentMouseState;
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
        }
        #endregion
    }
}
