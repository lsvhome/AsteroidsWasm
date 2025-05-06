#nullable disable
using Asteroids.Standard.Components;
using Asteroids.Standard.Enums;
using Asteroids.Standard.Helpers;
using Asteroids.Standard.Managers;
using Asteroids.Standard.Screen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
        internal ShipEnvironmentObjectLocation Target { get; private set; }

        public void Execute()
        {
            try
            {
                bool allowFire = false;

                var staticView = _env.TransformViewToFPV();

                Target = staticView.OrderByDescending(x => x.ObjectType).ThenBy(x => x.CenterCoordinates.Distance).FirstOrDefault();



                var mostJeopardizeObject = staticView.OrderBy(x => x.Distance).FirstOrDefault();

                if (mostJeopardizeObject?.Distance < 800)
                {
                    if (Math.Abs(mostJeopardizeObject.CenterCoordinates.Angle.ValueDegee) > 90)
                    {
                        foreach (var bullet in _env._cache.GetBulletsAvailable())
                        {
                            bullet.ScreenObject.Shoot(_ship);
                            break;
                        }
                        _ship.Thrust();
                        foreach (var bullet in _env._cache.GetBulletsAvailable())
                        {
                            bullet.ScreenObject.Shoot(_ship);
                            break;
                        }
                    }
                }

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

                
                var inFrame = GeometryHelper.IsInsidePolygon(
                    (Point)_ship.GetCurrentLocation(),
                    _ship.IntFrame()
                );

                if (!inFrame)
                {
                    Point center = new Point(ScreenCanvas.CanvasWidth / 2, ScreenCanvas.CanvasHeight / 2);
                    VectorD v = new VectorD
                    {
                        Start = _ship.GetCurrentLocation(),
                        End = center
                    };

                    var centerDirection = MathHelper.TransformDecartToPolar(v);

                    var diff = MathHelper.NormalizeAngle(centerDirection.Angle) - MathHelper.NormalizeAngle(_ship.GetRadians());

                    if (Math.Abs(diff) < MathHelper.ToRadians(30))
                    {
                        _ship.Thrust();
                    }
                }



                //bool allowFireForShape = false;


                if (Target != null /*&& false*/)
                {




                    var diff = Target.CenterCoordinates.Angle;//.CenterRelativeLocationPolar(_ship).Angle;
                    //if (Target.Velocity.HasValue)
                    //{
                    //    TriangleInfo triangle = TargetPredictionTriangleInfo(Target);
                    //    var targetAngle = (triangle.C - triangle.A).GetPolarCoordinates().Angle;

                    //    var diff2 = targetAngle - _ship.GetRadians();

                    //    diff = MathHelper.NormalizeAngle( diff2);
                    //}

                    diff = Target.GetCenterCoordinatesPredicted(_ship).Angle;


                    StatusText.TextVal = $" SHIP=[ {(Angle)_ship.GetRadians()} ] TARGETANGLE=[{Target.CenterRelativeLocationPolar(_ship).Angle}] DIFF=[{diff}]";
                    //_ship.Game._textDraw.DrawText(
                    //    
                    //    //$"T {target.Velocity}, {Target.VelocityP}"
                    //    , TextManager.Justify.Center
                    //    , ScreenCanvas.CanvasHeight / 3 * 2
                    //    , 100, 200
                    //);

                    //Console.WriteLine($"allowFire = {allowFire} {diff} {Target.LeftRelativeToCenterAngle + diff} = {Target.RightRelativeToCenterAngle + diff}, {Target.Distance}");

                    Angle rotateAngle = 0;

                    if (Math.Abs(diff) > MathHelper.ToRadians(3))
                    {
                        rotateAngle = Math.Sign(diff.Value) * Ship.MaxRotateSpeedRadians;
                    }
                    else
                    {
                        if (Math.Sign(_ship.LastRotationSpeedDegrees) == Math.Sign(diff.Value))
                        {
                            rotateAngle = diff.Value * ScreenCanvas.FramesPerSecond;
                        }
                    }

                    if (rotateAngle != 0)
                    {
                        var rotatedAngle = _ship.Rotate(rotateAngle);
                        foreach (var each in staticView)
                        {
                            each.CenterCoordinates.Angle -= rotatedAngle;
                        }
                    }

                    //Target.CenterCoordinates.Angle += rotateAngle;


                    //var diffPolarPredict = TargetPredictionPolarRelativeToShip(Target);

                    //if (Target.Velocity.HasValue)
                    //{
                    //diff = TragetPredictedAngleToShipDiff(Target);


                    //if (Target.Velocity.HasValue)
                    //{
                    //    TriangleInfo triangle2 = TargetPredictionTriangleInfo(Target);
                    //    var targetAngle = (triangle2.C - triangle2.A).GetPolarCoordinates().Angle;

                    //    var diff2 = targetAngle - _ship.GetRadians();
                    //    if (Math.Abs(diff.ValueDegee) <= 2)
                    //    { 
                    //    }
                    //    diff = diff2;
                    //}
                    //}
                    //double targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(diff));



                    //diff = Target.CenterCoordinates.Angle;//.CenterRelativeLocationPolar(_ship).Angle;
                    //if (Target.Velocity.HasValue)
                    //{
                    //    TriangleInfo triangle = TargetPredictionTriangleInfo(Target);
                    //    var targetAngle = (triangle.C - triangle.A).GetPolarCoordinates().Angle;

                    //    var diff2 = targetAngle - _ship.GetRadians();

                    //    diff = MathHelper.NormalizeAngle(diff2);
                    //}
                    diff = Target.GetCenterCoordinatesPredicted(_ship).Angle;



                    if (Math.Abs(diff) < MathHelper.ToRadians(1))
                    {
                        allowFire |= true;

                    }

                    foreach (var each in staticView)
                    {
                        if (each == Target)
                        {
                            continue;
                        }

                        var eachDiff = each.GetCenterCoordinatesPredicted(_ship).Angle;
                        if (Math.Abs(eachDiff) < MathHelper.ToRadians(3))
                        {
                            allowFire |= true;
                        }
                    }

                    /*
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
                    */

                    // if (allowFire)
                    //Console.WriteLine($"allowFire = {allowFire} {diff} {Target.LeftRelativeToCenterAngle + diff} = {Target.RightRelativeToCenterAngle + diff}, {Target.Distance}");


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
                    //if (allowFireForShape)
                    //{
                    //    var t = TargetPredictionDecartAbsolute(Target);
                    //    Vectors.Add(t);
                    //}
                    // Console.WriteLine($"Target angle = [ {MathHelper.ToDegrees(target.CenterCoordinates.Angle)} ], Distance = {target.CenterCoordinates.Distance}, L [{MathHelper.ToDegrees(target.LeftAngle)}] R[{MathHelper.ToDegrees(target.RightAngle)}]");
                    // Fire bullets that are not already moving
                    foreach (var bullet in _env._cache.GetBulletsAvailable())
                    {
                        bullet.ScreenObject.Shoot(_ship);
                        /*
                        var c = _ship.GetCurrentLocation();
                        var v = new VectorD { Start = c, End = c, Color = DrawColor.Orange };
                        v.SetPolarCoordinates(new PolarCoordinates { Angle = _ship.GetRadians(), Distance = 10000 });
                        lock (_ship._lock_BulletDirections)
                        {
                            _ship.BulletDirections.Add(v);
                        }
                        Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(t =>
                        {
                            lock (_ship._lock_BulletDirections)
                            {
                                _ship.BulletDirections.Remove(v);
                            }
                        });
                        */
                        //_ship.Game.Pause();


                        //Sounds.ActionSounds.PlaySound(this, ActionSound.Fire);
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
                    Color = Color.Red,
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

/*
                if (Target != null)
                {

                    // target movement prediction
                    var t = TargetPredictionTriangleInfo(Target);
                    ret.Add(new VectorD { Start = t.A, End = t.C, Color = DrawColor.Yellow });
                    ret.Add(new VectorD { Start = t.A, End = t.B });
                    ret.Add(new VectorD { Start = t.B, End = t.C, Color = DrawColor.Blue });
                }

                if (shoots.Any())
                {
                    //ret.AddRange(shoots);
                }
*/
                return ret;
            }
        }

        public IList<IPoligonD> Poligons
        {
            get
            {
                /*
                if (Target != null)
                {

                    TriangleInfo triangle = TargetPredictionTriangleInfo(Target);

                    var targetAngle = (triangle.C - triangle.A).GetPolarCoordinates().Angle;
                    var diff = Target.CenterRelativeLocationPolar(_ship).Angle;
                    //if (Target.Velocity.HasValue)
                    {
                        diff = TragetPredictedAngleToShipDiff(Target);
                    }


                    //_ship.Game._textDraw.DrawText(
                    //    $" SHIP=[ {(Angle)_ship.GetRadians()} ] TARGETANGLE=[{targetAngle}] DIFF=[{diff}]"
                    //    //$"T {target.Velocity}, {Target.VelocityP}"
                    //    , TextManager.Justify.Center
                    //    , ScreenCanvas.CanvasHeight / 3 * 2
                    //    , 100, 200
                    //);
                }
                */

                var ret = new List<IPoligonD>
                {
                    //    TargetPolygon 
                };
                //if (TargetPredictionPolygon != null)
                //{
                //    ret.Add(TargetPredictionPolygon);
                //}

                return ret;
            }
        }

        DrawableText StatusText = new DrawableText(
                        $"",
                        new PointD { X = 100, Y = 500 },
                        Color.Red,
                        DrawableText.Justify.Center,
                        500,
                        100, 200
                        );

        public IList<DrawableText> Texts
        {
            get
            {
                var ret = new List<DrawableText>();

                /*
                                if (Target != null)
                                {

                                    TriangleInfo triangle = TargetPredictionTriangleInfo(Target);

                                    var targetAngle = (triangle.C - triangle.A).GetPolarCoordinates().Angle;
                                    var diff = Target.CenterRelativeLocationPolar(_ship).Angle;
                                    //if (Target.Velocity.HasValue)
                                    {
                                        diff = TragetPredictedAngleToShipDiff(Target);
                                    }


                                    if (!string.IsNullOrWhiteSpace(StatusText.TextVal))
                                        ret.Add(StatusText);
                                }

                */
                return ret;
            }
        }

        #endregion IDrawableObject
    }
}
#nullable restore
