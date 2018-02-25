using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Game1
{
    class TeleportProjectile : Projectile
    {
        public TeleportProjectile(ActorModel projectileModel, Vector3 startPosition, int movementSpeed, float angleOfFire) : 
            base (ProjectileClassification.teleport, projectileModel, startPosition, movementSpeed, angleOfFire)
        {

        }

        public override void startAction()
        {
            base.startAction();
            requiresDeletion = true;
        }
    }
}
