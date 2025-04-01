using System;
using System.Drawing;

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

        public static Point TransformPolarToDecart(PolarCoordinates vector)
        {
            try
            {
                var x = vector.Distance * -Math.Sin(vector.Angle);
                var y = vector.Distance * Math.Cos(vector.Angle);
                return new Point((int)x, (int)y);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
