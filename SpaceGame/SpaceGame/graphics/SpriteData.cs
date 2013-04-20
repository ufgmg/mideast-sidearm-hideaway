using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.graphics
{
    //use to initialize a sprite
    class SpriteData
    {
        public string Name;
        public string AssetName;
        public int FrameWidth;
        public int FrameHeight;
        public int NumFrames;
        public int NumStates;
        public float DefaultScale;
        public float SecondsPerAnimation;
        public float ZLayer;
    }
}
