using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    class Player : Character
    {

        public float AutoRotationTargetAngle { get; set; }

        public Player(ActorModel actorModel, Vector3 startPosition, int movementSpeed) : base(actorModel, startPosition, movementSpeed)
        {
            
        }

        public override void move(Vector3? changeInPosition)
        {
            if (!currentlyPulledDownByGravity())
            {
                Vector3 displacement = changeInPosition ?? default(Vector3); // converts Vector3Nullable to Vector3

                if (!wouldCollideWithTerrain(position + (speed * displacement)) && !Falling)
                {
                    displace(displacement);
                }
            }
        }
    }
}
