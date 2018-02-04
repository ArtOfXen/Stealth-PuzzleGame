using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game1
{
    class MovementActivatedTrigger : Trigger
    {

        private Character currentlyCollidingCharacter;

        public MovementActivatedTrigger(ActorModel actorModel, Vector3 startPosition, VariableObstacle newLinkedObstacle, bool canBeActivatedMultipleTimes) : base(actorModel, startPosition, newLinkedObstacle, canBeActivatedMultipleTimes)
        {
            currentlyCollidingCharacter = null;
        }

        private bool characterOnTrigger()
        {
            if (currentlyCollidingCharacter != null)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public void checkCurrentlyCollidingCharacter()
        {
            if (characterOnTrigger())
            {
                if (!(collisionHitbox.Intersects(currentlyCollidingCharacter.collisionHitbox)))
                {
                    currentlyCollidingCharacter = null;
                    if (canBeActivated())
                    {
                        activateTrigger(true);
                    }
                }
            }
        }

        public void collisionWithCharacter(Character collidingCharacter)
        {
            if (characterOnTrigger())
            {
                return;
            }

            currentlyCollidingCharacter = collidingCharacter;
            activateTrigger();
        }
    }
}
