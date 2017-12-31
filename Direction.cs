using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game1
{
    public class Direction
    {
        public Direction nextDirectionClockwise;
        public Direction nextDirectionCounterClockwise;
        public Direction nextRightAngleDirectionClockwise;
        public Direction nextRightAngleDirectionCounterClockwise;
        public Direction oppositeDirection;

        public Vector3 vector;
        private float angleInDegrees;
        private float angleInRadians;

        public Direction(float degrees, Vector3 directionVector)
        {
            angleInDegrees = degrees;
            angleInRadians = MathHelper.ToRadians(degrees);
            vector = directionVector;
        }

        public void setAdjacentDirections(Direction rightAngleCounterClockwise, Direction counterClockwise, Direction clockwise, Direction rightAngleClockwise, Direction opposite)
        {
            nextDirectionClockwise = clockwise;
            nextDirectionCounterClockwise = counterClockwise;
            nextRightAngleDirectionClockwise= rightAngleClockwise;
            nextRightAngleDirectionCounterClockwise = rightAngleCounterClockwise;
            oppositeDirection = opposite;
        }

        public float getAngleDegrees()
        {
            return angleInDegrees;
        }

        public float getAngleRadians()
        {
            return angleInRadians;
        }

    }
}
