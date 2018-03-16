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
    class HelpScreen
    {
        struct HelpScreenEntry
        {
            public string name;
            public string description;
        }
        
        string helpBoxText;
        Rectangle helpBox;

        GraphicsDevice graphicsDevice;

        List<HelpScreenEntry> allEntries;
        List<UIButton> buttons;
        UIButton returnButton;

        GameState lastGameState;
        MouseState lastMouseState;

        public HelpScreen(GameState gameState, GraphicsDevice currentGraphicsDevice)
        {
            graphicsDevice = currentGraphicsDevice;

            allEntries = new List<HelpScreenEntry>();
            buttons = new List<UIButton>();
            setUpEntries();

            for(int i = 0; i < allEntries.Count(); i++)
            {
                buttons.Add(new UIButton(null, new Vector2(currentGraphicsDevice.Viewport.Width * (1f/5), currentGraphicsDevice.Viewport.Height * ((float)(i + 1) / (allEntries.Count() + 1))), Vector2.One, true, allEntries[i].name, Game1.buttonText));
            }

            returnButton = new UIButton(null, new Vector2(50f, 50f), Vector2.One, true, "X", Game1.buttonText);

            helpBox = new Rectangle(new Vector2(currentGraphicsDevice.Viewport.Width * (1.5f / 5), currentGraphicsDevice.Viewport.Height * (1f / 5)).ToPoint(), new Vector2(currentGraphicsDevice.Viewport.Width * (3f / 5), currentGraphicsDevice.Viewport.Height * (3f / 5)).ToPoint());

            helpBoxText = "";

            // can access help screen from either main menu or game in progress, store which in this variable
            lastGameState = gameState;

            lastMouseState = Mouse.GetState();
        }

        public GameState update(MouseState mouse)
        {
            Vector2 mousePos = mouse.Position.ToVector2();

            if (returnButton.isMouseOnButton(mousePos.X, mousePos.Y) && mouse.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
            {
                return lastGameState;
            }

            foreach (UIButton b in buttons)
            {
                if (b.isMouseOnButton(mousePos.X, mousePos.Y) && mouse.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
                {
                    foreach (HelpScreenEntry hse in allEntries)
                    {
                        if (b.buttonText == hse.name)
                        {
                            helpBoxText = Game1.calculateLineBreaks(hse.description, helpBox.Width, Game1.buttonText);
                            break;
                        }
                    }
                    break;
                }
            }

            lastMouseState = mouse;

            return GameState.helpScreen;
        }

        public void drawHelpScreen(GameTime gameTime, SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.SandyBrown);

            foreach(UIButton b in buttons)
            {
                b.draw(spriteBatch);
                spriteBatch.DrawString(b.buttonFont, b.buttonText, new Vector2(b.getPosition().X - (b.textSize.X / 2), b.getPosition().Y - (b.textSize.Y / 2)), Color.White);
            }

            returnButton.draw(spriteBatch);
            spriteBatch.DrawString(returnButton.buttonFont, returnButton.buttonText, new Vector2(returnButton.getPosition().X - (returnButton.textSize.X / 2), returnButton.getPosition().Y - (returnButton.textSize.Y / 2)), Color.Red);

            spriteBatch.DrawString(Game1.buttonText, helpBoxText, helpBox.Location.ToVector2(), Color.White);

        }

        private HelpScreenEntry createEntry(string name, string description)
        {
            HelpScreenEntry newEntry;
            newEntry.name = name;
            newEntry.description = description;
            return newEntry;
        }

        private void setUpEntries()
        {
            allEntries.Add(createEntry("How to Play", "The aim of each level is to locate the sarcophagus while avoiding the deadly traps and mummies. Move using the arrow keys or the WASD keys, turn with the mouse, the number keys to select projectiles, fire projectiles with the left mouse button, and activate projectiles with the right mouse button. Holding down tab will switch the point of view from first person to a birds eye view of your immediate surroundings, but you will unable to enter any action except for right mouse clicks while in a birds eye view."));
            allEntries.Add(createEntry("Mummies", "Mummies roam the halls of the tomb. If you are seen by, or touch, one of the mummies, then you will be forced to restart the level. The mummies will patrol the same paths repeatedly, so pay attention and figure out how to best evade them! Mummies are affected by traps just like you are, and will fall through holes in the floor, so bare in mind the ways that you can use the environment to your advantage."));
            allEntries.Add(createEntry("Traps and Hazards", "A tomb may have been built with traps to deter any treasure hunters - luckily, most of them also come with ways of switching them off. If you come into contact with any hazard, you will instantly lose and have to restart the level. Some traps will alternate between active and inactive at set time intervals, some will be affected by triggers on the floor, some will be controlled by power-boxes - make sure to pay attention to figure out how each trap functions. And, yes; it is odd that the ancient Egyptian had power-boxes, isn't it?"));
            allEntries.Add(createEntry("Projectiles", "There are a variety of projectiles avaiable to help you complete the mazes, each of which has its own entry on this help screen. The projectiles available to you are dependant on the current level, some levels forbid the use of certain projectiles; you can tell which aren't avaiable at the moment as there will be a red cross through its UI element. Select which projectile you wish to equip using the number keys, the UI element for each projectile will show you which number you need to press. Fire the currently selected projectile with the left mouse button. Some projectiles will have a secondary action, you will only be able to fire one of these at a time, and will require you to activate the secondary action using the right mouse button before you can fire another. Please note that the UI element for each projectile will indicate whether pressing the left mouse button will fire another projectile or if pressing the right mouse button will activate a secondary action. You can change your currently selected projectile while your previous projectile is still awaiting the activation of its secondary action."));
            allEntries.Add(createEntry("Power Projectile", "This projectile is used to activate or deactivate poxer-boxes. Power-boxes are linked to traps, which will also activate or deactivate if the corroponding power-box is hit by a Power projectile. This projectile is harmlessly destroyed if it collides with anything else, including traps."));
            allEntries.Add(createEntry("Pull Projectile", "This projectile attaches itself to the first object or enemy it collides with. Once it activated with the right mouse button, it pulls all nearby enemies and other projectiles towards it. Only one Pull projectile can be in play at any time, it must be activated with the right mouse button before another can be fired."));
            allEntries.Add(createEntry("Port Projectile", "This projectile, when activated with the right mouse button, immediately teleports the player to its location, then disappears. This projectile is harmlessly destroyed if it collides with anything. Only one Port projectile can be in play at any time, it must be activated with the right mouse button or otherwise destroyed before another can be fired."));
            allEntries.Add(createEntry("Uploading New Level Packs", "To load a new level pack, the .txt file must first be placed in the 'LevelPacks' folder in the game files. Afterwards, click on 'Level Packs' in the main menu, and type in the filename (excluding '.txt') of the level pack you wish the play."));
        }

        public void setReturnState(GameState returnState)
        {
            lastGameState = returnState;
        }
    }
}
