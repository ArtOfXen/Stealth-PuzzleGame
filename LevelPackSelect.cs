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
    class LevelPackSelect
    {
        string infoText;
        Rectangle infoBox;

        string keyboardInputText;
        Rectangle keyboardInputArea;

        GraphicsDevice graphicsDevice;

        UIButton acceptButton;
        UIButton cancelButton;

        MouseState lastMouseState;
        Keys[] lastKeyboardState;

        public LevelPackSelect(GraphicsDevice currentGraphicsDevice)
        {
            graphicsDevice = currentGraphicsDevice;
            infoBox = new Rectangle(new Vector2(graphicsDevice.Viewport.Width * (1f / 3), graphicsDevice.Viewport.Height * (3f / 10)).ToPoint(), new Vector2(graphicsDevice.Viewport.Width * (1f / 3), graphicsDevice.Viewport.Height * (2f / 5)).ToPoint());
            infoText = Game1.calculateLineBreaks("To load a new level pack, place it in the 'LevelPacks' folder within the game files then enter the name here excluding the .txt extension (file names are not case sensitive but should only contain letters; no symbols, spaces, or numbers can be used). To load the default level pack, enter 'Default'. If the default level pack is still shown after clicking 'start' then it is likely that you have entered an incorrect file name", infoBox.Width, Game1.buttonText);

            keyboardInputText = "DEFAULT";
            keyboardInputArea = new Rectangle(new Vector2(graphicsDevice.Viewport.Width * (1f / 3), graphicsDevice.Viewport.Height * (3f / 5)).ToPoint(), new Vector2(graphicsDevice.Viewport.Height * (2f / 5), graphicsDevice.Viewport.Height * (1f / 5)).ToPoint());

            acceptButton = new UIButton(null, new Vector2(graphicsDevice.Viewport.Width * (2f / 5), graphicsDevice.Viewport.Height * (4f / 5)), Vector2.One, true, "Accept", Game1.buttonText);

            cancelButton = new UIButton(null, new Vector2(graphicsDevice.Viewport.Width * (4f / 5), graphicsDevice.Viewport.Height * (4f / 5)), Vector2.One, true, "Cancel", Game1.buttonText);

            lastMouseState = Mouse.GetState();
            lastKeyboardState = new Keys[20];

        }

        public GameState update(MouseState mouse)
        {
            Vector2 mousePos = mouse.Position.ToVector2();
            KeyboardState keyboard = Keyboard.GetState();
            Keys[] pressedKeys = keyboard.GetPressedKeys();

            foreach(Keys key in pressedKeys)
            {
                if (!lastKeyboardState.Contains(key)) // don't enter more than one instance of letter for a key press
                {
                    // backspace
                    if (key.Equals(Keys.Back))
                    {
                        if (keyboardInputText.Length > 0)
                        {
                            keyboardInputText = keyboardInputText.Remove(keyboardInputText.Length - 1, 1);
                        }
                    }
                    // exclude non alpha-numeric keys
                    else if (!key.Equals(Keys.Enter) && !key.Equals(Keys.Tab) && !key.Equals(Keys.Escape) && !key.Equals(Keys.Insert) && !key.Equals(Keys.Delete) && !key.Equals(Keys.Home) && !key.Equals(Keys.End) && !key.Equals(Keys.PageDown) && !key.Equals(Keys.PageUp) && !key.Equals(Keys.LeftShift) && !key.Equals(Keys.RightShift) && !key.Equals(Keys.LeftAlt) && !key.Equals(Keys.RightAlt) && !key.Equals(Keys.Left) && !key.Equals(Keys.Right) && !key.Equals(Keys.Up) && !key.Equals(Keys.Down) && !key.Equals(Keys.LeftControl) && !key.Equals(Keys.RightControl) && !key.Equals(Keys.CapsLock) && !key.Equals(Keys.NumLock) && !key.Equals(Keys.Scroll) && !key.Equals(Keys.PrintScreen) && !key.Equals(Keys.LeftWindows) && !key.Equals(Keys.RightWindows))
                    {
                        keyboardInputText += key.ToString();
                    }
                }
            }

            if (cancelButton.isMouseOnButton(mousePos.X, mousePos.Y) && mouse.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
            {
                return GameState.mainMenu;
            }

            if (acceptButton.isMouseOnButton(mousePos.X, mousePos.Y) && mouse.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
            {
                Game1.loadLevelPack(keyboardInputText + ".txt");
                return GameState.mainMenu;
            }

            lastMouseState = mouse;
            lastKeyboardState = pressedKeys;

            return GameState.levelPackScreen;
        }

        public void drawLevelPackScreen(GameTime gameTime, SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.SandyBrown);

            cancelButton.draw(spriteBatch);
            spriteBatch.DrawString(cancelButton.buttonFont, cancelButton.buttonText, new Vector2(cancelButton.getPosition().X - (cancelButton.textSize.X / 2), cancelButton.getPosition().Y - (cancelButton.textSize.Y / 2)), Color.Red);

            acceptButton.draw(spriteBatch);
            spriteBatch.DrawString(acceptButton.buttonFont, acceptButton.buttonText, new Vector2(acceptButton.getPosition().X - (acceptButton.textSize.X / 2), acceptButton.getPosition().Y - (acceptButton.textSize.Y / 2)), Color.Red);

            spriteBatch.DrawString(Game1.buttonText, infoText, infoBox.Location.ToVector2(), Color.White);
            spriteBatch.DrawString(Game1.buttonText, keyboardInputText, keyboardInputArea.Location.ToVector2(), Color.Purple);

        }
    }
}
