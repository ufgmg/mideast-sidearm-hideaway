using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceGame.equipment;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceGame.ui
{
    public delegate void ClickDelegate(Merchandise item);

    public class Merchandise : UIElement, IClickable
    {
        const string c_merchandiseTexturePrefix = "merchandise/";
        const int c_weaponPrice = 0;
        const int c_gadgetPrice = 0;
        const int c_consumablePrice = 0;

        public enum ItemType
        {
            Weapon,
            Gadget,
            Consumable
        }

        public string Name { get; private set; }
        public ItemType Category { get; private set; }
        public int Value { get; private set; }
        bool _clickable;
        Rectangle _clickArea;
        ClickDelegate onClick;

        public Merchandise(string name, Vector2 position, ItemType category, ClickDelegate clickDelegate)
            :base(c_merchandiseTexturePrefix + name, position)
        {
            Name = name;
            Category = category;
            Value = 0;
            onClick = clickDelegate;
            _clickable = true;
            _clickArea = new Rectangle((int)position.X, (int)position.Y, _width, _height);
        }

        public void HandleClick(Vector2 mousepos, bool leftClick)
        {
            if (_clickable && _clickArea.Contains((int)mousepos.X, (int)mousepos.Y))
            {
                onClick(this);
                _clickable = false;
            }
        }
    }
}
