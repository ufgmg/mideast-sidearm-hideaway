using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame
{
    public class GameStateDrawMenu
    {
        private List<string> MenuItems;
        private int iterator;
        public string infoText { get; set; }
        public string title { get; set; }

        public GameStateDrawMenu()
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

        public void DrawMenu(SpriteBatch batch, int screenWidth, SpriteFont arial)
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

        public void DrawEndScreen(SpriteBatch batch, int screenWidth, SpriteFont arial)
        {
            batch.DrawString(arial, infoText, new Vector2(screenWidth / 2 - arial.MeasureString(infoText).X / 2, 300), Color.White);
            string prompt = "Press Enter to Continue";
            batch.DrawString(arial, prompt, new Vector2(screenWidth / 2 - arial.MeasureString(prompt).X / 2, 400), Color.White);
        }


    }


}
