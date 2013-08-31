using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.equipment
{
    interface IConsumable
    {
        int NumUses { get; set; }
        void Use(Vector2 target);
        void Update(GameTime gameTime);
        void Draw(SpriteBatch sb);
    }
}
