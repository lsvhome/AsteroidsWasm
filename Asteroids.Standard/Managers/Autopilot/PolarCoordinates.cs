namespace Asteroids.Standard
{
    public class PolarCoordinates
    {
        public double Distance { get; set; }
        public double Angle { get; set; }

        public override string ToString()
        {
            return $"Angle = {Angle} ({MathHelper.ToDegrees(Angle)}), Distance={Distance}";
        }
    }
}
