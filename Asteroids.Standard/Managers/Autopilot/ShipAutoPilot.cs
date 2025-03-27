using Asteroids.Standard.Components;
using System;
using System.Linq;

namespace Asteroids.Standard
{
    internal class ShipAutoPilot
    {
        private readonly ShipEnvironmentFPVManager _env;
        private readonly Ship _ship;
        public ShipAutoPilot(ShipEnvironmentFPVManager env, Ship ship)
        {
            _env = env;
            _ship = ship;
        }

        public void Execute()
        {
            try
            {
                var staticView = _env.TransformViewToFPV();

                var target = staticView.OrderByDescending(x => x.ObjectType).ThenBy(x => x.CenterCoordinates.Distance).FirstOrDefault();

                if (target == null)
                {
                    return;
                }

                var mostJeopardizeObject = staticView.OrderBy(x => x.Distance).First();
                if (mostJeopardizeObject.Distance < 400)
                {
                    _ship.Hyperspace();
                    return;
                }

                var shipRotationSpeedFactor = 2;
                var targetCenterAngleDegrees = Math.Abs(MathHelper.ToDegrees(target.CenterCoordinates.Angle));
                var shipRotationSpeed = shipRotationSpeedFactor * targetCenterAngleDegrees;

                var targetDeltaDegreesForSmallRotationSpeed = 8;
                if (targetCenterAngleDegrees >= targetDeltaDegreesForSmallRotationSpeed)
                {
                    shipRotationSpeed = Ship.RotateSpeed;
                }

                if (target.CenterCoordinates.Angle > 0)
                {
                    // right
                    _ship.Rotate(shipRotationSpeed);
                }
                else
                {
                    // left
                    _ship.Rotate(-shipRotationSpeed);
                }

                bool allowFire = false;

                // for dots (like misiles)
                allowFire |= target.LeftAngle == target.RightAngle && Math.Abs(target.RightAngle) <= 0.01;

                // for shapes
                allowFire |= (
                    target.LeftAngle < 0 && target.RightAngle > 0                               

                    && Math.Abs(target.CenterCoordinates.Angle) < Math.Abs(target.LeftAngle)
                    && Math.Abs(target.CenterCoordinates.Angle) < Math.Abs(target.RightAngle)
                    );

                if (allowFire && _ship?.IsAlive == true)
                {
                    // Console.WriteLine($"Target angle = [ {MathHelper.ToDegrees(target.CenterCoordinates.Angle)} ], Distance = {target.CenterCoordinates.Distance}, L [{MathHelper.ToDegrees(target.LeftAngle)}] R[{MathHelper.ToDegrees(target.RightAngle)}]");
                    // Fire bullets that are not already moving
                    foreach (var bullet in _env._cache.GetBulletsAvailable())
                    {
                        bullet.ScreenObject.Shoot(_ship);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
