﻿using System;
using System.Linq;
using DigitalRise.Animation;
using DigitalRise.Geometry;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AssetManagementBase;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to use the XNA SkinnedEffect and how to procedurally create a mesh 
for a sky dome.",
    "",
    24)]
  public class SkinnedEffectSample : BasicSample
  {
    // The animation controller of the dude's animation.
    private AnimationController _animationController;

    // A sky dome - see ProceduralSkyDome.cs.
    private readonly MeshNode _sky;


    public SkinnedEffectSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3(1, 1, 3), 0.2f, 0);

      // Create a sky mesh and add an instance of this mesh to the scene.
      var skyMesh = ProceduralSkyDome.CreateMesh(GraphicsService, AssetManager.LoadTexture2D(GraphicsService.GraphicsDevice, "sky.png"));
      _sky = new MeshNode(skyMesh);
      _sky.Name = "Sky.png"; // Always set a name - very useful for debugging!
      GraphicsScreen.Scene.Children.Add(_sky);

      // Load the skinned model. This model is processed using the DigitalRise Model 
      // Processor - not the default XNA model processor!
      // In the folder that contains dude.fbx, there are several XML files (*.drmdl and *.drmat) 
      // which define the materials of the model. These material description files are 
      // automatically processed by the DigitalRise Model Processor. Please browse 
      // to the content folder and have a look at the *.drmdl and *.drmat files.
      var dudeModel = AssetManager.LoadDRModel(GraphicsService, "Dude/Dude.drmdl");
      dudeModel = dudeModel.Clone();
      dudeModel.PoseWorld = new Pose(Matrix33F.CreateRotationY(ConstantsF.Pi));
      GraphicsScreen.Scene.Children.Add(dudeModel);

      // The dude model consists of a single mesh.
      var dudeMeshNode = dudeModel.FindFirstMeshNode();
      var mesh = dudeMeshNode.Mesh;

      /*
        // The dude mesh consists of different materials (head, eyes, torso, ...).
        // We could change some of the material properties...
        foreach (var material in mesh.Materials)
        {
          // Get all SkinnedEffectBindings which wrap the XNA SkinnedEffect. 
          // A material can consist of several effects - one effect for each render pass.
          // (Per default there is only one render pass called "Default".)
          foreach (var effectBinding in material.EffectBindings.OfType<SkinnedEffectBinding>())
          {
            // We could change effect parameters here, for example:
            effectBinding.PreferPerPixelLighting = true;
          }
        }
      */

      // The DigitalRise Model Processor also loads animations.
      // Start the first animation of the dude and let it loop forever.
      // (We keep the animation controller to be able to stop the animation in 
      // Dispose() below.)
      var timeline = new TimelineClip(mesh.Animations.Values.First())
      {
        Duration = TimeSpan.MaxValue,
        LoopBehavior = LoopBehavior.Cycle,
      };
      _animationController = AnimationService.StartAnimation(timeline, (IAnimatableProperty)dudeMeshNode.SkeletonPose);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _animationController.Stop();
        _animationController.Recycle();
      }

      base.Dispose(disposing);
    }
  }
}
