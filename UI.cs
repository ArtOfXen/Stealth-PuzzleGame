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

        public UI (Texture2D newSprite, Vector2 newPosition)
        {
            sprite = newSprite;
            position = newPosition;
            scale = 3f;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, position, null, Color.White, 0f, new Vector2(sprite.Width / 2, sprite.Height / 2), scale, SpriteEffects.None, 0.5f);
        }

        public void setSprite(Texture2D newSprite)
        {
            sprite = newSprite;
        }
    }
}
