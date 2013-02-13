using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        SpriteFont _spritefont;

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

        private void handleInput(InputManager input)
        {
            if (input.Exit)
                this.PopState = true;
            
            // Allows the game to exit
            if (input.Exit)
                this.PopState() = true;

                if (currKeyState.IsKeyDown(Keys.Up))
                {
                    if ((!oldKeyState.IsKeyDown(Keys.Up)))
                        this.Iterator--;
                }

                else if (currKeyState.IsKeyDown(Keys.Down))
                {
                    if ((!oldKeyState.IsKeyDown(Keys.Down)))
                        this.Iterator++;
                }

                if (currKeyState.IsKeyDown(Keys.Enter))
                {
                    if (this.Iterator == 0)
                    {
                        //load game
                       
                        gamestate = GameStates.Running;
                        


                    }
                    else if (this.Iterator == 1)
                    {
                        //call method to load settings 
                        
                    }

                    else if (this.Iterator == 2)
                    {
                        //quit game 
                        this.PopState = true;
                    }

                    //Add more select menu logic here as menu items increase

                    this.Iterator = 0;



        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            input.Update();

            handleInput(input);

            input.Update();
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            batch.DrawString(arial, title, new Vector2(screenWidth / 2 - arial.MeasureString(title).X / 2, 20), Color.White);
            int yPos = 100;
            for (int i = 0; i < GetNumberOfOptions(); i++)
            {
                Color colour = Color.White;
                if (i == Iterator)
                {
                    colour = Color.Cyan;
                }
                batch.DrawString(arial, GetItem(i), new Vector2(screenWidth / 2 - arial.MeasureString(GetItem(i)).X / 2, yPos), colour);
                yPos += 50;
            }
        }
    }
}
