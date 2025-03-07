﻿using AssetManagementBase;
using DigitalRise;
using DigitalRise.Graphics;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Particles;
using DigitalRise.Particles.Effectors;
using Microsoft.Xna.Framework;
using System;

namespace Samples.Particles
{
  // Creates a flame jet effect. 
  public static class FlameJet
  {
    public static ParticleSystem Create(IServiceProvider services)
    {
			var assetManager = services.GetService<AssetManager>();
			var graphicsService = services.GetService<IGraphicsService>();

			var ps = new ParticleSystem
      {
        Name = "FlameJet",
        MaxNumberOfParticles = 500,
      };

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Lifetime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Lifetime,
        Distribution = new UniformDistributionF(0.8f, 1.2f),
      });

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector
      {
        Parameter = ParticleParameterNames.Position,
        DefaultValue = Vector3.Zero,
      });

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Direction);
      ps.Effectors.Add(new StartDirectionEffector
      {
        Parameter = ParticleParameterNames.Direction,
        Distribution = new DirectionDistribution { Deviation = 0.05f, Direction = Vector3.Forward },
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.LinearSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.LinearSpeed,
        Distribution = new UniformDistributionF(8, 12),
      });

      ps.Effectors.Add(new LinearVelocityEffector
      {
        PositionParameter = ParticleParameterNames.Position,
        DirectionParameter = ParticleParameterNames.Direction,
        SpeedParameter = ParticleParameterNames.LinearSpeed,
      });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Damping).DefaultValue = 1.0f;
      ps.Effectors.Add(new SingleDampingEffector
      {
        ValueParameter = ParticleParameterNames.LinearSpeed,
        DampingParameter = ParticleParameterNames.Damping,
      });

      ps.Parameters.AddUniform<Vector3>("Buoyancy").DefaultValue = new Vector3(0, 4, 0);
      ps.Effectors.Add(new LinearAccelerationEffector { AccelerationParameter = "Buoyancy" });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Angle);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.Angle,
        Distribution = new UniformDistributionF(-ConstantsF.Pi, ConstantsF.Pi),
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.AngularSpeed);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AngularSpeed,
        Distribution = new UniformDistributionF(-2f, 2f),
      });

      ps.Effectors.Add(new AngularVelocityEffector
      {
        AngleParameter = ParticleParameterNames.Angle,
        SpeedParameter = ParticleParameterNames.AngularSpeed,
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Size);
      ps.Parameters.AddUniform<float>("StartSize").DefaultValue = 0.0f;
      ps.Parameters.AddUniform<float>("EndSize").DefaultValue = 0.8f;
      ps.Effectors.Add(new SingleLerpEffector
      {
        ValueParameter = ParticleParameterNames.Size,
        FactorParameter = ParticleParameterNames.NormalizedAge,
        StartParameter = "StartSize",
        EndParameter = "EndSize",
      });

      ps.Parameters.AddUniform<Vector3>("StartColor").DefaultValue = new Vector3(0.25f, 0.25f, 1.0f);
      ps.Parameters.AddUniform<Vector3>("EndColor").DefaultValue = new Vector3(1.0f, 0.9f, 0.8f);
      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Color);
      ps.Effectors.Add(new Vector3LerpEffector
      {
        ValueParameter = ParticleParameterNames.Color,
        StartParameter = "StartColor",
        EndParameter = "EndColor",
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Parameters.AddUniform<float>("TargetAlpha").DefaultValue = 0.2f;
      ps.Effectors.Add(new SingleFadeEffector
      {
        ValueParameter = ParticleParameterNames.Alpha,
        TargetValueParameter = "TargetAlpha",
        TimeParameter = ParticleParameterNames.NormalizedAge,
        FadeInStart = 0.0f,
        FadeInEnd = 0.1f,
        FadeOutStart = 0.8f,
        FadeOutEnd = 1.0f,
      });

      // DigitalRise Graphics supports "texture atlases": The class PackedTexture 
      // describes a single texture or tile set packed into a texture atlas. The 
      // fire texture in this example consists of 4 textures.
      ps.Parameters.AddUniform<PackedTexture>(ParticleParameterNames.Texture).DefaultValue =
        new PackedTexture(
          "Flames",
          assetManager.LoadTexture2D(graphicsService.GraphicsDevice, "Particles/Flames.dds"),
          Vector2.Zero, Vector2.One,
          4, 1);

      // The particle parameter "AnimationTime" determines which tile is used,
      // where 0 = first tile, 1 = last tile.
      // --> Chooses a random tile for each particle when it is created.
      ps.Parameters.AddVarying<float>(ParticleParameterNames.AnimationTime);
      ps.Effectors.Add(new StartValueEffector<float>
      {
        Parameter = ParticleParameterNames.AnimationTime,
        Distribution = new UniformDistributionF(0, 1),
      });

      // Fire needs additive blending.
      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      return ps;
    }
  }
}