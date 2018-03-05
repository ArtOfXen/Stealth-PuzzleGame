using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class UIButton : UI
    {

        float buttonWidth;
        float buttonHeight;
        public SpriteFont buttonFont;
        public string buttonText;
        public Vector2 textSize;

        public UIButton(Texture2D newSprite, Vector2 newPosition, Vector2 newScale, bool initiallyActive, string text, SpriteFont font) : base(newSprite, newPosition, newScale, initiallyActive)
        {
            buttonFont = font;
            buttonText = text;
            textSize = buttonFont.MeasureString(buttonText);

            if (sprite == null)
            {
                Vector2 recalculatedScale;

                sprite = Game1.genericButtonSprite;
                recalculatedScale.X = scale.X * ((textSize.X * 1.2f)/ sprite.Width);
                recalculatedScale.Y = scale.Y * ((textSize.Y * 2)/ sprite.Height);

                if (recalculatedScale.X > scale.X)
                {
                    scale.X = recalculatedScale.X;
                }
                if (recalculatedScale.Y > scale.Y)
                {
                    scale.Y = recalculatedScale.Y;
                }
            }

            buttonWidth = sprite.Width * scale.X;
            buttonHeight = sprite.Height * scale.Y;
        }

        public bool isMouseOnButton(float mouseX, float mouseY)
        {
            float mouseMinX = position.X - (buttonWidth / 2);
            float mouseMaxX = position.X + (buttonWidth / 2);
            float mouseMinY = position.Y - (buttonHeight / 2);
            float mouseMaxY = position.Y + (buttonHeight / 2);

            if (mouseX >= mouseMinX && mouseX <= mouseMaxX && mouseY >= mouseMinY && mouseY <= mouseMaxY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
