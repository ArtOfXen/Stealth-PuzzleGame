using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class Hazard : Character
    {
        /// <summary>
        ///  Kill player and NPCs who collide with them
        /// </summary>
        /// 

        public Hazard(ActorModel actorModel, Vector3 startPosition, bool initiallyActive = false) : base(actorModel, startPosition, 10)
        {
            Active = initiallyActive;
        }

        //public bool checkForHazardCollision(Actor collidingActor)
        //{
        //    if (!Active)
        //    {
        //        return false;
        //    }

        //    if (collisionHitbox.Intersects(collidingActor.collisionHitbox))
        //    {
        //        for (int i = 1; i < attachedActors.Count - 1; i++)
        //        {
        //            if (attachedActors[i].collisionHitbox.Intersects(collidingActor.collisionHitbox))
        //            {
        //                return false;
        //            }
        //        }

        //        return true;
        //    }
        //    return false;
        //}

        public override void draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            updateHitboxes();

            for(int i = 0; i < attachedActors.Count; i++)
            {
                if (i == 0 && !Active)
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

        public bool Active { get; set; }
    }
}
