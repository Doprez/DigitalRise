﻿using System;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Plane = DigitalRise.Geometry.Shapes.Plane;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples tests oblique view frustums.",
    @"",
    1000)]
  public class ObliqueFrustumSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private DebugRenderer _debugRenderer;
    private float _defaultPlaneMeshSize = PlaneShape.MeshSize;


    public ObliqueFrustumSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, graphicsScreen);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      GameObjectService.Objects.Add(_cameraObject);

      var spriteFont = DefaultAssets.DefaultFont;
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);

      TestClippedProjection();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        PlaneShape.MeshSize = _defaultPlaneMeshSize;
      }

      base.Dispose(disposing);
    }


    private void TestClippedProjection()
    {
      RandomHelper.Random = new Random(1234567);

      var p = new PerspectiveProjection();
      p.SetOffCenter(-0.1f, 0.2f, -0.1f, 0.1f, 0.1f, 1);
      _debugRenderer.DrawViewVolume(p.ViewVolume, new Pose(new Vector3(0, 2, 0)), Color.Red, true, false);

      p.NearClipPlane = new Plane(new Vector3(-0.1f, +0.1f, 1).Normalized(), -0.4f);

      PlaneShape.MeshSize = 2;
      _debugRenderer.DrawShape(new PlaneShape(p.NearClipPlane.Value), new Pose(new Vector3(0, 2, 0)), Vector3.One, Color.Green, false, false);
      
      Matrix44F m = p.ToMatrix44F();

      for (int i = 0; i < 100000; i++)
      {
        Aabb aabb = p.ViewVolume.GetAabb(Pose.Identity);
        aabb.Minimum -= new Vector3(1);
        aabb.Maximum += new Vector3(1);
        float x = RandomHelper.Random.NextFloat(aabb.Minimum.X, aabb.Maximum.X);
        float y = RandomHelper.Random.NextFloat(aabb.Minimum.Y, aabb.Maximum.Y);
        float z = RandomHelper.Random.NextFloat(aabb.Minimum.Z, aabb.Maximum.Z);

        //if (RandomHelper.Random.NextBool())
        //  x = 0;
        //else
        //  y = 0;

        Vector4 c = m * new Vector4(x, y, z, 1);
        c /= c.W;
        Color color = Color.Orange;
        if (c.X < -1 || c.X > 1 || c.Y < -1 || c.Y > 1 || c.Z < 0 || c.Z > 1)
          continue;// color = Color.Gray;

        _debugRenderer.DrawPoint(new Vector3(x, y + 2, z), color, false);
      }
    }


    private void Render(RenderContext context)
    {
      var cameraNode = _cameraObject.CameraNode;
      context.CameraNode = cameraNode;

      var device = context.GraphicsService.GraphicsDevice;
      device.Clear(Color.White);
      device.DepthStencilState = DepthStencilState.Default;
      device.RasterizerState = RasterizerState.CullCounterClockwise;
      device.BlendState = BlendState.Opaque;

      _debugRenderer.Render(context);

      context.CameraNode = null;
    }
  }
}
