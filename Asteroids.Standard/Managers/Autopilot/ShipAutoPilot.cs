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
                Target = FindTarget(staticView);

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

                if (Target != null)
                {

                    var diff = Target.GetCenterCoordinatesPredicted(_ship).Angle;

                    StatusText.TextVal = $" SHIP=[ {(Angle)_ship.GetRadians()} ] TARGETANGLE=[{Target.CenterRelativeLocationPolar(_ship).Angle}] DIFF=[{diff}]";

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

                    allowFire |= staticView.Any(t =>
                    {
                        var diffPolarPredict = t.GetCenterCoordinatesPredicted(_ship);
                        var ret = (
                            diffPolarPredict.Distance < Ship.ShipDirectionVectorLenght
                            &&
                            Math.Abs(diffPolarPredict.Angle.ValueDegee) < 10
                        );

                        if (ret && t != Target)
                        {

                        }

                        return ret;
                    });

                }

                if (allowFire && _ship?.IsAlive == true)
                {
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

        private ShipEnvironmentObjectLocation FindTarget(IEnumerable<ShipEnvironmentObjectLocation> staticView)
        {
            var targetByAngle = staticView.Select(x => new { x, a = (Angle)x.GetAngleToShip(_ship) }).OrderByDescending(x => x.x.ObjectType).ThenBy(x => x.a).FirstOrDefault();
            if (targetByAngle?.x != null && (targetByAngle?.a.ValueDegee < 10 || targetByAngle?.x.ObjectType != ObjectType.Asteroid))
            {
                return targetByAngle.x;
            }
            else
            {
                var targetBySpeed = staticView.OrderByDescending(x => x.VelocityP.Distance).FirstOrDefault();
                return targetBySpeed;
            }
        }

        #region IDrawableObject

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
                        //squareCenter,
                        new PointD { X = squareCenter.X + sideLength, Y = squareCenter.Y - sideLength},
                        new PointD { X = squareCenter.X - sideLength, Y = squareCenter.Y - sideLength},
                        //squareCenter
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
                    TargetPolygon
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
