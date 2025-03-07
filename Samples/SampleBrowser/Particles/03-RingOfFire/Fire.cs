﻿using AssetManagementBase;
using DigitalRise;
using DigitalRise.Graphics;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Particles;
using DigitalRise.Particles.Effectors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Samples.Particles
{
  // Creates a fire effect.
  public static class Fire 
  {
    public static ParticleSystem Create(IServiceProvider services)
    {
			var assetManager = services.GetService<AssetManager>();
			var graphicsService = services.GetService<IGraphicsService>();

			ParticleSystem ps = new ParticleSystem
      {
        Name = "Fire",
        MaxNumberOfParticles = 300
      };

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 2;

      ps.Effectors.Add(new StreamEmitter
      {
        DefaultEmissionRate = 120,
      });

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        Distribution = new CircleDistribution { OuterRadius = 2, InnerRadius = 2}
      });

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        
        // The start direction can be any direction (direction deviation is 360°).
        Distribution = new DirectionDistribution { Deviation = ConstantsF.TwoPi, Direction = Vector3.UnitY },
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(0, 0.5f),
      });

      ps.Effectors.Add(new LinearVelocityEffector());

      ps.Parameters.AddUniform<Vector3>(ParticleParameterNames.LinearAcceleration).DefaultValue = new Vector3(0, 1, 0);
      ps.Effectors.Add(new LinearAccelerationEffector());

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.Pi, ConstantsF.Pi),
      });

      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 1f;
      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        FadeInStart = 0f,
        FadeInEnd = 0.1f,
        FadeOutStart = 0.2f,
        FadeOutEnd = 1f,
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Size,
        Distribution = new UniformDistributionF(0.5f, 1),
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
        assetManager.LoadTexture2D(graphicsService.GraphicsDevice, "Particles/Fire.dds");

      // Fire needs additive blending.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0.0f; 

      // Fire should be drawn on top of other effects (like smoke). 
      // "DrawOrder" is supported by the ParticleBatch renderer.
      ps.Parameters.AddUniform<int>(ParticleParameterNames.DrawOrder).DefaultValue = 100; 

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}