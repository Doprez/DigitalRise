﻿#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRise.Geometry;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to draw an object with fake reflection, refraction and
chromatic aberration.",
    @"A dude model is loaded which uses a custom effect Refraction.fx. The effect renders
the dude with mesh skinning, reflection and refraction. 
To fake reflections the effect makes a lookup in a texture which contains the back 
buffer using a sphere mapping. This is obviously not correct but looks interesting.
To create a refraction effect the shader makes lookups in the back buffer texture.
Normally, the back buffer texture is not available in the shaders (because the hardware
cannot sample from a texture which is the current render target). Therefore, we 
wrap the MeshRenderer and copy the current back buffer into a texture before
rendering the objects.",
    108)]
  public class RefractionSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;


    public RefractionSample(Microsoft.Xna.Framework.Game game)
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
      var lavaBallsObject = new LavaBallsObject(Services);
      GameObjectService.Objects.Add(lavaBallsObject);
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new StaticSkyObject(Services));
      GameObjectService.Objects.Add(new CampfireObject(Services));
      for (int i = 0; i < 10; i++)
      {
        GameObjectService.Objects.Add(new DynamicObject(Services, 1));
        GameObjectService.Objects.Add(new DynamicObject(Services, 2));
        GameObjectService.Objects.Add(new DynamicObject(Services, 3));
      }

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3 position = new Vector3(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree.drmdl", scale, new Pose(position, orientation)));
      }

      // Add a few dudes which use the refraction effect.
      for (int i = 0; i < 5; i++)
      {
        Vector3 position = new Vector3(random.NextFloat(-4, 4), 0, random.NextFloat(2, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        var dudeObject = new DudeObject(Services, "DudeRefracted/Dude.drmdl")
        {
          Pose = new Pose(position, orientation)
        };
        GameObjectService.Objects.Add(dudeObject);
        //dudeObject.AnimationController.Pause();
      }

      // The DeferredGraphicsScreen has a SceneRenderer, which contains all 
      // renderers necessary to render transparent objects (e.g. BillboardRenderer 
      // for particles, MeshRenderer for meshes, etc.).
      // We remove the MeshRenderer and add our own RefractionMeshRenderer instead.
      var meshRenderer = _graphicsScreen.AlphaBlendSceneRenderer.Renderers.OfType<MeshRenderer>().First();
      _graphicsScreen.AlphaBlendSceneRenderer.Renderers.Remove(meshRenderer);
      _graphicsScreen.AlphaBlendSceneRenderer.Renderers.Add(new RefractionMeshRenderer(GraphicsService));
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}
#endif