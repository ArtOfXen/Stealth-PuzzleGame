using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{





    /*
     * 
     * 
     * 
     * if nothing else gets added to this class, change it be part of an enum in base class or a bool (killCharacterOnContact = true/false)
     * 
     * 
     * 
     */





    class Hazard : VariableObstacle
    {
        /// <summary>
        ///  Kill player and NPCs who collide with them
        /// </summary>
        /// 

        public Hazard(ActorModel actorModel, Vector3 startPosition, bool initiallyActive = true, double? automaticIntervalTimer = null) : base(actorModel, startPosition, initiallyActive, automaticIntervalTimer)
        {

        }
    }
}
