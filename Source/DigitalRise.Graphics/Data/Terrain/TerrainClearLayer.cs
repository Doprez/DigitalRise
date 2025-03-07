﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics.Effects;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Graphics
{
  /// <summary>
  /// Clears the terrain clipmaps.
  /// </summary>
  internal class TerrainClearLayer : IInternalTerrainLayer
  {
    /// <inheritdoc/>
    Aabb? IInternalTerrainLayer.Aabb
    {
      get { return null; }
    }


    /// <inheritdoc/>
    int IInternalTerrainLayer.FadeInStart
    {
      get { return 0; }
    }


    /// <inheritdoc/>
    int IInternalTerrainLayer.FadeOutEnd
    {
      get { return int.MaxValue; }
    }


    /// <inheritdoc/>
    public Material Material { get; private set; }


    /// <inheritdoc/>
    public MaterialInstance MaterialInstance { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainClearLayer"/> class.
    /// </summary>
    /// <param name="graphicService">The graphic service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicService"/> is <see langword="null"/>.
    /// </exception>
    public TerrainClearLayer(IGraphicsService graphicService)
    {
      if (graphicService == null)
        throw new ArgumentNullException("graphicService");

      var effect = graphicService.GetStockEffect("DigitalRise/Terrain/TerrainClearLayer");
      var effectBinding = new EffectBinding(graphicService, effect, null, EffectParameterHint.Material);
      Material = new Material
      {
        { "Base", effectBinding },
        { "Detail", effectBinding }
      };
      MaterialInstance = new MaterialInstance(Material);
    }


    /// <inheritdoc/>
    public void OnDraw(GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2 topLeftPosition, Vector2 bottomRightPosition)
    {
      graphicsDevice.DrawQuad(rectangle, topLeftPosition, bottomRightPosition);
    }
  }
}
