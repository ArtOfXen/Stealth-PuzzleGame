using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{

    public enum EnemyClassification
    {
        pawn,
        armoured
    };

    class NPC : Character
    { 
        Direction currentDirection;
        public List<BoundingSphere> detectionArea; // vision area
        const int visionRange = 12;
        bool dead;
        EnemyClassification classification;
        ActorModel alertActorType;
        ProjectileEffectiveness projectileEffectiveness;

        public NPC(EnemyStruct enemyStruct, Vector3 startPosition, int movementSpeed) : base(enemyStruct.unalertModel, startPosition, movementSpeed)
        {
            dead = false;
            detectionArea = new List<BoundingSphere>();
            classification = enemyStruct.classification;
            alertActorType = enemyStruct.alertModel;
            projectileEffectiveness = enemyStruct.projectileEffectiveness;
        }


        public void update(List<Actor> visionBlockers)
        {
            updateHitboxes();
            updateVisionDetection(visionBlockers);
        }

        public override void move(Vector3? changeInPosition, List<Actor> movementBlockers = null)
        {
            // converts Vector3Nullable to Vector3
            Vector3 displacement = changeInPosition ?? default(Vector3);

            if (!dead)
            {
                displace(displacement);
            }

        }

        public void changeDirection(bool clockwise)
        {
            if (clockwise)
                currentDirection = currentDirection.nextRightAngleDirectionClockwise;
            else
                currentDirection = currentDirection.nextRightAngleDirectionCounterClockwise;
        }

        public override void updateHitboxes()
        {

            base.updateHitboxes();
            
        }

        private void updateVisionDetection(List<Actor> visionBlockers)
        {
            detectionArea.Clear();

            // create detection area
            for (int i = 0; i < visionRange; i++)
            {
                BoundingSphere nextVisionSphere;

                nextVisionSphere = new BoundingSphere(new Vector3(
                    position.X + (float)Math.Sin(MathHelper.ToRadians(currentYawAngleDeg)) * modelData.boxExtents.Z * (i + 1),
                    position.Y + modelData.boxExtents.Y,
                    position.Z + (float)Math.Cos(MathHelper.ToRadians(currentYawAngleDeg)) * modelData.boxExtents.Z * (i + 1)), modelData.boxSize.Z);

                // stop detection area short if it collides with a wall
                foreach (Actor v in visionBlockers)
                {
                    if (nextVisionSphere.Intersects(v.collisionHitbox) && v.getModelData().blocksVision)
                    {
                        return;
                    }
                }

                detectionArea.Add(nextVisionSphere);
            }
        }

        public void kill()
        {
            changePitch(MathHelper.ToRadians(90));
            setPosition(position + new Vector3(0f, modelData.boxExtents.Z, 0f));
            modelData.blocksMovement = false;
            modelData.blocksVision = false;

            dead = true;
        }


        public bool isDead()
        {
            return dead;
        }

        public void setEnemyType(EnemyStruct newStruct)
        {
            modelData = newStruct.unalertModel;
            alertActorType = newStruct.alertModel;
            classification = newStruct.classification;
        }

        public void detectPlayer()
        {
            modelData = alertActorType;
        }

        public bool isEffectedBy(ProjectileClassification p)
        {
            switch (p)
            {
                case ProjectileClassification.shock:
                    return projectileEffectiveness.shock;

                case ProjectileClassification.pull:
                    return projectileEffectiveness.pull;

                default:
                    return false;
            }
        }
    }
}
