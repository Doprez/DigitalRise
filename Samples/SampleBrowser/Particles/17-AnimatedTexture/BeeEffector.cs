﻿using System;
using DigitalRise.Geometry;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Particles;
using Microsoft.Xna.Framework;

namespace Samples.Particles
{
  // Creates a random bee movement. Each bee chooses a target position, flies to 
  // this position and then chooses a new target position.
  // This effector needs the CameraPose (via a uniform parameter) to adjust the 
  // billboard orientation.
  public class BeeEffector : ParticleEffector
  {
    private readonly BoxDistribution _distribution;
    private IParticleParameter<Vector3> _positionParameter;
    private IParticleParameter<Vector3> _targetPositionParameter;
    private IParticleParameter<float> _speedParameter;
    private IParticleParameter<float> _sizeXParameter;
    private IParticleParameter<Pose> _cameraPoseParameter;


    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string PositionParameter { get; set; }

    [ParticleParameter(ParticleParameterUsage.Out)]
    public string TargetPositionParameter { get; set; }

    [ParticleParameter(ParticleParameterUsage.In)]
    public string SpeedParameter { get; set; }

    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string SizeXParameter { get; set; }

    [ParticleParameter(ParticleParameterUsage.In, Optional = true)]
    public string CameraPoseParameter { get; set; }

    public bool InvertLookDirection { get; set; }
    public float MaxRange { get; set; }


    public BeeEffector()
    {
      PositionParameter = ParticleParameterNames.Position;
      SpeedParameter = ParticleParameterNames.LinearSpeed;
      SizeXParameter = ParticleParameterNames.SizeX;
      _distribution = new BoxDistribution { MinValue = -Vector3.One, MaxValue = Vector3.One };
      MaxRange = 1.0f;
    }


    protected override ParticleEffector CreateInstanceCore()
    {
      return new BeeEffector();
    }


    protected override void CloneCore(ParticleEffector source)
    {
      base.CloneCore(source);

      var sourceTyped = (BeeEffector)source;
      PositionParameter = sourceTyped.PositionParameter;
      TargetPositionParameter = sourceTyped.TargetPositionParameter;
      SpeedParameter = sourceTyped.SpeedParameter;
      SizeXParameter = sourceTyped.SizeXParameter;
      CameraPoseParameter = sourceTyped.CameraPoseParameter;
      InvertLookDirection = sourceTyped.InvertLookDirection;
      MaxRange = sourceTyped.MaxRange;
    }


    protected override void OnRequeryParameters()
    {
      _positionParameter = ParticleSystem.Parameters.Get<Vector3>(PositionParameter);
      _targetPositionParameter = ParticleSystem.Parameters.Get<Vector3>(TargetPositionParameter);
      _speedParameter = ParticleSystem.Parameters.Get<float>(SpeedParameter);
      _sizeXParameter = ParticleSystem.Parameters.Get<float>(SizeXParameter);
      _cameraPoseParameter = ParticleSystem.Parameters.Get<Pose>(CameraPoseParameter);
    }


    protected override void OnUninitialize()
    {
      _positionParameter = null;
      _targetPositionParameter = null;
      _speedParameter = null;
      _sizeXParameter = null;
      _cameraPoseParameter = null;
    }


    protected override void OnInitializeParticles(int startIndex, int count, object emitter)
    {
      if (_positionParameter == null
          || _targetPositionParameter == null)
        return;

      Vector3[] positions = _positionParameter.Values;
      Vector3[] targetPositions = _targetPositionParameter.Values;
      if (positions == null || targetPositions == null)
      {
        // This effector only works with varying parameters.
        return;
      }

      // Initialize the TargetPositions.
      for (int i = startIndex; i < startIndex + count; i++)
        targetPositions[i] = positions[i];
    }


    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_positionParameter == null
          || _targetPositionParameter == null
          || _speedParameter == null
          || _sizeXParameter == null)
      {
        return;
      }

      Vector3[] positions = _positionParameter.Values;
      Vector3[] targetPositions = _targetPositionParameter.Values;
      float[] speeds = _speedParameter.Values;
      float[] sizes = _sizeXParameter.Values;
      Pose cameraPose = (_cameraPoseParameter != null) ? _cameraPoseParameter.DefaultValue : Pose.Identity;

      if (positions == null || targetPositions == null || speeds == null)
      {
        // This effector only works with varying parameters.
        return;
      }

      Random random = ParticleSystem.Random;
      Vector3 center = ParticleSystem.Pose.Position;
      float dt = (float)deltaTime.TotalSeconds;
      for (int i = startIndex; i < startIndex + count; i++)
      {
        Vector3 targetPosition = targetPositions[i];
        Vector3 lineSegment = targetPosition - positions[i];
        float distance = lineSegment.Length();
        if (Numeric.IsZero(distance))
        {
          // The bee has reached the target position. Choose a new direction and distance.
          Vector3 d = _distribution.Next(random);

          // Keep the new target position within a certain range.
          if (Math.Abs(targetPosition.X + d.X - center.X) > MaxRange)
            d.X = -d.X;
          if (Math.Abs(targetPosition.Y + d.Y - center.Y) > MaxRange)
            d.Y = -d.Y;
          if (Math.Abs(targetPosition.Z + d.Z - center.Z) > MaxRange)
            d.Z = -d.Z;

          targetPositions[i] = targetPosition + d;
        }
        else
        {
          // Move bee towards target position.
          Vector3 movementDirection = lineSegment / distance;
          float movementDistance = speeds[i] * dt;
          if (movementDistance > distance)
            movementDistance = distance;

          positions[i] += movementDirection * movementDistance;

          // The bee should look in the direction it is flying. Transform the direction
          // into the view space and check the sign of x-component.
          movementDirection = cameraPose.ToLocalDirection(movementDirection);
          float sign = (movementDirection.X >= 0) ? +1 : -1;
          if (InvertLookDirection)
            sign *= -1;

          if (Math.Sign(sizes[i]) != (int)sign)
            sizes[i] *= -1;
        }
      }
    }
  }
}
