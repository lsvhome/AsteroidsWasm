using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Asteroids.Standard.Enums;

namespace Asteroids.Standard.Helpers
{
    /// <summary>
    /// Drawing colors used by the game engine.
    /// </summary>
    internal static class ColorHelper
    {
        /// <summary>
        /// Collection of <see cref="Color"/> HEX string values used by the game engine.
        /// </summary>
        public static IDictionary<Color, string> DrawColorMap { get; } = new ReadOnlyDictionary<Color, string>(
            new Dictionary<Color, string>
            {
                [Color.White] = System.Drawing.Color.White.ToHexString(),
                [Color.Red] = System.Drawing.Color.Red.ToHexString(),
                [Color.Yellow] = System.Drawing.Color.Yellow.ToHexString(),
                [Color.Orange] = System.Drawing.Color.Orange.ToHexString(),
                [Color.Blue] = System.Drawing.Color.Blue.ToHexString(),
                [Color.Gray] = System.Drawing.Color.Gray.ToHexString(),
            }
        );

        /// <summary>
        /// Collection of <see cref="Color"/> keys in <see cref="DrawColorMap"/>.
        /// </summary>
        public static IList<Color> DrawColorList { get; } = DrawColorMap.Keys.OrderBy(k => k.GetBrightness()).ToList();

        /// <summary>
        /// Converts a <see cref="System.Drawing.Color"/> to an html-formatted text string (e.g. #RRGGBB).
        /// </summary>
        public static string ToHexString(this System.Drawing.Color c)
        {
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }
    }
}
