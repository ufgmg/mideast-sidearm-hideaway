using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.utility
{

    public class Camera2D 
    {
        public Vector2 Position;
        public float Zoom;
        public float Rotation;
        public Vector2 ScreenCenter;
        private bool UpdateMatrix;
        public float MaxZoom { get; set; }
        public float MinZoom { get; set; }
        public Rectangle Viewport { get; set; }
        public Rectangle WorldRect { get; set; }
        public Matrix Transform = Matrix.Identity;
      
        public Camera2D(Vector2 playerPosition, int levelWidth, int levelHeight)
        {

            Zoom = 1;
            Rotation = 0.0f;
            
            //TODO: change this to be relative to center on the player
            Position = new Vector2(0, 0);
           
            ScreenCenter = new Vector2(Viewport.Width / 2, Viewport.Height / 2);
            WorldRect = new Rectangle(0, 0, levelWidth, levelHeight);
            Viewport = new Rectangle((int)(Position.X), (int)(Position.Y), (int)Game1.SCREENWIDTH, (int)Game1.SCREENHEIGHT); 
        }

        

        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
  
            Position.X = playerPosition.X - (int)Game1.SCREENWIDTH / 2;
            Position.Y = playerPosition.Y - (int)Game1.SCREENHEIGHT / 2;
            UpdateMatrix = true;          

            if (Position.X < (Viewport.Left / Zoom))
                Position.X = Viewport.Left / Zoom;

            if (Position.Y < (Viewport.Top / Zoom))
                Position.Y = Viewport.Top / Zoom;

            if ((Position.X > WorldRect.Width - Viewport.Right / Zoom))
                Position.X = WorldRect.Width - Viewport.Right / Zoom;

            if (Position.Y > (WorldRect.Height - Viewport.Bottom / Zoom))
                Position.Y = WorldRect.Height - Viewport.Bottom / Zoom;

        }

        public Matrix TransformMatrix()
        {
            if (UpdateMatrix)
            {

                Transform = Matrix.CreateTranslation(new Vector3(-Position, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                    Matrix.CreateTranslation(new Vector3(ScreenCenter, 0));

                UpdateMatrix = false;
            }
            
            return Transform;
        }
       
        private Vector2 calculateNewPosition(Vector2 playerPosition)
        {
            return Position - playerPosition;
        }
    }

}

