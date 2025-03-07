﻿using DigitalRise;
using DigitalRise.GameBase;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Physics;
using AssetManagementBase;
using DigitalRise.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace Samples
{
	// Loads the "Sandbox" model and creates 5 rigid bodies that represent the walls.
  public class SandboxObject : GameObject
  {
    private readonly IServiceProvider _services;
    private ModelNode _modelNode;
    private RigidBody _floorRigidBody;
    private RigidBody _leftWallRigidBody;
    private RigidBody _rightWallRigidBody;
    private RigidBody _backWallRigidBody;
    private RigidBody _frontWallRigidBody;


    public SandboxObject(IServiceProvider services)
    {
      _services = services;
      Name = "Sandbox";
    }


    // OnLoad() is called when the GameObject is added to the IGameObjectService.
    protected override void OnLoad()
    {
      // Load sandbox model.
			var assetManager = _services.GetService<AssetManager>();
			var graphicsService = _services.GetService<IGraphicsService>();

			_modelNode = assetManager.LoadDRModel(graphicsService, "Sandbox/Sandbox.drmdl").Clone();

      foreach (var node in _modelNode.GetSubtree())
      {
        // Disable the CastsShadows flag. The inside of the box should be fully lit.
        node.CastsShadows = false;

        // If models will never move, set the IsStatic flag. This gives the engine 
        // more room for optimizations. Additionally, some effects, like certain 
        // decals, may only affect static geometry.
        node.IsStatic = true;
      }

      // Add the "Sandbox" model to the scene.
      var scene = _services.GetService<IScene>();
      scene.Children.Add(_modelNode);

      // Create rigid bodies for the sides of the sandbox.
      _floorRigidBody = new RigidBody(new PlaneShape(new Vector3(0, 1, 0), 0))
      {
        Name = "Floor",
        MotionType = MotionType.Static,
      };
      _floorRigidBody.CollisionObject.CollisionGroup = 1;
      _leftWallRigidBody = new RigidBody(new PlaneShape(new Vector3(1, 0, 0), -10))
      {
        Name = "WallLeft",
        MotionType = MotionType.Static,
      };
      _leftWallRigidBody.CollisionObject.CollisionGroup = 1;
      _rightWallRigidBody = new RigidBody(new PlaneShape(new Vector3(-1, 0, 0), -10))
      {
        Name = "WallRight",
        MotionType = MotionType.Static,
      };
      _rightWallRigidBody.CollisionObject.CollisionGroup = 1;
      _backWallRigidBody = new RigidBody(new PlaneShape(new Vector3(0, 0, 1), -10))
      {
        Name = "WallBack",
        MotionType = MotionType.Static,
      };
      _backWallRigidBody.CollisionObject.CollisionGroup = 1;
      _frontWallRigidBody = new RigidBody(new PlaneShape(new Vector3(0, 0, -1), -10))
      {
        Name = "WallFront",
        MotionType = MotionType.Static,
      };
      _frontWallRigidBody.CollisionObject.CollisionGroup = 1;

      // Add rigid bodies to simulation.
      var simulation = _services.GetService<Simulation>();
      simulation.RigidBodies.Add(_floorRigidBody);
      simulation.RigidBodies.Add(_leftWallRigidBody);
      simulation.RigidBodies.Add(_rightWallRigidBody);
      simulation.RigidBodies.Add(_backWallRigidBody);
      simulation.RigidBodies.Add(_frontWallRigidBody);
    }


    // OnUnload() is called when the GameObject is removed from the IGameObjectService.
    protected override void OnUnload()
    {
      // Remove the model from the scene.
      _modelNode.Parent.Children.Remove(_modelNode);
      _modelNode.Dispose(false);
      _modelNode = null;

      // Remove rigid bodies from simulation.
      var simulation = _floorRigidBody.Simulation;
      simulation.RigidBodies.Remove(_floorRigidBody);
      simulation.RigidBodies.Remove(_leftWallRigidBody);
      simulation.RigidBodies.Remove(_rightWallRigidBody);
      simulation.RigidBodies.Remove(_backWallRigidBody);
      simulation.RigidBodies.Remove(_frontWallRigidBody);
      _floorRigidBody = null;
      _leftWallRigidBody = null;
      _rightWallRigidBody = null;
      _backWallRigidBody = null;
      _frontWallRigidBody = null;
    }
  }
}
