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
        // boulder? moves patrol path constantly, never stopping for any reason. Does not react to projectiles nor chase the player
        // stalker? actively searches for the player, does not have a normal patrol path. Senses when another guard dies and moves to that location, then searches around that area
        // gnome? Small and fast, only if implementing pitch controls for player
    };

    public struct Instruction
    {
        public char type; // T = turn, W = wait, X = moveX, Z = moveZ
        public int factor; // angle, time, newtile, newTile
    }

    public class NPC : Character
    { 
        public List<BoundingSphere> detectionArea; // vision area
        const int visionRange = 12;

        public List<Instruction> instructionList;
        List<Instruction> originalInstructionList;
        int currentInstructionIndex;

        bool dead;
        public bool chasingPlayer;

        EnemyClassification classification;
        ProjectileEffectiveness projectileEffectiveness;

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
            projectileEffectiveness = enemyStruct.projectileEffectiveness;
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
        }


        //public void patrol(List<Actor> movementBlockers = null)
        //{
        //    if (!chasingPlayer && patrolPath.Count > 1)
        //    {
        //        if (patrolPath[currentTileIndex].coordinates.X == patrolPath[nextTileIndex].coordinates.X)
        //        {
        //            if (patrolPath[currentTileIndex].coordinates.Y > patrolPath[nextTileIndex].coordinates.Y)
        //            {
        //                // move up
        //                move(new Vector3(0f, 0f, -speed.Z), movementBlockers);
        //            }
        //            else if (patrolPath[currentTileIndex].coordinates.Y < patrolPath[nextTileIndex].coordinates.Y)
        //            {
        //                // move down
        //                move(new Vector3(0f, 0f, speed.Z), movementBlockers);
        //            }
        //        }

        //        else if (patrolPath[currentTileIndex].coordinates.Y == patrolPath[nextTileIndex].coordinates.Y)
        //        {
        //            {
        //                if (patrolPath[currentTileIndex].coordinates.X > patrolPath[nextTileIndex].coordinates.X)
        //                {
        //                    // move left
        //                    move(new Vector3(-speed.X, 0f, 0f), movementBlockers);
        //                }
        //                else if (patrolPath[currentTileIndex].coordinates.X < patrolPath[nextTileIndex].coordinates.X)
        //                {
        //                    // move right
        //                    move(new Vector3(speed.X, 0f, 0f), movementBlockers);
        //                }
        //            }

        //            // check if next tile reached
        //            // if pos -> pos - speed, == nextTile.pos
        //            // currentTile = nextTile
        //            // set NextTile to nextTile++
        //            // set pos to currentTileCentre
        //        }
        //    }
        //}

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
            
            if (!chasingPlayer)
            {
                
            }
            else
            {
                /* note enemy position when player first seen
                 * always note last position that player was seen and translate to tile coordinates.
                 * move towards tile
                 * once tile is reached, look left, then right for player
                 * if it sees player, repeat
                 * else, return to original location when player was first seen
                 */
                
            }
        }

        public override void move(Vector3? changeInPosition)
        {
            // converts Vector3Nullable to Vector3
            Vector3 displacement = changeInPosition ?? default(Vector3);

            if (!dead && !wouldCollideWithTerrain(position + (speed * displacement)) && !Falling)
            {
                displace(displacement);
            }

            base.move();
        }

        //public void changeDirection(bool clockwise)
        //{
        //    if (clockwise)
        //        currentDirection = currentDirection.nextRightAngleDirectionClockwise;
        //    else
        //        currentDirection = currentDirection.nextRightAngleDirectionCounterClockwise;
        //}

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
                    move(new Vector3(-1f, 0f, 0f)); 
                }
                else if (position.X < destinationTile.centre.X)
                {
                    // move right
                    move(new Vector3(1f, 0f, 0f));
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
                    position.Z = destinationTile.centre.Z;
                }

                if (position.Z > destinationTile.centre.Z)
                {
                    // move up
                    move(new Vector3(0f, 0f, -1f));
                }
                else if (position.Z < destinationTile.centre.Z)
                {
                    //move down
                    move(new Vector3(0f, 0f, 1f));
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
                if (resettingAngle)
                {
                    instructionList.RemoveAt(instructionList.Count - 1);
                    resettingAngle = false;

                    if (!(currentTile.Equals(patrolPath[0])))
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
                        currentTileIndex = 0;
                        destinationTileIndex = 0;
                        if (patrolPath.Count > 1)
                        {
                            destinationTileIndex = 1;
                        }
                        destinationTile = patrolPath[destinationTileIndex];

                        // if last instruction was a wait instruction, skip it when reversing instructions
                        //if (instructionList[0].type == 'W' && instructionList[instructionList.Count - 1].type == 'W')
                        //{
                        //    currentInstructionIndex = 1;
                        //}

                        // if wait instruction was only at one end of list, then move it to the other end when reversing instruction list
                        //if (instructionList[0].type == 'W' && instructionList[instructionList.Count - 1].type != 'W')
                        //{
                        //    Instruction waitAtEndInstruction;
                        //    waitAtEndInstruction.type = 'W';
                        //    waitAtEndInstruction.factor = instructionList[0].factor;

                        //    instructionList.RemoveAt(0);
                        //    instructionList.Add(waitAtEndInstruction);
                        //}
                    }
                    else
                    {
                        currentInstructionIndex = 0;
                        currentTileIndex = 0;
                        destinationTileIndex = 0;
                        if (patrolPath.Count > 1)
                        {
                            destinationTileIndex = 1;
                        }
                        destinationTile = patrolPath[destinationTileIndex];
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
                else if (!(currentTile.Equals(patrolPath[0])) && instructionList[instructionList.Count - 1].type != 'T')
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
                    // drop down into Z - same code used

                    //currentTileIndex = destinationTileIndex;
                    //destinationTileIndex = (destinationTileIndex + 1>= patrolPath.Count) ? 0 : destinationTileIndex + 1;
                    //destinationTile = patrolPath[destinationTileIndex];
                    //break;
                case 'Z':
                    //currentTileIndex = destinationTileIndex;
                    //destinationTileIndex = (destinationTileIndex + 1 >= patrolPath.Count) ? 0 : destinationTileIndex + 1;
                    //destinationTile = patrolPath[destinationTileIndex];
                    break;
                    //case 'Z':
                    //    for (int i = 0; i < patrolPath.Count; i++)
                    //    {
                    //        if (patrolPath[i].coordinates.Y == instructionList[currentInstructionIndex].factor &&
                    //            patrolPath[i].coordinates.X == patrolPath[currentTileIndex].coordinates.X)
                    //        {
                    //            destinationTileIndex = i;
                    //            destinationTile = patrolPath[destinationTileIndex];
                    //            break;
                    //        }
                    //    }
                    //    break;
            }
        }

        //public void setPatrolPath(List<Tile> tilesToPatrol)
        //{
        //    if (tilesToPatrol.Count != 0)
        //    {
        //        patrolPath = tilesToPatrol;
        //        currentTileIndex = 0;
        //        position = patrolPath[currentTileIndex].centre;
        //        if (tilesToPatrol.Count > 1)
        //        {
        //            nextTileIndex = 1;
        //        }
        //    }
        //}

        public void detectPlayer()
        {
            chasingPlayer = true;
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
