﻿#if !MONOGAME && !WP7 && !WP8
// TODO: Add annotation support to MonoGame.

using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Physics;
using DigitalRise.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using AssetManagementBase;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples is similar to the FurSample, but it uses a custom technique binding to repeat an effect pass.",
    @"The Fur.fx effect in the FurSample uses a fixed number of passes. In this sample we use a
similar effect which uses an annotation like this:
  pass
  < string RepeatParameter = ""NumberOfLayers""; >
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }

EffectTechniqueBindings allow to control how an effect is executed. In this sample, we use a custom
EffectTechniqueBinding class which checks the annotation and repeats the pass when the object is rendered.

The fur is not animated in this sample.",
    112)]
  public class TechniqueBindingSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;

    private readonly RepeatTechniqueInterpreter _repeatTechniqueInterpreter;
    private readonly RepeatTechniqueBinder _repeatTechniqueBinder;

    private readonly ModelNode _modelNode;
    private readonly RigidBody _rigidBody;


    public TechniqueBindingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);

      Services.AddService(typeof(DebugRenderer), _graphicsScreen.DebugRenderer);
      Services.AddService(typeof(IScene), _graphicsScreen.Scene);

      // Add gravity and damping to the physics Simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      // More standard objects.
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services));
      GameObjectService.Objects.Add(new GroundObject(Services));

      // Tell the graphics service how to treat effect techniques which use
      // the "RepeatParameter" annotation.
      _repeatTechniqueInterpreter = new RepeatTechniqueInterpreter();
      GraphicsService.EffectInterpreters.Insert(0, _repeatTechniqueInterpreter);
      _repeatTechniqueBinder = new RepeatTechniqueBinder();
      GraphicsService.EffectBinders.Insert(0, _repeatTechniqueBinder);

      // Load model.
      _modelNode = AssetManager.LoadDRModel(GraphicsService, "Fur2/FurBall.drmdl").Clone();
      _rigidBody = new RigidBody(new SphereShape(0.5f));

      // Set a random pose.
      _rigidBody.Pose = new Pose(new Vector3(0, 1, 0), RandomHelper.Random.NextQuaternion());
      _modelNode.PoseWorld = _rigidBody.Pose;

      // Add rigid body to physics simulation and model to scene.
      Simulation.RigidBodies.Add(_rigidBody);
      _graphicsScreen.Scene.Children.Add(_modelNode);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        GraphicsService.EffectInterpreters.Remove(_repeatTechniqueInterpreter);
        GraphicsService.EffectBinders.Remove(_repeatTechniqueBinder);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Update SceneNode.LastPoseWorld - this is required for some effects 
      // like object motion blur. 
      _modelNode.SetLastPose(true);

      // Synchronize graphics <--> physics.
      _modelNode.PoseWorld = _rigidBody.Pose;

      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}
#endif