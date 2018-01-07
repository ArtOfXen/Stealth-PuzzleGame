using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    class PullProjectile : Projectile
    {
        

        // use DateTime as timer instead of GameTime, since GameTime needs update/draw to function
        int pullStartSeconds;
        int currentSeconds;
        float pullAnimationLength;

        public PullProjectile(ActorModel projectileModel, Vector3 startPosition, int movementSpeed, float angleOfFire) : 
            base (ProjectileClassification.pull, projectileModel, startPosition, movementSpeed, angleOfFire)
        {
            actionEffectMinimumArea = new BoundingSphere(startPosition, modelData.boxSize.X / 2);
            actionEffectMaximumArea = new BoundingSphere(startPosition, modelData.boxSize.X * 10);

            pullAnimationLength = 1f;
        }

        public override void move(Vector3? changeInPosition = null, List<Actor> movementBlockers = null)
        {
            if (!actionStarted)
            {
                displace(new Vector3((float)Math.Sin(MathHelper.ToRadians(currentYawAngleDeg)), 0f, (float)Math.Cos(MathHelper.ToRadians(currentYawAngleDeg))));

                actionEffectMinimumArea = new BoundingSphere(position, modelData.boxSize.X / 2);
                actionEffectMaximumArea = new BoundingSphere(position, modelData.boxSize.X * 10);
            }
            else
            {
                currentSeconds = DateTime.Now.Second;

                // if current < start, new minute has started and seconds have reset to 0. So add 60
                if (currentSeconds < pullStartSeconds)
                {
                    currentSeconds += 60;
                }

                if (currentSeconds >= pullStartSeconds + pullAnimationLength)
                {
                    requiresDeletion = true;
                }
                // check for enemies in max pull area
                // move them to min pull area
                
            }

        }

        public override void startAction()
        {
            base.startAction();
            pullStartSeconds = DateTime.Now.Second;
        }

        
    }
}
