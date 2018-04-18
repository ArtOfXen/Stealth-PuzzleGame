using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    public class Character : Actor
    {
        /// <summary>
        ///  Actors which have movement properties
        /// </summary>
        
        protected Vector3 speed;
        protected float fallDistance;

        private float velocityDueToGravity;
        private static List<Actor> movementBlockers;

        public Character(ActorModel actorModel, Vector3 startPosition, int movementSpeed) : base(actorModel, startPosition)
        {
            speed = new Vector3(movementSpeed, 0f, movementSpeed);
            Falling = false;
            velocityDueToGravity = 3f;
            fallDistance = -50f;
            MovementBlocked = false;
        }

        public bool MovementBlocked { get; set; }

        protected bool wouldCollideWithTerrain(Vector3 newPosition)
        {
            /// checks if potential move will collide with anything
            BoundingBox newPositionBox = new BoundingBox(newPosition - modelData.boxExtents, newPosition + modelData.boxExtents);

            foreach (Actor a in movementBlockers)
            {
                for (int i = 0; i < a.numberOfAttachedActors(); i++)
                {
                    if (a.getAttachedActor(i).collisionHitbox.Intersects(newPositionBox) && a.getAttachedActor(i).getModelData().blocksMovement)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected void displace(Vector3 changeInPosition)
        {
            foreach (Actor a in attachedActors)
            {
                a.position += speed * changeInPosition;
                updateHitboxes();
            }
        }

        public virtual void move(Vector3? changeInPosition = null, bool checkTerrainCollision = true)
        {
            if (!currentlyPulledDownByGravity())
            {
                Vector3 displacement = changeInPosition ?? default(Vector3);
                displace(displacement);
            }
        }

        public bool currentlyPulledDownByGravity()
        {
            if (Falling)
            {
                if (position.Y > fallDistance)
                {
                    speed = Vector3.Down; // unable to move left or right anymore, only down
                    displace(new Vector3(0f, velocityDueToGravity, 0f));
                    velocityDueToGravity++;
                    return true;
                }
                else
                {
                    Falling = false;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Falling { get; set; }
        public bool IsAffectedByGravity { get; set; }

        public static void setMovementBlockers(List<Actor>newListOfMovementBlockers)
        {
            movementBlockers = newListOfMovementBlockers;
        }
    }
}
