using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{

    public enum ProjectileClassification
    {
        none,
        shock, // kills unarmoured enemies
        pull // pulls enemies towards the projectile when action activated
    };


    public class Projectile : Character   
    {
        ProjectileClassification classification;
        public bool requiresDeletion;
        protected bool actionStarted; // some projectiles have an action activated by right click

        protected BoundingSphere actionEffectMaximumArea;
        protected BoundingSphere actionEffectMinimumArea;

        public Projectile(ProjectileClassification projectileClassification, ActorModel projectileModel, Vector3 startPosition, int movementSpeed, float angleOfFire) : base(projectileModel, startPosition, movementSpeed)
        {
            requiresDeletion = false;
            actionStarted = false;
            classification = projectileClassification;
            changeYaw(MathHelper.ToRadians(angleOfFire));
        }

        public override void move(Vector3? changeInPosition = null)
        {
            if (!MovementBlocked)
            {
                displace(new Vector3((float)Math.Sin(MathHelper.ToRadians(currentYawAngleDeg)), 0f, (float)Math.Cos(MathHelper.ToRadians(currentYawAngleDeg))));
            }
        }

        public ProjectileClassification getClassification()
        {
            return classification;
        }

        public virtual void startAction()
        {
            detachFromParentActor();
            actionStarted = true;
        }

        public bool hasActionStarted()
        {
            return actionStarted;
        }

        public bool enemyInActionRadius(BoundingBox enemyHitbox)
        {
            if (enemyHitbox.Intersects(actionEffectMaximumArea) && !enemyHitbox.Intersects(actionEffectMinimumArea))
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
