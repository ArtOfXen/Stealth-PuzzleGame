using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game1
{
    class TimeActivatedTrigger : Trigger
    {

        double activationIntervalTime;
        double lastActivationTime;

        public TimeActivatedTrigger(ActorModel actorModel, Vector3 startPosition, VariableObstacle newLinkedObstacle, bool canBeActivatedMultipleTimes, double timeBetweenTriggers) : base(actorModel, startPosition, newLinkedObstacle, canBeActivatedMultipleTimes)
        {
            lastActivationTime = DateTime.Now.TimeOfDay.TotalSeconds;
            activationIntervalTime = timeBetweenTriggers;
        }

        public void checkTimer()
        {
            double currentSeconds = DateTime.Now.TimeOfDay.TotalSeconds;

            if (currentSeconds > lastActivationTime + activationIntervalTime)
            {
                activateTrigger();
                lastActivationTime = currentSeconds;
            }
        }
    }
}
