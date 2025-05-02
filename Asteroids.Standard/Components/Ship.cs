using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Asteroids.Standard.Enums;
using Asteroids.Standard.Helpers;
using Asteroids.Standard.Managers;
using Asteroids.Standard.Screen;
using static Asteroids.Standard.MathHelper;
using static Asteroids.Standard.Sounds.ActionSounds;

namespace Asteroids.Standard.Components
{
    /// <summary>
    /// Primary craft for the user to control.
    /// </summary>
    internal sealed class Ship : ScreenObjectBase, IDrawableObject
    {
        /// <summary>
        /// ~Pi = 179.999 degrees (in radians).
        /// </summary>
        internal const double MaxRotateSpeedRadians = Math.PI-0.01;

        public Game Game { get; private set; }
        /// <summary>
        /// Creates and immediately draws an instance of <see cref="Ship"/>.
        /// </summary>
        public Ship(Game game) : base(new Point(ScreenCanvas.CanvasWidth / 2, ScreenCanvas.CanvasHeight / 2))
        {
            IsThrustOn = false;
            ExplosionLength = 2;
            InitPoints();
            Game = game;
        }

        public ShipAutoPilot? AutoPilot { get; set; }

        /// <summary>
        /// Initialize the internal point collections base on the template.
        /// </summary>
        private void InitPoints()
        {
            ClearPoints();
            AddPoints(PointsTemplate);
        }

        /// <summary>
        /// Indicates if the ship is currently accelerating via thrust.
        /// </summary>
        public bool IsThrustOn { get; private set; }

        /// <summary>
        /// Jump to another part of the canvas with a 10% of failure.
        /// </summary>
        /// <returns>Indication if the jump was considered a failure.</returns>
        public bool Hyperspace()
        {
            const int w = ScreenCanvas.CanvasWidth;
            const int h = ScreenCanvas.CanvasHeight;

            CurrentLocation.X = RandomizeHelper.Random.Next((int)(0.1 * w), (int)(0.9 * w));
            CurrentLocation.Y = RandomizeHelper.Random.Next((int)(0.1 * h), (int)(0.9 * h));

            return RandomizeHelper.Random.Next(10) != 1;
        }

        /// <summary>
        /// Blows up the ship.
        /// </summary>
        /// <returns>Collection of the ships last location polygon.</returns>
        public override IList<Explosion> Explode()
        {
            PlaySound(this, ActionSound.Explode1);
            PlaySound(this, ActionSound.Explode2);
            PlaySound(this, ActionSound.Explode3);

            return base.Explode();
        }

        /// <summary>
        /// Reduces speed by 1 frame's worth.
        /// </summary>
        public void DecayThrust()
        {
            IsThrustOn = false;

            VelocityX *= (1 - 1 / ScreenCanvas.FramesPerSecond);
            VelocityY *= (1 - 1 / ScreenCanvas.FramesPerSecond);
        }

        /// <summary>
        /// Increase speed by 1 frame's worth.
        /// </summary>
        public void Thrust()
        {
            IsThrustOn = true;

            var sinVal = Math.Sin(Radians);
            var cosVal = Math.Cos(Radians);
            const double addThrust = 90 / ScreenCanvas.FramesPerSecond;
            const double maxThrustSpeed = 5000 / ScreenCanvas.FramesPerSecond;

            var incX = (addThrust * sinVal);
            var incY = addThrust * cosVal;

            VelocityX += incX;
            if (VelocityX > maxThrustSpeed)
                VelocityX = maxThrustSpeed;
            if (VelocityX < -maxThrustSpeed)
                VelocityX = -maxThrustSpeed;

            VelocityY += incY;
            if (VelocityY > maxThrustSpeed)
                VelocityY = maxThrustSpeed;
            if (VelocityY < -maxThrustSpeed)
                VelocityY = -maxThrustSpeed;

            PlaySound(this, ActionSound.Thrust);
        }


        public Angle LastRotationSpeedDegrees { get; private set; } = 0;

        protected internal override double Rotate(double radians)
        {
            if (Math.Abs(radians) > MaxRotateSpeedRadians)
            {
                radians = Math.Sign(radians) * MaxRotateSpeedRadians;
            }

            LastRotationSpeedDegrees = radians;
            return base.Rotate(radians);
        }

        /// <summary>
        /// Rotate the ship left by one frame's worth.
        /// </summary>
        public void RotateLeft()
        {
            Rotate(-MaxRotateSpeedRadians);
        }

        /// <summary>
        /// Rotate the ship right by one frame's worth.
        /// </summary>
        public void RotateRight()
        {
            Rotate(MaxRotateSpeedRadians);
        }

        public void DoAutoPilot()
        {
            this.AutoPilot?.Execute();
        }

        #region Statics

        /// <summary>
        /// Non-transformed point template for creating a new ship.
        /// </summary>
        private static readonly IList<Point> PointsTemplate = new List<Point>();

        /// <summary>
        /// Index location in <see cref="PointsTemplate"/> for thrust point 1.
        /// </summary>
        public static int PointThrust1 { get; }

        /// <summary>
        /// Index location in <see cref="PointsTemplate"/> for thrust point 2.
        /// </summary>
        public static int PointThrust2 { get; }

        /// <summary>
        /// Setup the <see cref="PointsTemplate"/>.
        /// </summary>
        static Ship()
        {
            const int shipWidthHalf = 100;
            const int shipHeightHalf = shipWidthHalf * 2;
            const int shipHeightInUp = (int)(shipHeightHalf * .6);
            const int shipWidthInSide = (int)(shipWidthHalf * .3);

            PointsTemplate.Add(new Point(0, shipHeightHalf));
            PointsTemplate.Add(new Point(shipWidthHalf / 2, 0)); // midpoint for collisions
            PointsTemplate.Add(new Point(shipWidthHalf, -shipHeightHalf));
            PointsTemplate.Add(new Point(shipWidthInSide, -shipHeightInUp));
            PointsTemplate.Add(new Point(-shipWidthInSide, -shipHeightInUp));
            PointsTemplate.Add(new Point(-shipWidthHalf, -shipHeightHalf));
            PointsTemplate.Add(new Point(-shipWidthHalf / 2, 0)); // midpoint for collisions

            PointThrust1 = 3;
            PointThrust2 = 4;
        }

        #endregion

        public const int ShipDirectionVectorLenght = 5900;

        private VectorD ShipDirectionVector
        {
            get
            {
                var cl = (PointD)this.GetCurrentLocation();
                var r = this.GetRadians();
                var dv = MathHelper.TransformPolarToDecart(new PolarCoordinates { Angle = this.GetRadians(), Distance = ShipDirectionVectorLenght });

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


        #region IDrawableObject
        public object _lock_BulletDirections = new object();
        public List<IVectorD> BulletDirections = new List<IVectorD>();


        public IList<PointD> Dots => new List<PointD>();

        public IList<IVectorD> Vectors
        {
            get
            {
                var ret = new List<IVectorD>();
                /*
                ret.Add(ShipDirectionVector);
                ret.Add(new VectorD { Start = new PointD(k, k), End = new PointD(1500, k), Color = DrawColor.Blue });
                ret.Add(new VectorD { Start = new PointD(k, k), End = new PointD(k, 1500), Color = DrawColor.Blue });
                lock (_lock_BulletDirections)
                {
                    ret.AddRange(BulletDirections);
                }
                */

                return ret;
            }
        }

        public IList<IPoligonD> Poligons => new List<IPoligonD> {

            new Poligon { Color = DrawColor.White, Points = GetPoints().Select(p => new PointD { X = p.X, Y = p.Y }).ToList() },

            //new Poligon { Color = DrawColor.Blue, Points = OutFrame().Select(p => new PointD { X = p.X, Y = p.Y }).ToList() },
            //new Poligon { Color = DrawColor.Blue, Points = IntFrame().Select(p => new PointD { X = p.X, Y = p.Y }).ToList() },
            //new Poligon { Color = DrawColor.Orange, Points = CanvasOrientationTriangle().Select(p => new PointD { X = p.X, Y = p.Y }).ToList() },
            
            //new Poligon { Color = DrawColor.Blue, Points = ShipOrientationTriangle().Select(p => new PointD { X = p.X, Y = p.Y }).ToList() }

        };

        IList<Point> OutFrame()
        {
            int min = 100;
            int maxX = 10000 - min;
            int maxY = 7500 - min;
            var ret = new List<Point>();
            ret.Add(new Point(min, min));
            ret.Add(new Point(maxX, min));
            ret.Add(new Point(maxX, maxY));
            ret.Add(new Point(min, maxY));

            return ret;
        }

        public IList<Point> IntFrame()
        {
            int minX = 3000;
            int minY = 3000;
            int maxX = 7000;
            int maxY = 5000;
            var ret = new List<Point>();
            ret.Add(new Point(minX, minY));
            ret.Add(new Point(maxX, minY));
            ret.Add(new Point(maxX, maxY));
            ret.Add(new Point(minX, maxY));

            return ret;
        }

        IList<Point> CanvasOrientationTriangle()
        {
            var ret = new List<Point>();
            int k = 200;
            ret.Add(new PointD(k, k));
            ret.Add(new PointD(10000 - k, 7500 - k));
            ret.Add(new PointD(k, 7500 - k));


            return ret;
        }

        IList<Point> ShipOrientationTriangle()
        {

            var ret = new List<Point>();
            int k = 200;
            ret.Add(new PointD(CurrentLocation.X, CurrentLocation.Y));
            ret.Add(new PointD(CurrentLocation.X + k, CurrentLocation.Y - k));
            ret.Add(new PointD(CurrentLocation.X - 0, CurrentLocation.Y - k));


            return ret;
        }

        public IList<DrawableText> Texts
        {
            get
            {
                var ret = new List<DrawableText>();

/*

                var p = new PointD { X = 100, Y = 800 };
                var t = new Text(
                    $" SHIP=[ {(Angle)this.GetRadians()} ] RS=[ {(int)LastRotationSpeedDegrees.ValueDegee} ] ",
                    p,
                    DrawColor.Red,
                    TextManager.Justify.Center,
                    (int)p.Y,
                    100, 200
                    );

                ret.Add(t);
*/
                return ret;
            }
        }

        #endregion IDrawableObject
    }
}
