﻿using System.Linq;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics;
using DigitalRise.Graphics.Effects;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Physics;
using DigitalRise.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AssetManagementBase;

namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows different methods to blend terrain detail textures.",
    @"The TerrainMaterialLayer class has several properties which help to make transitions between
layers more interesting. The terrain in this example is a simple, flat terrain (no height values).",
    134)]
  public class TerrainTextureSample : Sample
  {
    private readonly DeferredGraphicsScreen _graphicsScreen;
    private readonly TerrainTile _terrainTile;


    public TerrainTextureSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;

      _graphicsScreen = new DeferredGraphicsScreen(Services);
      _graphicsScreen.DrawReticle = true;
      GraphicsService.Screens.Insert(0, _graphicsScreen);
      GameObjectService.Objects.Add(new DeferredGraphicsOptionsObject(Services));

      Services.AddService(typeof(DebugRenderer), _graphicsScreen.DebugRenderer);

      var scene = _graphicsScreen.Scene;
      Services.AddService(typeof(IScene), scene);

      // Add gravity and damping to the physics simulation.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a custom game object which controls the camera.
      var cameraGameObject = new CameraObject(Services, 60);
      cameraGameObject.ResetPose(new Vector3(0, 1.8f, 0), 0, 0);
      GameObjectService.Objects.Add(cameraGameObject);
      _graphicsScreen.ActiveCameraNode = cameraGameObject.CameraNode;

      for (int i = 0; i < 10; i++)
        GameObjectService.Objects.Add(new DynamicObject(Services, 1));

      GameObjectService.Objects.Add(new DynamicSkyObject(Services, true, false, true));

      // Create a simple flat terrain.
      var terrain = new Terrain();
      _terrainTile = new TerrainTile(GraphicsService)
      {
        OriginX = -100,
        OriginZ = -100,
        CellSize = 1,
      };
      terrain.Tiles.Add(_terrainTile);

      // Create a flat dummy height texture.
      float[] heights = new float[200 * 200];
      Texture2D heightTexture = null;
      TerrainHelper.CreateHeightTexture(
        GraphicsService.GraphicsDevice,
        heights,
        200,
        200,
        false,
        ref heightTexture);
      _terrainTile.HeightTexture = heightTexture;
      
      var shadowMapEffect = GraphicsService.GetStockEffect("DigitalRise/Terrain/TerrainShadowMap");
      var gBufferEffect = GraphicsService.GetStockEffect("DigitalRise/Terrain/TerrainGBuffer");
      var materialEffect = GraphicsService.GetStockEffect("DigitalRise/Terrain/TerrainMaterial");
      var material = new Material
      {
        { "ShadowMap", new EffectBinding(GraphicsService, shadowMapEffect, null, EffectParameterHint.Material) },
        { "GBuffer", new EffectBinding(GraphicsService, gBufferEffect, null, EffectParameterHint.Material) },
        { "Material", new EffectBinding(GraphicsService, materialEffect, null, EffectParameterHint.Material) }
      };
      var terrainNode = new TerrainNode(terrain, material)
      {
        DetailClipmap =
        {
          CellsPerLevel = 1364,
          NumberOfLevels = 9,
          EnableMipMap = true,
        },
      };
      scene.Children.Add(terrainNode);

      // Add 3 detail textures layers: gravel, rock, snow.
      float detailCellSize = terrainNode.DetailClipmap.CellSizes[0];
      var materialGravel = new TerrainMaterialLayer(GraphicsService)
      {
        DiffuseTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Gravel-Diffuse.dds"),
        NormalTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Gravel-Normal.dds"),
        SpecularTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Gravel-Specular.dds"),
        TileSize = detailCellSize * 512,
        BlendRange = 0.1f,
      };
      _terrainTile.Layers.Add(materialGravel);

      var noiseTexture = NoiseHelper.GetNoiseTexture(GraphicsService, 128, 60);

      var materialRock = new TerrainMaterialLayer(GraphicsService)
      {
        DiffuseTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Rock-02-Diffuse.dds"),
        NormalTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Rock-02-Normal.dds"),
        SpecularTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Rock-02-Specular.dds"),
        HeightTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Rock-02-Height.dds"),
        TileSize = detailCellSize * 1024,
        DiffuseColor = new Vector3(1 / 0.702f),
        BlendTexture = noiseTexture,
        BlendTextureChannel = 0,
        BlendRange = 0.1f,
        TerrainHeightBlendRange = 0.1f,
      };
      _terrainTile.Layers.Add(materialRock);

      var materialSnow = new TerrainMaterialLayer(GraphicsService)
      {
        DiffuseTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Snow-Diffuse.dds"),
        NormalTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Snow-Normal.dds"),
        SpecularTexture = AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "Terrain/Snow-Specular.dds"),
        TileSize = detailCellSize * 512,
        BlendTexture = noiseTexture,
        BlendTextureChannel = 1,
        BlendRange = 0.1f,
      };
      _terrainTile.Layers.Add(materialSnow);

      // Create a flat plane for collision detection.
      var rigidBody = new RigidBody(new PlaneShape(), new MassFrame(), null)
      {
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(rigidBody);

      CreateGuiControls();
    }


    private void CreateGuiControls()
    {
      var panel = SampleFramework.AddOptions("Terrain");

      // TerrainMaterialLayer.BlendRange controls how fast one layer fades out.
      SampleHelper.AddSlider(
        panel, "Blend range", null, 0, 1,
        _terrainTile.Layers.OfType<TerrainMaterialLayer>().First().BlendRange,
        value =>
        {
          foreach (var materialLayer in _terrainTile.Layers.OfType<TerrainMaterialLayer>())
            materialLayer.BlendRange = value;

          _terrainTile.Invalidate();
        });

      // TerrainMaterialLayer.BlendThreshold defines the threshold value for the blend texture.
      // Layers are drawn where their blend texture value is greater than the threshold.
      // Changing the threshold value can be used, for example, to decrease/increase the snow cover.
      SampleHelper.AddSlider(
        panel, "Snow blend threshold", null, -1, 1,
        ((TerrainMaterialLayer)_terrainTile.Layers[2]).BlendThreshold,
        value =>
        {
          ((TerrainMaterialLayer)_terrainTile.Layers[2]).BlendThreshold = value;
          _terrainTile.Invalidate();
        });

      // The TerrainMaterialLayer with the rock texture contains a height map. The height values
      // can be used to modify the blending, e.g. to show more gravel in the creases of the rock.
      SampleHelper.AddSlider(
        panel, "Rock height influence", null, -1, 1,
        ((TerrainMaterialLayer)_terrainTile.Layers[1]).BlendHeightInfluence,
        value =>
        {
          ((TerrainMaterialLayer)_terrainTile.Layers[1]).BlendHeightInfluence = value;
          _terrainTile.Invalidate();
        });

      // The layer blending can be modified by noise to make the transition borders irregular.
      SampleHelper.AddSlider(
        panel, "Snow noise influence", null, 0, 1,
        ((TerrainMaterialLayer)_terrainTile.Layers[2]).BlendNoiseInfluence,
        value =>
        {
          ((TerrainMaterialLayer)_terrainTile.Layers[2]).BlendNoiseInfluence = value;
          _terrainTile.Invalidate();
        });
      SampleHelper.AddSlider(
        panel, "Snow noise tile size", null, 1, 100,
        ((TerrainMaterialLayer)_terrainTile.Layers[2]).NoiseTileSize,
        value =>
        {
          ((TerrainMaterialLayer)_terrainTile.Layers[2]).NoiseTileSize = value;
          _terrainTile.Invalidate();
        });

      SampleFramework.ShowOptionsWindow("Terrain");
    }


    public override void Update(GameTime gameTime)
    {
      _graphicsScreen.DebugRenderer.Clear();
    }
  }
}
