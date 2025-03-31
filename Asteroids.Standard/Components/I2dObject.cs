using Asteroids.Standard.Enums;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Text;

namespace Asteroids.Standard.Components
{
    public class PointD
    {
        public DrawColor Color { get; set; } = DrawColor.White;
        public double X { get; set; }
        public double Y { get; set; }

        public static implicit operator Point(PointD p) => new PointD { X = p.X, Y = p.Y };
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
