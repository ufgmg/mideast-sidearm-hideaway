using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.utility;

namespace SpaceGame.states
{
    class Gamemenu : Gamestate
    {
        private List<string> MenuItems;
        private int iterator;
        public string infoText { get; set; }
        public string title { get; set; }
        public static SpriteFont spriteFont;

        public Gamemenu() : base(false)
        {
            title = "Space Game";
            MenuItems = new List<string>();
            MenuItems.Add("Play Game");
            MenuItems.Add("Settings");
            MenuItems.Add("Exit Game");
            Iterator = 0;
            infoText = string.Empty;
            
        }

        public int Iterator
        {
            get
            {
                return iterator;
            }
            set
            {
                iterator = value;
                if (iterator > MenuItems.Count - 1) iterator = MenuItems.Count - 1;
                if (iterator < 0) iterator = 0;
            }
        }

        public int GetNumberOfOptions()
        {
            return MenuItems.Count;
        }

        public string GetItem(int index)
        {
            return MenuItems[index];
        }

        private void handleInput(InputManager input, InventoryManager wm)
        {
             // Allows the game to exit
            if (input.Exit)
                this.PopState = true;

                if (input.SelectUp)
                {
                    this.Iterator--;
                }

                else if (input.SelectDown)
                {
                    this.Iterator++;
                }

                if (input.Confirm)
                {
                    if (this.Iterator == 0)
                    {
                        //load game
                        ReplaceState = new Level(1, wm);
                    }
                    else if (this.Iterator == 1)
                    {
                        //call method to load settings
                        this.PopState = true;

                    }

                    else if (this.Iterator == 2)
                    {
                        //quit game 
                        this.PopState = true;
                    }

                    this.Iterator = 0;
                }
                    //Add more select menu logic here as menu items increase

                    
                
        }

        public override void Update(GameTime gameTime, InputManager input, InventoryManager wm)
        {  
            handleInput(input, wm);
            //update logic goes here
        }

        public static void LoadContent(ContentManager contentManager)
        {
            spriteFont = contentManager.Load<SpriteFont>("MenuFont");
        }

        public void DrawMenu(SpriteBatch spriteBatch, SpriteFont mFont)
        {
            int screenWidth = 1280;
            spriteBatch.Begin();
            spriteBatch.DrawString(mFont, title, new Vector2(screenWidth / 2 - mFont.MeasureString(title).X / 2, 20), Color.White);
            int yPos = 100;
            for (int i = 0; i < GetNumberOfOptions(); i++)
            {
                Color colour = Color.White;
                if (i == Iterator)
                {
                    colour = Color.Cyan;
                }
                spriteBatch.DrawString(mFont, GetItem(i), new Vector2(screenWidth / 2 - mFont.MeasureString(GetItem(i)).X / 2, yPos), colour);
                yPos += 50;
            }
            spriteBatch.End();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawMenu(spriteBatch, spriteFont);
        }
       
        //method to set selection value to pass up to game1.cs and Game1.cs getSelection, set gamestate.
    }
}
