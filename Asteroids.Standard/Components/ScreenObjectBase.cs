using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Asteroids.Standard.Helpers;
using Asteroids.Standard.Screen;

namespace Asteroids.Standard.Components
{
    /// <summary>
    /// ScreenObjectBase - defines an object to be displayed on screen
    /// This object is based on a cartesian coordinate system 
    /// centered at 0, 0
    /// </summary>
    internal abstract class ScreenObjectBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="ScreenObjectBase"/>.
        /// </summary>
        /// <param name="location">Absolute origin (bottom-left) of the object.</param>
        protected ScreenObjectBase(Point location)
        {
            IsAlive = true;

            _updatePointsLock = new object();
            _updatePointsTransformedLock = new object();

            //templates are drawn nose "up"
            //Radians = 180 * ScreenCanvas.RadiansPerDegree;
            Radians = 0;

            _points = new List<Point>();
            PointsTransformed = new List<Point>();

            CurrentLocation = location;
        }

        #region State

        /// <summary>
        /// Relative time length at which the object explodes.
        /// </summary>
        protected int ExplosionLength = ScreenCanvas.DefaultExplosionLength;

        /// <summary>
        /// Indicates if the object is alive.
        /// </summary>
        public bool IsAlive { get; protected set; }

        /// <summary>
        /// Blow up the object.
        /// </summary>
        /// <returns>Explosion collection.</returns>
        public virtual IList<Explosion> Explode()
        {
            IsAlive = false;

            VelocityX = 0;
            VelocityY = 0;

            return GetPoints()
                .Select(p => new Explosion(p, ExplosionLength))
                .ToList();
        }

        #endregion

        #region Points for Drawing Object

        private readonly object _updatePointsLock;
        private readonly object _updatePointsTransformedLock;

        /// <summary>
        /// Points is used for the internal cartesian system.
        /// </summary>
        private readonly IList<Point> _points;

        /// <summary>
        /// Points is used for the internal cartesian system with rotation angle applied.
        /// </summary>
        /// <remarks>Exposed to simplify explosions.</remarks>
        protected IList<Point> PointsTransformed;

        /// <summary>
        /// Add points to internal collection used to calculate drawn polygons.
        /// </summary>
        /// <param name="points"><see cref="Point"/> collection to add to internal collections.</param>
        /// <returns>Index of the last point inserted.</returns>
        public int AddPoints(IList<Point> points)
        {
            lock (_updatePointsLock)
                foreach (var point in points)
                    _points.Add(point);

            lock (_updatePointsTransformedLock)
            {
                foreach (var point in points)
                    PointsTransformed.Add(point);

                return PointsTransformed.Count - 1;
            }
        }

        /// <summary>
        /// Returns transformed <see cref="Point"/>s to generate object 
        /// polygon in a thead-safe manner relative to current location on the 
        /// <see cref="ScreenCanvas"/>.
        /// </summary>
        /// <returns>Collection of <see cref="Point"/>s.</returns>
        public IList<Point> GetPoints()
        {
            var points = new List<Point>();

            lock (_updatePointsTransformedLock)
                foreach (var pt in PointsTransformed)
                    points.Add(new Point(
                        pt.X + CurrentLocation.X
                        , pt.Y + CurrentLocation.Y
                    ));

            return points;
        }

        /// <summary>
        /// Clears all internal and transformed <see cref="Point"/>s used to generate 
        /// polygons in a thead-safe manner.
        /// </summary>
        public void ClearPoints()
        {
            lock (_updatePointsLock)
                _points.Clear();

            lock (_updatePointsTransformedLock)
                PointsTransformed.Clear();
        }

        #endregion

        #region Rotation

        /// <summary>
        /// Max number of clockwise radians allow for an <see cref="Align"/> call.
        /// </summary>
        protected const double RotationLimit = 5 * ScreenCanvas.RadiansPerDegree;

        /// <summary>
        /// Max number of counter-clockwise radians allow for an <see cref="Align"/> call.
        /// </summary>
        protected const double RotationLimitNeg = -5 * ScreenCanvas.RadiansPerDegree;

        /// <summary>
        /// Get the current rotational radians.
        /// </summary>
        public Angle GetRadians() => Radians;

        /// <summary>
        /// Current rotation.
        /// </summary>
        protected Angle Radians;

        /// <summary>
        /// Rotates all internal <see cref="Point"/>s used to generate polygons on draw
        /// based on the alignment with the target point but no more then 5 degrees at a time.
        /// </summary>
        /// <param name="alignPoint"><see cref="Point"/> to target.</param>
        protected void Align(Point alignPoint)
        {
            var t = MathHelper.TransformDecartToPolar(new VectorD { Start = CurrentLocation, End = alignPoint });
            //var radsToPoint = GeometryHelper.GetAngle(CurrentLocation, alignPoint);
            //if (radsToPoint != t.Angle)
            //{ 
            //}
            var delta = t.Angle - Radians;

            Radians += delta >= 0
                ? Math.Min(delta, RotationLimit)
                : Math.Max(delta, RotationLimitNeg);

            RotateInternal();
        }

        /// <summary>
        /// Rotates all internal <see cref="Point"/>s used to generate polygons on draw
        /// by a number of decimal degrees.
        /// </summary>
        /// <param name="degrees">Rotation amount in degrees.</param>
        protected internal virtual void Rotate(double degrees)
        {
            //Get radians in 1/FramesPerSecond increments
            var radiansAdjust = degrees * ScreenCanvas.RadiansPerDegree;
            Radians += radiansAdjust / ScreenCanvas.FramesPerSecond;

            RotateInternal();
        }

        /// <summary>
        /// Rotates all internal <see cref="Point"/>s used to generate polygons on draw
        /// based on decimal degrees.
        /// </summary>
        private void RotateInternal()
        {
            //Radians = Radians % ScreenCanvas.RadiansPerCircle;
            Radians = MathHelper.NormalizeAngle(Radians);
            var radians = Radians;
            Angle r = Radians;
            var sinVal = Math.Sin(radians);
            var cosVal = Math.Cos(radians);

            //Get points with some thread safety
            var newPointsTransformed = new List<Point>();
            var newPointsTransformed2 = new List<Point>();

            var points = new List<Point>();
            lock (_updatePointsLock)
                points.AddRange(_points);

            //Re-transform the points
            var ptTransformed = new Point(0, 0);
            var ptTransformed2 = new Point(0, 0);
            foreach (var pt in points)
            {
                ptTransformed.X = (int)(pt.X * cosVal - pt.Y * sinVal);
//#warning #error here
                ptTransformed.Y = (int)(pt.X * sinVal + pt.Y * cosVal);

                ptTransformed2 = RotateInternal(radians, pt);
                newPointsTransformed.Add(ptTransformed);
                newPointsTransformed2.Add(ptTransformed2);
                if (ptTransformed.X != ptTransformed2.X)
                { 
                }
                if (ptTransformed.Y != ptTransformed2.Y)
                {
                }
            }

            //Add the points
            lock (_updatePointsTransformedLock)
            {
                PointsTransformed.Clear();
                foreach (var pt in newPointsTransformed2)
                    PointsTransformed.Add(pt);
            }
        }
        
        private Point RotateInternal(double Radians, Point pt)
        {
            return MathHelper.RotateInternal(Radians, pt);
        }

        #endregion

        #region Movement

        /// <summary>
        /// Get the current absolute origin (top-left) of the object.
        /// </summary>
        public Point GetCurrentLocation() => CurrentLocation;

        /// <summary>
        /// Current absolute origin (top-left).
        /// </summary>
        protected Point CurrentLocation;

        /// <summary>
        /// Get the current velocity along the X axis.
        /// </summary>
        public double GetVelocityX() => VelocityX;

        /// <summary>
        /// Current velocity along the X axis.
        /// </summary>
        protected double VelocityX;

        /// <summary>
        /// Get the current velocity along the Y axis.
        /// </summary>
        public double GetVelocityY() => VelocityY;

        /// <summary>
        /// Current velocity along the Y axis.
        /// </summary>
        protected double VelocityY;

        /// <summary>
        /// Move the object a single increment based on <see cref="GetVelocityX"/>
        /// and <see cref="GetVelocityY"/> and set current location.
        /// </summary>
        /// <returns>Indication of the move being completed successfully.</returns>
        public virtual bool Move()
        {
            CurrentLocation.X += (int)VelocityX;
            CurrentLocation.Y += (int)VelocityY;

            if (CurrentLocation.X < 0)
                CurrentLocation.X = ScreenCanvas.CanvasWidth - 1;
            if (CurrentLocation.X >= ScreenCanvas.CanvasWidth)
                CurrentLocation.X = 0;

            if (CurrentLocation.Y < 0)
                CurrentLocation.Y = ScreenCanvas.CanvasHeight - 1;
            if (CurrentLocation.Y >= ScreenCanvas.CanvasHeight)
                CurrentLocation.Y = 0;

            return true;
        }

        #endregion
        
    }
}
