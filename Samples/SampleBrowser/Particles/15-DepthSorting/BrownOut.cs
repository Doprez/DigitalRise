﻿using AssetManagementBase;
using DigitalRise;
using DigitalRise.Geometry;
using DigitalRise.Graphics;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Particles;
using DigitalRise.Particles.Effectors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Samples.Particles
{
  public class BrownOut : ParticleSystem
  {
    public bool IsDepthSorted
    {
      get { return _isDepthSortedParameter.DefaultValue; }
      set { _isDepthSortedParameter.DefaultValue = value; }
    }
    private readonly IParticleParameter<bool> _isDepthSortedParameter;


    public BrownOut(IServiceProvider services)
    {
			Pose = new Pose(Matrix33F.CreateRotationX(-ConstantsF.PiOver2));

      // Smoke on a ring.
      var outerRingSmoke = CreateSmoke(services);
      outerRingSmoke.Effectors.Add(new StreamEmitter { DefaultEmissionRate = 30 });
      outerRingSmoke.Effectors.Add(new StartPositionEffector
      {
        Distribution = new CircleDistribution { OuterRadius = 5, InnerRadius = 4 }
      });

      // Smoke in the area inside the ring.
      var innerCircleSmoke = CreateSmoke(services);
      innerCircleSmoke.Effectors.Add(new StreamEmitter { DefaultEmissionRate = 10 });
      innerCircleSmoke.Effectors.Add(new StartPositionEffector
      {
        Distribution = new CircleDistribution { OuterRadius = 4, InnerRadius = 0 }
      });

      // Uniform particle parameter that are the same for all child particle systems.
      Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 5;
      _isDepthSortedParameter = Parameters.AddUniform<bool>(ParticleParameterNames.IsDepthSorted);
      _isDepthSortedParameter.DefaultValue = true;

      Children = new ParticleSystemCollection { outerRingSmoke, innerCircleSmoke };
    }


    private static ParticleSystem CreateSmoke(IServiceProvider services)
    {
			var assetManager = services.GetService<AssetManager>();
			var graphicsService = services.GetService<IGraphicsService>();

			var ps = new ParticleSystem
      {
        MaxNumberOfParticles = 200,
      };

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Position);

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution
        {
          Direction = new Vector3(0, 0, 1),
          Deviation = 0.6f,
        }
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0, 2),
      });

      ps.Effectors.Add(new LinearVelocityEffector());

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);

      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-1, 1),
      });

      ps.Effectors.Add(new AngularVelocityEffector());

      ps.Parameters.AddVarying<float>("StartSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "StartSize",
        Distribution = new UniformDistributionF(2, 3),
      });

      ps.Parameters.AddVarying<float>("EndSize");
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = "EndSize",
        Distribution = new UniformDistributionF(4, 10),
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new SingleLerpEffector
      {
        StartParameter = "StartSize",
        EndParameter = "EndSize",
        ValueParameter = "Size",
        FactorParameter = ParticleParameterNames.NormalizedAge,
      });

      ps.Parameters.AddUniform<Vector3>(ParticleParameterNames.Color).DefaultValue = new Vector3(0.6f, 0.5f, 0.4f);

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.0f,
        FadeInEnd = 0.1f,
        FadeOutStart = 0.7f,
        FadeOutEnd = 1.0f,
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
      assetManager.LoadTexture2D(graphicsService.GraphicsDevice, "Particles/Smoke.png");

      return ps;
    }
  }
}