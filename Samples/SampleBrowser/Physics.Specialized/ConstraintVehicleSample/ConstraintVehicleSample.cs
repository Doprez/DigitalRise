﻿#if !ANDROID && !IOS   // Cannot read from vertex buffer in MonoGame/OpenGLES.
using DigitalRise.Physics.ForceEffects;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample shows how to implement vehicle physics using an alternative vehicle implementation.",
    @"This sample contains an alternative implementation to the DigitalRise.Physics.Specialized.Vehicle
class. This implementation uses constraints which make the car more stable - but the code is
probably more difficult to understand and modify.",
    51)]
  public class ConstraintVehicleSample : PhysicsSpecializedSample
  {
    public ConstraintVehicleSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a game object which loads the test obstacles.
      GameObjectService.Objects.Add(new VehicleLevelObject(Services));

      // Add a game object which controls a vehicle.
      var vehicleObject = new ConstraintVehicleObject(Services);
      GameObjectService.Objects.Add(vehicleObject);

      // Add a camera that is attached to the chassis of the vehicle.
      var vehicleCameraObject = new VehicleCameraObject(vehicleObject.Vehicle.Chassis, Services);
      GameObjectService.Objects.Add(vehicleCameraObject);
      GraphicsScreen.CameraNode = vehicleCameraObject.CameraNode;
    }
  }
}
#endif