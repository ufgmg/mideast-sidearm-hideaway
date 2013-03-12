using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace SpaceGame.utility
{
    class Camera
    {
        int cameraWidth;
        int cameraHeight;
        Rectangle cameraRectangle; 

        public Camera(int width, int height)
        {
            cameraWidth = width;
            cameraHeight = height;
            cameraRectangle = new Rectangle(0, 0, cameraWidth, cameraHeight);
        }

        private void adjustCamera()
        {
        }
    }
}
