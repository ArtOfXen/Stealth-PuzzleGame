using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game1
{
    class LevelSelect
    {
        static GraphicsDevice graphicsDevice;

        static List<Level> levelPack;

        static List<UIButton> buttons;
        UIButton returnButton;
        
        MouseState lastMouseState;

        public LevelSelect(GraphicsDevice currentGraphicsDevice)
        {
            graphicsDevice = currentGraphicsDevice;

            changeLevelPack();

            returnButton = new UIButton(null, new Vector2(50f, 50f), Vector2.One, true, "X", Game1.buttonText);

            lastMouseState = Mouse.GetState();
        }

        public GameState update(MouseState mouse)
        {
            Vector2 mousePos = mouse.Position.ToVector2();

            if (returnButton.isMouseOnButton(mousePos.X, mousePos.Y) && mouse.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
            {
                return GameState.mainMenu;
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].isMouseOnButton(mousePos.X, mousePos.Y) && mouse.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
                {
                    if (levelPack[i].unlocked)
                    {
                        Game1.setCurrentLevel(i, levelPack.Count);
                        return GameState.gameInProgress;
                    }
                }
            }

            lastMouseState = mouse;

            return GameState.levelSelectScreen;
        }

        public void drawLevelSelect(GameTime gameTime, SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.SandyBrown);

            foreach(UIButton b in buttons)
            {
                b.draw(spriteBatch);
                spriteBatch.DrawString(b.buttonFont, b.buttonText, new Vector2(b.getPosition().X - (b.textSize.X / 2), b.getPosition().Y - (b.textSize.Y / 2)), Color.White);
            }

            returnButton.draw(spriteBatch);
            spriteBatch.DrawString(returnButton.buttonFont, returnButton.buttonText, new Vector2(returnButton.getPosition().X - (returnButton.textSize.X / 2), returnButton.getPosition().Y - (returnButton.textSize.Y / 2)), Color.Red);
        }

        public static void changeLevelPack()
        {
            levelPack = Game1.currentLevelPack;
            updateButtons();
        }

        private static void updateButtons()
        {
            buttons = new List<UIButton>();
            string buttonText;

            for (int i = 0; i < levelPack.Count(); i++)
            {
                if (levelPack[i].unlocked)
                {
                    buttonText = levelPack[i].name;
                }
                else
                {
                    buttonText = "- Locked -";
                }
                buttons.Add(new UIButton(null, new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height * ((float)(i + 1) / (levelPack.Count() + 1))), Vector2.One, true, buttonText, Game1.buttonText));
            }
        }
    }
}
