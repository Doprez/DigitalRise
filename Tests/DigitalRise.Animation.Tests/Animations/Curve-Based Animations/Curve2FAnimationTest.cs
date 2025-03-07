﻿using System;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Interpolation;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Animation.Tests
{
  [TestFixture]
  public class Curve2FAnimationTest
  {
    [Test]
    public void CheckDefaultValues()
    {
      var animation = new Curve2FAnimation();
      Assert.IsNull(animation.Curve);  
    }


    [Test]
    public void ShouldDoNothingWhenCurveIsNull()
    {
      var animation = new Curve2FAnimation();

      Assert.AreEqual(1.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 1.0f, 100.0f));
    }


    [Test]
    public void ShouldDoNothingWhenCurveIsEmpty()
    {
      var animation = new Curve2FAnimation();
      animation.Curve = new Curve2F();

      Assert.AreEqual(1.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 1.0f, 100.0f));
    }


    [Test]
    public void GetTotalDurationTest()
    {
      var animation = new Curve2FAnimation();
      animation.Curve = new Curve2F
      {
        new CurveKey2F { Point = new Vector2(2.0f, 22.0f) },
        new CurveKey2F { Point = new Vector2(3.0f, 33.0f) },
        new CurveKey2F { Point = new Vector2(4.0f, 44.0f) },
      };
      animation.Curve.PreLoop = Mathematics.Interpolation.CurveLoopType.Linear;
      animation.Curve.PostLoop = Mathematics.Interpolation.CurveLoopType.Cycle;
      animation.EndParameter = float.NaN;

      Assert.AreEqual(TimeSpan.FromSeconds(4.0), animation.GetTotalDuration());

      animation.EndParameter = float.PositiveInfinity;
      Assert.AreEqual(TimeSpan.MaxValue, animation.GetTotalDuration());
    }


    [Test]
    public void SimpleCurve()
    {
      var animation = new Curve2FAnimation();
      animation.Curve = new Curve2F
      {
        new CurveKey2F { Point = new Vector2(2.0f, 22.0f) },
        new CurveKey2F { Point = new Vector2(3.0f, 33.0f) },
        new CurveKey2F { Point = new Vector2(4.0f, 44.0f) },
      };
      animation.Curve.PreLoop = Mathematics.Interpolation.CurveLoopType.Linear;
      animation.Curve.PostLoop = Mathematics.Interpolation.CurveLoopType.Cycle;
      animation.EndParameter = float.PositiveInfinity;

      float defaultSource = -100.0f;
      float defaultTarget = 100.0f;

      // Pre-Loop
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget));
      Assert.AreEqual(11.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), defaultSource, defaultTarget));

      Assert.AreEqual(22.0f, animation.GetValue(TimeSpan.FromSeconds(2.0), defaultSource, defaultTarget));
      Assert.AreEqual(33.0f, animation.GetValue(TimeSpan.FromSeconds(3.0), defaultSource, defaultTarget));
      Assert.AreEqual(44.0f, animation.GetValue(TimeSpan.FromSeconds(4.0), defaultSource, defaultTarget));

      // Post-Loop
      Assert.AreEqual(33.0f, animation.GetValue(TimeSpan.FromSeconds(5.0), defaultSource, defaultTarget));
    }


    [Test]
    public void AdditiveCurve()
    {
      var animation = new Curve2FAnimation();
      animation.Curve = new Curve2F
      {
        new CurveKey2F { Point = new Vector2(2.0f, 22.0f) },
        new CurveKey2F { Point = new Vector2(3.0f, 33.0f) },
        new CurveKey2F { Point = new Vector2(4.0f, 44.0f) },
      };
      animation.Curve.PreLoop = Mathematics.Interpolation.CurveLoopType.Linear;
      animation.Curve.PostLoop = Mathematics.Interpolation.CurveLoopType.Cycle;
      animation.EndParameter = float.PositiveInfinity;
      animation.IsAdditive = true;

      float defaultSource = -100.0f;
      float defaultTarget = 100.0f;

      // Pre-Loop
      Assert.AreEqual(defaultSource + 0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget));
      Assert.AreEqual(defaultSource + 11.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), defaultSource, defaultTarget));

      Assert.AreEqual(defaultSource + 22.0f, animation.GetValue(TimeSpan.FromSeconds(2.0), defaultSource, defaultTarget));
      Assert.AreEqual(defaultSource + 33.0f, animation.GetValue(TimeSpan.FromSeconds(3.0), defaultSource, defaultTarget));
      Assert.AreEqual(defaultSource + 44.0f, animation.GetValue(TimeSpan.FromSeconds(4.0), defaultSource, defaultTarget));

      // Post-Loop
      Assert.AreEqual(defaultSource + 33.0f, animation.GetValue(TimeSpan.FromSeconds(5.0), defaultSource, defaultTarget));
    }
  }
}
