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
        mummy,
        boulder,
    };

    public struct Instruction
    {
        public char type; // T = turn, W = wait, X = moveX, Z = moveZ
        public int factor; // angle, time, number of tiles to move
    }

    public class NPC : Character
    { 
        public List<BoundingSphere> detectionArea; // vision area
        int visionRange;
        const float visionSphereRadius = 50f;

        public List<Instruction> instructionList;
        List<Instruction> originalInstructionList;
        int currentInstructionIndex;

        bool dead;

        EnemyClassification classification;

        double waitStart;
        double waitLength;

        float rotationTargetAngle;
        float startingAngle;
        bool resettingAngle;

        public List<Tile> patrolPath;
        List<Tile> originalPatrolPath;
        int currentTileIndex;
        int destinationTileIndex;

        Tile startTile;
        public Tile currentTile;
        Tile destinationTile;

        public NPC(EnemyStruct enemyStruct, Tile startingTile, int movementSpeed, float initialAngle) : base(enemyStruct.model, new Vector3(startingTile.centre.X, 0f, startingTile.centre.Z), movementSpeed)
        {
            dead = false;
            detectionArea = new List<BoundingSphere>();
            classification = enemyStruct.classification;
            instructionList = new List<Instruction>();
            originalInstructionList = new List<Instruction>();
            currentInstructionIndex = 0;

            patrolPath = new List<Tile>();
            originalPatrolPath = new List<Tile>();
            startTile = startingTile;
            currentTile = startingTile;

            currentTileIndex = 0;
            destinationTileIndex = 1;

            startingAngle = initialAngle;
            changeYaw(MathHelper.ToRadians(startingAngle));
            resettingAngle = false;

            visionRange = enemyStruct.visionRange;
        }

        public void update(List<Actor> visionBlockers)
        {
            updateHitboxes();
            updateVisionDetection(visionBlockers);
            if (Falling)
            {
                move(null);
            }
            else
            {
                executeCurrentInstruction();
            }
        }

        public override void move(Vector3? changeInPosition, bool checkTerrainCollision = true)
        {
            // converts Vector3Nullable to Vector3
            Vector3 displacement = changeInPosition ?? default(Vector3);

            if (!dead && (!wouldCollideWithTerrain(position + (speed * displacement)) || !checkTerrainCollision) && !Falling)
            {
                displace(displacement);
            }

            base.move();
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
                    position.X + (float)Math.Sin(MathHelper.ToRadians(currentYawAngleDeg)) * visionSphereRadius * (i + 1),
                    position.Y + modelData.boxExtents.Y,
                    position.Z + (float)Math.Cos(MathHelper.ToRadians(currentYawAngleDeg)) * visionSphereRadius * (i + 1)),
                    visionSphereRadius);

                // stop detection area short if it collides with a wall
                foreach (Actor v in visionBlockers)
                {
                    if (nextVisionSphere.Intersects(v.collisionHitbox) && v.getModelData().blocksVision && !v.Equals(this))
                    {
                        return;
                    }
                }

                detectionArea.Add(nextVisionSphere);
            }
        }

        public void createPatrolPath(List<Tile> tilesToAdd)
        {
            patrolPath = new List<Tile>();
            originalPatrolPath = new List<Tile>();

            patrolPath.Add(currentTile);
            foreach(Tile t in tilesToAdd)
            {
                patrolPath.Add(t);
            }

            if (patrolPath[patrolPath.Count - 1].Equals(patrolPath[0]) && patrolPath.Count > 1)
            {
                patrolPath.RemoveAt(patrolPath.Count - 1);
            }

            if (patrolPath.Count > 1)
            {
                destinationTile = patrolPath[1];
                destinationTileIndex = 1;
            }
            else
            {
                destinationTile = patrolPath[0];
                destinationTileIndex = 0;
            }

            originalPatrolPath = patrolPath;
            currentTileIndex = 0;
        }

        public void createInstructionList(List<Instruction> instructionsToAdd)
        {
            instructionList = instructionsToAdd;
            originalInstructionList = instructionList;

            if (instructionList.Count > 0)
            {
                if (instructionList[0].type == 'W')
                {
                    waitStart = DateTime.Now.TimeOfDay.TotalSeconds;
                    waitLength = instructionList[currentInstructionIndex].factor;
                }
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
            modelData = newStruct.model;
            classification = newStruct.classification;
        }

        public void executeCurrentInstruction()
        {
            Instruction currentInstruction = instructionList[currentInstructionIndex];

            if (currentInstruction.type == 'W')
            {
                double currentSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
                if (currentSeconds - waitStart > waitLength)
                {
                    nextInstruction();
                }
            }

            else if (currentInstruction.type == 'T')
            {
                if (currentYawAngleDeg != rotationTargetAngle)
                {
                    // new and current angle both on right of screen, or both on left of screen
                    if ((rotationTargetAngle > 0 && currentYawAngleDeg > 0) || (rotationTargetAngle <= 0 && currentYawAngleDeg <= 0))
                    {
                        // counter clockwise rotation
                        if (rotationTargetAngle > currentYawAngleDeg)
                        {
                            changeYaw(MathHelper.ToRadians(rotationSpeed));
                        }
                        // clockwise rotation
                        else
                        {
                            changeYaw(MathHelper.ToRadians(-rotationSpeed));
                        }
                    }

                    // new angle on left, current on right
                    else if (rotationTargetAngle <= 0 && currentYawAngleDeg > 0)
                    {
                        // clockwise if angle difference > 180, else counterclockwise
                        if (Math.Abs(rotationTargetAngle) + currentYawAngleDeg >= 180)
                        {
                            changeYaw(MathHelper.ToRadians(rotationSpeed));
                        }
                        else
                        {
                            changeYaw(MathHelper.ToRadians(-rotationSpeed));
                        }
                    }
                    // new angle on right, current on left
                    else if (rotationTargetAngle > 0 && currentYawAngleDeg <= 0)
                    {
                        // counter if angle difference > 180, else clockwise
                        if (Math.Abs(currentYawAngleDeg) + rotationTargetAngle >= 180)
                        {
                            changeYaw(MathHelper.ToRadians(-rotationSpeed));
                        }
                        else
                        {
                            changeYaw(MathHelper.ToRadians(rotationSpeed));
                        }
                    }
                    normaliseAngle(ref currentYawAngleDeg);
                }

                else
                {
                    nextInstruction();
                }
            }

            else if (currentInstruction.type == 'X')
            {
                if (Math.Abs(position.X - destinationTile.centre.X) < speed.X)
                {
                    position.X = destinationTile.centre.X;
                }

                if (position.X > destinationTile.centre.X)
                {
                    // move left
                    move(new Vector3(-1f, 0f, 0f), false); 
                }
                else if (position.X < destinationTile.centre.X)
                {
                    // move right
                    move(new Vector3(1f, 0f, 0f), false);
                }
                else
                {
                    currentTile = destinationTile;
                    currentTileIndex = destinationTileIndex;
                    destinationTileIndex = (destinationTileIndex + 1 >= patrolPath.Count) ? 0 : destinationTileIndex + 1;
                    destinationTile = patrolPath[destinationTileIndex];
                    nextInstruction();
                }
            }

            else if(currentInstruction.type == 'Z')
            {
                if (Math.Abs(position.Z - destinationTile.centre.Z) < speed.Z)
                {
                    if (getClassification() == EnemyClassification.boulder)
                    {
                        //asdad
                    }
                    position.Z = destinationTile.centre.Z;
                }

                if (position.Z > destinationTile.centre.Z)
                {
                    // move up
                    move(new Vector3(0f, 0f, -1f), false);
                }
                else if (position.Z < destinationTile.centre.Z)
                {
                    //move down
                    move(new Vector3(0f, 0f, 1f), false);
                }
                else
                {
                    currentTile = destinationTile;
                    currentTileIndex = destinationTileIndex;
                    destinationTileIndex = (destinationTileIndex + 1 >= patrolPath.Count) ? 0 : destinationTileIndex + 1;
                    destinationTile = patrolPath[destinationTileIndex];
                    nextInstruction();
                }
            }
        }

        public void nextInstruction()
        {
            currentInstructionIndex++;

            // end of instructions reached
            if (currentInstructionIndex >= instructionList.Count)
            {
                if (resettingAngle || getClassification() == EnemyClassification.boulder)
                {
                    if (getClassification() != EnemyClassification.boulder)
                    {
                        instructionList.RemoveAt(instructionList.Count - 1);
                    }
                    resettingAngle = false;

                    if (!(currentTile.coordinates.Equals(patrolPath[0].coordinates)))
                    {
                        List<Instruction> reverseInstructionList = new List<Instruction>(instructionList);
                        instructionList.Clear();
                        reverseInstructionList.Reverse();
                        foreach (Instruction i in reverseInstructionList)
                        {
                            if (i.type == 'T' || i.type == 'X' || i.type == 'Z')
                            {
                                Instruction reverseInstruction;
                                reverseInstruction.type = i.type;
                                reverseInstruction.factor = -(i.factor);
                                instructionList.Add(reverseInstruction);
                            }
                            else
                            {
                                instructionList.Add(i);
                            }
                        }

                        patrolPath.Reverse();
                        currentInstructionIndex = 0;
                    }
                    else
                    {
                        currentInstructionIndex = 0;
                    }
                }

                else if (currentYawAngleDeg != startingAngle && currentTile.Equals(patrolPath[0]))
                {
                    Instruction turnToOriginalAngle;

                    turnToOriginalAngle.type = 'T';
                    turnToOriginalAngle.factor = (int)(startingAngle - currentYawAngleDeg);
                    instructionList.Add(turnToOriginalAngle);
                    resettingAngle = true;
                }
                else if (!currentTile.Equals(patrolPath[0]))
                {
                    Instruction turnAround;
                    turnAround.type = 'T';
                    turnAround.factor = 180;
                    instructionList.Add(turnAround);
                    resettingAngle = true;
                }
                else
                {
                    currentInstructionIndex = 0;
                    if (currentTile.Equals(patrolPath[0]))
                    {
                        createInstructionList(originalInstructionList);
                        createPatrolPath(originalPatrolPath);
                    }
                }
            }

            // begin next instruction
            switch (instructionList[currentInstructionIndex].type)
            {
                case 'W':
                    waitStart = DateTime.Now.TimeOfDay.TotalSeconds;
                    waitLength = instructionList[currentInstructionIndex].factor;
                    break;
                case 'T':
                    rotationTargetAngle = currentYawAngleDeg + instructionList[currentInstructionIndex].factor;
                    normaliseAngle(ref rotationTargetAngle);
                    break;
                case 'X':
                case 'Z':
                    break;
            }
        }

        public void detectPlayer()
        {
            
        }

        public EnemyClassification getClassification()
        {
            return classification;
        }
    }
}
