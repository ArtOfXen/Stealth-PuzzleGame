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
        bool hasBeenActivated;
        bool initiallyActive;
        bool currentlyActive;
        double? intervalTimer;
        protected double? resetTimer;
        double lastIntervalTime;
        protected double activationTime;


        protected Trigger(ActorModel actorModel, Vector3 startPosition, VariableObstacle newLinkedObstacle, bool canBeActivatedMultipleTimes, double? automaticIntervalTimer = null, double? automaticResetTimer = null) : base(actorModel, startPosition)
        {
            hasBeenActivated = false;
            linkedObstacle = newLinkedObstacle;
            reactivatable = canBeActivatedMultipleTimes;
            intervalTimer = automaticIntervalTimer;
            resetTimer = automaticResetTimer;
            lastIntervalTime = DateTime.Now.TimeOfDay.TotalSeconds;
            currentlyActive = linkedObstacle.isActive();
            initiallyActive = currentlyActive;
        }

        //public virtual void update()
        //{
        //    double currentTime = DateTime.Now.TimeOfDay.TotalSeconds;

        //    if (intervalTimer != null)
        //    {
        //        if (currentTime > lastIntervalTime + intervalTimer)
        //        {
        //            lastIntervalTime = currentTime;
        //            activateTrigger(true);
        //        }

        //        if (resetTimer != null)
        //        {
        //            if (activated && (currentTime > activationTime + resetTimer))
        //            {
        //                activateTrigger(true);
        //                activated = false;
        //            }
        //        }
        //    }
        //}
        public void checkResetTimer()
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalSeconds;

            if (resetTimer != null)
            {
                if (currentTime > activationTime + resetTimer && currentlyActive != initiallyActive)
                {
                    activateTrigger(true);
                }
            }
        }

        protected void activateTrigger(bool overwriteConditionalCheck = false)
        {
            if (canBeActivated() || overwriteConditionalCheck) // check that trigger can still be activated
            {
                hasBeenActivated = true;
                linkedObstacle.changeActiveStatus();
                currentlyActive = linkedObstacle.isActive();
                activationTime = DateTime.Now.TimeOfDay.TotalSeconds;
            }
        }

        protected bool canBeActivated()
        {
            if (!hasBeenActivated || reactivatable)
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
