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

        public ProjectileActivatedTrigger(ActorModel actorModel, Vector3 startPosition, VariableObstacle newLinkedObstacle, bool canBeActivatedMultipleTimes, ProjectileClassification triggeringProjectile, double? automaticIntervalTimer = null, double? automaticResetTimer = null) : base(actorModel, startPosition, newLinkedObstacle, canBeActivatedMultipleTimes, automaticIntervalTimer, automaticResetTimer)
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
