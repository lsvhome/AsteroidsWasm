using System;

namespace Asteroids.Standard
{
    public static class MathHelper
    {
        public static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        public static double NormalizeAngle(double radians)
        {
            while (radians < 0)
                radians += 2 * Math.PI;
            while (radians >= 2 * Math.PI)
                radians -= 2 * Math.PI;
            return radians;
        }
    }
}
