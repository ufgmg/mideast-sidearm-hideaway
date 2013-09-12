using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SpaceGame.ui
{
    public class UIElement
    {
		protected const string c_uiTexturePath = "gui/";
		public static ContentManager s_Content;
        protected Texture2D _texture;
		protected Vector2 _position;
        protected int _width, _height;
        protected Vector2 _center;

		public UIElement (string textureName, Vector2 position)
		{
			_texture = s_Content.Load<Texture2D>(c_uiTexturePath + textureName);
			_position = position;
            _width = _texture.Width;
            _height = _texture.Height;
            _center = new Vector2(_width / 2, _height / 2);
		}

		public virtual void Draw(SpriteBatch sb)
		{
            sb.Draw(_texture, _position, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
		}
    }
}
