﻿using System.Collections.Generic;
using System.Linq;
using DigitalRise;
using DigitalRise.GameBase;
using DigitalRise.Graphics;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using AssetManagementBase;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;
using System;

namespace Samples
{
  // Creates a set of predefined static decals.
  [Controls(@"Decals
  Press <F4> to show Options window where you can toggle decals.")]
  public class EnvironmentDecalsObject : GameObject
  {
    private readonly IServiceProvider _services;
    private DebugRenderer _debugRenderer;
    private readonly List<DecalNode> _decals = new List<DecalNode>();


    public bool IsEnabled
    {
      get { return _decals[0].IsEnabled; }
      set
      {
        foreach (var node in _decals)
          node.IsEnabled = value;
      }
    }


    public EnvironmentDecalsObject(IServiceProvider services)
    {
      _services = services;
      Name = "EnvironmentDecals";
    }


    protected override void OnLoad()
    {
      var graphicsService = _services.GetService<IGraphicsService>();
      var assetManager = _services.GetService<AssetManager>();
      var scene = _services.GetService<IScene>();
      _debugRenderer = _services.GetService<DebugRenderer>();

      // Load materials (*.drmat files) which define the used shaders and material 
      // properties (e.g. textures, colors, etc.).
      var bloodMaterial = assetManager.LoadDRMaterial(graphicsService, "Decals/Decal.drmat");        // Original: "Decals/Blood"
      var crackMaterial = assetManager.LoadDRMaterial(graphicsService, "Decals/Decal.drmat");        // Original: "Decals/Crack"
      var bulletHoleMaterial = assetManager.LoadDRMaterial(graphicsService, "Decals/Decal.drmat");   // Original: "Decals/BulletHole"

      // Decal materials (like materials of meshes) usually have several render passes
      // (such as "GBuffer", "Material"). Decal materials without a "Material" pass can
      // be used to render only normal maps without color changes:
      //crackMaterial.Remove("Material");

      // Add some DecalNodes, which define where a material is projected onto the scene.
      var bloodDecal0 = new DecalNode(bloodMaterial);
      bloodDecal0.NormalThreshold = MathHelper.ToRadians(75);
      bloodDecal0.LookAt(new Vector3(0.7f, 0.6f, 0.7f), new Vector3(0.7f, 0.6f, 0), new Vector3(0.1f, 1, 0));
      bloodDecal0.Width = 1.1f;
      bloodDecal0.Height = 1.1f;
      bloodDecal0.Depth = 1;
      bloodDecal0.Options = DecalOptions.ProjectOnStatic;
      scene.Children.Add(bloodDecal0);
      _decals.Add(bloodDecal0);

      var bloodDecal1 = new DecalNode(bloodMaterial);
      bloodDecal1.LookAt(new Vector3(0.0f, 0.2f, 1.9f), new Vector3(0.0f, 0, 1.9f), new Vector3(1.0f, 0, -0.5f));
      bloodDecal1.Width = 1.6f;
      bloodDecal1.Height = 1.6f;
      bloodDecal1.Depth = 0.5f;
      scene.Children.Add(bloodDecal1);
      _decals.Add(bloodDecal1);

      var crackDecal0 = new DecalNode(crackMaterial);
      crackDecal0.NormalThreshold = MathHelper.ToRadians(75);
      crackDecal0.LookAt(new Vector3(-0.7f, 0.7f, 0.7f), new Vector3(-0.7f, 0.7f, 0), new Vector3(0.1f, 1, 0));
      crackDecal0.Width = 1.75f;
      crackDecal0.Height = 1.75f;
      crackDecal0.Depth = 0.6f;
      crackDecal0.Options = DecalOptions.ProjectOnStatic;
      scene.Children.Add(crackDecal0);
      _decals.Add(crackDecal0);

      var crackDecal1 = crackDecal0.Clone();
      var position = new Vector3(2.0f, 0.2f, 2.0f);
      crackDecal1.LookAt(position, position + new Vector3(0, -1, 0), new Vector3(-0.8f, 0, 1.0f));
      crackDecal1.Width = 2.5f;
      crackDecal1.Height = 2.5f;
      crackDecal1.Depth = 0.5f;
      scene.Children.Add(crackDecal1);
      _decals.Add(crackDecal1);

      var bulletHole0 = new DecalNode(bulletHoleMaterial);
      bulletHole0.NormalThreshold = MathHelper.ToRadians(90);
      bulletHole0.LookAt(new Vector3(0.0f, 0.8f, 0.7f), new Vector3(0.0f, 0.7f, 0), new Vector3(0.1f, -1, 0));
      bulletHole0.Width = 0.20f;
      bulletHole0.Height = 0.20f;
      bulletHole0.Depth = 1f;
      bulletHole0.DrawOrder = 10;   // Draw over other decals.
      scene.Children.Add(bulletHole0);
      _decals.Add(bulletHole0);

      var bulletHole1 = bulletHole0.Clone();
      bulletHole1.LookAt(new Vector3(-0.4f, 0.9f, 0.7f), new Vector3(-0.4f, 0.9f, 0), new Vector3(0.1f, 1, 0));
      scene.Children.Add(bulletHole1);
      _decals.Add(bulletHole1);

      var bulletHole2 = bulletHole0.Clone();
      bulletHole2.LookAt(new Vector3(-0.2f, 0.8f, 0.7f), new Vector3(-0.2f, 0.0f, 0), new Vector3(0.1f, -1, 0));
      scene.Children.Add(bulletHole2);
      _decals.Add(bulletHole2);

      var bulletHole3 = bulletHole0.Clone();
      bulletHole3.LookAt(new Vector3(3.0f, 1.0f, 2.0f), new Vector3(3.0f, 1.0f, 1), new Vector3(0.3f, 1, 0));
      scene.Children.Add(bulletHole3);
      _decals.Add(bulletHole3);

      var bulletHole4 = bulletHole0.Clone();
      bulletHole4.LookAt(new Vector3(2.5f, 0.7f, 2.0f), new Vector3(3.0f, 0.7f, 1.0f), new Vector3(-0.1f, -1, 0));
      scene.Children.Add(bulletHole4);
      _decals.Add(bulletHole4);

      var bulletHole5 = bulletHole0.Clone();
      bulletHole5.LookAt(new Vector3(2.7f, 1.2f, 2.0f), new Vector3(3.0f, 1.2f, 1.0f), new Vector3(-0.5f, -1, 0));
      scene.Children.Add(bulletHole5);
      _decals.Add(bulletHole5);

      var bulletHole6 = bulletHole0.Clone();
      bulletHole6.LookAt(new Vector3(3.2f, 0.4f, 2.0f), new Vector3(3.0f, 0.4f, 1), new Vector3(-0.3f, -0.5f, 0));
      scene.Children.Add(bulletHole6);
      _decals.Add(bulletHole6);

      // Get the first dynamic mesh (the rusty cube) and add a decal as a child.
      MeshNode meshNode = ((Scene)scene).MeshNodes().First(n => !n.IsStatic);
      var bulletHole7 = bulletHole0.Clone();
      bulletHole7.LookAt(new Vector3(0, 0, -0.6f), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
      bulletHole7.Depth = 0.2f;
      meshNode.Children = new SceneNodeCollection { bulletHole7 };
      _decals.Add(bulletHole7);

      // Add GUI controls to the Options window.
      var sampleFramework = _services.GetService<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("Game Objects");
      var panel = SampleHelper.AddGroupBox(optionsPanel, "EnvironmentDecalsObject");
      SampleHelper.AddCheckBox(
          panel,
          "Enable decals",
          IsEnabled,
          isChecked => IsEnabled = isChecked);
    }


    protected override void OnUnload()
    {
      foreach (var decal in _decals)
      {
        decal.Parent.Children.Remove(decal);
        decal.Dispose(false);
      }

      _decals.Clear();
    }


    protected override void OnUpdate(System.TimeSpan deltaTime)
    {
      // For debugging: Render the decal bounding boxes.

      //if (!IsEnabled)
      //  return;

      //foreach (var decalNode in _decals)
      //  _debugRenderer.DrawObject(decalNode, Color.Pink, true, true);
    }
  }
}