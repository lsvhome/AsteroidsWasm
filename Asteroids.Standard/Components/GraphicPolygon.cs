using System.Collections.Generic;
using System.Drawing;
using Asteroids.Standard.Enums;
using Asteroids.Standard.Interfaces;

namespace Asteroids.Standard.Components
{
    internal sealed class GraphicPolygon : IGraphicPolygon
    {
        public GraphicPolygon(Color color, IList<Point> points)
        {
            Color = color;
            Points = points;
        }

        public Color Color { get; }

        public IList<Point> Points { get; }
    }
}
