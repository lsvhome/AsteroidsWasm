using Asteroids.Standard.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Asteroids.Standard.Helpers
{
    /// <summary>
    /// Drawing colors used by the game engine.
    /// </summary>
    internal static class ColorHelper
    {
        private static IEnumerable<Color> AllColors
        {
            get
            {
                PropertyInfo[] colorProperties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (PropertyInfo property in colorProperties)
                {
                    if (property.PropertyType == typeof(Color))
                    {
                        yield return (Color)property.GetValue(null, null);
                    }
                }
            }
        }

        /// <summary>
        /// Collection of <see cref="Color"/> HEX string values used by the game engine.
        /// </summary>
        public static IDictionary<Color, string> DrawColorMap { get; } =
            new ReadOnlyDictionary<Color, string>(
                AllColors.ToDictionary(c => c, c => c.ToHexString())
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
