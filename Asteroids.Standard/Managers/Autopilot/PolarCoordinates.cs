using Asteroids.Standard.Components;
using System;

namespace Asteroids.Standard
{
#nullable disable
    public class Angle: IComparable<Angle>
    {
        public int CompareTo(Angle other)
        {
            if (other == null) return 1;
            return this.Value.CompareTo(other.Value);
        }

        public static Angle Zero => new Angle(0);
        public static Angle Right => new Angle(MathHelper.ToRadians(90));
        public static Angle Left => new Angle(MathHelper.ToRadians(-90));
        public static Angle Up => new Angle(MathHelper.ToRadians(180));
        public static Angle Down => new Angle(MathHelper.ToRadians(-180));
    
        private double _value { get; set; }
        public double Value { get { return _value; } set { _value = MathHelper.NormalizeAngle(value); } }
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
