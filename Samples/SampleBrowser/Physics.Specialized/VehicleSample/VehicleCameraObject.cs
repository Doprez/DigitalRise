﻿using System;
using DigitalRise;
using DigitalRise.GameBase;
using DigitalRise.Input;
using DigitalRise.Geometry;
using DigitalRise.Graphics;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRise.Mathematics.MathHelper;
using Microsoft.Xna.Framework;

namespace Samples.Physics.Specialized
{
  /// <summary>
  /// Controls the camera.
  /// </summary>
  /// <remarks>
  /// The camera has two modes: It is either attached to the vehicle or at a fixed position 
  /// looking at the vehicle ("spectator view"). 
  /// </remarks>
  [Controls(@"Camera
  Press <Enter> to toggle camera between player camera and spectator mode.
  Use Mouse or <Right Stick> to control camera in car-fixed camera mode.")]
  public class VehicleCameraObject : GameObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IServiceProvider _services;
    private readonly IInputService _inputService;
    private readonly IGeometricObject _vehicle;
    private bool _useSpectatorView;
    private float _yaw;
    private float _pitch;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public CameraNode CameraNode { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public VehicleCameraObject(IGeometricObject vehicle, IServiceProvider services)
    {
      Name = "VehicleCamera";

      _vehicle = vehicle;
      _services = services;
      _inputService = services.GetService<IInputService>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    protected override void OnLoad()
    {
      var graphicsService = _services.GetService<IGraphicsService>();

      // Define camera projection.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(
        ConstantsF.PiOver4,
        graphicsService.GraphicsDevice.Viewport.AspectRatio,
        0.1f,
        1000.0f);

      // Create a camera node.
      CameraNode = new CameraNode(new Camera(projection));
      
      SetCameraPose();
    }


    protected override void OnUnload()
    {
      CameraNode.Dispose(false);
      CameraNode = null;
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Mouse centering (controlled by the MenuComponent) is disabled if the game
      // is inactive or if the GUI is active. In these cases, we do not want to move
      // the player.
      if (!_inputService.EnableMouseCentering)
        return;

      if (_inputService.IsPressed(Keys.Enter, true))
      {
        // Toggle between player camera and spectator view.
        _useSpectatorView = !_useSpectatorView;
      }
      else
      {
        float deltaTimeF = (float)deltaTime.TotalSeconds;

        // Compute new yaw and pitch from mouse movement.
        float deltaYaw = 0;
        deltaYaw -= _inputService.MousePositionDelta.X;
        deltaYaw -= _inputService.GetGamePadState(LogicalPlayerIndex.One).ThumbSticks.Right.X * 10;
        _yaw += deltaYaw * deltaTimeF * 0.1f;

        float deltaPitch = 0;
        deltaPitch -= _inputService.MousePositionDelta.Y;
        deltaPitch += _inputService.GetGamePadState(LogicalPlayerIndex.One).ThumbSticks.Right.Y * 10;
        _pitch += deltaPitch * deltaTimeF * 0.1f;

        // Limit the pitch angle to less than +/- 90°.
        float limit = ConstantsF.PiOver2 - 0.01f;
        _pitch = MathHelper.Clamp(_pitch, -limit, limit);
      }

      // Update SceneNode.LastPoseWorld - this is required for some effects, like
      // camera motion blur. 
      CameraNode.LastPoseWorld = CameraNode.PoseWorld;

      SetCameraPose();
    }


    private void SetCameraPose()
    {
      var vehiclePose = _vehicle.Pose;

      if (_useSpectatorView)
      {
        // Spectator Mode:
        // Camera is looking at the car from a fixed location in the level.
        Vector3 position = new Vector3(10, 8, 10);
        Vector3 target = vehiclePose.Position;
        Vector3 up = Vector3.UnitY;

        // Set the new camera view matrix. (Setting the View matrix changes the Pose. 
        // The pose is simply the inverse of the view matrix). 
        CameraNode.View = Matrix44F.CreateLookAt(position, target, up);
      }
      else
      {
        // Player Camera:
        // Camera moves with the car. The look direction can be changed by moving the mouse.
        Matrix33F yaw = Matrix33F.CreateRotationY(_yaw);
        Matrix33F pitch = Matrix33F.CreateRotationX(_pitch);
        Matrix33F orientation = vehiclePose.Orientation * yaw * pitch;
        Vector3 forward = orientation * -Vector3.UnitZ;
        Vector3 up = Vector3.UnitY;
        Vector3 position = vehiclePose.Position - 10 * forward + 5 * up;
        Vector3 target = vehiclePose.Position + 1 * up;

        CameraNode.View = Matrix44F.CreateLookAt(position, target, up);
      }
    }
    #endregion
  }
}
