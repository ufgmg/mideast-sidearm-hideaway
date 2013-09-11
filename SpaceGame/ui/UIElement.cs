using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceGame.ui
{
    class UIElement
    {
		protected const string c_uiTexturePath = "gui/";
		public static ContentManager s_Content;
        protected Texture2D _texture;
		protected Vector2 _position;

		public UIElement (string textureName, Vector2 position)
		{
			_texture = s_Content.Load<Texture2D>(c_uiTexturePath + textureName);
			_position = position;
		}

		public virtual void Draw(SpriteBatch sb)
		{
			sb.Draw (_texture, _position, null, Color.White);
		}
    }
}
