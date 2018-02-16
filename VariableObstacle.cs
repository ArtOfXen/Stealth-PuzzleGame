using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class VariableObstacle : Actor
    {

        /*
         * 
         * 
         * 
         * 
         * need a way to turn off collision hitbox of variable obstacles
         * 
         * 
         * 
         * 
         */

        private bool active;
        private bool initiallyActive;
        private double lastIntervalTime;
        private double? intervalTimer;

        public VariableObstacle(ActorModel actorModel, Vector3 startPosition, bool activeAtLevelStart = true, double? automaticIntervalTimer = null) : base(actorModel, startPosition)
        {
            initiallyActive = activeAtLevelStart;
            active = activeAtLevelStart;
            intervalTimer = automaticIntervalTimer;
            if (intervalTimer != null)
            {
                lastIntervalTime = DateTime.Now.TimeOfDay.TotalSeconds;
            }
        }

        public void update()
        {
            if (intervalTimer == null)
            {
                return;
            }
            else { 
                double currentSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                if (currentSeconds > lastIntervalTime + intervalTimer)
                {
                    changeActiveStatus();
                    lastIntervalTime = currentSeconds;
                }
            }
        }

        public void changeActiveStatus()
        {

            /* 
             * need to add sounds for activation and deactivation, eminating from the player, not the obstacle, to let player know when obstacles change
             */

            if (active)
            {
                active = false;
            }
            else
            {
                active = true;
            }
        }

        public bool isActive()
        {
            return active;
        }

        public override void draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            /// obstacles need their own draw method, as they can be set to inactive during gameplay
            
            updateHitboxes();

            for (int i = 0; i < attachedActors.Count; i++)
            {
                if (i == 0 && !isActive())
                {
                    continue;
                }
                foreach (ModelMesh mesh in attachedActors[i].getModelData().model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        //effect.AmbientLightColor = new Vector3(1f, 0, 0);
                        effect.EnableDefaultLighting();
                        effect.View = viewMatrix;
                        effect.World = attachedActors[i].rotation * Matrix.CreateTranslation(attachedActors[i].position);
                        effect.Projection = projectionMatrix;
                    }
                    mesh.Draw();
                }
            }
        }
    }
}
