using Asteroids.Standard.Components;
using Asteroids.Standard.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;

namespace Asteroids.Standard
{
    internal class ShipAutoPilot : IDrawableObject
    {
        private readonly ShipEnvironmentFPVManager _env;
        private readonly Ship _ship;
        public ShipAutoPilot(ShipEnvironmentFPVManager env, Ship ship)
        {
            _env = env;
            _ship = ship;
        }

        internal ShipEnvironmentObjectLocation? Target { get; private set; }

        public void Execute()
        {
            try
            {
                var staticView = _env.TransformViewToFPV();

                Target = staticView.OrderByDescending(x => x.ObjectType).ThenBy(x => x.CenterCoordinates.Distance).FirstOrDefault();

                if (Target == null)
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
                if (Target.Velocity.HasValue)
                {
                    double asteroidAngleRadians = Math.Atan2(Target.Velocity.Value.Y, Target.Velocity.Value.X);
                    double velocity = Math.Sqrt(Math.Pow(Target.Velocity.Value.X, 2) + Math.Pow(Target.Velocity.Value.Y, 2));
                    var b = Target.CenterCoordinates.Distance;
                    var c = velocity;
                    var predictDistance = Math.Sqrt(Math.Pow(b, 2) + Math.Pow(c, 2) - 2 * b * c * Math.Cos(Target.CenterCoordinates.Angle));


                    targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(Target.CenterCoordinates.Angle));
                }
                else
                {
                    targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(Target.CenterCoordinates.Angle));
                }

                var shipRotationSpeed = shipRotationSpeedFactor * targetCenterAngleDegrees;

                var targetDeltaDegreesForSmallRotationSpeed = 2;
                if (targetCenterAngleDegrees >= targetDeltaDegreesForSmallRotationSpeed)
                {
                    shipRotationSpeed = Ship.RotateSpeed;
                }

                if (Target.CenterCoordinates.Angle > 0)
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
                allowFire |= Target.LeftAngle == Target.RightAngle && Math.Abs(Target.RightAngle) <= 0.03;

                // for shapes
                allowFire |= (
                    Target.LeftAngle < 0 && Target.RightAngle > 0

                    && Math.Abs(Target.CenterCoordinates.Angle) < Math.Abs(Target.LeftAngle)
                    && Math.Abs(Target.CenterCoordinates.Angle) < Math.Abs(Target.RightAngle)
                    );

                /*
                allowFire |= (
                    Target.LeftAngle < 0 && Target.RightAngle > 0

                    && Math.Abs(Target.CenterCoordinates.Angle) < Math.Abs(Target.LeftAngle)
                    && Math.Abs(Target.CenterCoordinates.Angle) < Math.Abs(Target.RightAngle)
                    );
                */

                allowFire |= staticView.Any(t=> (
                    t.LeftAngle < 0 && t.RightAngle > 0

                    && Math.Abs(t.CenterCoordinates.Angle) < Math.Abs(t.LeftAngle)
                    && Math.Abs(t.CenterCoordinates.Angle) < Math.Abs(t.RightAngle)
                    ));

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

        private Poligon TargetPolygon
        {
            get
            {
                if (Target == null)
                {
                    return new Poligon();
                }
                int sideLength = 100;
                var cl = _ship.GetCurrentLocation();
                //var p = Target.CenterCoordinates;
                //var dv = MathHelper.TransformPolarToDecart(Target.CenterCoordinates);
                var r = _ship.GetRadians();
                var dv = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = _ship.GetRadians(), Distance = Target.Distance });

                var squareCenter = new Point(cl.X + dv.X, cl.Y + dv.Y);
                Console.WriteLine($"Target center = {squareCenter}, cl={cl}, dv={dv}, cc={Target.CenterCoordinates}");
                var ret = new Poligon
                {
                    Color = DrawColor.Red,
                    Points = new List<PointD>
                    {
                        new PointD { X = squareCenter.X - sideLength, Y = squareCenter.Y + sideLength},
                        new PointD { X = squareCenter.X + sideLength, Y = squareCenter.Y + sideLength},
                        new PointD { X = squareCenter.X + sideLength, Y = squareCenter.Y - sideLength},
                        new PointD { X = squareCenter.X - sideLength, Y = squareCenter.Y - sideLength},
                    }
                };
                return ret;
            }
        }

        private VectorD ShipDirectionVector
        {
            get
            {

                var cl = _ship.GetCurrentLocation();
                var r = _ship.GetRadians();
                var dv = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = _ship.GetRadians(), Distance = shipDirectionVectorLenght });
                //var dx = -Math.Sin(r);
                //var dy = Math.Cos(r);
                var start = new PointD { X = cl.X, Y = cl.Y };
                //var end = new PointD { X = start.X + shipDirectionVectorLenght * dx, Y = start.Y + shipDirectionVectorLenght * dy };
                var end = new PointD { X = start.X + dv.X, Y = start.Y + dv.Y };
                var v = new VectorD
                {
                    Start = start,
                    End = end,
                    Color = DrawColor.Red
                };

                //Console.WriteLine($"Ship direction vector: {dx} {dy} ; r = {r}={MathHelper.ToDegrees(r)}:= {MathHelper.NormalizeAngle(r)}={MathHelper.ToDegrees(MathHelper.NormalizeAngle(r))}");
                return v;
            }
        }


        private VectorD ShipPredictionVector
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

                //Console.WriteLine($"Ship direction vector: {dx} {dy} ; r = {r}={MathHelper.ToDegrees(r)}:= {MathHelper.NormalizeAngle(r)}={MathHelper.ToDegrees(MathHelper.NormalizeAngle(r))}");
                return v;
            }
        }



        public IList<IVectorD> Vectors
        {
            get
            {
                return new List<IVectorD> { ShipDirectionVector, ShipPredictionVector };
            }
        }

        public IList<IPoligonD> Poligons => new List<IPoligonD> { TargetPolygon };

        public IList<Text> Texts => new List<Text>();

        #endregion IDrawableObject
    }
}
