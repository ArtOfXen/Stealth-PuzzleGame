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

        public Player(ActorModel actorModel, Vector3 startPosition, int movementSpeed) : base(actorModel, startPosition, movementSpeed)
        {

        }

        public override void move(Vector3? changeInPosition, List<Actor> movementBlockers)
        {
            Vector3 displacement = changeInPosition ?? default(Vector3); // converts Vector3Nullable to Vector3

            if (!wouldCollideWithTerrain(position + (speed * displacement), movementBlockers))
            {
                displace(displacement);
            }
            
        }
    }
}
