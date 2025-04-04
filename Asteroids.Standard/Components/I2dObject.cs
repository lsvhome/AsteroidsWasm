using Asteroids.Standard.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Text;

namespace Asteroids.Standard.Components
{
    public class PointD
    {
        public PointD()
        {
        }
        public PointD(PolarCoordinates data)
        {
            var t = MathHelper.TransformPolarToDecart(data);
            this.X = t.X;
            this.Y = t.Y;
        }

        public DrawColor Color { get; set; } = DrawColor.White;
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
    }

    public interface Text
    {
        public DrawColor Color { get; set; }
        public PointD Start { get; }
        public string Text { get; }
    }


    public interface IVectorD
    {
        public DrawColor Color { get; set; }
        public PointD Start { get; }
        public PointD End { get; }
    }

    public class VectorD : IVectorD
    {
        public DrawColor Color { get; set; } = DrawColor.White;
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
        public DrawColor Color { get; set; }
        public IList<PointD> Points { get; }
    }

    public class Poligon : IPoligonD
    {
        public DrawColor Color { get; set; } = DrawColor.White;
        public IList<PointD> Points { get; set; } = new List<PointD>();
    }

    public interface IDrawableObject
    {
        public IList<PointD> Dots { get; }

        public IList<IVectorD> Vectors { get; }

        public IList<IPoligonD> Poligons { get; }

        // public IList<Text> Texts { get; }
    }
}
