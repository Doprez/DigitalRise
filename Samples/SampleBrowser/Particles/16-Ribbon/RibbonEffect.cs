﻿using AssetManagementBase;
using DigitalRise;
using DigitalRise.Graphics;
using DigitalRise.Mathematics.Statistics;
using DigitalRise.Particles;
using DigitalRise.Particles.Effectors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Samples.Particles
{
  // A particle system that draws one ribbon by connecting all particles.
  public static class RibbonEffect
  {
    public static ParticleSystem Create(IServiceProvider services)
    {
			var assetManager = services.GetService<AssetManager>();
			var graphicsService = services.GetService<IGraphicsService>();

			var ps = new ParticleSystem
      {
        Name = "Ribbon",
        MaxNumberOfParticles = 50,
      };

      // Ribbons are enabled by setting the "Type" to ParticleType.Ribbon. Consecutive 
      // living particles are connected and rendered as ribbons (quad strips). At least 
      // two living particles are required to create a ribbon. Dead particles 
      // ("NormalizedAge" ≥ 1) can be used as delimiters to terminate one ribbon and 
      // start the next ribbon. 
      ps.Parameters.AddUniform<ParticleType>(ParticleParameterNames.Type).DefaultValue =
        ParticleType.Ribbon;

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Lifetime).DefaultValue = 1;

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Position);
      ps.Effectors.Add(new StartPositionEffector());

      // The parameter "Axis" determines the orientation of the ribbon. 
      // We could use a fixed orientation. It is also possible to "twist" the ribbon
      // by using a varying parameter.
      //ps.Parameters.AddUniform<Vector3>(ParticleParameterNames.Axis).DefaultValue =
      //  Vector3.Up;

      ps.Effectors.Add(new RibbonEffector());
      ps.Effectors.Add(new ReserveParticleEffector { Reserve = 1 });

      ps.Parameters.AddUniform<float>(ParticleParameterNames.Size).DefaultValue = 1;

      ps.Parameters.AddVarying<Vector3>(ParticleParameterNames.Color);
      ps.Effectors.Add(new StartValueEffector<Vector3>
      {
        Parameter = ParticleParameterNames.Color,
        Distribution = new BoxDistribution { MinValue = new Vector3(0.5f, 0.5f, 0.5f), MaxValue = new Vector3(1, 1, 1) }
      });

      ps.Parameters.AddVarying<float>(ParticleParameterNames.Alpha);
      ps.Effectors.Add(new FuncEffector<float, float>
      {
        InputParameter = ParticleParameterNames.NormalizedAge,
        OutputParameter = ParticleParameterNames.Alpha,
        Func = age => 6.7f * age * (1 - age) * (1 - age),
      });

      ps.Parameters.AddUniform<Texture2D>(ParticleParameterNames.Texture).DefaultValue =
      assetManager.LoadTexture2D(graphicsService.GraphicsDevice, "Particles/Ribbon.dds");

      // The parameter "TextureTiling" defines how the texture spreads across the ribbon.
      // 0 ... no tiling, 
      // 1 ... repeat every particle, 
      // n ... repeat every n-th particle
      ps.Parameters.AddUniform<int>(ParticleParameterNames.TextureTiling).DefaultValue =
        1;

      ps.Parameters.AddUniform<float>(ParticleParameterNames.BlendMode).DefaultValue = 0;

      ParticleSystemValidator.Validate(ps);

      return ps;
    }
  }
}
