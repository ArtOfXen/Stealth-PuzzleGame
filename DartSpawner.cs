using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    class DartSpawner : VariableObstacle
    {
        List<Hazard> activeDarts;
        ActorModel dartModelData;
        Actor spawnWall;
        Actor despawnWall;
        double lastDartTime;
        const double dartSpawnInterval = 0.15;
        Random rng;

        public DartSpawner(ActorModel actorModel, Vector3 startPosition, Actor dartSpawnerWall, Actor otherWall, ActorModel dartModel, bool activeAtLevelStart = true, double? automaticIntervalTimer = null) : base(actorModel, startPosition, activeAtLevelStart, automaticIntervalTimer)
        {
            spawnWall = dartSpawnerWall;
            despawnWall = otherWall;
            attachNewActor(spawnWall);
            attachNewActor(despawnWall);
            lastDartTime = DateTime.Now.TimeOfDay.TotalSeconds;
            activeDarts = new List<Hazard>();
            dartModelData = dartModel;
            rng = new Random();
        }

        public override void changeActiveStatus()
        {
            base.changeActiveStatus();
            lastDartTime = DateTime.Now.TimeOfDay.TotalSeconds;
        }

        public override void update()
        {
            List<Hazard> dartsToNotDestroy = new List<Hazard>();

            if (isActive() && (DateTime.Now.TimeOfDay.TotalSeconds > lastDartTime + dartSpawnInterval))
            {
                addNewDart();
                lastDartTime = DateTime.Now.TimeOfDay.TotalSeconds;
            }

            foreach (Hazard dart in activeDarts)
            {
                // check whether darts needs to move along X or Z axis
                if (spawnWall.position.X > despawnWall.position.X || spawnWall.position.X < despawnWall.position.X)
                {
                    dart.move(new Vector3((despawnWall.position.X - spawnWall.position.X) / (Math.Abs(despawnWall.position.X - spawnWall.position.X)), 0f, 0f));
                }
                else
                {
                    dart.move(new Vector3(0f, 0f, (despawnWall.position.Z - spawnWall.position.Z) / (Math.Abs(despawnWall.position.Z - spawnWall.position.Z))));
                }

                if (!(dart.collidesWith(despawnWall)))
                {
                    dartsToNotDestroy.Add(dart);
                }
            }

            activeDarts.Clear();
            activeDarts = dartsToNotDestroy;
        }
        private void addNewDart()
        {
            Hazard newDart;

            float dartPosX; float dartPosY; float dartPosZ;
            float objectHeight = spawnWall.collisionHitbox.Max.Y - spawnWall.collisionHitbox.Min.Y;

            if (currentYawAngleDeg == -90 || currentYawAngleDeg == 90)
            {
                dartPosX = rng.Next((int)spawnWall.collisionHitbox.Min.X, (int)spawnWall.collisionHitbox.Max.X);
                dartPosZ = spawnWall.position.Z;
            }
            else
            {
                dartPosZ = rng.Next((int)spawnWall.collisionHitbox.Min.Z, (int)spawnWall.collisionHitbox.Max.Z);
                dartPosX = spawnWall.position.X;
            }
            
            dartPosY = rng.Next((int)(spawnWall.collisionHitbox.Min.Y + objectHeight * 1/4), (int)(spawnWall.collisionHitbox.Max.Y - objectHeight * 1/4));

            newDart = new Hazard(dartModelData, new Vector3(dartPosX, dartPosY, dartPosZ), 10);
            newDart.changeYaw(MathHelper.ToRadians(currentYawAngleDeg - 90));

            activeDarts.Add(newDart);
        }

        public List<Hazard> getDartList()
        {
            return activeDarts;
        }



    }
}
