using Asteroids.Standard.Components;
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
            while (radians < -Math.PI)
                radians += 2 * Math.PI;
            while (radians >= Math.PI)
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

        public static PolarCoordinates TransformDecartToPolar(PointD vector)
        {
            try
            {
                var distance = Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
                var angle = Math.Atan2(vector.Y, vector.X) - Math.PI / 2;
                angle = NormalizeAngle(angle);
                return new PolarCoordinates { Distance = distance, Angle = angle };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public static PolarCoordinates TransformDecartToPolar(VectorD vector)
        {
            return TransformDecartToPolar(vector.End - vector.Start);
        }
    }
}
