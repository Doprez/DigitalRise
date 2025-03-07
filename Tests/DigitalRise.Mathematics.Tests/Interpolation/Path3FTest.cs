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
  public class Path3FTest
  {
    private Path3F CreatePath()
    {
      Path3F path = new Path3F();
      path.Add(new PathKey3F()
      {
        Parameter = 10,
        Point = new Vector3(0, 0, 1),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 12,
        Point = new Vector3(1, 2, 3),
        Interpolation = SplineInterpolation.StepLeft,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 15,
        Point = new Vector3(4, 5, 7),
        Interpolation = SplineInterpolation.StepCentered,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 18,
        Point = new Vector3(5, 7, 10),
        Interpolation = SplineInterpolation.StepRight,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 20,
        Point = new Vector3(5, 7, 13),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 31,
        Point = new Vector3(8, 10, 16),
        Interpolation = SplineInterpolation.BSpline,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 35,
        Point = new Vector3(10, 12, 14),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 25,
        Point = new Vector3(6, 7, 14),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector3(5, 6, 13),
        TangentOut = new Vector3(7, 8, 15),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 40,
        Point = new Vector3(10, 14, 8),
        Interpolation = SplineInterpolation.CatmullRom,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 50,
        Point = new Vector3(20, 14, 8),
        Interpolation = SplineInterpolation.CatmullRom,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.Sort();
      return path;
    }


    [Test]
    public void GetPointShouldReturnNanIfPathIsEmpty()   
    {
      Path3F empty = new Path3F();
      empty.Sort();

      Vector3 p = empty.GetPoint(-0.5f);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);
      Assert.IsNaN(p.Z);

      p = empty.GetPoint(0);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);
      Assert.IsNaN(p.Z);

      p = empty.GetPoint(0.5f);
      Assert.IsNaN(p.X);
      Assert.IsNaN(p.Y);
      Assert.IsNaN(p.Z);
    }


    [Test]
    public void GetPoint()
    {
      Path3F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(0, 0, 1), path.GetPoint(-10)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(4, 5, 7), path.GetPoint(13)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(4, 5, 7), path.GetPoint(16)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(5, 7, 10), path.GetPoint(17)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(5, 7, 10), path.GetPoint(19)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(5, 7, 13)*0.5f + new Vector3(6, 7, 14)*0.5f, path.GetPoint(22.5f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(5, 7, 13) * 0.5f + new Vector3(6, 7, 14) * 0.5f, path.GetPoint(22.5f)));
      CatmullRomSegment3F catmullOscillate = new CatmullRomSegment3F()
      {
        Point1 = new Vector3(10, 12, 14),
        Point2 = new Vector3(10, 14, 8),
        Point3 = new Vector3(20, 14, 8),
        Point4 = new Vector3(30, 14, 8),
      };
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetPoint(0.3f), path.GetPoint(43)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetPoint(0.9f), path.GetPoint(51)));

      CatmullRomSegment3F catmullCircle = new CatmullRomSegment3F()
      {
        Point1 = new Vector3(10, 12, 14),
        Point2 = new Vector3(10, 14, 8),
        Point3 = new Vector3(20, 14, 8),
        Point4 = new Vector3(30, 14, 8),
      };
      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(0, 0, 1) - (new Vector3(1, 2, 3) - new Vector3(0, 0, 1)) / 2 * 9, path.GetPoint(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullCircle.GetPoint(0.3f), path.GetPoint(43)));

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(10, 14, 8) + new Vector3(20, 14, 7), path.GetPoint(80f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(10, 14, 8) + new Vector3(20, 14, 7) * 2, path.GetPoint(120f)));

      path.PreLoop = CurveLoopType.CycleOffset;
      path.PostLoop = CurveLoopType.Linear;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(10, 14, 8) - new Vector3(20, 14, 7), path.GetPoint(0f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(10, 14, 8) - new Vector3(20, 14, 7) * 2, path.GetPoint(-40f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(20, 14, 8) + catmullOscillate.GetTangent(1) / 10 * 50, path.GetPoint(100f)));

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Constant;
    }


    [Test]
    public void GetTangentShouldReturnZeroIfPathIsEmpty()
    {
      Path3F empty = new Path3F();
      empty.Sort();
      Assert.AreEqual(Vector3.Zero, empty.GetTangent(-0.5f));
      Assert.AreEqual(Vector3.Zero, empty.GetTangent(0));
      Assert.AreEqual(Vector3.Zero, empty.GetTangent(0.5f));
    }


    [Test]
    public void GetTangent()
    {
      Path3F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(0, 0, 0), path.GetTangent(-10)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector3(1, 2, 3) - new Vector3(0, 0, 1)) / 2, path.GetTangent(10)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector3(1, 2, 3) - new Vector3(0, 0, 1)) / 2, path.GetTangent(11.5f)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(0, 0, 0), path.GetTangent(16)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(0, 0, 0), path.GetTangent(85)));

      CatmullRomSegment3F catmullOscillate = new CatmullRomSegment3F()
      {
        Point1 = new Vector3(10, 12, 14),
        Point2 = new Vector3(10, 14, 8),
        Point3 = new Vector3(20, 14, 8),
        Point4 = new Vector3(30, 14, 8),
      };
      Assert.IsTrue(MathHelper.AreNumericallyEqual(catmullOscillate.GetTangent(0.3f) / 10.0f, path.GetTangent(43)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(-catmullOscillate.GetTangent(0.7f) / 10.0f, path.GetTangent(53)));
      
      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Cycle;
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector3(1, 2, 3) - new Vector3(0, 0, 1)) / 2, path.GetTangent(0)));

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

      path = new Path3F();
      path.Add(new PathKey3F()
      {
        Parameter = 25,
        Point = new Vector3(6, 7, 14),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector3(5, 6, 13),
        TangentOut = new Vector3(7, 8, 15),
      });
      path.Add(new PathKey3F()
      {
        Parameter = 35,
        Point = new Vector3(10, 12, 14),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.PreLoop = CurveLoopType.Linear;
      path.PostLoop = CurveLoopType.Linear;
      float Δu = path[1].Parameter - path[0].Parameter;
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector3(6, 7, 14) - new Vector3(5, 6, 13)) * 3 / Δu, path.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(1, 0, 0) / Δu, path.GetTangent(100)));

      path[1].Parameter = 25;
      path[0].Parameter = 35;
      path.Sort();
      Δu = path[1].Parameter - path[0].Parameter;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(1, 0, 0) / Δu, path.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual((new Vector3(7, 8, 15) - new Vector3(6, 7, 14)) * 3 / Δu, path.GetTangent(100)));

      path.Add(new PathKey3F()
      {
        Parameter = 15,
        Point = new Vector3(0, 0, 0),
        Interpolation = SplineInterpolation.BSpline,
      });
      path.Sort();
    }


    [Test]
    public void GetLength()
    {
      Path3F empty = new Path3F();
      empty.Sort();
      Assert.AreEqual(0, empty.GetLength(0, 1, 100, 0.0001f));

      Path3F path = CreatePath();
      path.PreLoop = CurveLoopType.Constant;
      path.PostLoop = CurveLoopType.Oscillate;
      Assert.IsTrue(Numeric.AreEqual((new Vector3(0, 0, 1) - new Vector3(1, 2, 3)).Length(), path.GetLength(-1, 12, 100, 0.0001f), 0.001f));
      Assert.IsTrue(Numeric.AreEqual((new Vector3(0, 0, 1) - new Vector3(1, 2, 3)).Length(), path.GetLength(-1, 20, 100, 0.0001f), 0.001f));

      CatmullRomSegment3F catmullOscillate = new CatmullRomSegment3F()
      {
        Point1 = new Vector3(10, 12, 14),
        Point2 = new Vector3(10, 14, 8),
        Point3 = new Vector3(20, 14, 8),
        Point4 = new Vector3(30, 14, 8),
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
      var points = new List<Vector3>();
      var tolerance = 0.1f;
      s.Flatten(points, 10, tolerance);
      for (int i = 0; i < s.Count; i++)
        if (s[i].Interpolation != SplineInterpolation.StepLeft
            && s[i].Interpolation != SplineInterpolation.StepCentered
            && s[i].Interpolation != SplineInterpolation.StepRight)
          Assert.IsTrue(points.Contains(s.GetPoint(s[i].Parameter)));
      var curveLength = s.GetLength(s[0].Parameter, s[s.Count - 1].Parameter, 100, tolerance / 100);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength);

      foreach (var key in s)
        key.Interpolation = SplineInterpolation.Linear;
      points.Clear();
      s.Flatten(points, 10, tolerance);
      for (int i = 0; i < s.Count; i++)
        Assert.IsTrue(points.Contains(s.GetPoint(s[i].Parameter)));
      // TODO: GetLength is not accurate enough for non-continuous paths.
      //curveLength = s.GetLength(s[0].Parameter, s[s.Count - 1].Parameter, 100, tolerance / 100); 
      Assert.AreEqual(2 + 2 * (s.Count - 2), points.Count);
      //Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance);
      //Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength);

      foreach (var key in s)
        key.Interpolation = SplineInterpolation.CatmullRom;
      points.Clear();
      s.Flatten(points, 10, tolerance);
      for (int i = 0; i < s.Count; i++)
        Assert.IsTrue(points.Contains(s.GetPoint(s[i].Parameter)));
      curveLength = s.GetLength(s[0].Parameter, s[s.Count - 1].Parameter, 100, tolerance / 10);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength);
    }


    [Test]
    public void ParameterizeByLength()
    {
      Path3F empty = new Path3F();
      empty.Sort();
      empty.ParameterizeByLength(20, 0.001f); // No exception, do nothing.
      
      Path3F path = CreatePath();
      Path3F lengthPath = CreatePath();
      lengthPath.ParameterizeByLength(20, 0.001f);      

      Assert.AreEqual(0, lengthPath[0].Parameter);
      Assert.AreEqual(3, lengthPath[1].Parameter);
      Assert.AreEqual(3, lengthPath[2].Parameter);
      Assert.AreEqual(3, lengthPath[3].Parameter);

      float step = 0.001f;
      float length = 3;
      int i = 4;
      float u = 20;
      Vector3 oldPoint = path.GetPoint(u);
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
        Vector3 newPoint = path.GetPoint(u + step);
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
      Path3F path = CreatePath();
      path.ParameterizeByLength(20, -0.01f);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetParameterByLengthException()
    {
      Path3F path = CreatePath();
      path.ParameterizeByLength(20, 0.1f);
      path.GetParameterFromLength(10, 20, -0.1f);
    }


    [Test]
    public void GetParameterByLength()
    {
      Path3F empty = new Path3F();
      empty.ParameterizeByLength(20, 0.001f); // No exception, do nothing.
      Assert.IsTrue(float.IsNaN(empty.GetParameterFromLength(10, 20, 0.1f)));

      Path3F path = CreatePath();
      path.ParameterizeByLength(20, 0.001f);

      Assert.AreEqual(0, path.GetParameterFromLength(0, 20, 0.001f));
      Assert.AreEqual(3, path.GetParameterFromLength(3, 20, 0.001f));
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

      desiredLength = 26;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.01f));

      desiredLength = 33.5f;
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
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -50;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -30;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 50;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 100;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 130;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 200;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));

      path.PreLoop = CurveLoopType.Oscillate;
      path.PostLoop = CurveLoopType.Cycle;
      path.ParameterizeByLength(20, 0.001f);
      desiredLength = -110;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(-3 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(-2 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -50;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(-2 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(-1 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -30;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(-1 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(0 > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 50;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(1 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(2 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 110;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(2 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(3 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 130;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(3 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(4 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 190;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(4 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(5 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.2f));   

      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.Oscillate;
      path.ParameterizeByLength(20, 0.001f);
      desiredLength = -90;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(-3 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(-2 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -50;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(-2 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(-1 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = -30;
      actualLength = -path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(-1 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(0 > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 50;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(2 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 110;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(2 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(3 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 130;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(3 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(4 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));
      desiredLength = 210;
      actualLength = path.GetLength(0, path.GetParameterFromLength(desiredLength, 20, 0.01f), 20, 0.01f);
      Assert.IsTrue(5 * pathLength < path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(6 * pathLength > path.GetParameterFromLength(desiredLength, 20, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(desiredLength, actualLength, 0.1f));   

      // Test path with zero length.
      path = new Path3F();
      path.Add(new PathKey3F()
      {
        Parameter = 10,
        Point = new Vector3(0, 0, 1),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.ParameterizeByLength(20, 0.001f);
      Assert.AreEqual(0, path.GetParameterFromLength(0, 20, 0.1f));
      path.Add(new PathKey3F()
      {
        Parameter = 20,
        Point = new Vector3(0, 0, 1),
        Interpolation = SplineInterpolation.Linear,
        TangentIn = new Vector3(1, 0, 0),
        TangentOut = new Vector3(1, 0, 0),
      });
      path.ParameterizeByLength(20, 0.001f);
      Assert.AreEqual(0, path.GetParameterFromLength(0, 20, 0.1f));
    }

    [Test]
    public void OneKeyCurvesTest()
    {
      // Test linear curves with 1 point
      Path3F curve = new Path3F();
      curve.Add(new PathKey3F()
      {
        Parameter = 1,
        Point = new Vector3(1, 2, 0),
        Interpolation = SplineInterpolation.Linear,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test step curves with 1 point
      curve = new Path3F();
      curve.Add(new PathKey3F()
      {
        Parameter = 1,
        Point = new Vector3(1, 2, 0),
        Interpolation = SplineInterpolation.StepLeft,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test B-spline curves with 1 point
      curve = new Path3F();
      curve.Add(new PathKey3F()
      {
        Parameter = 1,
        Point = new Vector3(1, 2, 0),
        Interpolation = SplineInterpolation.BSpline,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test Catmull-Rom curves with 1 point
      curve = new Path3F();
      curve.Add(new PathKey3F()
      {
        Parameter = 1,
        Point = new Vector3(1, 2, 0),
        Interpolation = SplineInterpolation.CatmullRom,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test Hermite curves with 1 point
      curve = new Path3F();
      curve.Add(new PathKey3F()
      {
        Parameter = 1,
        Point = new Vector3(1, 2, 0),
        Interpolation = SplineInterpolation.Hermite,
        TangentIn = new Vector3(2, -2, 0),
        TangentOut = new Vector3(2, 2, 0),
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 20, 0.01f));
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(3, 4, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(2, 2, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(2, 2, 0), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector3(2, 2, 0).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(-1, 4, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(2, -2, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(2, -2, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector3(2, 2, 0).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(-1, 4, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(3, 4, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(2, -2, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(2, 2, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(2, 2, 0), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector3(4, 4, 0).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));

      // Test Bezier curves with 1 point
      curve = new Path3F();
      curve.Add(new PathKey3F()
      {
        Parameter = 1,
        Point = new Vector3(1, 2, 0),
        Interpolation = SplineInterpolation.Bezier,
        TangentIn = new Vector3(1, 2, 0) - new Vector3(2, -2, 0) / 3,
        TangentOut = new Vector3(1, 2, 0) + new Vector3(2, 2, 0) / 3,
      });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(3, 4, 0), curve.GetPoint(2)));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, 2, 0), curve.GetTangent(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, 2, 0), curve.GetTangent(2)));
      Assert.IsTrue(Numeric.AreEqual(new Vector3(2, 2, 0).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(-1, 4, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, -2, 0), curve.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, -2, 0), curve.GetTangent(1)));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.IsTrue(Numeric.AreEqual(new Vector3(2, 2, 0).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Linear;
      curve.PostLoop = CurveLoopType.Linear;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(-1, 4, 0), curve.GetPoint(0));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(3, 4, 0), curve.GetPoint(2)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, -2, 0), curve.GetTangent(0)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, 2, 0), curve.GetTangent(1)));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, 2, 0), curve.GetTangent(2)));
      Assert.IsTrue(Numeric.AreEqual(new Vector3(4, 4, 0).Length(), curve.GetLength(0, 2, 10, 0.01f), 0.1f));
      curve.PreLoop = CurveLoopType.Cycle;
      curve.PostLoop = CurveLoopType.Cycle;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.CycleOffset;
      curve.PostLoop = CurveLoopType.CycleOffset;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
      curve.PreLoop = CurveLoopType.Oscillate;
      curve.PostLoop = CurveLoopType.Oscillate;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(2));
      Assert.AreEqual(0, curve.GetLength(0, 2, 10, 0.01f));
    }


    [Test]
    public void TwoKeyCurvesTest()
    {
      Path3F curve = new Path3F();
      curve.Add(new PathKey3F()
                  {
                    Parameter = 1,
                    Point = new Vector3(1, 2, 0),
                    Interpolation = SplineInterpolation.CatmullRom,
                  });
      curve.Add(new PathKey3F()
                  {
                    Parameter = 3,
                    Point = new Vector3(3, 4, 0),
                    Interpolation = SplineInterpolation.CatmullRom,
                  });
      curve.PreLoop = CurveLoopType.Constant;
      curve.PostLoop = CurveLoopType.Constant;
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(0));
      Assert.AreEqual(new Vector3(1, 2, 0), curve.GetPoint(1));
      Assert.AreEqual(new Vector3(2, 3, 0), curve.GetPoint(2));
      Assert.AreEqual(new Vector3(3, 4, 0), curve.GetPoint(3));
      Assert.AreEqual(new Vector3(3, 4, 0), curve.GetPoint(4));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(0));
      Assert.AreEqual(new Vector3(1, 1, 0), curve.GetTangent(1));
      Assert.AreEqual(new Vector3(1, 1, 0), curve.GetTangent(2));
      Assert.AreEqual(new Vector3(1, 1, 0), curve.GetTangent(3));
      Assert.AreEqual(new Vector3(0, 0, 0), curve.GetTangent(4));
      Assert.IsTrue(Numeric.AreEqual(new Vector3(2, 2, 0).Length(), curve.GetLength(0, 4, 10, 0.01f), 0.01f));
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
      PathKey3F pathKey1 = new PathKey3F
      {
        Interpolation = SplineInterpolation.Bezier,
        Parameter = 56.7f,
        Point = new Vector3(1.2f, 3.4f, 5.6f),
        TangentIn = new Vector3(0.7f, 2.6f, 5.1f),
        TangentOut = new Vector3(1.9f, 3.3f, 5.9f)
      };
      PathKey3F pathKey2 = new PathKey3F
      {
        Interpolation = SplineInterpolation.Hermite,
        Parameter = 66.7f,
        Point = new Vector3(2.2f, 1.4f, 6.6f),
        TangentIn = new Vector3(1.7f, 3.6f, 4.1f),
        TangentOut = new Vector3(2.9f, 2.3f, 6.9f)
      };
      Path3F path = new Path3F { pathKey1, pathKey2 };
      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      path.SmoothEnds = true;

      const string fileName = "SerializationPath3F.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Path3F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, path);
      writer.Close();

      serializer = new XmlSerializer(typeof(Path3F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      path = (Path3F)serializer.Deserialize(fileStream);

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
      PathKey3F pathKey1 = new PathKey3F
      {
        Interpolation = SplineInterpolation.Bezier,
        Parameter = 56.7f,
        Point = new Vector3(1.2f, 3.4f, 5.6f),
        TangentIn = new Vector3(0.7f, 2.6f, 5.1f),
        TangentOut = new Vector3(1.9f, 3.3f, 5.9f)
      };
      PathKey3F pathKey2 = new PathKey3F
      {
        Interpolation = SplineInterpolation.Hermite,
        Parameter = 66.7f,
        Point = new Vector3(2.2f, 1.4f, 6.6f),
        TangentIn = new Vector3(1.7f, 3.6f, 4.1f),
        TangentOut = new Vector3(2.9f, 2.3f, 6.9f)
      };
      Path3F path = new Path3F { pathKey1, pathKey2 };
      path.PreLoop = CurveLoopType.Cycle;
      path.PostLoop = CurveLoopType.CycleOffset;
      path.SmoothEnds = true;

      const string fileName = "SerializationPath3F.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, path);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      path = (Path3F)formatter.Deserialize(fs);
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
