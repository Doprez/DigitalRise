﻿using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Physics;
using DigitalRise.Physics.ForceEffects;
using AssetManagementBase;
using Microsoft.Xna.Framework;

namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "Loads a model and a convex hull that was generated in the XNA content pipeline.",
    "Tip: Press <M> to toggle wireframe mode to see the model.",
    31)]
  public class ConvexHullSample : PhysicsSample
  {
    // This sample loads the "saucer" model. In the content project, the processor was set to the
    // custom ModelWithConvexHullProcessor (see project "Samples.Content.Pipeline"). 
    // This processor  generates a convex shape and stores it in the UserData of the ModelNode.

    private readonly ModelNode _saucerModelNode;
    private readonly RigidBody _saucerBody;


    public ConvexHullSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3.UnitY, 0))
      {
        Name = "GroundPlane",            // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // Load model and add it to the graphics scene.
      _saucerModelNode = AssetManager.LoadDRModel(GraphicsService, "Saucer2/saucer.drmdl").Clone();
      GraphicsScreen.Scene.Children.Add(_saucerModelNode);

      // Create rigid body for this model.
      // The tag contains the collision shape (created in the content processor).
      Shape saucerShape = (Shape)_saucerModelNode.UserData;
      _saucerBody = new RigidBody(saucerShape)
      {
        Pose = new Pose(new Vector3(0, 2, 0), RandomHelper.Random.NextQuaternion())
      };
      Simulation.RigidBodies.Add(_saucerBody);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _saucerModelNode.Dispose(false);

        // Detach shape from rigid body to avoid any "memory leaks".
        _saucerBody.Shape = Shape.Empty;
      }

      base.Dispose(disposing);
    }


    public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
      // Update SceneNode.LastPoseWorld (required for optional effects, like motion blur).
      _saucerModelNode.SetLastPose(true);

      // Synchronize pose of rigid body and model.
      _saucerModelNode.PoseWorld = _saucerBody.Pose;

      base.Update(gameTime);
    }
  }
}
