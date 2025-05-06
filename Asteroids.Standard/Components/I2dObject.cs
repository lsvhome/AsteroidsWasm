#nullable disable
using Asteroids.Standard.Enums;
using Asteroids.Standard.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Text;
using static Asteroids.Standard.Managers.TextManager;

namespace Asteroids.Standard.Components
{
    public class PointD
    {
        public PointD()
        {
        }

        public PointD(double x, double y): this(x,y, Color.White)
        {
            this.X = x;
            this.Y = y;
        }

        public PointD(double x, double y, Color Color)
        {
            this.X = x;
            this.Y = y;
        }

        public PointD(PolarCoordinates data)
        {
            var t = MathHelper.TransformPolarToDecart(data);
            this.X = t.X;
            this.Y = t.Y;
        }

        public Color Color { get; set; } = Color.White;
        public double X { get; set; }
        public double Y { get; set; }

        public static PointD operator + (PointD a, PointD b) => new PointD { X = a.X + b.X, Y = a.Y+b.Y };
        public static PointD operator -(PointD a, PointD b) => new PointD { X = a.X - b.X, Y = a.Y - b.Y };

        public static implicit operator Point(PointD p) => new Point { X = (int)p.X, Y = (int)p.Y };
        public static implicit operator PointD(Point p) => new PointD { X = p.X, Y = p.Y };
        public PolarCoordinates GetPolarCoordinates()
        {
            var ret = MathHelper.TransformDecartToPolar(this);
            return ret;
        }

        public override string ToString()
        {
            return $"[X={X};Y={Y}]";
        }
    }


    public interface IVectorD
    {
        public Color Color { get; set; }
        public PointD Start { get; }
        public PointD End { get; }
    }

    public class VectorD : IVectorD
    {
        public Color Color { get; set; } = Color.White;
        public PointD Start { get; set; } = new PointD();
        public PointD End { get; set; } = new PointD();

        public PolarCoordinates GetPolarCoordinates()
        {
            var ret = MathHelper.TransformDecartToPolar(new PointD { X = End.X - Start.X, Y = End.Y - Start.Y });
            return ret;
        }
        public void SetPolarCoordinates(PolarCoordinates data)
        {
            var ret = MathHelper.TransformPolarToDecart(data);
            End.X = Start.X + ret.X;
            End.Y = Start.Y + ret.Y;
        }
    }


    public interface IPoligonD
    {
        public Color Color { get; set; }
        public IList<PointD> Points { get; }
    }

    public class Poligon : IPoligonD
    {
        public Color Color { get; set; } = Color.White;
        public IList<PointD> Points { get; set; } = new List<PointD>();
    }
}
#nullable restore
