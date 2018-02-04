using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    abstract class Trigger : Actor // abstact because this base class has no way of being triggered
    {
        /// <summary>
        /// used to change the status of a variable obstacle, such as a door or moving platform
        /// </summary>

        VariableObstacle linkedObstacle;
        bool reactivatable;
        bool activated;

        protected Trigger(ActorModel actorModel, Vector3 startPosition, VariableObstacle newLinkedObstacle, bool canBeActivatedMultipleTimes) : base(actorModel, startPosition)
        {
            activated = false;
            linkedObstacle = newLinkedObstacle;
            reactivatable = canBeActivatedMultipleTimes;
        }

        protected void activateTrigger(bool overwriteConditionalCheck = false)
        {
            if (!(activated == true && !reactivatable)) // check that trigger can still be activated
            {
                activated = true;
                linkedObstacle.changeActiveStatus();
            }
        }

        protected bool canBeActivated()
        {
            if (!activated || reactivatable)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
