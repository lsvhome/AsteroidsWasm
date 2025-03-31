using Asteroids.Standard.Components;
using Asteroids.Standard.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Asteroids.Standard
{
    internal class ShipAutoPilot: IDrawableObject
    {
        private readonly ShipEnvironmentFPVManager _env;
        private readonly Ship _ship;
        public ShipAutoPilot(ShipEnvironmentFPVManager env, Ship ship)
        {
            _env = env;
            _ship = ship;
        }

        public void Execute()
        {
            try
            {
                var staticView = _env.TransformViewToFPV();

                var target = staticView.OrderByDescending(x => x.ObjectType).ThenBy(x => x.CenterCoordinates.Distance).FirstOrDefault();

                if (target == null)
                {
                    return;
                }

                var mostJeopardizeObject = staticView.OrderBy(x => x.Distance).First();
                if (mostJeopardizeObject.Distance < 400)
                {
                    foreach (var bullet in _env._cache.GetBulletsAvailable())
                    {
                        bullet.ScreenObject.Shoot(_ship);
                        break;
                    }
                    _ship.Hyperspace();
                    return;
                }

                var shipRotationSpeedFactor = 2;
                double targetCenterAngleDegrees = 0;
                if (target.Velocity.HasValue)
                {
                    double asteroidAngleRadians = Math.Atan2(target.Velocity.Value.Y, target.Velocity.Value.X);
                    double velocity = Math.Sqrt(Math.Pow(target.Velocity.Value.X, 2) + Math.Pow(target.Velocity.Value.Y, 2));
                    var b = target.CenterCoordinates.Distance;
                    var c = velocity;
                    var predictDistance = Math.Sqrt(Math.Pow(b, 2) + Math.Pow(c, 2) - 2 * b * c * Math.Cos(target.CenterCoordinates.Angle));


                    targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(target.CenterCoordinates.Angle));





                }
                else
                {
                    targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(target.CenterCoordinates.Angle));
                }

                var shipRotationSpeed = shipRotationSpeedFactor * targetCenterAngleDegrees;

                var targetDeltaDegreesForSmallRotationSpeed = 2;
                if (targetCenterAngleDegrees >= targetDeltaDegreesForSmallRotationSpeed)
                {
                    shipRotationSpeed = Ship.RotateSpeed;
                }

                if (target.CenterCoordinates.Angle > 0)
                {
                    // right
                    _ship.Rotate(shipRotationSpeed);
                }
                else
                {
                    // left
                    _ship.Rotate(-shipRotationSpeed);
                }

                bool allowFire = false;

                // for dots (like misiles)
                allowFire |= target.LeftAngle == target.RightAngle && Math.Abs(target.RightAngle) <= 0.03;

                // for shapes
                allowFire |= (
                    target.LeftAngle < 0 && target.RightAngle > 0                               

                    && Math.Abs(target.CenterCoordinates.Angle) < Math.Abs(target.LeftAngle)
                    && Math.Abs(target.CenterCoordinates.Angle) < Math.Abs(target.RightAngle)
                    );

                if (allowFire && _ship?.IsAlive == true)
                {
                    // Console.WriteLine($"Target angle = [ {MathHelper.ToDegrees(target.CenterCoordinates.Angle)} ], Distance = {target.CenterCoordinates.Distance}, L [{MathHelper.ToDegrees(target.LeftAngle)}] R[{MathHelper.ToDegrees(target.RightAngle)}]");
                    // Fire bullets that are not already moving
                    foreach (var bullet in _env._cache.GetBulletsAvailable())
                    {
                        bullet.ScreenObject.Shoot(_ship);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #region IDrawableObject

        public IList<PointD> Dots => new List<PointD>();

        private int shipDirectionVectorLenght = 5900;
        public IList<IVectorD> Vectors
        {
            get
            {
                var cl = _ship.GetCurrentLocation();
                var r = _ship.GetRadians();
                var dx = -Math.Sin(r);
                var dy = Math.Cos(r);
                var start = new PointD { X = cl.X, Y = cl.Y };
                var end = new PointD { X = start.X + shipDirectionVectorLenght * dx, Y = start.Y + shipDirectionVectorLenght * dy };
                var v = new VectorD
                {
                    Start = start,
                    End = end,
                    Color = DrawColor.Blue
                };

                Console.WriteLine($"Ship direction vector: {dx} {dy} ; r = {r}={MathHelper.ToDegrees(r)}:= {MathHelper.NormalizeAngle(r)}={MathHelper.ToDegrees(MathHelper.NormalizeAngle(r))}");
                return new List<IVectorD> { v };
            }
        }

        public IList<IPoligonD> Poligons => new List<IPoligonD>();

        public IList<Text> Texts => new List<Text>();

        #endregion IDrawableObject
    }
}
