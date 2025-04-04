using Asteroids.Standard.Components;
using Asteroids.Standard.Enums;
using Asteroids.Standard.Screen;
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

        /// <summary>
        /// Target relative location regarding to ship (ship FPV).
        /// </summary>
        internal ShipEnvironmentObjectLocation? Target { get; private set; }

        PointD TargetPredictionDecart(ShipEnvironmentObjectLocation target)
        {
            if (target?.Velocity == null)
            {
                return new PointD { X = 0, Y = 0 };
            }
            else
            {
                var timeout = target.CenterRelativeLocationPolar(_ship).Distance / Bullet.InitialVelocity;
                var x = target.Velocity!.Value.X * timeout;
                var y = target.Velocity!.Value.Y * timeout;
                var ret = new PointD { X = x, Y = y };
                return ret;
            }
        }


        /// <summary>
        /// Vector from current position to predicted position
        /// </summary>
        VectorD TargetPredictionDecartAbsolute(ShipEnvironmentObjectLocation target)
        {
            var ret = new VectorD
            {
                Start = target.CenterAbsoluteLocationDecart(_ship),
                End = target.CenterAbsoluteLocationDecart(_ship) + TargetPredictionDecart(target)
            };

            return ret;
        }

        VectorD TargetPredictionDecartRelative(ShipEnvironmentObjectLocation target)
        {
            var s = _ship.GetCurrentLocation();
            var ret = new VectorD { Start = s, End = TargetPredictionDecartAbsolute(target).End };
            return ret;
        }

        PolarCoordinates TargetPredictionPolarRelative(ShipEnvironmentObjectLocation target)
        {
            var ret = MathHelper.TransformDecartToPolar(TargetPredictionDecartRelative(target));
            return ret;
        }

        double TragetPredictedAngleToShipDiff(ShipEnvironmentObjectLocation target)
        {
            var a = TargetPredictionPolarRelative(target).Angle;
            var b = _ship.GetRadians();
            var ret = MathHelper.NormalizeAngle(a - b);
            //Console.WriteLine($"TragetPredictedAngleToShipDiff = ret= = {ret} = {MathHelper.ToDegrees(ret)}, a=  [ {a} = {MathHelper.ToDegrees(a)} ], b= = {b} = {MathHelper.ToDegrees(b)}");
            return ret;
        }

        public void Execute()
        {
            try
            {
                bool allowFire = false;

                var staticView = _env.TransformViewToFPV();

                Target = staticView.OrderByDescending(x => x.ObjectType).ThenBy(x => x.CenterCoordinates.Distance).FirstOrDefault();



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

                if (Target != null)
                {
                    var shipRotationSpeedFactor = (int)ScreenCanvas.FramesPerSecond / 5;
                    var diff = Target.CenterRelativeLocationPolar(_ship).Angle;
                    //if (Target.Velocity.HasValue)
                    {
                        diff = TragetPredictedAngleToShipDiff(Target);
                    }
                    double targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(diff));


                    var shipRotationSpeed = shipRotationSpeedFactor * targetCenterAngleDegrees;

                    var targetDeltaDegreesForSmallRotationSpeed = 5;
                    if (targetCenterAngleDegrees >= targetDeltaDegreesForSmallRotationSpeed)
                    {
                        shipRotationSpeed = Ship.RotateSpeed;
                    }

                    if (diff == 0)
                    {
                    }
                    else if (diff > 0)
                    {
                        // right
                        _ship.Rotate(shipRotationSpeed);
                        if (Math.Abs(TragetPredictedAngleToShipDiff(Target)) < 0.001)
                        {
                        }
                    }
                    else
                    {
                        // left
                        _ship.Rotate(-shipRotationSpeed);
                        if (Math.Abs(TragetPredictedAngleToShipDiff(Target)) < 0.001)
                        {
                        }
                    }
                    
                    // for dots (like misiles)
                    allowFire |= Target.LeftRelativeToCenterAngle == Target.RightRelativeToCenterAngle && Math.Abs(Target.RightRelativeToCenterAngle) <= 0.03;

                    // for shapes
                    allowFire |= (
                        Target.LeftRelativeToCenterAngle + diff < 0.1 && Target.RightRelativeToCenterAngle + diff > -0.1

                                                                      //&& Math.Abs(Target.CenterCoordinates.Angle + diff) < Math.Abs(Target.LeftAngle)
                                                                      //&& Math.Abs(Target.CenterCoordinates.Angle + diff) < Math.Abs(Target.RightAngle)
                                                                      && Target.Distance < shipDirectionVectorLenght
                    );

                    Console.WriteLine($"allowFire = {Target.LeftRelativeToCenterAngle + diff} = {Target.RightRelativeToCenterAngle + diff}, {Target.Distance}");

                }

                

                allowFire |= staticView.Any(t=> {
                    var diff = TragetPredictedAngleToShipDiff(t);
                    //var diff = t.CenterRelativeLocationPolar(_ship).Angle;
                    var ret = (
                    t.LeftRelativeToCenterAngle + diff < 0.1 && t.RightRelativeToCenterAngle + diff > -0.1

                    //&& (Math.Abs(t.CenterCoordinates.Angle + diff) < Math.Abs(t.LeftAngle + diff)
                    //|| Math.Abs(t.CenterCoordinates.Angle + diff) < Math.Abs(t.RightAngle + diff)
                    //)
                    && t.Distance < shipDirectionVectorLenght
                    );
                    return ret;
                });
                
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
                //var cl = _ship.GetCurrentLocation();
                //var r = _ship.GetRadians();
                //var dv = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = _ship.GetRadians(), Distance = Target.Distance });
                
                var squareCenter = Target.CenterAbsoluteLocationDecart(_ship);// new Point(cl.X + dv.X, cl.Y + dv.Y);
                //Console.WriteLine($"Target center = {squareCenter}, cl={cl}, dv={dv}, cc={Target.CenterCoordinates}");
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

        [Obsolete("error", true)]
        private Poligon? TargetPredictionPolygon
        {
            get
            {
                if (Target == null)
                {
                    return null;
                }
                int sideLength = 120;
                var cl = _ship.GetCurrentLocation();
                var r = _ship.GetRadians();
                var dv = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = _ship.GetRadians(), Distance = Target.Distance });

                if (Target?.Velocity != null)
                {
                    var vx = Bullet.InitialVelocity + _ship.GetVelocityX();
                    var vy = Bullet.InitialVelocity + _ship.GetVelocityY();
                    var dx = Target.Distance * -Math.Sin(Target.CenterCoordinates.Angle);
                    var dy = Target.Distance * Math.Cos(Target.CenterCoordinates.Angle);
                    var t = (dx / vx + dy / vy) / 2;

                    var pX = Target.Velocity.Value.X * t;
                    var pY = Target.Velocity.Value.Y * t;





                    //var squareCenter = new Point((int)(cl.X + dv.X + pX), (int)(cl.Y + dv.Y + pY));

                    //Console.WriteLine($"TargetP center = {squareCenter}, cl={cl}, dv={dv}, cc={Target.CenterCoordinates}");

                    var squareCenter = this.TargetPredictionVector?.End;
                    if (squareCenter == null)
                        return null;

                    var ret = new Poligon
                    {
                        Color = DrawColor.Blue,
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

                return null;
            }
        }

        private VectorD ShipDirectionVector
        {
            get
            {
                var cl = (PointD)_ship.GetCurrentLocation();
                var r = _ship.GetRadians();
                var dv = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = _ship.GetRadians(), Distance = shipDirectionVectorLenght });

                var end = cl + dv;
                var v = new VectorD
                {
                    Start = cl,
                    End = end,
                    Color = DrawColor.Red
                };

                //Console.WriteLine($"Ship direction vector: {dx} {dy} ; r = {r}={MathHelper.ToDegrees(r)}:= {MathHelper.NormalizeAngle(r)}={MathHelper.ToDegrees(MathHelper.NormalizeAngle(r))}");
                return v;
                //return TargetPredictionVector ?? v;
            }
        }



        [Obsolete("error", true)]
        private VectorD? TargetMovementVector
        {
            get
            {
                try
                {


                    if (Target == null)
                    {
                        return null;
                    }

                    var cl = _ship.GetCurrentLocation();
                    var r = _ship.GetRadians();
                    var start = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = r+Target.CenterCoordinates.Angle, Distance = Target.CenterCoordinates.Distance });

                    if (Target?.Velocity != null)
                    {
                        var vx = Bullet.InitialVelocity + _ship.GetVelocityX();
                        var vy = Bullet.InitialVelocity + _ship.GetVelocityY();
                        var dx = Target.Distance * -Math.Sin(Target.CenterCoordinates.Angle);
                        var dy = Target.Distance * Math.Cos(Target.CenterCoordinates.Angle);
                        var t = (dx / vx + dy / vy) / 2;

                        var pX = Target.Velocity.Value.X * t;
                        var pY = Target.Velocity.Value.Y * t;

                        ///var pDirection = MathHelper.TransformDecartToPolar(new PointD { X = dx+pX, Y = dy+pY });
                        var x = Target.CenterRelativeLocationPolar(_ship);

                        var ret = new VectorD
                        {
                            Start = new PointD { X = cl.X+ start.X, Y = cl.Y+ start.Y },
                            End = new PointD { X = cl.X + start.X + pX, Y = cl.Y + start.Y + pY },
                            Color = DrawColor.Yellow
                        };

                        return ret;
                    }

                    return null;


                    //    var squareCenter = new Point((int)(cl.X + dv.X + pX), (int)(cl.Y + dv.Y + pY));


                    //    var cl = _ship.GetCurrentLocation();
                    //var r = _ship.GetRadians()+0.1;
                    //var dx = -Math.Sin(r);
                    //var dy = Math.Cos(r);
                    //var start = new PointD { X = cl.X, Y = cl.Y };
                    //var end = new PointD { X = start.X + shipDirectionVectorLenght * dx, Y = start.Y + shipDirectionVectorLenght * dy };
                    //var v = new VectorD
                    //{
                    //    Start = start,
                    //    End = end,
                    //    Color = DrawColor.Yellow
                    //};

                    //Console.WriteLine($"Ship direction vector: {dx} {dy} ; r = {r}={MathHelper.ToDegrees(r)}:= {MathHelper.NormalizeAngle(r)}={MathHelper.ToDegrees(MathHelper.NormalizeAngle(r))}");
                    //return v;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }

        [Obsolete("error", true)]
        private VectorD? TargetPredictionVector
        {
            get
            {
                try
                {

                    var cl = _ship.GetCurrentLocation();

                    if (Target == null)
                {
                    return null;
                }
                    var t1 = this.TargetMovementVector;
                    if (t1 != null)
                    {
                        return new VectorD { Start= cl, End= t1.End };
                        //var k = MathHelper.TransformDecartToPolar(t.End);

                    }

                var r = _ship.GetRadians();
                var dv = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = _ship.GetRadians(), Distance = Target.Distance });

                if (Target?.Velocity != null)
                {
                    var vx = Bullet.InitialVelocity + _ship.GetVelocityX();
                    var vy = Bullet.InitialVelocity + _ship.GetVelocityY();
                    var dx = Target.Distance * -Math.Sin(Target.CenterCoordinates.Angle);
                    var dy = Target.Distance * Math.Cos(Target.CenterCoordinates.Angle);
                    var t = (dx / vx + dy / vy) / 2;

                    var pX = Target.Velocity.Value.X * t;
                    var pY = Target.Velocity.Value.Y * t;

                    ///var pDirection = MathHelper.TransformDecartToPolar(new PointD { X = dx+pX, Y = dy+pY });

                    var ret = new VectorD
                    {
                        Start = new PointD { X = cl.X, Y = cl.Y },
                        End = new PointD { X = cl.X + dx + pX, Y = cl.Y + dy + pY },
                        Color = DrawColor.Blue
                    };

                    return ret;
                }

                return null;


                    //    var squareCenter = new Point((int)(cl.X + dv.X + pX), (int)(cl.Y + dv.Y + pY));


                    //    var cl = _ship.GetCurrentLocation();
                    //var r = _ship.GetRadians()+0.1;
                    //var dx = -Math.Sin(r);
                    //var dy = Math.Cos(r);
                    //var start = new PointD { X = cl.X, Y = cl.Y };
                    //var end = new PointD { X = start.X + shipDirectionVectorLenght * dx, Y = start.Y + shipDirectionVectorLenght * dy };
                    //var v = new VectorD
                    //{
                    //    Start = start,
                    //    End = end,
                    //    Color = DrawColor.Yellow
                    //};

                    //Console.WriteLine($"Ship direction vector: {dx} {dy} ; r = {r}={MathHelper.ToDegrees(r)}:= {MathHelper.NormalizeAngle(r)}={MathHelper.ToDegrees(MathHelper.NormalizeAngle(r))}");
                    //return v;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }



        public IList<IVectorD> Vectors
        {
            get
            {
                var ret = new List<IVectorD> { ShipDirectionVector };
                //var p = TargetPredictionVector;
                //if (p != null)
                //{
                //    ret.Add(p);
                //}
                if (Target != null)
                {
                    IVectorD m = TargetPredictionDecartAbsolute(Target);
                    ret.Add(m);

                    var a = TargetPredictionDecartRelative(Target);
                    ret.Add(a);
                }

                ret.AddRange(_env.TransformViewToFPV().Select(v=> TargetPredictionDecartAbsolute(v)).ToArray());

                return ret;
            }
        }

        public IList<IPoligonD> Poligons
        {
            get
            {
                var ret = new List<IPoligonD> { TargetPolygon };
                //if (TargetPredictionPolygon != null)
                //{
                //    ret.Add(TargetPredictionPolygon);
                //}

                return ret;
            }
        }

        public IList<Text> Texts => new List<Text>();

        #endregion IDrawableObject
    }
}
