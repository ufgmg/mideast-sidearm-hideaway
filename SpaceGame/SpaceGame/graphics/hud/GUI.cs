using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SpaceGame.graphics.hud
{
    class GUI
    {
        //Ugly constants for figuring things out about the health and void wheels
        public static float ARC_ADJUST = 2.38f;
        public static float VOID_ARC_ADJUST = 1.45f;
        public static float RADIUS_ADJUST = 1.974f;
        public static float HEIGHT_ADJUST = 1.146f;

        //Static textures to draw to the screen
        public static Texture2D targetWheel;
        public static Texture2D leftClick;
        public static Texture2D rightClick;
        public static Texture2D spaceClick;
        public static Texture2D shiftClick;
        public static Texture2D button1;
        public static Texture2D button3;
        public static Texture2D button5;
        public static Texture2D button2;
        public static Texture2D button4;
        public static Texture2D button6;
        public static Texture2D voidWheel;

        //Screen dimensions
        private int screenWidth;
        private int screenHeight;

        //Rectangles for static GUI elements
        private Rectangle targetWheelRec;
        private Rectangle leftClickRec;
        private Rectangle rightClickRec;
        private Rectangle spaceClickRec;
        private Rectangle shiftClickRec;
        private Rectangle button1Rec;
        private Rectangle button3Rec;
        private Rectangle button5Rec;
        private Rectangle button2Rec;
        private Rectangle button4Rec;
        private Rectangle button6Rec;
        private Rectangle voidWheelRec;

        //Radial bars for health and void
        private RadialBar healthBar;
        private RadialBar voidBar;

        //Player and blackhole objects - to get the health and void numbers
        private SpaceGame.units.Spaceman player;
        private SpaceGame.units.BlackHole blackhole;

        public GUI(SpaceGame.units.Spaceman player, SpaceGame.units.BlackHole blackhole)
        {
            this.screenHeight = Game1.SCREENHEIGHT;
            this.screenWidth = Game1.SCREENWIDTH;

            targetWheelRec.X = (screenWidth / 2) - 32;
            targetWheelRec.Y = screenHeight - targetWheel.Height;
            targetWheelRec.Width = targetWheel.Width;
            targetWheelRec.Height = targetWheel.Height;

            leftClickRec.X = (screenWidth / 2) - 99;
            leftClickRec.Y = screenHeight - leftClick.Height;
            leftClickRec.Width = leftClick.Width;
            leftClickRec.Height = leftClick.Height;

            rightClickRec.X = (screenWidth / 2) + 148;
            rightClickRec.Y = screenHeight - rightClick.Height;
            rightClickRec.Width = rightClick.Width;
            rightClickRec.Height = rightClick.Height;

            spaceClickRec.X = (screenWidth / 2) + 256;
            spaceClickRec.Y = screenHeight - spaceClick.Height;
            spaceClickRec.Width = spaceClick.Width;
            spaceClickRec.Height = spaceClick.Height;

            shiftClickRec.X = (screenWidth / 2) - 340;
            shiftClickRec.Y = screenHeight - shiftClick.Height;
            shiftClickRec.Width = shiftClick.Width;
            shiftClickRec.Height = shiftClick.Height;

            button1Rec.X = (screenWidth / 2) - 412;
            button1Rec.Y = screenHeight - button1.Height;
            button1Rec.Width = button1.Width;
            button1Rec.Height = button1.Height;

            button3Rec.X = button1Rec.X;
            button3Rec.Y = button1Rec.Y - button1.Height - 5;
            button3Rec.Width = button3.Width;
            button3Rec.Height = button3.Height;

            button5Rec.X = button1Rec.X;
            button5Rec.Y = button1Rec.Y - 2*button1.Height - 2*5;
            button5Rec.Width = button5.Width;
            button5Rec.Height = button5.Height;

            button2Rec.X = (screenWidth / 2) - 484;
            button2Rec.Y = screenHeight - button2.Height;
            button2Rec.Width = button2.Width;
            button2Rec.Height = button2.Height;

            button4Rec.X = button2Rec.X;
            button4Rec.Y = button2Rec.Y - button2.Height - 5;
            button4Rec.Width = button4.Width;
            button4Rec.Height = button4.Height;

            button6Rec.X = button2Rec.X;
            button6Rec.Y = button2Rec.Y - 2 * button2.Height - 2 * 5;
            button6Rec.Width = button6.Width;
            button6Rec.Height = button6.Height;

            voidWheelRec.X = targetWheelRec.X + targetWheel.Width / 2 - voidWheel.Width / 2;
            voidWheelRec.Y = 0;
            voidWheelRec.Width = voidWheel.Width;
            voidWheelRec.Height = voidWheel.Height;

            this.player = player;
            this.blackhole = blackhole;
            
            //Initialize health bar in its location
            Vector2 healthBarLoc = new Vector2(targetWheelRec.X + targetWheelRec.Width / 2, (int)(targetWheelRec.Y + HEIGHT_ADJUST * targetWheelRec.Height));
            healthBar = new RadialBar(healthBarLoc, targetWheelRec.Width / RADIUS_ADJUST, 25, -(float)Math.PI / ARC_ADJUST, (float)Math.PI / ARC_ADJUST, Color.Red);

            //Initialize void bar in its location
            Vector2 voidBarLoc = new Vector2(voidWheelRec.X + voidWheelRec.Width / 2, (int)(voidWheelRec.Y + voidWheelRec.Height - 10));
            voidBar = new RadialBar(voidBarLoc, voidWheelRec.Width / RADIUS_ADJUST, 25, (float)(2 * Math.PI - (float)Math.PI / VOID_ARC_ADJUST), (float)Math.PI / VOID_ARC_ADJUST, Color.Purple);
        }

        public void draw(SpriteBatch batch)
        {
            //Draw health and void bars first so they are under the wheels
            healthBar.Draw(batch, player.health, player.maxHealth);
            voidBar.Draw(batch, blackhole.capacityUsed, blackhole.totalCapacity);

            batch.Draw(targetWheel, targetWheelRec, Color.White);
            batch.Draw(leftClick, leftClickRec, Color.White);
            batch.Draw(rightClick, rightClickRec, Color.White);
            batch.Draw(spaceClick, spaceClickRec, Color.White);
            batch.Draw(shiftClick, shiftClickRec, Color.White);
            batch.Draw(button1, button1Rec, Color.White);
            batch.Draw(button3, button3Rec, Color.White);
            batch.Draw(button5, button5Rec, Color.White);
            batch.Draw(button2, button2Rec, Color.White);
            batch.Draw(button4, button4Rec, Color.White);
            batch.Draw(button6, button6Rec, Color.White);
            batch.Draw(voidWheel, voidWheelRec, Color.White);
        }
    }
}
