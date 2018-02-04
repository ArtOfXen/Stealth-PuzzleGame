using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game1
{
    class ProjectileActivatedTrigger : Trigger
    {
        ProjectileClassification nessecaryProjectile;

        public ProjectileActivatedTrigger(ActorModel actorModel, Vector3 startPosition, VariableObstacle newLinkedObstacle, bool canBeActivatedMultipleTimes, ProjectileClassification triggeringProjectile) : base(actorModel, startPosition, newLinkedObstacle, canBeActivatedMultipleTimes)
        {
            nessecaryProjectile = triggeringProjectile;
        }

        public void hitByProjectile(ProjectileClassification projectileType)
        {
            if (nessecaryProjectile == projectileType)
            {
                activateTrigger();
            }
            else
            {
                
            }
        }
    }
}
