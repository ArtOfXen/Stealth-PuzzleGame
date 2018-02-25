﻿using System;
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
        private float velocityDueToGravity;
        private static List<Actor> movementBlockers;

        public Character(ActorModel actorModel, Vector3 startPosition, int movementSpeed) : base(actorModel, startPosition)
        {
            speed = new Vector3(movementSpeed, 0f, movementSpeed);
            Falling = false;
            velocityDueToGravity = 3f;
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

        public virtual void move(Vector3? changeInPosition = null)
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
                speed = Vector3.Down; // unable to move left or right anymore, only down
                displace(new Vector3(0f, velocityDueToGravity, 0f));
                velocityDueToGravity++;
                return true;
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

        //public void move(Direction movementDirection)
        //{
        //    if (movementDirection != null)
        //    {
        //        position += movementDirection.vector * speed;
        //        collisionHitbox = new BoundingBox(position - type.boxExtents, position + type.boxExtents);
        //        actionHitbox = new BoundingBox(position - type.boxSize * 3 / 4, position + type.boxSize * 3 / 4);

        //        // turn character to face direction of movement
        //        normaliseAngle();

        //        // check character is facing right way
        //        if (currentYawAngle != movementDirection.getAngleDegrees())
        //        {
        //            // new and current angle both on right of screen, or both on left of screen
        //            if ((movementDirection.getAngleDegrees() > 0 && currentYawAngle > 0) || (movementDirection.getAngleDegrees() <= 0 && currentYawAngle <= 0))
        //            {
        //                // counter clockwise rotation
        //                if (movementDirection.getAngleDegrees() > currentYawAngle)
        //                {
        //                    changeYaw(MathHelper.ToRadians(rotationSpeed));
        //                }
        //                // clockwise rotation
        //                else
        //                {
        //                    changeYaw(MathHelper.ToRadians(-rotationSpeed));
        //                }
        //            }

        //            // new angle on left, current on right
        //            else if (movementDirection.getAngleDegrees() <= 0 && currentYawAngle > 0)
        //            {
        //                // clockwise if angle difference > 180, else counterclockwise
        //                if (Math.Abs(movementDirection.getAngleDegrees()) + currentYawAngle >= 180)
        //                {
        //                    changeYaw(MathHelper.ToRadians(rotationSpeed));
        //                }
        //                else
        //                {
        //                    changeYaw(MathHelper.ToRadians(-rotationSpeed));
        //                }
        //            }
        //            // new angle on right, current on left
        //            else if (movementDirection.getAngleDegrees() > 0 && currentYawAngle <= 0)
        //            {
        //                // counter if angle difference > 180, else clockwise
        //                if (Math.Abs(currentYawAngle) + movementDirection.getAngleDegrees() >= 180)
        //                {
        //                    changeYaw(MathHelper.ToRadians(-rotationSpeed));
        //                }
        //                else
        //                {
        //                    changeYaw(MathHelper.ToRadians(rotationSpeed));
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
