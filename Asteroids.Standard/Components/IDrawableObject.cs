#nullable disable
using System.Collections.Generic;

namespace Asteroids.Standard.Components
{
    public interface IDrawableObject
    {
        public IList<PointD> Dots { get; }

        public IList<IVectorD> Vectors { get; }

        public IList<IPoligonD> Poligons { get; }

        public IList<DrawableText> Texts { get; }
    }
}
#nullable restore
