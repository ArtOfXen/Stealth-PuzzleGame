﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    class Hazard : Character
    {
        /// <summary>
        ///  Kill player and NPCs who collide with them
        /// </summary>
        public Hazard(ActorModel actorModel, Vector3 startPosition) : base(actorModel, startPosition, 10)
        {

        }

        public bool checkForHazardCollision(Actor collidingActor)
        {
            /// 
            if (collisionHitbox.Intersects(collidingActor.collisionHitbox))
            {
                for (int i = 1; i < attachedActors.Count - 1; i++)
                {
                    if (attachedActors[i].collisionHitbox.Intersects(collidingActor.collisionHitbox))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }
    }
}
