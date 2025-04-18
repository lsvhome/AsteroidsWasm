using Asteroids.Standard.Components;
using System;
using System.Drawing;

namespace Asteroids.Standard
{
#nullable disable
    public static class MathHelper
    {
        public static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double NormalizeAngle(double radians)
        {
            while (radians < -Math.PI)
                radians += 2 * Math.PI;
            while (radians >= Math.PI)
                radians -= 2 * Math.PI;
            return radians;
        }

        public static PointD TransformPolarToDecart(PolarCoordinates vector)
        {
            try
            {
                var x = vector.Distance * Math.Sin(vector.Angle);
                var y = vector.Distance * Math.Cos(vector.Angle);
                return new PointD { X = x, Y = y };
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
                if (vector.X == 0 && vector.Y == 0)
                {
                    return new PolarCoordinates { Distance = 0, Angle = 0 };
                }

                var distance = Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
                Angle angle = Math.Atan2(vector.X, vector.Y);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="angleBeta"></param>
        /// <returns>Time factor for Va and Vb</returns>
        internal static double GetT(PointD A, PointD B, double Va, double Vb, double angleBeta)
        {
            // image taken from https://ru.onlinemschool.com/math/formula/triangle/

            var Lc = (B - A).GetPolarCoordinates().Distance;

            var cosBeta = Math.Cos(angleBeta);

            if (Va == Vb)
            {
                var t0 = Lc / (2 * Va * cosBeta);
                return t0;
            }
            // sqare equision
            var _a = Math.Pow(Va, 2) - Math.Pow(Vb, 2);
            var _b = -2 * Lc * Va * cosBeta;
            var _c = Math.Pow(Lc, 2);


            var D = Math.Pow(_b, 2) - 4 * _a * _c;

            if (D < 0)
            {
                throw new InvalidOperationException();
            }

            double t1 = -1;
            double t2 = -1;
            try
            {
                t1 = (-_b + Math.Sqrt(D)) / (2 * _a);
            }
            catch (Exception)
            {
            }

            try
            {
                t2 = (-_b - Math.Sqrt(D)) / (2 * _a);
            }
            catch (Exception)
            {
            }


            if (t1 < 0 && t2 < 0)
            {
                throw new InvalidOperationException();
            }

            if (t1 > 0 && t2 > 0)
            {
                throw new InvalidOperationException();
            }

            var t = t1 > t2 ? t1 : t2;

            return t;
        }

        public static PolarCoordinates Triangle(PointD A, PointD B, double Va, double Vb, double angleBeta)
        {
            var t = GetT(A, B, Va, Vb, angleBeta);

            var Lb = Vb * t;
            var La = Va * t;

            var sinAlpga = La / Lb * Math.Sin(angleBeta);
            var alpha = Math.Asin(sinAlpga);

            var ret = new PolarCoordinates
            {
                Angle = alpha,
                Distance = Lb
            };

            return ret;
        }

        public static TriangleInfo GetTriangleInfo(PointD A, PointD B, double Va, double Vb, Angle angleBeta)
        {
            var t = GetT(A, B, Va, Vb, angleBeta);

            var Lb = Vb * t;
            var La = Va * t;

            var sinAlpga = La / Lb * Math.Sin(angleBeta);
            Angle alpha = Math.Asin(sinAlpga);

            var a = new VectorD
            {
                Start = A,
                End = B
            }.GetPolarCoordinates().Angle;

            //var finalAngle = a - angleBeta;
            //var finalAngle = angleBeta;
            var finalAngle = a-alpha;

            Console.WriteLine($"finalAngle=[{finalAngle.ValueDegee}] sin = [{Math.Sin(finalAngle)}]  cos = [{Math.Cos(finalAngle)}]");

            var Lax = La * Math.Sin(finalAngle);
            var cX = B.X + Lax;
            var Lay = La * Math.Cos(finalAngle);
            var cY = - B.Y + Lay;


            var Lbx = Lb * Math.Sin(finalAngle);
            cX = A.X + Lbx;
            var Lby = Lb * Math.Cos(finalAngle);
            cY = A.Y + Lby;

            var ret = new TriangleInfo
            {
                A = A,
                B = B,
                C = new PointD ( cX, cY ),
            };
#if DEBUG
            //System.Diagnostics.Debug.Assert(Math.Abs(ret.Lb - Lb) < 0.001);
            //System.Diagnostics.Debug.Assert(Math.Abs(ret.La - La) < 0.001);
            //System.Diagnostics.Debug.Assert(Math.Abs(ret.AngleAlpha - Math.Abs(alpha)) < 0.0001);
            //System.Diagnostics.Debug.Assert(Math.Abs(ret.AngleBeta - Math.Abs(angleBeta)) < 0.0001);
#endif

            return ret;
        }

        public class TriangleInfo
        {
            public PointD A { get; set; }
            
            public PointD B { get; set; }
            
            public PointD C { get; set; }

            public double La => (B - C).GetPolarCoordinates().Distance;
            
            public double Lb => (C - A).GetPolarCoordinates().Distance;
            
            public double Lc => (B - A).GetPolarCoordinates().Distance;

            public Angle AngleAlpha => Math.Abs(NormalizeAngle((B - A).GetPolarCoordinates().Angle - (C - A).GetPolarCoordinates().Angle));
         
            public Angle AngleBeta => Math.Abs(NormalizeAngle((A-B).GetPolarCoordinates().Angle - (C - B).GetPolarCoordinates().Angle));

            public Angle AngleGamma => Math.Abs(NormalizeAngle((A - C).GetPolarCoordinates().Angle - (B - C).GetPolarCoordinates().Angle));

            public bool AngleAlphaDirectionPositive => (B - C).GetPolarCoordinates().Angle > 0;
            //public bool AngleBetaDirection => (B - C).GetPolarCoordinates().Angle > 0;
            //public bool AngleGammaDirection => (B - C).GetPolarCoordinates().Angle > 0;

        }



        public static PointD RotateInternal(double Radians, PointD pt)
        {
            var sinVal = Math.Sin(Radians); // sin 0 = 0 sin 90 = 1
            var cosVal = Math.Cos(Radians); // cos 0 = 1 sin 90 = 0

            var ret = new PointD(0, 0);
            ret.X = (pt.X * cosVal - pt.Y * sinVal);
            ret.Y = (pt.X * sinVal + pt.Y * cosVal);

            return ret;
        }
    }

#nullable restore
}
