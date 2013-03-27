using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using SpaceGame.utility;

namespace SpaceGame.states
{
    abstract class Gamestate
    {
        #region properties
        //request to exit state (pop off state stack)
        public bool PopState { get; protected set; }
        //request to push a new state onto the stack
        public Gamestate PushState { get; protected set; }
        //request to replace state with another state
        public Gamestate ReplaceState { get; protected set; }
        //if true, the state below on the stack should also be drawn
        public bool Transparent { get; protected set; }
        #endregion

        #region constructor
        public Gamestate(bool transparent)
        {
            Transparent = transparent;
        }
        #endregion

        #region methods
        public abstract void Update(GameTime gameTime, InputManager input, InventoryManager im);
        public abstract void Draw(SpriteBatch spriteBatch);
        
        #endregion
    }
}
