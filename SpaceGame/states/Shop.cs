using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using SpaceGame.equipment;
using SpaceGame.utility;
using SpaceGame.ui;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceGame.states
{
    class Shop : Gamestate
    {
        InventoryManager _inventoryManager;
        Merchandise[] _merchandise;
        Texture2D _background;

        public Shop(ContentManager content, InventoryManager im)
            :base(content, false)
        {
            _inventoryManager = im;
            _background = Content.Load<Texture2D>("gui/Shop_GUI");
        }

        public override void Update(GameTime gameTime, InputManager input, InventoryManager im)
        {
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_background, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}
