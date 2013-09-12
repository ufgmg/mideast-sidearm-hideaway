using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpaceGame.ui
{
    interface IClickable
    {
        public abstract void HandleClick(Vector2 mousepos, bool leftClick);
    }
}
