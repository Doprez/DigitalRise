﻿#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics;
using DigitalRise.Graphics.PostProcessing;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DirectionalLight = DigitalRise.Graphics.DirectionalLight;
using MathHelper = DigitalRise.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use image-based lighting (IBL).",
    @"An image-based light uses an environment map to add diffuse lighting and specular reflections
to the scene. 
This sample scene contains 3 image-based lights:
- One IBL is infinite and covers the whole scene.
- Two IBLs have a box shape and cover only selected parts. 
The infinite IBL replaces the normal ambient light of the scene.
Local, box-shaped IBLs always replace the global, infinite IBL.
The yellow wireframe boxes show the bounding boxes of the lights.
The box-shaped IBLs use localized reflections, that means the specify AABBs (drawn in orange)
to correct the reflections. This is used to create better reflections of the palm trees the brick
wall and the barrel wall. 
The small spheres show the IBL positions and are highly reflective to visualize the environment
maps. The larger spheres have different diffuse and specular properties to show the effect of the
image-based lighting. 
The environment maps are usually captured offline in a game editor. However, this sample computes
them at runtime using SceneCaptureNodes.
Press <F4> to open the Options window where you can change IBL settings.
Some things you can test using the available Options:
- Change the dynamic sky and then update the environment maps. 
- In the DeferredGraphicsScreen options, change the debug mode to visualize only the diffuse
  or specular light contributions of the lights.",
    129)]
  public class ImageBasedLightingSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;

    // Rotate scene to show that lights do not have to be axis-aligned.
    private static readonly Pose _globalRotation = new Pose(Matrix33F.CreateRotationY(MathHelper.ToRadians(15)));

    // 3 image-based lights.
    private ImageBasedLight[] _imageBasedLights = new ImageBasedLight[3];
    private LightNode[] _lightNodes = new LightNode[3];

    // Some stuff used to render environment maps for the IBL at runtime.
    private bool _updateEnvironmentMaps = true;
    private int _oldEnvironmentMapTimeStamp;
    private SceneCaptureNode[] _sceneCaptureNodes = new SceneCaptureNode[3];
    private ColorEncoder _colorEncoder;


    public ImageBasedLightingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.AddService(typeof(DebugRenderer), _graphicsScreen.DebugRenderer);
      Services.AddService(typeof(IScene), _graphicsScreen.Scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      // Add standard game objects.
      GameObjectService.Objects.Add(new GrabObject(Services));
      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true));
      GameObjectService.Objects.Add(new GroundObject(Services));
      GameObjectService.Objects.Add(new ObjectCreatorObject(Services));
      GameObjectService.Objects.Add(new DudeObject(Services) { Pose = new Pose(new Vector3(-4, 0, 4)) });
      GameObjectService.Objects.Add(new LavaBallsObject(Services));

      // Create 3 image-based lights.
      for (int i = 0; i < 3; i++)
      {
        _imageBasedLights[i] = new ImageBasedLight
        {
          DiffuseIntensity = 1f,
          SpecularIntensity = 1f,
          FalloffRange = 0.2f,
          BlendMode = 1,    // 0 = add to ambient light, 1 = replace ambient light
        };
      }

      // The first IBL is infinite.
      _imageBasedLights[0].Shape = Shape.Infinite;

      // The other 2 IBLs have a box shape and use specify an AABB which is 
      // aligned to the with scene objects (walls, barrels, palm tree line).
      _imageBasedLights[1].Shape = new BoxShape(7.5f, 10, 7);
      _imageBasedLights[1].EnableLocalizedReflection = true;
      _imageBasedLights[1].LocalizedReflectionBox = new Aabb(new Vector3(-100, -2, -100), new Vector3(3.5f, 2, 100));

      _imageBasedLights[2].Shape = new BoxShape(7.5f, 10, 7);
      _imageBasedLights[2].EnableLocalizedReflection = true;
      _imageBasedLights[2].LocalizedReflectionBox = new Aabb(new Vector3(-3, -2, -100), new Vector3(3.5f, 2, 100));

      // Add 3 light nodes to add the IBL to the scene.
      for (int i = 0; i < 3; i++)
      {
        _lightNodes[i] = new LightNode(_imageBasedLights[i]);
        _graphicsScreen.Scene.Children.Add(_lightNodes[i]);
      }

      _lightNodes[0].PoseLocal = _globalRotation * new Pose(new Vector3(-5, 5, 5));
      _lightNodes[1].PoseLocal = _globalRotation * new Pose(new Vector3(0, 2, 0));
      _lightNodes[2].PoseLocal = _globalRotation * new Pose(new Vector3(0, 2, -4));

      // Increase specular power of ground to create sharper reflections.
      var groundModelNode = (ModelNode)_graphicsScreen.Scene.GetSubtree().Where(m => m.Name != null && m.Name.Contains("Ground")).First();
      var groundMaterial = groundModelNode.FindFirstMeshNode().Mesh.Materials[0];
      groundMaterial["GBuffer"].Set("SpecularPower", 1000000.0f);

      // Add more test objects to the scenes.
      AddLightProbes();
      AddTestSpheres();
      AddTestObjects();

      CreateGuiControls();
    }


    /// <summary>
    /// Adds small reflective spheres at the IBL positions to visualize the environment maps.
    /// </summary>
    private void AddLightProbes()
    {
      // Create sphere mesh with high specular exponent.
      var mesh = SampleHelper.CreateMesh(
        GraphicsService,
        new SphereShape(0.1f),
        new Vector3(0, 0, 0),
        new Vector3(1, 1, 1),
        1000000.0f);

      // Add a sphere under each image-based light node.
      for (int i = 0; i < _lightNodes.Length; i++)
      {
        _lightNodes[i].Children = new SceneNodeCollection(1);
        _lightNodes[i].Children.Add(new MeshNode(mesh));
      }
    }


    /// <summary>
    /// Adds bigger spheres with different diffuse and specular properties.
    /// </summary>
    private void AddTestSpheres()
    {
      const int NumX = 5;
      const int NumZ = 11;
      for (int x = 0; x < NumX; x++)
      {
        var mesh = SampleHelper.CreateMesh(
          GraphicsService,
          new SphereShape(0.2f),
          new Vector3(1 - (float)x / (NumX - 1)),  // Diffuse goes from 1 to 0.
          new Vector3((float)x / (NumX - 1)),      // Specular goes from 0 to 1.
          (float)Math.Pow(10, x));     // Specular power

        for (int z = 0; z < NumZ; z++)
        {
          _graphicsScreen.Scene.Children.Add(
            new MeshNode(mesh)
            {
              PoseWorld = _globalRotation * new Pose(new Vector3(x - 2, 1.5f, 3 - z))
            });
        }
      }
    }


    /// <summary>
    /// Adds test objects (walls, palm trees, ...) to the scene.
    /// </summary>
    private void AddTestObjects()
    {
      // Add a few palm trees.
      Random random = new Random(12345);
      for (int i = 0; i < 10; i++)
      {
        Vector3 position = new Vector3(3.5f, 0, 2.5f - i / 2.0f);
        Matrix33F orientation = Matrix33F.CreateRotationY(random.NextFloat(0, ConstantsF.TwoPi));
        float scale = random.NextFloat(0.5f, 1.2f);
        Pose pose = _globalRotation * new Pose(position, orientation);
        GameObjectService.Objects.Add(new StaticObject(Services, "PalmTree/palm_tree.drmdl", scale, pose));
      }

      // Add a vertical concrete wall.
      GameObjectService.Objects.Add(
        new StaticObject(
          Services,
          "Building/concrete_small_window_1.drmdl",
#if XNA
          new Vector3(1.0f, 1.6f, 1.8f),
          _globalRotation * new Pose(new Vector3(3.5f, 1.8f, -4f), Matrix33F.CreateRotationY(0)),
#else
          new Vector3(1.8f, 1.6f, 1.0f),
          _globalRotation * new Pose(new Vector3(3.5f, 1.8f, -4f), Matrix33F.CreateRotationY(ConstantsF.PiOver2)),
#endif
          true,
          false));

      // Add a vertical brick wall.
      GameObjectService.Objects.Add(
        new StaticObject(
          Services,
          "Building/wall_brick_1.drmdl",
          new Vector3(1.8f, 1.6f, 1),
          _globalRotation * new Pose(new Vector3(-3f, 1.8f, -4f), Matrix33F.CreateRotationY(ConstantsF.PiOver2)),
          true,
          false));

      // Create a ceiling using a rotated wall.
      GameObjectService.Objects.Add(
        new StaticObject(
          Services,
          "Building/wall_brick_1.drmdl",
          new Vector3(1.8f, 2.8f, 1),
          _globalRotation * new Pose(new Vector3(0.3f, 4, -4), Matrix33F.CreateRotationZ(ConstantsF.PiOver2) * Matrix33F.CreateRotationY(ConstantsF.PiOver2)),
          true,
          false));
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("IBL");

      SampleHelper.AddButton(
        panel,
        "Update environment maps",
        () => _updateEnvironmentMaps = true,
        null);

      for (int i = 0; i < _lightNodes.Length; i++)
      {
        int index = i;
        SampleHelper.AddCheckBox(
          panel,
          "Enable image-based light " + i,
          true,
          isChecked =>
          {
            _lightNodes[index].IsEnabled = isChecked;
          });
      }

      SampleHelper.AddCheckBox(
        panel,
        "Colorize image-based lights to show influence zones",
        false,
        isChecked =>
        {
          if (isChecked)
          {
            _imageBasedLights[0].Color = new Vector3(1, 0, 0);
            _imageBasedLights[1].Color = new Vector3(0, 1, 0);
            _imageBasedLights[2].Color = new Vector3(0, 0, 1);
          }
          else
          {
            _imageBasedLights[0].Color = new Vector3(1);
            _imageBasedLights[1].Color = new Vector3(1);
            _imageBasedLights[2].Color = new Vector3(1);
          }
        });

      SampleHelper.AddCheckBox(
        panel,
        "Enable localized reflections",
        true,
        isChecked =>
        {
          _imageBasedLights[1].EnableLocalizedReflection = isChecked;
          _imageBasedLights[2].EnableLocalizedReflection = isChecked;
        });

      SampleHelper.AddSlider(
        panel,
        "Fade-out range",
        "F2",
        0,
        1,
        _imageBasedLights[1].FalloffRange,
        value =>
        {
          _imageBasedLights[1].FalloffRange = value;
          _imageBasedLights[2].FalloffRange = value;
        });

      SampleFramework.ShowOptionsWindow("IBL");
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Unload content.
        // We have modified the material of a mesh. These changes should not
        // affect other samples. Therefore, we unload the assets. The next sample
        // will reload them with default values.)
        AssetManager.Unload();

        for (int i = 0; i < _sceneCaptureNodes.Length; i++)
        {
          _sceneCaptureNodes[i].RenderToTexture.Texture.Dispose();
          _sceneCaptureNodes[i].Dispose(false);
        }

        _colorEncoder.Dispose();
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Debug rendering:
      _graphicsScreen.DebugRenderer.Clear();
      for (int i = 0; i < _lightNodes.Length; i++)
      {
        if (!_lightNodes[i].IsEnabled)
          continue;
        
        // Draw axes at IBL positions.
        _graphicsScreen.DebugRenderer.DrawAxes(_lightNodes[i].PoseWorld, 0.5f, false);

        // Draw bounding boxes in yellow.
        if (_imageBasedLights[i].Shape is BoxShape)
          _graphicsScreen.DebugRenderer.DrawObject(_lightNodes[i], Color.Yellow, true, false);

        // Draw box for localization of reflections in orange.
        if (_imageBasedLights[i].EnableLocalizedReflection)
        {
          var aabb = _imageBasedLights[i].LocalizedReflectionBox.GetValueOrDefault();
          aabb.Minimum *= _lightNodes[i].ScaleLocal;
          aabb.Maximum *= _lightNodes[i].ScaleLocal;
          var extent = aabb.Extent;
          var pose = _lightNodes[i].PoseWorld * new Pose(aabb.Center);
          _graphicsScreen.DebugRenderer.DrawBox(extent.X, extent.Y, extent.Z, pose, Color.Orange, true, false);
        }
      }

      UpdateEnvironmentMaps();
    }


    /// <summary>
    /// Renders the environment maps for the image-based lights.
    /// </summary>
    /// <remarks>
    /// This method uses the current DeferredGraphicsScreen to render new environment maps at
    /// runtime. The DeferredGraphicsScreen has a SceneCaptureRenderer which we can use to
    /// capture environment maps of the current scene.
    /// To capture new environment maps the flag _updateEnvironmentMaps must be set to true.
    /// When this flag is set, SceneCaptureNodes are added to the scene. When the graphics
    /// screen calls the SceneCaptureRenderer the next time, the new environment maps will be
    /// captured.
    /// The flag _updateEnvironmentMaps remains true until the new environment maps are available.
    /// This method checks the SceneCaptureNode.LastFrame property to check if new environment maps
    /// have been computed. Usually, the environment maps will be available in the next frame.
    /// (However, the XNA Game class can skip graphics rendering if the game is running slowly.
    /// Then we would have to wait more than 1 frame.)
    /// When environment maps are being rendered, the image-based lights are disabled to capture
    /// only the scene with ambient and directional lights. Dynamic objects are also disabled
    /// to capture only the static scene.
    /// </remarks>
    private void UpdateEnvironmentMaps()
    {
      if (!_updateEnvironmentMaps)
        return;

      // One-time initializations: 
      if (_sceneCaptureNodes[0] == null)
      {
        // Create cube maps and scene capture nodes.
        // (Note: A cube map size of 256 is enough for surfaces with a specular power
        // in the range [0, 200000].)
        for (int i = 0; i < _sceneCaptureNodes.Length; i++)
        {
          var renderTargetCube = new RenderTargetCube(
            GraphicsService.GraphicsDevice,
            256,
            true,
            SurfaceFormat.Color,
            DepthFormat.None);

          var renderToTexture = new RenderToTexture { Texture = renderTargetCube };
          var projection = new PerspectiveProjection();
          projection.SetFieldOfView(ConstantsF.PiOver2, 1, 1, 100);
          _sceneCaptureNodes[i] = new SceneCaptureNode(renderToTexture)
          {
            CameraNode = new CameraNode(new Camera(projection))
            {
              PoseWorld = _lightNodes[i].PoseWorld,
            },
          };

          _imageBasedLights[i].Texture = renderTargetCube;
        }

        // We use a ColorEncoder to encode a HDR image in a normal Color texture.
        _colorEncoder = new ColorEncoder(GraphicsService, ColorEncoding.Rgb, ColorEncoding.Rgbm);

        // The SceneCaptureRenderer has a render callback which defines what is rendered
        // into the scene capture render targets.
        _graphicsScreen.SceneCaptureRenderer.RenderCallback = context =>
        {
          var graphicsDevice = GraphicsService.GraphicsDevice;
          var renderTargetPool = GraphicsService.RenderTargetPool;

          // Get scene nodes which are visible by the current camera.
          CustomSceneQuery sceneQuery = context.Scene.Query<CustomSceneQuery>(context.CameraNode, context);

          // The final image has to be rendered into this render target.
          var ldrTarget = context.RenderTarget;

          // Use an intermediate HDR render target with the same resolution as the final target.
          var format = new RenderTargetFormat(ldrTarget)
          {
            SurfaceFormat = SurfaceFormat.HdrBlendable,
            DepthStencilFormat = DepthFormat.Depth24Stencil8
          };
          var hdrTarget = renderTargetPool.Obtain2D(format);

          graphicsDevice.SetRenderTarget(hdrTarget);
          context.RenderTarget = hdrTarget;

          // Render scene (without post-processing, without lens flares, no debug rendering, no reticle).
          _graphicsScreen.RenderScene(sceneQuery, context, false, false, false, false);

          // Convert the HDR image to RGBM image.
          context.SourceTexture = hdrTarget;
          context.RenderTarget = ldrTarget;
          _colorEncoder.Process(context);
          context.SourceTexture = null;

          // Clean up.
          renderTargetPool.Recycle(hdrTarget);
          context.RenderTarget = ldrTarget;
        };
      }

      if (_sceneCaptureNodes[0].Parent == null)
      {
        // Add the scene capture nodes to the scene.
        for (int i = 0; i < _sceneCaptureNodes.Length; i++)
          _graphicsScreen.Scene.Children.Add(_sceneCaptureNodes[i]);

        // Remember the old time stamp of the nodes.
        _oldEnvironmentMapTimeStamp = _sceneCaptureNodes[0].LastFrame;

        // Disable all lights except ambient and directional lights.
        // We do not capture the image-based lights or any other lights (e.g. point lights)
        // in the cube map.
        foreach (var lightNode in _graphicsScreen.Scene.GetDescendants().OfType<LightNode>())
          lightNode.IsEnabled = (lightNode.Light is AmbientLight) || (lightNode.Light is DirectionalLight);

        // Disable dynamic objects.
        foreach (var node in _graphicsScreen.Scene.GetDescendants())
          if (node is MeshNode || node is LodGroupNode)
            if (!node.IsStatic)
              node.IsEnabled = false;
      }
      else
      {
        // The scene capture nodes are part of the scene. Check if they have been
        // updated.
        if (_sceneCaptureNodes[0].LastFrame != _oldEnvironmentMapTimeStamp)
        {
          // We have new environment maps. Restore the normal scene.
          for (int i = 0; i < _sceneCaptureNodes.Length; i++)
            _graphicsScreen.Scene.Children.Remove(_sceneCaptureNodes[i]);

          _updateEnvironmentMaps = false;

          foreach (var lightNode in _graphicsScreen.Scene.GetDescendants().OfType<LightNode>())
            lightNode.IsEnabled = true;

          foreach (var node in _graphicsScreen.Scene.GetDescendants())
            if (node is MeshNode || node is LodGroupNode)
              if (!node.IsStatic)
                node.IsEnabled = true;
        }
      }
    }
  }
}
#endif
