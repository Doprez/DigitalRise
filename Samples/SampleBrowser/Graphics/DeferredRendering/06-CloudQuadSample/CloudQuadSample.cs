﻿using System;
using System.Linq;
using DigitalRise.Geometry;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how draw clouds using textured quads with a custom shader
and custom effect parameter bindings.",
    @"The clouds are defined using a CloudQuad.fbx model and the shader Cloud.fx. They
are rendered with other transparent meshes in the 'AlphaBlend' pass of the 
DeferredGraphicsScreen. No new scene node types or renderers are needed! We only 
need to register an effect interpreter and an effect binder to automatically 
update the new effect parameters used by Cloud.fx.",
    106)]
  public class CloudQuadSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly SkyEffectInterpreter _skyEffectInterpreter;
    private readonly SkyEffectBinder _skyEffectBinder;


    public CloudQuadSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.AddService(typeof(DebugRenderer), _graphicsScreen.DebugRenderer);
      Services.AddService(typeof(IScene), _graphicsScreen.Scene);

      // Add gravity and damping to the physics Simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new GroundObject(Services));
      GameObjectService.Objects.Add(new DudeObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new LavaBallsObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new StaticObject(Services, "Barrier/Barrier.drmdl", 0.9f, new Pose(new Vector3(0, 0, -2))));
      GameObjectService.Objects.Add(new StaticObject(Services, "Barrier/Cylinder.drmdl", 0.9f, new Pose(new Vector3(3, 0, 0), MathHelper.CreateRotationY(MathHelper.ToRadians(-20)))));

      // The DynamicSkyObject creates the dynamic sky and lights but no clouds.
      var dynamicSkyObject = new DynamicSkyObject(Services, false, false, false);
      GameObjectService.Objects.Add(dynamicSkyObject);

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3 position = new Vector3(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree.drmdl", scale, new Pose(position, orientation)));
      }

      // The model CloudQuad.fbx consists of a textured quad with a custom effect 
      // "Cloud.fx". The effect uses several effect parameters. Constant effect 
      // parameters are set in the Cloud.drmat material file. 
      // The effect parameters, like "World", "WorldViewProjection", "CameraPosition",
      // are automatically updated by the graphics service. But the effect 
      // contains 3 new effect parameters which must be set at runtime: 
      // "SunDirection", "SunLight" and "SkyLight".
      // Therefore we add a custom effect interpreter and a custom effect binder 
      // which tell the graphics manager what it should do with these parameters.
      // The effect interpreter and binder must be registered before the CloudQuad
      // model is loaded!
      _skyEffectInterpreter = GraphicsService.EffectInterpreters.OfType<SkyEffectInterpreter>().FirstOrDefault();
      if (_skyEffectInterpreter == null)
      {
        _skyEffectInterpreter = new SkyEffectInterpreter();
        GraphicsService.EffectInterpreters.Add(_skyEffectInterpreter);
      }

      _skyEffectBinder = GraphicsService.EffectBinders.OfType<SkyEffectBinder>().FirstOrDefault();
      if (_skyEffectBinder == null)
      {
        _skyEffectBinder = new SkyEffectBinder();
        GraphicsService.EffectBinders.Add(_skyEffectBinder);
      }

      // The effect binder defines several delegates which update the effect parameters
      // using values which are computed by the DynamicSkyObject.
      _skyEffectBinder.DynamicSkyObject = dynamicSkyObject;

      // Add several CloudQuad models in the sky with random scales and poses.
      for (int i = 0; i < 20; i++)
      {
        var scale = new Vector3(
          RandomHelper.Random.NextFloat(100, 200),
          0,
          RandomHelper.Random.NextFloat(100, 200));

        var position = new Vector3(
          RandomHelper.Random.NextFloat(-500, 500),
          RandomHelper.Random.NextFloat(100, 200),
          RandomHelper.Random.NextFloat(-500, 500));

        var orientation = Matrix33F.CreateRotationY(RandomHelper.Random.NextFloat(0, ConstantsF.TwoPi));
        GameObjectService.Objects.Add(new StaticObject(Services, "CloudQuad/CloudQuad.drmdl", scale, new Pose(position, orientation), false, false));
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Clean up.
        GraphicsService.EffectBinders.OfType<SkyEffectBinder>().First().DynamicSkyObject = null;
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // This sample clears the debug renderer each frame.
      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}