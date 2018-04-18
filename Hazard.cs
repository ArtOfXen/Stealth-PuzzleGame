using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class Hazard : Character
    {
        /// <summary>
        ///  Kill player and NPCs who collide with them
        /// </summary>
        /// 

        public Hazard(ActorModel actorModel, Vector3 startPosition, int movementSpeed) : base(actorModel, startPosition, movementSpeed)
        {

        }

        public override void move(Vector3? changeInPosition = default(Vector3?), bool checkTerrainCollision = true)
        {
            base.move(changeInPosition);
        }
    }
}
