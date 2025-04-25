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

        public PointD(double x, double y, DrawColor Color = DrawColor.White)
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

        public override string ToString()
        {
            return $"[X={X};Y={Y}]";
        }
    }

    public class Text
    {
        public Text(string textVal, PointD start, DrawColor color, Justify justification, int locationTop, int letterWidth, int letterHeight)
        {
            TextVal = textVal;
            Start = start;
            Color = color;
            Justification = justification;
            LocationTop = locationTop;
            LetterWidth = letterWidth;
            LetterHeight = letterHeight;
        }
        public DrawColor Color { get; set; }
        public PointD Start { get; set; }
        public string TextVal { get; set; }

        public Justify Justification { get; set; }

        public int LocationTop { get; set; }
        public int LetterWidth { get; set; }

        public int LetterHeight { get; set; }
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

        public IList<Text> Texts { get; }
    }
}
#nullable restore
