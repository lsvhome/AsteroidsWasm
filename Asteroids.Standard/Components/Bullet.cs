using System;
using System.Collections.Generic;
using System.Drawing;
using Asteroids.Standard.Enums;
using System.Linq;
using Asteroids.Standard.Screen;

namespace Asteroids.Standard.Components
{
    /// <summary>
    /// Bullet is a missile fired by an object (ship or UFO)
    /// </summary>
    internal sealed class Bullet : ScreenObjectBase, IDrawableObject
    {
        private int _remainingFrames;

        public Bullet() : base(new Point(0, 0))
        {
            _remainingFrames = 0;
            InitPoints();
        }

        /// <summary>
        /// Setup the point template for the bullet.
        /// </summary>
        private void InitPoints()
        {
            ClearPoints();
            AddPoints(PointsTemplate);
        }

        /// <summary>
        /// Indicates if the bullet is current shooting.
        /// </summary>
        public bool IsInFlight => _remainingFrames > 0;

        /// <summary>
        /// Prevents the bullet from being redrawn.
        /// </summary>
        public void Disable()
        {
            _remainingFrames = 0;
        }

        public static double InitialVelocity = 100;
        /// <summary>
        /// Fire the bullet from a parent ship.
        /// </summary>
        /// <param name="parentShip">Parent <see cref="Ship"/> the bullet was fired from.</param>
        public void Shoot(Ship parentShip)
        {
            _remainingFrames = (int)ScreenCanvas.FramesPerSecond; // bullets live 1 sec
            CurrentLocation = parentShip.GetCurrentLocation();
            Radians = parentShip.GetRadians();

            var sinVal = Math.Sin(Radians);
            var cosVal = Math.Cos(Radians);

            VelocityX = (int)(-InitialVelocity * sinVal) + parentShip.GetVelocityX();
            VelocityY = (int)(InitialVelocity * cosVal) + parentShip.GetVelocityY();
        }

        /// <summary>
        /// Decrement the bullets life and move.
        /// </summary>
        /// <returns></returns>
        public override bool Move()
        {
            // only draw if in flight
            if (!IsInFlight)
                return false;

            _remainingFrames -= 1;
            return base.Move();
        }

        #region Statics

        /// <summary>
        /// Non-transformed point template for creating a new bullet.
        /// </summary>
        private static readonly IList<Point> PointsTemplate = new List<Point>();

        /// <summary>
        /// Setup the point templates.
        /// </summary>
        static Bullet()
        {
            const int bulletSize = 35;

            PointsTemplate.Add(new Point(0, -bulletSize));
            PointsTemplate.Add(new Point(bulletSize, 0));
            PointsTemplate.Add(new Point(0, bulletSize));
            PointsTemplate.Add(new Point(-bulletSize, 0));
        }

        #endregion

        #region IDrawableObject

        public IList<PointD> Dots => new List<PointD>();

        public IList<IVectorD> Vectors => new List<IVectorD>();

        public IList<IPoligonD> Poligons => new List<IPoligonD> { new Poligon { Color = DrawColor.White, Points = GetPoints().Select(p => new PointD { X = p.X, Y = p.Y }).ToList() } };

        public IList<Text> Texts => new List<Text>();

        #endregion IDrawableObject
    }
}
