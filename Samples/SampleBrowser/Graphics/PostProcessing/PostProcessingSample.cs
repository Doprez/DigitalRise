﻿using DigitalRise.Geometry;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace Samples.Graphics
{
  // The base class for post-processing samples. It adds the PostProcessingGraphicsScreen
  // to the graphics service and renders a small scene. Derived classes only need
  // to register PostProcessors.
  public class PostProcessingSample : Sample
  {
    protected readonly PostProcessingGraphicsScreen GraphicsScreen;


    public PostProcessingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      // Add a PostProcessingGraphicsScreen. This graphics screen has a Scene and does
      // the rendering including post-processing. Please look at PostProcessingGraphicsScreen 
      // for more details.
      GraphicsScreen = new PostProcessingGraphicsScreen(Services);
      GraphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, GraphicsScreen);

      // GameObjects that need to render stuff will retrieve the DebugRenderers or
      // Scene through the service provider.
      Services.AddService(typeof(DebugRenderer), GraphicsScreen.DebugRenderer);
      Services.AddService(typeof(IScene), GraphicsScreen.Scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services, 100);
      GameObjectService.Objects.Add(cameraGameObject);
      GraphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services) { SkyExposure = 1 });
      GameObjectService.Objects.Add(new GroundObject(Services));

      for (int i = 0; i < 20; i++)
        GameObjectService.Objects.Add(new DynamicObject(Services, 2));

      for (int i = 0; i < 10; i++)
      {
        var randomPosition = new Vector3(
          RandomHelper.Random.NextFloat(-5, 5),
          0,
          RandomHelper.Random.NextFloat(-10, 0));
        var randomOrientation = MathHelper.CreateRotationY(RandomHelper.Random.NextFloat(0, ConstantsF.TwoPi));

        GameObjectService.Objects.Add(new DudeObject(Services)
        {
          Pose = new Pose(randomPosition, randomOrientation)
        });
      }
    }


    public override void Update(GameTime gameTime)
    {
      GraphicsScreen.DebugRenderer.Clear();
    }
  }
}