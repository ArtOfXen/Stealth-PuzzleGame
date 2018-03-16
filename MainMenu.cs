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
    class MainMenu
    {
        List<UIButton> buttons;
        UIButton startButton;
        UIButton helpButton;
        UIButton levelPackButton;
        UIButton exitButton;
        GraphicsDevice graphicsDevice;

        Texture2D texture;
        Texture2D hoveredTexture;

        MouseState lastMouseState;

        public MainMenu(GraphicsDevice currentGraphicsDevice, Texture2D buttonTexture, Texture2D hoveredButtonTexture)
        {
            graphicsDevice = currentGraphicsDevice;

            texture = buttonTexture;
            hoveredTexture = hoveredButtonTexture;

            startButton = new UIButton(texture, new Vector2(graphicsDevice.Viewport.Width * (1f / 5f), graphicsDevice.Viewport.Height * (2f / 3f)), new Vector2(5f, 5f), true, "Start", Game1.buttonText);
            helpButton = new UIButton(texture, new Vector2(graphicsDevice.Viewport.Width * (2f / 5f), graphicsDevice.Viewport.Height * (2f / 3f)), new Vector2(5f, 5f), true, "Help", Game1.buttonText);
            levelPackButton = new UIButton(texture, new Vector2(graphicsDevice.Viewport.Width * (3f / 5f), graphicsDevice.Viewport.Height * (2f / 3f)), new Vector2(5f, 5f), true, " Level\nPacks", Game1.buttonText);
            exitButton = new UIButton(texture, new Vector2(graphicsDevice.Viewport.Width * (4f / 5f), graphicsDevice.Viewport.Height * (2f / 3f)), new Vector2(5f, 5f), true, "Exit", Game1.buttonText);
            
            buttons = new List<UIButton>();
            buttons.Add(startButton);
            buttons.Add(helpButton);
            buttons.Add(levelPackButton);
            buttons.Add(exitButton);

            lastMouseState = Mouse.GetState();
        }

        public GameState update(MouseState mouse)
        {
            Vector2 mousePos = mouse.Position.ToVector2();
            foreach(UIButton b in buttons)
            {
                if (b.isMouseOnButton(mousePos.X, mousePos.Y))
                {
                    if (mouse.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
                    {
                        if (b.Equals(startButton))
                        {
                            return GameState.levelSelectScreen;
                        }
                        else if (b.Equals(helpButton))
                        {
                            return GameState.helpScreen;
                        }
                        else if (b.Equals(levelPackButton))
                        {
                            return GameState.levelPackScreen;
                        }
                        else if (b.Equals(exitButton))
                        {
                            return GameState.exitgame;
                        }
                    }

                    b.setSprite(hoveredTexture);
                }
                else
                {
                    b.setSprite(texture);
                }
            }

            lastMouseState = mouse;
            return GameState.mainMenu;
        }

        public void drawMainMenu(GameTime gameTime, SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.SandyBrown);
            

            startButton.draw(spriteBatch);
            spriteBatch.DrawString(startButton.buttonFont, startButton.buttonText, new Vector2(startButton.getPosition().X - (startButton.textSize.X / 2), startButton.getPosition().Y + startButton.getHeight() * (1f/5f)), Color.White);
            
            helpButton.draw(spriteBatch);
            spriteBatch.DrawString(helpButton.buttonFont, helpButton.buttonText, new Vector2(helpButton.getPosition().X - (helpButton.textSize.X / 2), helpButton.getPosition().Y + helpButton.getHeight() * (1f/5f)), Color.White);

            levelPackButton.draw(spriteBatch);
            spriteBatch.DrawString(levelPackButton.buttonFont, levelPackButton.buttonText, new Vector2(levelPackButton.getPosition().X - (levelPackButton.textSize.X / 2), levelPackButton.getPosition().Y + levelPackButton.getHeight() * (1f / 6f)), Color.White);

            exitButton.draw(spriteBatch);
            spriteBatch.DrawString(exitButton.buttonFont, exitButton.buttonText, new Vector2(exitButton.getPosition().X - (exitButton.textSize.X / 2), exitButton.getPosition().Y + exitButton.getHeight() * (1f / 5f)), Color.White);

        }
    }
}
