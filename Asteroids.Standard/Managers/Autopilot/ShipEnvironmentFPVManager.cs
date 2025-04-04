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
                
                var misiles = _cache.MissilePoints?.ToList();
                if (misiles != null)
                {
                    foreach (var obj in misiles)
                    {
                        var objLoc = new ShipEnvironmentObjectLocation(TransformViewToFPV(obj, _cache.Ship), 0, 0);

                        objLoc.ObjectType = ObjectType.Misile;
                        objLoc.Distance = objLoc.CenterCoordinates.Distance;
                        ret.Add(objLoc);
                    }
                }

                if (_cache.Saucer != null)
                {
                    var objLoc = TransformViewToFPV(_cache.Saucer, _cache.Ship);
                    objLoc.ObjectType = ObjectType.Saucer;
                    var allpoints = _cache.Saucer.GetPoints();
                    var half = allpoints.Select(p => Math.Max(Math.Abs(p.X- _cache.Saucer.GetCurrentLocation().X), Math.Abs(p.Y- _cache.Saucer.GetCurrentLocation().Y))).Max();
                    objLoc.Distance = objLoc.CenterCoordinates.Distance - half;
                    ret.Add(objLoc);
                }

                var allAsteroids = _cache.GetAsteroids();
                foreach (var obj in allAsteroids)
                {
                    var asteroidRadius = (int)obj.ScreenObject.Size / 2 ;
                    var objLoc = TransformViewToFPV(obj.ScreenObject, _cache.Ship);
                    objLoc.ObjectType = ObjectType.Asteroid;
                    objLoc.Distance = objLoc.CenterCoordinates.Distance - asteroidRadius;
                    objLoc.Velocity = new PointF((float)obj.ScreenObject.GetVelocityX(), (float)obj.ScreenObject.GetVelocityY());
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
                var left = obj.GetPoints().Select(p => TransformDecartToPolar(shipLocation, p).Angle - shipDirectionRadians).Min();
                var right = obj.GetPoints().Select(p => TransformDecartToPolar(shipLocation, p).Angle - shipDirectionRadians).Max();
                var left1 = center.Angle - left;
                var right1 = right - center.Angle;
                left = NormalizeAngle(left1);
                right = NormalizeAngle(right1);

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
                var polarPoint = TransformDecartToPolar(ship.GetCurrentLocation(), point);
                polarPoint.Angle = NormalizeAngle(polarPoint.Angle - ship.GetRadians());
                return polarPoint;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        
        internal PolarCoordinates TransformDecartToPolar(Point observer, Point target)
        {
            try
            {
                Point relativeLocation = new Point(target.X - observer.X, target.Y - observer.Y);
                return MathHelper.TransformDecartToPolar(relativeLocation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        public static double NormalizeAngle(double angle)
        {
            while (angle < -Math.PI)
                angle += 2 * Math.PI;
            while (angle > Math.PI)
                angle -= 2 * Math.PI;
            return angle;   
        }
    }
}
