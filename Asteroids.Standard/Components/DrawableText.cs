#nullable disable
using Asteroids.Standard.Enums;

namespace Asteroids.Standard.Components
{
    public class DrawableText
    {
        /// <summary>
        /// Text horizontal justification.
        /// </summary>
        public enum Justify { Left, Center, Right };

        public DrawableText(string textVal, PointD start, DrawColor color, Justify justification, int locationTop, int letterWidth, int letterHeight)
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
}
#nullable restore
