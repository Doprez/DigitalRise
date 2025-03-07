using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class Path2FTest
  {
    private Path2F CreatePath()
    {
      Path2F path = new Path2F();
      path.Add(new PathKey2F()
      {
        Parameter = 10,
        Point = new Vector2(0, 1),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 12,
        Point = new Vector2(1, 2),
        Interpolation = SplineInterpolation.StepLeft,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 15,
        Point = new Vector2(4, 5),
        Interpolation = SplineInterpolation.StepCentered,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 18,
        Point = new Vector2(5, 7),
        Interpolation = SplineInterpolation.StepRight,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 20,
        Point = new Vector2(5, 7),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 25,
        Point = new Vector2(6, 7),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2(5, 6),
        TangentOut = new Vector2(7, 8),
      });      
      path.Add(new PathKey2F()
      {
        Parameter = 31,
        Point = new Vector2(8, 10),
        Interpolation = SplineInterpolation.BSpline,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 35,
        Point = new Vector2(10, 12),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 40,
        Point = new Vector2(10, 14),
        Interpolation = SplineInterpolation.CatmullRom,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 50,
        Point = new Vector2(20, 14),
        Interpolation = SplineInterpolation.CatmullRom,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      return path;
    }


    [Test]
    public void GetPointShouldReturnNanIfPathIsEmpty()
    {
      Path2F empty = new Path2F();
      empty.Sort();

      Vector2 p = empty.GetPoint(-0.5f);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);

      p = empty.GetPoint(0);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);

      p = empty.GetPoint(0.5f);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);
    }


    [Test]
    public void GetPoint()
    {
      Path2F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(0, 1), path.GetPoint(-10)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(4, 5), path.GetPoint(13)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(4, 5), path.GetPoint(16)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(5, 7), path.GetPoint(17)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(5, 7), path.GetPoint(19)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(5, 7) * 0.5f + new Vector2(6, 7)*0.5f, path.GetPoint(22.5f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(5, 7) * 0.5f + new Vector2(6, 7) * 0.5f, path.GetPoint(22.5f)));
      CatmullRomSegment2F catmullOscillate = new CatmullRomSegment2F()
      {
        Point1 = new Vector2(10, 12),
        Point2 = new Vector2(10, 14),
        Point3 = new Vector2(20, 14),
        Point4 = new Vector2(30, 14),
      };
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetPoint(0.3f), path.GetPoint(43)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetPoint(0.9f), path.GetPoint(51)));

      CatmullRomSegment2F catmullCircle = new CatmullRomSegment2F()
      {
        Point1 = new Vector2(10, 12),
        Point2 = new Vector2(10, 14),
        Point3 = new Vector2(20, 14),
        Point4 = new Vector2(30, 14),
      };
      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(0, 1) - (new Vector2(1, 2) - new Vector2(0, 1)) / 2 * 9, path.GetPoint(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullCircle.GetPoint(0.3f), path.GetPoint(43)));

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      var cycleOffset = new Vector2(20, 14) - new Vector2(0, 1);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(10, 14) + cycleOffset, path.GetPoint(80f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(10, 14) + cycleOffset * 2, path.GetPoint(120f)));

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.Linear;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(10, 14) - cycleOffset, path.GetPoint(0f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(10, 14) - cycleOffset * 2, path.GetPoint(-40f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(20, 14) + catmullOscillate.GetTangent(1) / 10 * 50, path.GetPoint(100f)));

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Constant;
    }


    [Test]
    public void GetTangentShouldReturnZeroIfPathIsEmpty()
    {
      Path2F empty = new Path2F();
      empty.Sort();
      Assert.AreEqual(Vector2.Zero, empty.GetTangent(-0.5f));
      Assert.AreEqual(Vector2.Zero, empty.GetTangent(0));
      Assert.AreEqual(Vector2.Zero, empty.GetTangent(0.5f));
    }


    [Test]
    public void GetTangent()
    {
      Path2F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(0, 0), path.GetTangent(-10)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector2(1, 2) - new Vector2(0, 1)) / 2, path.GetTangent(10)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector2(1, 2) - new Vector2(0, 1)) / 2, path.GetTangent(11.5f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(0, 0), path.GetTangent(16)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(0, 0), path.GetTangent(85)));

      CatmullRomSegment2F catmullOscillate = new CatmullRomSegment2F()
      {
        Point1 = new Vector2(10, 12),
        Point2 = new Vector2(10, 14),
        Point3 = new Vector2(20, 14),
        Point4 = new Vector2(30, 14),
      };
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetTangent(0.3f) / 10.0f, path.GetTangent(43)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(-catmullOscillate.GetTangent(0.7f) / 10.0f, path.GetTangent(53)));
      
      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector2(1, 2) - new Vector2(0, 1)) / 2, path.GetTangent(0)));

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetTangent(0.4f) / 10.0f, path.GetTangent(-36)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetTangent(0.4f) / 10.0f, path.GetTangent(4)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetTangent(0.3f) / 10.0f, path.GetTangent(83)));

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.Linear;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetTangent(1f) / 10.0f, path.GetTangent(434)));

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Constant;

      path = new Path2F();
      path.Add(new PathKey2F()
      {
        Parameter = 25,
        Point = new Vector2(6, 7),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2(5, 6),
        TangentOut = new Vector2(7, 8),
      });
      path.Add(new PathKey2F()
      {
        Parameter = 35,
        Point = new Vector2(10, 12),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Linear;
      float Δu = path[1].Parameter - path[0].Parameter;
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector2(6, 7) - new Vector2(5, 6)) * 3 / Δu, path.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(1, 0) / Δu, path.GetTangent(100)));

      path[1].Parameter = 25;
      path[0].Parameter = 35;
      path.Sort();
      Δu = path[1].Parameter - path[0].Parameter;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(1, 0) / Δu, path.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector2(7, 8) - new Vector2(6, 7)) * 3 / Δu, path.GetTangent(100)));

      path.Add(new PathKey2F()
      {
        Parameter = 15,
        Point = new Vector2(0, 0),
        Interpolation = SplineInterpolation.BSpline,
      });
      path.Sort();
    }


    [Test]
    public void GetLength()
    {
      Path2F empty = new Path2F();
      empty.Sort();
      Assert.AreEqual(0, empty.GetLength(0, 1, 100, 0.0001f));

      Path2F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(Numeric.AreEqual((new Vector2(0, 1) - new Vector2(1, 2)).Length(), path.GetLength(-1, 12, 100, 0.0001f), 0.001f));
      Assert.IsTrue(Numeric.AreEqual((new Vector2(0, 1) - new Vector2(1, 2)).Length(), path.GetLength(-1, 20, 100, 0.0001f), 0.001f));

      CatmullRomSegment2F catmullOscillate = new CatmullRomSegment2F()
      {
        Point1 = new Vector2(10, 12),
        Point2 = new Vector2(10, 14),
        Point3 = new Vector2(20, 14),
        Point4 = new Vector2(30, 14),
      };
      float desiredLength = catmullOscillate.GetLength(0, 1, 20, 0.0001f);
      float actualLength = path.GetLength(40, 50, 20, 0.0001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.001f));
      desiredLength = catmullOscillate.GetLength(1, 0.8f, 20, 0.0001f);
      actualLength = path.GetLength(52, 50, 20, 0.0001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.001f));
      desiredLength = catmullOscillate.GetLength(1, 0.8f, 20, 0.0001f) * 2;
      actualLength = path.GetLength(52, 48, 20, 0.0001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.001f));
      
      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;      

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.Linear;

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Constant;
    }


    [Test]
    public void Flatten()
    {
      var s = CreatePath();
      var points = new List<Vector2>();
      var tolerance = 0.1f;
      s.Flatten(points, 10, tolerance);
      for (int i = 0; i < s.Count; i++)
        if (s[i].Interpolation != SplineInterpolation.StepLeft
            && s[i].Interpolation != SplineInterpolation.StepCentered
            && s[i].Interpolation != SplineInterpolation.StepRight)
          Assert.IsTrue(points.Contains(s.GetPoint(s[i].Parameter)));
      var curveLength = s.GetLength(s[0].Parameter, s[s.Count - 1].Parameter, 100, tolerance / 10);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance * points.Count / 2);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength); 

      foreach (var key in s)
        key.Interpolation = SplineInterpolation.Linear;
      points.Clear();
      s.Flatten(points, 10, tolerance);
      for (int i = 0; i < s.Count; i++)
        Assert.IsTrue(points.Contains(s.GetPoint(s[i].Parameter)));
      curveLength = s.GetLength(s[0].Parameter, s[s.Count - 1].Parameter, 100, tolerance / 10);
      Assert.AreEqual(2 + 2 * (s.Count - 2), points.Count);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance * points.Count / 2);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength); 

      foreach(var key in s)
        key.Interpolation = SplineInterpolation.CatmullRom;
      points.Clear();
      s.Flatten(points, 10, tolerance);
      for (int i = 0; i < s.Count; i++)
        Assert.IsTrue(points.Contains(s.GetPoint(s[i].Parameter)));
      curveLength = s.GetLength(s[0].Parameter, s[s.Count - 1].Parameter, 100, tolerance / 10);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance * points.Count / 2);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength); 
    }


    [Test]
    public void ParameterizeByLength()
    {
      Path2F empty = new Path2F();
      empty.Sort();
      empty.ParameterizeByLength(20, 0.001f); // No exception, do nothing.
      
      Path2F path = CreatePath();
      Path2F lengthPath = CreatePath();
      lengthPath.ParameterizeByLength(20, 0.001f);

      float length = (lengthPath[1].Point - lengthPath[0].Point).Length();
      Assert.AreEqual(0, lengthPath[0].Parameter);
      Assert.AreEqual(length, lengthPath[1].Parameter);
      Assert.AreEqual(length, lengthPath[2].Parameter);
      Assert.AreEqual(length, lengthPath[3].Parameter);

      float step = 0.001f;
      int i = 4;
      float u = 20;
      Vector2 oldPoint = path.GetPoint(u);
      for (; u < 51 && i<10; u += step)
      {
        if (Numeric.AreEqual(u, path[i].Parameter))
        {
          Assert.IsTrue(Numeric.AreEqual(length, lengthPath[i].Parameter, 0.01f));
          
          // Set explicit values against numerical problems.
          length = lengthPath[i].Parameter;
          u = path[i].Parameter;
          oldPoint = path.GetPoint(u);

          i++;
        }
        Vector2 newPoint = path.GetPoint(u + step);
        length += (newPoint - oldPoint).Length();
        oldPoint = newPoint;
      }
      Assert.AreEqual(10, i); // Have we checked all keys?

      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;

      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.Linear;

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Constant;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ParameterizeByLengthException()
    {
      Path2F path = CreatePath();
      path.ParameterizeByLength(20, -0.01f);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetParameterByLengthException()
    {
      Path2F path = CreatePath();
      path.ParameterizeByLength(20, 0.1f);
      path.GetParameterFromLength(10, 20, -0.1f);
    }


    [Test]
    public void GetParameterByLength()
    {
      Path2F empty = new Path2F();
      empty.ParameterizeByLength(20, 0.001f); // No exception, do nothing.
      Assert.IsTrue(float.IsNaN(empty.GetParameterFromLength(10, 20, 0.1f)));

      Path2F path = CreatePath();
      path.ParameterizeByLength(20, 0.001f);

      Assert.AreEqual(0, path.GetParameterFromLength(0, 20, 0.001f));
      Assert.AreEqual(path[2].Parameter, path.GetParameterFromLength(path[2].Parameter, 20, 0.01f));
      Assert.AreEqual(path[4].Parameter, path.GetParameterFromLength(path[4].Parameter, 20, 0.01f));
      Assert.AreEqual(path[5].Parameter, path.GetParameterFromLength(path[5].Parameter, 20, 0.01f));
      Assert.AreEqual(path[6].Parameter, path.GetParameterFromLength(path[6].Parameter, 20, 0.01f));
      Assert.AreEqual(path[7].Parameter, path.GetParameterFromLength(path[7].Parameter, 20, 0.01f));
      Assert.AreEqual(path[8].Parameter, path.GetParameterFromLength(path[8].Parameter, 20, 0.01f));
      Assert.AreEqual(path[9].Parameter, path.GetParameterFromLength(path[9].Parameter, 20, 0.01f));

      float pathLength = path[9].Parameter;

      float desiredLength = 11;
      float actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.01f));

      desiredLength = 20;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.01f));

      desiredLength = 26f;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.01f));

      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Linear;
      desiredLength = 60;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));

      desiredLength = -10f;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.01f));

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.CycleOffset;
      path.ParameterizeByLength(20, 0.001f);
      desiredLength = -90;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -50;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -30;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 50;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 100;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 130;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 200;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Cycle;
      path.ParameterizeByLength(20, 0.001f);
      desiredLength = -66;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -30;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -20;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 40;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 70;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 100;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 190;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));   

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.Oscillate;
      path.ParameterizeByLength(20, 0.001f);
      desiredLength = -90;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -50;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -30;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 50;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 110;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 130;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 210;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.001f), 20, 0.001f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));   

      // Test path with zero length.
      path = new Path2F();
      path.Add(new PathKey2F()
      {
        Parameter = 10,
        Point = new Vector2(0, 1),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.ParameterizeByLength(20, 0.001f);
      Assert.AreEqual(0, path.GetParameterFromLength(0, 20, 0.1f));
      path.Add(new PathKey2F()
      {
        Parameter = 20,
        Point = new Vector2(0, 1),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector2(1, 0),
        TangentOut = new Vector2(1, 0),
      });
      path.ParameterizeByLength(20, 0.001f);
      Assert.AreEqual(0, path.GetParameterFromLength(0, 20, 0.1f));
    }

    [Test]
    public void OneKeyCurvesTest()
    {
      // Test linear curves with 1 point
      Path2F curve = new Path2F();
      curve.Add(new PathKey2F()
      {
        Parameter = 1,
        Point = new Vector2(1, 2),
        Interpolation = SplineInterpolation.Linear,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test step curves with 1 point
      curve = new Path2F();
      curve.Add(new PathKey2F()
      {
        Parameter = 1,
        Point = new Vector2(1, 2),
        Interpolation = SplineInterpolation.StepLeft,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test B-spline curves with 1 point
      curve = new Path2F();
      curve.Add(new PathKey2F()
      {
        Parameter = 1,
        Point = new Vector2(1, 2),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test Catmull-Rom curves with 1 point
      curve = new Path2F();
      curve.Add(new PathKey2F()
      {
        Parameter = 1,
        Point = new Vector2(1, 2),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test Hermite curves with 1 point
      curve = new Path2F();
      curve.Add(new PathKey2F()
      {
        Parameter = 1,
        Point = new Vector2(1, 2),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector2(2, -2),
        TangentOut = new Vector2(2, 2),
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 20, 0.01f));
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(3, 4), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(2, 2), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(2, 2), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector2(2, 2).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(-1, 4), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(2, -2), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(2, -2), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector2(2, 2).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(-1, 4), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(3, 4), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(2, -2), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(2, 2), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(2, 2), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector2(4, 4).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test Bezier curves with 1 point
      curve = new Path2F();
      curve.Add(new PathKey2F()
      {
        Parameter = 1,
        Point = new Vector2(1, 2),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector2(1, 2) - new Vector2(2, -2) / 3,
        TangentOut = new Vector2(1, 2) + new Vector2(2, 2) / 3,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(3, 4), curve.GetPoint(2)));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(2, 2), curve.GetTangent(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(2, 2), curve.GetTangent(2)));
      Assert.IsTrue(Numeric.AreEqual(new Vector2(2, 2).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(-1, 4), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(2, -2), curve.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(2, -2), curve.GetTangent(1)));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector2(2, 2).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(-1, 4), curve.GetPoint(0));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(3, 4), curve.GetPoint(2)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(2, -2), curve.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(2, 2), curve.GetTangent(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector2(2, 2), curve.GetTangent(2)));
      Assert.IsTrue(Numeric.AreEqual(new Vector2(4, 4).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
    }


    [Test]
    public void TwoKeyCurvesTest()
    {
      Path2F curve = new Path2F();
      curve.Add(new PathKey2F()
                  {
                    Parameter = 1,
                    Point = new Vector2(1, 2),
                    Interpolation = SplineInterpolation.CatmullRom,
                  });
      curve.Add(new PathKey2F()
                  {
                    Parameter = 3,
                    Point = new Vector2(3, 4),
                    Interpolation = SplineInterpolation.CatmullRom,
                  });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(0));
      Assert.AreEqual(new Vector2(1, 2), curve.GetPoint(1));
      Assert.AreEqual(new Vector2(2, 3), curve.GetPoint(2));
      Assert.AreEqual(new Vector2(3, 4), curve.GetPoint(3));
      Assert.AreEqual(new Vector2(3, 4), curve.GetPoint(4));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector2(1, 1), curve.GetTangent(1));
      Assert.AreEqual(new Vector2(1, 1), curve.GetTangent(2));
      Assert.AreEqual(new Vector2(1, 1), curve.GetTangent(3));
      Assert.AreEqual(new Vector2(0, 0), curve.GetTangent(4));
      Assert.IsTrue(Numeric.AreEqual(new Vector2(2, 2).Length(), curve.GetLength(0, 4, 10, 0.01f), 0.01f));
      
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
    }


    [Test]
    public void SerializationXml()
    {
      PathKey2F pathKey1 = new PathKey2F
      {
        Interpolation = SplineInterpolation.Bezier,
        Parameter = 56.7f,
        Point = new Vector2(1.2f, 3.4f),
        TangentIn = new Vector2(0.7f, 2.6f),
        TangentOut = new Vector2(1.9f, 3.3f)
      };
      PathKey2F pathKey2 = new PathKey2F
      {
        Interpolation = SplineInterpolation.Hermite,
        Parameter = 66.7f,
        Point = new Vector2(2.2f, 1.4f),
        TangentIn = new Vector2(1.7f, 3.6f),
        TangentOut = new Vector2(2.9f, 2.3f)
      };
      Path2F path = new Path2F { pathKey1, pathKey2 };
      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      path.SmoothEnds = true;

      const string fileName = "SerializationPath2F.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Path2F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, path);
      writer.Close();

      serializer = new XmlSerializer(typeof(Path2F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      path = (Path2F)serializer.Deserialize(fileStream);

      Assert.AreEqual(2, path.Count);
      MathAssert.AreEqual(pathKey1, path[0]);
      MathAssert.AreEqual(pathKey2, path[1]);
      Assert.AreEqual(CurveLoopType.Cycle, path.PreLoop);
      Assert.AreEqual(CurveLoopType.CycleOffset, path.PostLoop);
      Assert.AreEqual(true, path.SmoothEnds);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      PathKey2F pathKey1 = new PathKey2F
      {
        Interpolation = SplineInterpolation.Bezier,
        Parameter = 56.7f,
        Point = new Vector2(1.2f, 3.4f),
        TangentIn = new Vector2(0.7f, 2.6f),
        TangentOut = new Vector2(1.9f, 3.3f)
      };
      PathKey2F pathKey2 = new PathKey2F
      {
        Interpolation = SplineInterpolation.Hermite,
        Parameter = 66.7f,
        Point = new Vector2(2.2f, 1.4f),
        TangentIn = new Vector2(1.7f, 3.6f),
        TangentOut = new Vector2(2.9f, 2.3f)
      };
      Path2F path = new Path2F { pathKey1, pathKey2 };
      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      path.SmoothEnds = true;

      const string fileName = "SerializationPath2F.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, path);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      path = (Path2F)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(2, path.Count);
      MathAssert.AreEqual(pathKey1, path[0]);
      MathAssert.AreEqual(pathKey2, path[1]);
      Assert.AreEqual(CurveLoopType.Cycle, path.PreLoop);
      Assert.AreEqual(CurveLoopType.CycleOffset, path.PostLoop);
      Assert.AreEqual(true, path.SmoothEnds);
    }
  }
}
