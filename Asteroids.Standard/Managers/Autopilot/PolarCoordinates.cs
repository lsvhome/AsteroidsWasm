using Asteroids.Standard.Components;
using System;

namespace Asteroids.Standard
{
#nullable disable
    public class Angle
    {
        public double Value { get; set; }
        public double ValueDegee => MathHelper.ToDegrees(Value);

        public Angle(double value)
        {
            Value = value;
        }

        public static Angle operator +(Angle a, Angle b) => new Angle(a.Value + b.Value);
        public static Angle operator -(Angle a, Angle b) => new Angle (a.Value - b.Value);


        public static implicit operator double(Angle angle) => angle.Value;
        public static implicit operator Angle(double value) => new Angle(value);

        public override string ToString()
        {
            return $"{(int)ValueDegee}";
        }
    }

    public class PolarCoordinates
    {
        public double Distance { get; set; }
        public Angle Angle { get; set; }

        public override string ToString()
        {
            return $"A=[{(int)MathHelper.ToDegrees(Angle)}], D=[{Distance}]";
        }
    }
#nullable restore
}
