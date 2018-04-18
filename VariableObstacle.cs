using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class VariableObstacle : Actor
    {
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

        public virtual void update()
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

        public override void updateHitboxes()
        {
            if (active)
            {
                base.updateHitboxes();
            }
            else
            {
                // place hitboxes outside of the level to prevent them from being used
                foreach (Actor a in attachedActors)
                {
                    a.collisionHitbox = new BoundingBox(new Vector3(-500f, -500f, -500f), new Vector3(-500f, -500f, -500f));
                    a.underfootHitbox = new BoundingBox(new Vector3(-500f, -500f, -500f), new Vector3(-500f, -500f, -500f));
                }
            }
        }

        public virtual void changeActiveStatus()
        {
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
