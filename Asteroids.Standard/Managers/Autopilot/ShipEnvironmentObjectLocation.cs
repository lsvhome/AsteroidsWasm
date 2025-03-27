namespace Asteroids.Standard
{
    internal class ShipEnvironmentObjectLocation
    {
        public ShipEnvironmentObjectLocation(PolarCoordinates centerCoordinates, double relativeLeftAngle, double relativeRightAngle)
        {
            CenterCoordinates = centerCoordinates;
            LeftAngle = ShipEnvironmentFPVManager.NormalizeAngle(centerCoordinates.Angle - relativeLeftAngle);
            RightAngle = ShipEnvironmentFPVManager.NormalizeAngle(centerCoordinates.Angle + relativeRightAngle);
        }

        public PolarCoordinates CenterCoordinates { get; set; }
        public double LeftAngle { get; set; }
        public double RightAngle { get; set; }

        public ObjectType ObjectType { get; set; }
        public double Distance { get; set; }
    }
}
