using Asteroids.Standard.Components;
using System.Drawing;

namespace Asteroids.Standard
{
    internal class ShipEnvironmentObjectLocation
    {
        public ShipEnvironmentObjectLocation(PolarCoordinates centerCoordinates, double relativeLeftAngle, double relativeRightAngle)
        {
            CenterCoordinates = centerCoordinates;
            LeftRelativeToCenterAngle = ShipEnvironmentFPVManager.NormalizeAngle(centerCoordinates.Angle - relativeLeftAngle);
            RightRelativeToCenterAngle = ShipEnvironmentFPVManager.NormalizeAngle(centerCoordinates.Angle + relativeRightAngle);
        }

        public PolarCoordinates CenterCoordinates { get; set; }
        public double LeftRelativeToCenterAngle { get; set; }
        public double RightRelativeToCenterAngle { get; set; }

        public ObjectType ObjectType { get; set; }
        public double Distance { get; set; }

        public PointF? Velocity { get; set; }

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
                { Angle = ship.GetRadians() + this.CenterCoordinates.Angle, Distance = this.CenterCoordinates.Distance };
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
