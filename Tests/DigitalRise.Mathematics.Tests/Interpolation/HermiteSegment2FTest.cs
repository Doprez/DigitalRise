﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class HermiteSegment2FTest
  {
    [Test]
    public void GetPoint()
    {
      HermiteSegment2F s = new HermiteSegment2F
      {
        Point1 = new Vector2(1, 2),
        Tangent1 = (new Vector2(10, 3) - new Vector2(1, 2)) * 3,
        Tangent2 = (new Vector2(10, 2) - new Vector2(7, 8)) * 3,
        Point2 = new Vector2(10, 2),
      };

      BezierSegment2F b = new BezierSegment2F
      {
        Point1 = new Vector2(1, 2),
        ControlPoint1 = new Vector2(10, 3),
        ControlPoint2 = new Vector2(7, 8),
        Point2 = new Vector2(10, 2),
      };

      Assert.IsTrue(MathHelper.AreNumericallyEqual(s.Point1, s.GetPoint(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(s.Point2, s.GetPoint(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(b.GetPoint(0.33f), s.GetPoint(0.33f)));
    }


    [Test]
    public void GetTangent()
    {
      HermiteSegment2F s = new HermiteSegment2F
      {
        Point1 = new Vector2(1, 2),
        Tangent1 = (new Vector2(10, 3) - new Vector2(1, 2)) * 3,
        Tangent2 = (new Vector2(10, 2) - new Vector2(7, 8)) * 3,
        Point2 = new Vector2(10, 2),
      };

      BezierSegment2F b = new BezierSegment2F
      {
        Point1 = new Vector2(1, 2),
        ControlPoint1 = new Vector2(10, 3),
        ControlPoint2 = new Vector2(7, 8),
        Point2 = new Vector2(10, 2),
      };

      Assert.IsTrue(MathHelper.AreNumericallyEqual(s.Tangent1, s.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(s.Tangent2, s.GetTangent(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(b.GetTangent(0.7f), s.GetTangent(0.7f)));
    }


    [Test]
    public void GetLength()
    {
      HermiteSegment2F s = new HermiteSegment2F
      {
        Point1 = new Vector2(1, 2),
        Tangent1 = (new Vector2(10, 3) - new Vector2(1, 2)) * 3,
        Tangent2 = (new Vector2(10, 2) - new Vector2(7, 8)) * 3,
        Point2 = new Vector2(10, 2),
      };

      BezierSegment2F b = new BezierSegment2F
      {
        Point1 = new Vector2(1, 2),
        ControlPoint1 = new Vector2(10, 3),
        ControlPoint2 = new Vector2(7, 8),
        Point2 = new Vector2(10, 2),
      };

      float length1 = s.GetLength(0, 1, 20, Numeric.EpsilonF);
      float length2 = b.GetLength(0, 1, 20, Numeric.EpsilonF);
      Assert.IsTrue(Numeric.AreEqual(length1, length2));

      float approxLength = 0;
      const float step = 0.0001f;
      for (float u = 0; u <= 1.0f; u += step)
        approxLength += (s.GetPoint(u) - s.GetPoint(u + step)).Length();

      Assert.IsTrue(Numeric.AreEqual(approxLength, length1, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(s.GetLength(0, 1, 100, Numeric.EpsilonF), s.GetLength(0, 0.5f, 100, Numeric.EpsilonF) + s.GetLength(0.5f, 1, 100, Numeric.EpsilonF)));
      Assert.IsTrue(Numeric.AreEqual(s.GetLength(0, 1, 100, Numeric.EpsilonF), s.GetLength(1, 0, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      var s = new BezierSegment2F
      {
        Point1 = new Vector2(1, 2),
        ControlPoint1 = new Vector2(4, 5),
        ControlPoint2 = new Vector2(7, 8),
        Point2 = new Vector2(10, 2),
      };
      var points = new List<Vector2>();
      var tolerance = 0.01f;
      s.Flatten(points, 10, tolerance);
      Assert.IsTrue(points.Contains(s.Point1));
      Assert.IsTrue(points.Contains(s.Point2));
      var curveLength = s.GetLength(0, 1, 10, tolerance);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance * points.Count / 2);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength);
    }
  }
}