using Asteroids.Standard.Components;
using Asteroids.Standard.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Asteroids.Standard
{

    internal class ShipEnvironmentFPVManager
    {
        public CacheManager _cache;
        public ShipEnvironmentFPVManager(CacheManager env)
        {
            _cache = env;
        }

        public IEnumerable<ShipEnvironmentObjectLocation> TransformViewToFPV()
        {
            var ret = new List<ShipEnvironmentObjectLocation>();
            try
            {


                if (_cache.Ship == null)
                {
                    throw new InvalidOperationException("Ship cannot be null.");
                }
                
                var misilePoints = _cache.MissilePoints?.ToList();
                if (misilePoints != null)
                {
                    var misilePointsFPV = new List<ShipEnvironmentObjectLocation>();
                    foreach (var obj in misilePoints)
                    {
                        var objLoc = new ShipEnvironmentObjectLocation(TransformViewToFPV(obj, _cache.Ship), 0, 0);

                        objLoc.ObjectType = ObjectType.Misile;
                        objLoc.Distance = objLoc.CenterCoordinates.Distance;
                        misilePointsFPV.Add(objLoc);
                    }
                    var misileAngle = misilePointsFPV.Select(p=>p.CenterCoordinates.Angle.Value).Average();
                    var misileDistance = misilePointsFPV.Select(p => p.CenterCoordinates.Distance).Average();

                    var objLoc1 = new ShipEnvironmentObjectLocation(new PolarCoordinates { Angle = misileAngle, Distance = misileDistance }, 0, 0);

                    objLoc1.ObjectType = ObjectType.Misile;
                    objLoc1.Distance = objLoc1.CenterCoordinates.Distance;
                    ret.Add(objLoc1);
                }

                if (_cache.Saucer != null)
                {
                    var obj = _cache.Saucer;
                    var objLoc = TransformViewToFPV(obj, _cache.Ship);
                    objLoc.ObjectType = ObjectType.Saucer;
                    var allpoints = obj.GetPoints();
                    var half = allpoints.Select(p => Math.Max(Math.Abs(p.X - obj.GetCurrentLocation().X), Math.Abs(p.Y - obj.GetCurrentLocation().Y))).Max();
                    objLoc.Distance = objLoc.CenterCoordinates.Distance - half;
                    objLoc.Velocity = new PointD { X = obj.GetVelocityX(), Y = obj.GetVelocityY() };
                    ret.Add(objLoc);
                }

                var allAsteroids = _cache.GetAsteroids();
                foreach (var obj in allAsteroids)
                {
                    var asteroidRadius = (int)obj.ScreenObject.Size / 2 ;
                    var objLoc = TransformViewToFPV(obj.ScreenObject, _cache.Ship);
                    objLoc.ObjectType = ObjectType.Asteroid;
                    objLoc.Distance = objLoc.CenterCoordinates.Distance - asteroidRadius;
                    objLoc.Velocity = new PointD(obj.ScreenObject.GetVelocityX(), obj.ScreenObject.GetVelocityY());
                    ret.Add(objLoc);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            return ret;
        }

        internal ShipEnvironmentObjectLocation TransformViewToFPV(ScreenObjectBase obj, Ship ship)
        {
            try
            {

                var shipLocation = ship.GetCurrentLocation();
                var shipDirectionRadians = ship.GetRadians();
                var objLocation = obj.GetCurrentLocation();
                //var p = new Point( l.X - shipLocation.X, l.Y - shipLocation.Y );

                var center = TransformViewToFPV(objLocation, ship);
                var left = obj.GetPoints().Select(p => (double)MathHelper.TransformDecartToPolar(new VectorD { Start = shipLocation, End = p }).Angle - shipDirectionRadians).Min();
                var right = obj.GetPoints().Select(p => (double)MathHelper.TransformDecartToPolar(new VectorD { Start = shipLocation, End = p }).Angle - shipDirectionRadians).Max();
                var left1 = center.Angle - left;
                var right1 = right - center.Angle;
                left = MathHelper.NormalizeAngle(left1);
                right = MathHelper.NormalizeAngle(right1);

                // Console.WriteLine($"Obj location = [ {l} ], shipLocation = {shipLocation}, relative [ {p} ] ");
                //Console.WriteLine($"Obj relative to ship angle = [ {center.Angle} = {MathHelper.ToDegrees(center.Angle)} ], obj dist = {center.Distance}");

                return new ShipEnvironmentObjectLocation(center, left1, right1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        internal PolarCoordinates TransformViewToFPV(Point point, Ship ship)
        {
            try
            {
                var v = new VectorD { Start = ship.GetCurrentLocation(), End = point };
                var polarPoint = MathHelper.TransformDecartToPolar(v);
                var sr = ship.GetRadians();
                polarPoint.Angle = MathHelper.NormalizeAngle(polarPoint.Angle - sr);
                return polarPoint;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
