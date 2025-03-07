﻿using System;
using DigitalRise.Animation.Character;
using DigitalRise.Geometry;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using AssetManagementBase;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample show how to manipulate a skeleton pose directly in code.",
    "The arm of the model is rotated in code.",
    52)]
  public class SkeletonManipulationSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    private float _upperArmAngle;
    private bool _moveArmDown;


    public SkeletonManipulationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = AssetManager.LoadDRModel(GraphicsService, "Dude/Dude.drmdl");
      _meshNode = modelNode.FindFirstMeshNode().Clone();
      _meshNode.PoseLocal = new Pose(new Vector3(-0.5f, 0, 0));
      SampleHelper.EnablePerPixelLighting(_meshNode);

      GraphicsScreen.Scene.Children.Add(_meshNode);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Change rotation angle.
      if (_moveArmDown)
        _upperArmAngle -= 0.3f * deltaTime;
      else
        _upperArmAngle += 0.3f * deltaTime;

      // Change direction when a certain angle is reached.
      if (Math.Abs(_upperArmAngle) > 0.5f)
        _moveArmDown = !_moveArmDown;

      // Get the bone index of the upper arm bone.
      var skeleton = _meshNode.Mesh.Skeleton;
      int upperArmIndex = skeleton.GetIndex("L_UpperArm");

      // Define the desired bone transform.
      SrtTransform boneTransform = new SrtTransform(MathHelper.CreateRotationY(_upperArmAngle));

      // Set the new bone transform.
      var skeletonPose = _meshNode.SkeletonPose;
      skeletonPose.SetBoneTransform(upperArmIndex, boneTransform);

      // The class SkeletonHelper provides some useful extension methods.
      // One is SetBoneRotationAbsolute() which sets the orientation of a bone relative 
      // to model space. 
      int handIndex = skeleton.GetIndex("L_Hand");
      SkeletonHelper.SetBoneRotationAbsolute(skeletonPose, handIndex, MathHelper.CreateRotationX(ConstantsF.Pi));
    }
  }
}
