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
using AssetManagementBase;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how create a new light type and light renderer to add environment
reflections to all opaque objects.",
    @"A new light type EnvironmentLight is defined and rendered with a new light renderer 
class. The EnvironmentLightRenderer is added to the LightRenderers of the DeferredGraphicsScreen.
It additively blends all environment lights into the light buffer. This way all opaque
objects will be affected and reflect the environment map. 
The EnvironmentLightRenderer uses a custom effect 'EnvironmentLight.fx'.

Note: The EnvironmentLight is a simple version of the new ImageBasedLight of DigitalRise Graphics. 
The built-in ImageBasedLights offer a lot more features. See sample 29 ""ImageBasedLighting"".",
    107)]
  public class EnvironmentLightSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;


    public EnvironmentLightSample(Microsoft.Xna.Framework.Game game)
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
      var lavaBallsObject = new LavaBallsObject(Services);
      GameObjectService.Objects.Add(lavaBallsObject);
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new FogObject(Services));
      GameObjectService.Objects.Add(new StaticObject(Services, "Barrier/Barrier.drmdl", 0.9f, new Pose(new Vector3(0, 0, -2))));
      GameObjectService.Objects.Add(new StaticObject(Services, "Barrier/Cylinder.drmdl", 0.9f, new Pose(new Vector3(3, 0, 0), MathHelper.CreateRotationY(MathHelper.ToRadians(-20)))));
      GameObjectService.Objects.Add(new StaticSkyObject(Services));

      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3 position = new Vector3(random.NextFloat(-3, -8), 0, random.NextFloat(0, -5));
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree.drmdl", scale, new Pose(position, orientation)));
      }

      // Add some more dynamic objects.
      for (int i = 0; i < 5; i++)
      {
        lavaBallsObject.Spawn();
        GameObjectService.Objects.Add(new ProceduralObject(Services));
        GameObjectService.Objects.Add(new DynamicObject(Services, 7));
      }

      // To show the effect of the EnvironmentLight in isolation, disable all other light sources.
      //foreach (var light in _graphicsScreen.Scene.GetDescendants().OfType<LightNode>())
      //  light.IsEnabled = false;

      // Add the environment light.
      var environmentLight = new EnvironmentLight
      {
        Color = new Vector3(0.1f),
        DiffuseIntensity = 0,
        SpecularIntensity = 1,
        EnvironmentMap = AssetManager.LoadTextureCube(GraphicsService.GraphicsDevice, "Sky2.dds"),
      };
      var environmentLightNode = new LightNode(environmentLight)
      {
        Name = "Environment",
      };
      _graphicsScreen.Scene.Children.Add(environmentLightNode);

      // The EnvironmentLight is a new light type. We have to register a light renderer
      // for this light in the LightRenderer of the DeferredGraphicsScreen.
      _graphicsScreen.LightBufferRenderer.LightRenderer.Renderers.Add(new EnvironmentLightRenderer(AssetManager, GraphicsService));

      // EnvironmentLight.fx uses the specular power of the materials to determine
      // which mip map level of the cube is reflected. 
      // In reality, a high specular power is necessary to reflect the cube map
      // with all its detail. To reflect a cube map level with 512 texels size, we 
      // need a specular power of ~200000.
      // To make the reflection effects more obvious, let's change some material properties
      // and make the more reflective.

      // ProceduralObject:
      var proceduralObjects = _graphicsScreen.Scene
                                             .GetDescendants()
                                             .OfType<MeshNode>()
                                             .Where(mn => mn.Mesh.Name == "ProceduralObject")
                                             .Select(mn => mn.Mesh);
      foreach (var mesh in proceduralObjects)
      {
        foreach (var material in mesh.Materials)
        {
          material["GBuffer"].Set("SpecularPower", 10000f);
          material["Material"].Set("DiffuseColor", new Vector3(0.01f));
          material["Material"].Set("SpecularColor", new Vector3(1));
        }
      }

      // Frame of GlassBox:
      var glassBoxes = _graphicsScreen.Scene
                                      .GetDescendants()
                                      .OfType<ModelNode>()
                                      .Where(mn => mn.Name == "GlassBox")
                                      .Select(mn => ((MeshNode)mn.Children[0]).Mesh);
      foreach (var mesh in glassBoxes)
      {
        foreach (var material in mesh.Materials.Where(m => m.Contains("GBuffer")))
        {
          material["GBuffer"].Set("SpecularPower", 100000f);
          material["Material"].Set("DiffuseColor", new Vector3(0.0f));
          material["Material"].Set("SpecularColor", new Vector3(1));
        }
      }

      // LavaBall:
      var lavaBalls = _graphicsScreen.Scene
                                     .GetDescendants()
                                     .OfType<ModelNode>()
                                     .Where(mn => mn.Name == "LavaBall")
                                     .Select(mn => ((MeshNode)mn.Children[0]).Mesh);
      foreach (var mesh in lavaBalls)
      {
        foreach (var material in mesh.Materials.Where(m => m.Contains("GBuffer")))
        {
          material["GBuffer"].Set("SpecularPower", 10000f);
          material["Material"].Set("DiffuseColor", new Vector3(0.0f));
          material["Material"].Set("SpecularColor", new Vector3(10));
          material["Material"].Set("EmissiveColor", new Vector3(0.0f));
        }
      }

      // Ground plane:
      var groundPlanes = _graphicsScreen.Scene
                                        .GetDescendants()
                                        .OfType<ModelNode>()
                                        .Where(mn => mn.Name == "Ground")
                                        .Select(mn => ((MeshNode)mn.Children[0]).Mesh);
      foreach (var mesh in groundPlanes)
      {
        foreach (var material in mesh.Materials.Where(m => m.Contains("GBuffer")))
        {
          material["GBuffer"].Set("SpecularPower", 200000.0f);
          material["Material"].Set("DiffuseColor", new Vector3(0.5f));
          material["Material"].Set("SpecularColor", new Vector3(0.4f));
        }
      }

      // Please note, XNA does not filter cube maps over cube map borders. Therefore, reflections
      // of low resolution mip map levels might show obvious borders between the cube map
      // sides. In this case you can change the EnvironmentLight.fx effect to always reflect
      // the mip map level 0.
      // This is not a problem with MonoGame because DirectX automatically filters cube map borders.
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Unload content.
        // (We have changed the properties of some loaded materials. Other samples
        // should use the default values. When we unload them now, the next sample
        // will reload them with default values.)
        AssetManager.Unload();
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