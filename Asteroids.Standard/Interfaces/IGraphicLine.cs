using System.Drawing;
using Asteroids.Standard.Enums;

namespace Asteroids.Standard.Interfaces
{
    /// <summary>
    /// Line vector to render in the UI.
    /// </summary>
    public interface IGraphicLine
    {
        /// <summary>
        /// <see cref="Color"/> for the graphic.
        /// </summary>
        Color Color { get; }

        /// <summary>
        /// Staring point.
        /// </summary>
        Point Point1 { get; }

        /// <summary>
        /// Ending point.
        /// </summary>
        Point Point2 { get; }
    }
}
