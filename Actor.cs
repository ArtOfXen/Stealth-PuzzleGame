using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public class Actor
    {
        public BoundingBox collisionHitbox;
        public BoundingBox underfootHitbox;

        public Vector3 position;
        public Matrix rotation;
        public float currentYawAngleDeg; // in degrees
        public float currentPitchAngleDeg;

        public const float rotationSpeed = 3f;

        protected ActorModel modelData;

        protected List<Actor> attachedActors; // child actors
        protected Actor parentActor;

        public Actor(ActorModel actorModel, Vector3 startPosition)
        {
            modelData = actorModel;
            position = startPosition;

            currentYawAngleDeg = 0f;
            currentPitchAngleDeg = 0f;

            rotation = Matrix.Identity;

            attachedActors = new List<Actor>();
            attachedActors.Add(this);
            parentActor = this;

            updateHitboxes();
        }

        public virtual void draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            updateHitboxes();

            foreach (Actor a in attachedActors)
            {
                foreach (ModelMesh mesh in a.modelData.model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.View = viewMatrix;
                        effect.World = a.rotation * Matrix.CreateTranslation(a.position);
                        effect.Projection = projectionMatrix;
                    }
                    mesh.Draw();
                }
            }
        }

        public virtual void updateHitboxes()
        {
            foreach (Actor a in attachedActors)
            {
                // calculate new hitbox size
               
                float hitboxX;
                float hitboxZ;
                float minimumHitboxSize;
                float differenceBetweenMinMax;
                float factorOfDifferenceToAddX;
                float factorOfDifferenceToAddZ;

                // find the lowest out of the X and Z coordinates of the hitbox, and then the difference between them
                if (a.modelData.boxSize.X <= a.modelData.boxSize.Z)
                {
                    minimumHitboxSize = a.modelData.boxSize.X;
                    differenceBetweenMinMax = a.modelData.boxSize.Z - a.modelData.boxSize.X;
                    factorOfDifferenceToAddX = Math.Abs(currentYawAngleDeg % 180) / 90;
                    if (factorOfDifferenceToAddX > 1)
                    {
                        factorOfDifferenceToAddX = 1 - (factorOfDifferenceToAddX - 1);
                    }

                    factorOfDifferenceToAddZ = 1 - factorOfDifferenceToAddX;
                }
                else
                {
                    minimumHitboxSize = a.modelData.boxSize.Z;
                    differenceBetweenMinMax = a.modelData.boxSize.X - a.modelData.boxSize.Z;
                    factorOfDifferenceToAddZ = Math.Abs(currentYawAngleDeg % 180) / 90;
                    if (factorOfDifferenceToAddZ > 1)
                    {
                        factorOfDifferenceToAddZ = 1 - (factorOfDifferenceToAddZ - 1);
                    }

                    factorOfDifferenceToAddX = 1 - factorOfDifferenceToAddZ;
                }

                // calculate how big the hitbox should be in each direction by adding a factor of the difference in size between the max and min size
                // size needs to change as actor rotates
                

                hitboxX = minimumHitboxSize + (factorOfDifferenceToAddX * differenceBetweenMinMax);
                hitboxZ = minimumHitboxSize + (factorOfDifferenceToAddZ * differenceBetweenMinMax);

                // calculate hitbox positions
                Vector3 boxMin = new Vector3(
                    a.position.X - hitboxX / 2,
                    a.position.Y,
                    a.position.Z - hitboxZ / 2);
                Vector3 boxMax = new Vector3(
                    a.position.X + hitboxX / 2,
                    a.position.Y + a.modelData.boxSize.Y,
                    a.position.Z + hitboxZ / 2);

                a.collisionHitbox = new BoundingBox(boxMin, boxMax);
                a.underfootHitbox = new BoundingBox(boxMin - new Vector3(0f, a.modelData.boxExtents.Y / 3, 0f), boxMax);
            }
        }

        public void changeYaw(float angleInRadians)
        {
            /// rotate by specified amount
            rotation *= Matrix.CreateFromAxisAngle(Matrix.CreateTranslation(parentActor.position).Up, angleInRadians);
            currentYawAngleDeg += MathHelper.ToDegrees(angleInRadians);
            normaliseAngle(ref currentYawAngleDeg);

            foreach (Actor a in attachedActors)
            {
                if (!a.Equals(this))
                {
                    Vector3 temp = a.position - position;
                    temp = Vector3.Transform(temp, Matrix.CreateRotationY(angleInRadians));
                    a.position = position + temp;

                    a.rotation *= Matrix.CreateFromAxisAngle(Matrix.CreateTranslation(a.parentActor.position).Up, angleInRadians);
                    a.currentYawAngleDeg += MathHelper.ToDegrees(angleInRadians);
                    normaliseAngle(ref a.currentYawAngleDeg);
                }
            }
            updateHitboxes();
        }

        public void setYawAngle(float angleInRadians)
        {
            /// rotate to specified angle
            rotation = Matrix.Identity;
            currentYawAngleDeg = 0f;
            changeYaw(angleInRadians);
        }

        public void changePitch(float angleInRadians)
        {
            
            if (currentPitchAngleDeg < 90f && currentPitchAngleDeg > -90f)
            {
                rotation *= Matrix.CreateFromAxisAngle(Matrix.CreateTranslation(position).Right, angleInRadians);
                currentPitchAngleDeg += MathHelper.ToDegrees(angleInRadians);
            }

            if (currentPitchAngleDeg > 90f)
            {
                currentPitchAngleDeg = 90f;
            }

            if (currentPitchAngleDeg < -90f)
            {
                currentPitchAngleDeg = -90f;
            }

            // match child actors pitch
            foreach (Actor a in attachedActors)
            {
                a.currentPitchAngleDeg = currentPitchAngleDeg;
            }
        }

        public void normaliseAngle(ref float angle)
        {
            /// sets yaw angle to a degree between -180 and 180
            if (angle > 180)
            {
                while (angle > 180)
                {
                    angle -= 360f;
                }
            }

            else if (angle <= -180)
            {
                while (angle <= -180)
                {
                    angle += 360f;
                }
            }
        }

        public ActorModel getModelData()
        {
            return modelData;
        }

        public void setModel(ActorModel newModel)
        {
            modelData = newModel;
        }

        public void setPosition(Vector3 newPosition)
        {
            foreach (Actor a in attachedActors)
            {
                a.position = newPosition;
            }
            updateHitboxes();
        }

        public Vector3 positionMax()
        {
            return new Vector3(position.X + modelData.boxExtents.X, position.Y + modelData.boxSize.Y, position.Z + modelData.boxExtents.Z);
        }

        public Vector3 positionMin()
        {
            return new Vector3(position.X - modelData.boxExtents.X, position.Y, position.Z - modelData.boxExtents.Z);
        }

        public void attachNewActor(ActorModel newModel, Vector3 displacementFromParent, float angleInRadians)
        {
            Actor newActor = new Actor(newModel, position + displacementFromParent);
            newActor.changeYaw(angleInRadians);
            newActor.parentActor = this;
            attachedActors.Add(newActor);
        }

        public void attachNewActor(Actor newActor)
        {
            newActor.parentActor = this;
            attachedActors.Add(newActor);
        }

        public void clearAttachedActors()
        {
            // remove all actors from object, except for parent actor
            for (int i = attachedActors.Count - 1; i >= 1; i--)
            {
                attachedActors.RemoveAt(i);
            }
        }

        public int numberOfAttachedActors()
        {
            return attachedActors.Count;
        }

        public Actor getAttachedActor(int index)
        {
            return attachedActors[index];
        }

        public Actor getParentActor()
        {
            return parentActor;
        }

        public bool hasNoParentActor()
        {
            if (parentActor == this)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void detachFromParentActor()
        {
            if (!parentActor.Equals(this))
            {
                for (int i = 0; i < parentActor.numberOfAttachedActors(); i++)
                {
                    if (parentActor.attachedActors[i].Equals(this))
                    {
                        parentActor.attachedActors.RemoveAt(i);
                        break;
                    }
                }

                parentActor = this;
            }
        }

        public bool collidesWith(Actor otherActor)
        {
            if (collisionHitbox.Intersects(otherActor.collisionHitbox))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
