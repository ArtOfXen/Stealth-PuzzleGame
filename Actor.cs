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

        protected ActorModel modelData;
        protected float rotationSpeed;

        protected List<Actor> attachedActors; // child actors

        public Actor(ActorModel actorModel, Vector3 startPosition)
        {
            modelData = actorModel;
            position = startPosition;
            rotationSpeed = 5f;

            currentYawAngleDeg = Game1.down.getAngleDegrees();
            currentPitchAngleDeg = 0f;

            rotation = Matrix.Identity;

            attachedActors = new List<Actor>();
            attachedActors.Add(this);

            updateHitboxes();
        }

        public void draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            updateHitboxes();

            foreach (Actor a in attachedActors)
            {
                foreach (ModelMesh mesh in a.modelData.model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        //effect.AmbientLightColor = new Vector3(1f, 0, 0);
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
                float hitboxX = ((float)Math.Abs(Math.Cos(MathHelper.ToRadians(a.currentYawAngleDeg))) * a.modelData.boxSize.X) + ((float)Math.Abs(Math.Sin(MathHelper.ToRadians(a.currentYawAngleDeg))) * a.modelData.boxSize.Z);
                float hitboxZ = ((float)Math.Abs(Math.Cos(MathHelper.ToRadians(a.currentYawAngleDeg))) * a.modelData.boxSize.Z) + ((float)Math.Abs(Math.Sin(MathHelper.ToRadians(a.currentYawAngleDeg))) * a.modelData.boxSize.X);

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
            foreach (Actor a in attachedActors)
            {
                a.rotation *= Matrix.CreateFromAxisAngle(Matrix.CreateTranslation(position).Up, angleInRadians);
                a.currentYawAngleDeg += MathHelper.ToDegrees(angleInRadians);
            }
            normaliseAngle();
            updateHitboxes();
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

        public void normaliseAngle()
        {
            /// sets yaw angle to a degree between -180 and 180
            if (currentYawAngleDeg > 180)
            {
                while (currentYawAngleDeg > 180)
                {
                    currentYawAngleDeg -= 360f;
                }
            }

            else if (currentYawAngleDeg <= -180)
            {
                while (currentYawAngleDeg <= -180)
                {
                    currentYawAngleDeg += 360f;
                }
            }

            foreach (Actor a in attachedActors)
            {
                a.currentYawAngleDeg = currentYawAngleDeg;
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
            attachedActors.Add(new Actor(newModel, position + displacementFromParent));
        }

        public void attachNewActor(Actor newActor)
        {
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
    }
}
