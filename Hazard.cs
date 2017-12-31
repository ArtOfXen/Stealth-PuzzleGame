using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    class Hazard : Actor
    {
        /// <summary>
        ///  Actors which kill characters who collide with them
        /// </summary>
        public Hazard(ActorModel actorModel, Vector3 startPosition) : base(actorModel, startPosition)
        {

        }
    }
}
