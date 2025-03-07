﻿using System.Linq;
using DigitalRise.Animation.Character;
using DigitalRise.Geometry;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AssetManagementBase;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample shows how to use a ClosedFormIKSample to let an arm reach for a target.",
    "",
    73)]
  [Controls(@"Sample
  Press <4>-<9> on the numpad to move the target.")]
  public class ClosedFormIKSample : CharacterAnimationSample
  {
    private readonly MeshNode _meshNode;

    private Vector3 _targetPosition = new Vector3(0.3f, 1, 0.3f);
    private readonly ClosedFormIKSolver _ikSolver;


    public ClosedFormIKSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var modelNode = AssetManager.LoadDRModel(GraphicsService, "Dude/Dude.drmdl");
      _meshNode = modelNode.FindFirstMeshNode().Clone();
      _meshNode.PoseLocal = new Pose(new Vector3(0, 0, 0));
      SampleHelper.EnablePerPixelLighting(_meshNode);
      GraphicsScreen.Scene.Children.Add(_meshNode);

      // Create the IK solver. The ClosedFormIKSolver uses an analytic solution to compute
      // IK for arbitrary long bone chains. It does not support bone rotation limits.
      _ikSolver = new ClosedFormIKSolver
      {
        SkeletonPose = _meshNode.SkeletonPose,

        // The chain starts at the upper arm.
        RootBoneIndex = 14,

        // The chain ends at the hand bone.
        TipBoneIndex = 16,

        // The offset from the hand center to the hand origin.
        TipOffset = new Vector3(0.1f, 0, 0),
      };
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // ----- Move target if <NumPad4-9> are pressed.
      Vector3 translation = new Vector3();
      if (InputService.IsDown(Keys.NumPad4))
        translation.X -= 1;
      if (InputService.IsDown(Keys.NumPad6))
        translation.X += 1;
      if (InputService.IsDown(Keys.NumPad8))
        translation.Y += 1;
      if (InputService.IsDown(Keys.NumPad5))
        translation.Y -= 1;
      if (InputService.IsDown(Keys.NumPad9))
        translation.Z += 1;
      if (InputService.IsDown(Keys.NumPad7))
        translation.Z -= 1;

      translation = translation * deltaTime;
      _targetPosition += translation;

      // Convert target world space position to model space. - The IK solvers work in model space.
      Vector3 localTargetPosition = _meshNode.PoseWorld.ToLocalPosition(_targetPosition);

      // Reset the affected bones. This is optional. It removes unwanted twist from the bones.
      _meshNode.SkeletonPose.ResetBoneTransforms(_ikSolver.RootBoneIndex, _ikSolver.TipBoneIndex);

      // Let IK solver update the bones.
      _ikSolver.Target = localTargetPosition;
      _ikSolver.Solve(deltaTime);

      // Draws the IK target.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      debugRenderer.DrawAxes(new Pose(_targetPosition), 0.1f, false);
    }
  }
}
