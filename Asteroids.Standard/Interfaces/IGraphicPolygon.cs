using System.Collections.Generic;
using System.Drawing;
using Asteroids.Standard.Enums;

namespace Asteroids.Standard.Interfaces
{
    /// <summary>
    /// Polygon vector to render in the UI.
    /// </summary>
    public interface IGraphicPolygon
    {
        /// <summary>
        /// <see cref="Color"/> for the graphic.
        /// </summary>
        Color Color { get; }

        /// <summary>
        /// Collection of points to connect (non-closing)
        /// </summary>
        IList<Point> Points { get; }
    }
}
