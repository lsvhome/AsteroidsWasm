using Asteroids.Standard.Components;
using Asteroids.Standard.Enums;
using Asteroids.Standard.Managers;
using Asteroids.Standard.Screen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using static Asteroids.Standard.MathHelper;

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


        TriangleInfo TargetPredictionTriangleInfo(ShipEnvironmentObjectLocation target)
        {
            if (target?.Velocity == null)
            {
                var zeroPoint = new PointD { X = 0, Y = 0 };
                return new TriangleInfo { A = zeroPoint, B = zeroPoint, C = zeroPoint };
            }
            else
            {
                var bVelocity = Math.Sqrt(Math.Pow(Bullet.InitialVelocity, 2) + Math.Pow(Bullet.InitialVelocity, 2));
                Angle ta = target.CenterRelativeLocationPolar(_ship).Angle;
                //Angle angleBeta = MathHelper.NormalizeAngle(Math.PI + target.VelocityP.Angle + ta );
                Angle angleBeta = MathHelper.NormalizeAngle(Math.PI-(Math.PI - target.VelocityP.Angle - (Math.PI - ta)));
                //var angleBetaD = MathHelper.ToDegrees(angleBeta);
                //var VelocityPAngleD = MathHelper.ToDegrees(target.VelocityP.Angle);
                //var centerAngleD = MathHelper.ToDegrees(target.CenterRelativeLocationPolar(_ship).Angle);
                //System.Diagnostics.Debug.WriteLine(MathHelper.ToDegrees(angleBeta));

                //Target.CenterRelativeLocationPolar(_ship).Angle;
/*
                _ship.Game._textDraw.DrawText(
                    $"A={angleBeta}= {target.VelocityP.Angle} - {target.CenterRelativeLocationPolar(_ship).Angle}"
                    //$"T {target.Velocity}, {Target.VelocityP}"
                    , TextManager.Justify.Center
                    , ScreenCanvas.CanvasHeight / 3 * 2
                    , 100, 200
                );
*/

                var triangle = MathHelper.GetTriangleInfo(_ship.GetCurrentLocation(), target.CenterAbsoluteLocationDecart(_ship), target.VelocityP.Distance, bVelocity, angleBeta);
                return triangle;
            }
        }

        PointD TargetPredictionDecart(ShipEnvironmentObjectLocation target)
        {
            if (target?.Velocity == null)
            {
                return new PointD { X = 0, Y = 0 };
            }
            else
            {
                //var bVelocity = Math.Sqrt(Math.Pow(Bullet.InitialVelocity, 2) + Math.Pow(Bullet.InitialVelocity, 2));
                //var angleBeta = MathHelper.NormalizeAngle( target.VelocityP.Angle - target.CenterRelativeLocationPolar(_ship).Angle);
                //var triangle = MathHelper.GetTriangleInfo(_ship.GetCurrentLocation(), target.CenterAbsoluteLocationDecart(_ship), target.VelocityP.Distance, bVelocity, angleBeta);
                var triangle = TargetPredictionTriangleInfo(target);
                var bulletRelativeToCurrentPositionVector = triangle.C - triangle.A;

                var ret = triangle.C - triangle.B;
                return ret;
            }
        }

        static object _lockObj = new object();
        /// <summary>
        /// Vector from current position to predicted position
        /// </summary>
        VectorD TargetPredictionDecartAbsolute(ShipEnvironmentObjectLocation target)
        {
            //lock (_lockObj)
            {
                var tc = target.CenterAbsoluteLocationDecart(_ship);
                var ret = new VectorD
                {
                    Start = tc,
                    End = tc - TargetPredictionDecart(target)
                };

                return ret;
            }
        }

        PolarCoordinates TargetPredictionPolarRelativeToShip(ShipEnvironmentObjectLocation target)
        {
            //var ret = MathHelper.TransformDecartToPolar(TargetPredictionDecartRelative(target));


            var bVelocity = Math.Sqrt(Math.Pow(Bullet.InitialVelocity, 2) + Math.Pow(Bullet.InitialVelocity, 2));
            var angleBeta = MathHelper.NormalizeAngle(target.VelocityP.Angle - target.CenterRelativeLocationPolar(_ship).Angle);
            var bulletRelativeToCurrentPositionVector = MathHelper.GetTriangleInfo(_ship.GetCurrentLocation(), target.CenterAbsoluteLocationDecart(_ship), target.VelocityP.Distance, bVelocity, angleBeta);
            //var bulletRelativeToCurrentPositionVector = MathHelper.Triangle(_ship.GetCurrentLocation(), target.CenterAbsoluteLocationDecart(_ship), target.VelocityP.Distance, bVelocity, angleBeta);


            Angle angleAlpha = bulletRelativeToCurrentPositionVector.AngleAlphaDirectionPositive ? bulletRelativeToCurrentPositionVector.AngleAlpha : -bulletRelativeToCurrentPositionVector.AngleAlpha;


            var p = new PolarCoordinates { Angle = target.CenterRelativeLocationPolar(_ship).Angle - bulletRelativeToCurrentPositionVector.AngleAlpha, Distance = bulletRelativeToCurrentPositionVector.Lb };


            return p;
        }

        double TragetPredictedAngleToShipDiff(ShipEnvironmentObjectLocation target)
        {
            var a = TargetPredictionPolarRelativeToShip(target).Angle;
            var b = _ship.GetRadians();
            var ret = MathHelper.NormalizeAngle(a - b);
            //Console.WriteLine($"TragetPredictedAngleToShipDiff = ret= = {ret} = {MathHelper.ToDegrees(ret)}, a=  [ {a} = {MathHelper.ToDegrees(a)} ], b= = {b} = {MathHelper.ToDegrees(b)}");
            return ret;
        }
        

        List<VectorD> shoots = new List<VectorD>();
        public void Execute()
        {
            try
            {
                bool allowFire = false;

                var staticView = _env.TransformViewToFPV();

                Target = staticView.OrderByDescending(x => x.ObjectType).ThenBy(x => x.CenterCoordinates.Distance).FirstOrDefault();



                var mostJeopardizeObject = staticView.OrderBy(x => x.Distance).FirstOrDefault();
                if (mostJeopardizeObject?.Distance < 400)
                {
                    foreach (var bullet in _env._cache.GetBulletsAvailable())
                    {
                        bullet.ScreenObject.Shoot(_ship);
                        break;
                    }
                    _ship.Hyperspace();
                    return;
                }





                bool allowFireForShape = false;


                if (Target != null && false)
                {

                    TriangleInfo triangle = TargetPredictionTriangleInfo(Target);

                    var targetAngle = (triangle.C - triangle.A).GetPolarCoordinates().Angle;
                    var diff = Target.CenterRelativeLocationPolar(_ship).Angle;
                    //if (Target.Velocity.HasValue)
                    {
                        diff = TragetPredictedAngleToShipDiff(Target);
                    }


                    _ship.Game._textDraw.DrawText(
                        $" Ship=[{(Angle)_ship.GetRadians()}] targetAngle=[{targetAngle}] diff={diff}"
                        //$"T {target.Velocity}, {Target.VelocityP}"
                        , TextManager.Justify.Center
                        , ScreenCanvas.CanvasHeight / 3 * 2
                        , 100, 200
                    );




                    double targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(diff));




                    var targetDeltaDegreesForSmallRotationSpeed = 2;
                    var shipRotationSpeedFactor = (int)ScreenCanvas.FramesPerSecond / (Math.Abs(targetCenterAngleDegrees));
                    var shipRotationSpeed = shipRotationSpeedFactor * targetCenterAngleDegrees;

                    //if (targetCenterAngleDegrees >= targetDeltaDegreesForSmallRotationSpeed)
                    //{
                    //    shipRotationSpeed = Ship.RotateSpeed;
                    //}

                    if (diff == 0)
                    {
                        //_ship.Rotate(0);
                    }
                    else if (diff > 0 && (_ship.LastRotationSpeedDegrees > 0 || targetCenterAngleDegrees > targetDeltaDegreesForSmallRotationSpeed))
                    {
                        // right
                        _ship.Rotate(shipRotationSpeed);
                    }
                    else if (_ship.LastRotationSpeedDegrees < 0 || targetCenterAngleDegrees > targetDeltaDegreesForSmallRotationSpeed)
                    {
                        // left
                        _ship.Rotate(-shipRotationSpeed);
                    }



                    var diffPolarPredict = TargetPredictionPolarRelativeToShip(Target);
                    
                    //if (Target.Velocity.HasValue)
                    {
                        diff = TragetPredictedAngleToShipDiff(Target);
                    }
                    //double targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(diff));


 
                    
                    // for dots (like misiles)
                    allowFire |= diffPolarPredict.Distance <= Target.Distance && Target.LeftRelativeToCenterAngle == Target.RightRelativeToCenterAngle && Math.Abs(Target.RightRelativeToCenterAngle) <= 0.03;
                    if (allowFire)
                    {
                        var v = new VectorD { Start = _ship.GetCurrentLocation(), End = TargetPredictionDecartAbsolute(Target).End, Color = DrawColor.Orange };
                        shoots.Add(v);
                        if (shoots.Count > 10) { shoots.RemoveAt(0); }
                    }

                    // for shapes
                    allowFireForShape =   
                    (
                        diffPolarPredict.Distance <= Target.Distance && 
                        Math.Abs(diff) < 0.01 
                        //Target.LeftRelativeToCenterAngle + diff < 0.1 && Target.RightRelativeToCenterAngle + diff > -0.1

                        //&& Math.Abs(Target.CenterCoordinates.Angle + diff) < Math.Abs(Target.LeftAngle)
                        //&& Math.Abs(Target.CenterCoordinates.Angle + diff) < Math.Abs(Target.RightAngle)
                        && Target.Distance < Ship.ShipDirectionVectorLenght
                    );
                    
                    allowFire |= allowFireForShape;


                   // if (allowFire)
                    Console.WriteLine($"allowFire = {allowFire} {diff} {Target.LeftRelativeToCenterAngle + diff} = {Target.RightRelativeToCenterAngle + diff}, {Target.Distance}");

  /*                  
                    allowFire |= staticView.Any(t=> {
                        var diff = TragetPredictedAngleToShipDiff(t);
                        var diffPolarPredict = TargetPredictionPolarRelative(Target);
                        //var diff = t.CenterRelativeLocationPolar(_ship).Angle;
                        var ret = (
                            diffPolarPredict.Distance <= t.Distance && 
                            
                            t.LeftRelativeToCenterAngle + diff < 0.1 && t.RightRelativeToCenterAngle + diff > -0.1

                                                                     //&& (Math.Abs(t.CenterCoordinates.Angle + diff) < Math.Abs(t.LeftAngle + diff)
                                                                     //|| Math.Abs(t.CenterCoordinates.Angle + diff) < Math.Abs(t.RightAngle + diff)
                                                                     //)
                                                                     && t.Distance < shipDirectionVectorLenght
                        );

                        if (ret)
                        {
                            
                        }

                        return ret;
                    });
*/
                }
                
                if (allowFire && _ship?.IsAlive == true)
                {
                    if (allowFireForShape)
                    {
                        var t = TargetPredictionDecartAbsolute(Target);
                        Vectors.Add(t);
                    }
                    // Console.WriteLine($"Target angle = [ {MathHelper.ToDegrees(target.CenterCoordinates.Angle)} ], Distance = {target.CenterCoordinates.Distance}, L [{MathHelper.ToDegrees(target.LeftAngle)}] R[{MathHelper.ToDegrees(target.RightAngle)}]");
                    // Fire bullets that are not already moving
                    foreach (var bullet in _env._cache.GetBulletsAvailable())
                    {
                        //bullet.ScreenObject.Shoot(_ship);

                        Sounds.ActionSounds.PlaySound(this, ActionSound.Fire);
                        //_ship._game.Pause();
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

        //private int shipDirectionVectorLenght = 5900;

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
                        squareCenter,
                        new PointD { X = squareCenter.X + sideLength, Y = squareCenter.Y - sideLength},
                        new PointD { X = squareCenter.X - sideLength, Y = squareCenter.Y - sideLength},
                        squareCenter
                    }
                };
                return ret;
            }
        }
        /*
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
        */
        public IList<PointD> Dots => new List<PointD>();

        public IList<IVectorD> Vectors
        {
            get
            {
                var ret = new List<IVectorD>();// { ShipDirectionVector };
                
                //var p = TargetPredictionVector;
                //if (p != null)
                //{
                //    ret.Add(p);
                //}
                if (Target != null)
                {

                    // target movement prediction
                    //IVectorD m = TargetPredictionDecartAbsolute(Target);
                    //var d = MathHelper.TransformPolarToDecart(Target.CenterCoordinates);
                    var t = TargetPredictionTriangleInfo(Target);
                    ret.Add(new VectorD { Start = t.A, End = t.C, Color = DrawColor.Yellow });
                    ret.Add(new VectorD { Start = t.A, End = t.B });
                    ret.Add(new VectorD { Start = t.B, End = t.C, Color = DrawColor.Blue  });

                    // ship optimal position for shooting in current target
                    //var a = TargetPredictionDecartRelativeToShip(Target);
                    //ret.Add(a);

                    //if (Math.Abs(a.End.X - m.End.X) > 0.000001)
                    //{
                    //    Debug.WriteLine($" X1=[{a.End.X}]    X2=[{m.End.X}]");
                    //}
                }

                //ret.AddRange(_env.TransformViewToFPV().Select(v=> TargetPredictionDecartAbsolute(v)).ToArray());
                //ret.AddRange(_env.TransformViewToFPV().Select(v => TargetPredictionDecartAbsolute(v)).ToArray());

                if (shoots.Any())
                {
                    //ret.AddRange(shoots);
                }
                
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
