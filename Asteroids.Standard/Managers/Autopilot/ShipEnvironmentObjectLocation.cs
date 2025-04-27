using Asteroids.Standard.Components;
using System;
using System.Diagnostics;
using System.Drawing;
using static Asteroids.Standard.MathHelper;

namespace Asteroids.Standard
{

    [DebuggerDisplay("{ObjectType} {CenterCoordinates}")]
    internal class ShipEnvironmentObjectLocation
    {
        public ShipEnvironmentObjectLocation(PolarCoordinates centerCoordinates, double relativeLeftAngle, double relativeRightAngle)
        {
            CenterCoordinates = centerCoordinates;
            LeftRelativeToCenterAngle = MathHelper.NormalizeAngle(centerCoordinates.Angle - relativeLeftAngle);
            RightRelativeToCenterAngle = MathHelper.NormalizeAngle(centerCoordinates.Angle + relativeRightAngle);
        }

        /// <summary>
        /// Location of target from ship FPV
        /// </summary>
        public PolarCoordinates CenterCoordinates { get; set; }

        public PolarCoordinates GetCenterCoordinatesPredicted(Ship ship)
        {

            var ret = this.CenterCoordinates;
            if (this.Velocity.HasValue)
            {
                TriangleInfo triangle = TargetPredictionTriangleInfo(ship);
                var targetAngle = (triangle.C - triangle.A).GetPolarCoordinates().Angle;

                var diff2 = targetAngle - ship.GetRadians();

                ret = new PolarCoordinates { Angle = MathHelper.NormalizeAngle( diff2), Distance = triangle.Lc };
            }

            return ret;
        }

        TriangleInfo TargetPredictionTriangleInfo(Ship ship)
        {

            ShipEnvironmentObjectLocation target = this;
            if (target?.Velocity == null)
            {
                var zeroPoint = new PointD { X = 0, Y = 0 };
                return new TriangleInfo { A = zeroPoint, B = zeroPoint, C = zeroPoint };
            }
            else
            {
                var bVelocity = Bullet.InitialVelocity;// Math.Sqrt(Math.Pow(Bullet.InitialVelocity, 2) + Math.Pow(Bullet.InitialVelocity, 2));
                Angle ta = target.CenterRelativeLocationPolar(ship).Angle;
                Angle angleBeta = MathHelper.NormalizeAngle(Math.PI - (Math.PI - target.VelocityP.Angle - (Math.PI - ta)));

                var triangle = MathHelper.GetTriangleInfo(ship.GetCurrentLocation(), target.CenterAbsoluteLocationDecart(ship), target.VelocityP.Distance, bVelocity, angleBeta);


                return triangle;
            }
        }


        public double LeftRelativeToCenterAngle { get; set; }
        public double RightRelativeToCenterAngle { get; set; }

        public ObjectType ObjectType { get; set; }
        public double Distance { get; set; }

        public Point? Velocity { get; set; }
        public PolarCoordinates VelocityP => MathHelper.TransformDecartToPolar(this.Velocity ?? new Point(0, 0));

        public double GetAbsoluteAngle(Ship ship)
        {
            return ship.GetRadians() + this.CenterCoordinates.Angle;
        }

        /// <summary>
        /// Relative location regarding to ship with absolute angle (idependent from ship direction).
        /// </summary>
        public PolarCoordinates CenterRelativeLocationPolar(Ship ship)
        {
            return new PolarCoordinates
                { Angle = MathHelper.NormalizeAngle( ship.GetRadians() + this.CenterCoordinates.Angle), Distance = this.CenterCoordinates.Distance };
        }

        /// <summary>
        /// Relative location regarding to observer (where observer location is (0,0))
        /// </summary>
        public PointD CenterRelativeLocationDecart(Ship ship)
        {
            PolarCoordinates t = this.CenterRelativeLocationPolar(ship);
            var ret = (PointD)MathHelper.TransformPolarToDecart(t);
            return ret;
        }

        public PointD CenterAbsoluteLocationDecart(Ship ship)
        {
            PointD t = this.CenterRelativeLocationDecart(ship);
            var sl = (PointD)ship.GetCurrentLocation();
            var ret = sl + t;
            return ret;
        }


    }
}
