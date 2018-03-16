using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class UI
    {
        protected Texture2D sprite;
        protected Vector2 position;
        protected Vector2 scale;
        protected bool active;

        public UI (Texture2D newSprite, Vector2 newPosition, Vector2 newScale, bool initiallyActive)
        {
            sprite = newSprite;
            position = newPosition;
            scale = newScale;
            active = initiallyActive;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (active)
            {
                spriteBatch.Draw(sprite, position, null, Color.White, 0f, new Vector2(sprite.Width / 2, sprite.Height / 2), scale, SpriteEffects.None, 0.5f);
            }
        }

        public void setSprite(Texture2D newSprite)
        {
            sprite = newSprite;
        }

        public void setActive(bool isActive)
        {
            active = isActive;
        }

        public bool isActive()
        {
            return active;
        }

        public bool currentlyUsesSprite(Texture2D comparisonSprite)
        {
            if (comparisonSprite == null || sprite == null)
            {
                if (comparisonSprite == null && sprite == null)
                { return true; }
                else
                { return false; }
            }
            if (sprite.Equals(comparisonSprite))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public float getWidth()
        {
            return (sprite.Width * scale.X);
        }

        public float getHeight()
        {
            return (sprite.Height * scale.Y);
        }
    }
}
