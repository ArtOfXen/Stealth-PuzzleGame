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
        double? intervalTimer;
        double? resetTimer;
        double lastIntervalTime;
        double activationTime;


        protected Trigger(ActorModel actorModel, Vector3 startPosition, VariableObstacle newLinkedObstacle, bool canBeActivatedMultipleTimes, int? automaticIntervalTimer = null, int? automaticResetTimer = null) : base(actorModel, startPosition)
        {
            activated = false;
            linkedObstacle = newLinkedObstacle;
            reactivatable = canBeActivatedMultipleTimes;
            intervalTimer = automaticIntervalTimer;
            resetTimer = automaticResetTimer;
            lastIntervalTime = DateTime.Now.TimeOfDay.TotalSeconds;
        }

        public void update()
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalSeconds;

            if (intervalTimer != null)
            {
                if (currentTime > lastIntervalTime + intervalTimer)
                {
                    lastIntervalTime = currentTime;
                    activateTrigger(true);
                }

                if (resetTimer != null)
                {
                    if (activated && (currentTime > activationTime + resetTimer))
                    {
                        activateTrigger(true);
                        activated = false;
                    }
                }
            }
        }

        protected void activateTrigger(bool overwriteConditionalCheck = false)
        {
            if (activated == false && !reactivatable) // check that trigger can still be activated
            {
                activated = true;
                linkedObstacle.changeActiveStatus();
                activationTime = DateTime.Now.TimeOfDay.TotalSeconds;
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
