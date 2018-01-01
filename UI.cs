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
        Texture2D sprite;
        Vector2 position;
        float scale;
        bool active;

        public UI (Texture2D newSprite, Vector2 newPosition, float newScale, bool initiallyActive)
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
    }
}
