using System;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class PlaneTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3(), new Plane().Normal);
      Assert.AreEqual(0, new Plane().DistanceFromOrigin);

      Assert.AreEqual(new Vector3(), new Plane(Vector3.Zero, 0).Normal);
      Assert.AreEqual(0, new Plane(Vector3.Zero, 0).DistanceFromOrigin);

      Assert.AreEqual(new Vector3(10, 20, 30), new Plane(new Vector3(10, 20, 30), 5).Normal);
      Assert.AreEqual(5, new Plane(new Vector3(10, 20, 30), 5).DistanceFromOrigin);

      Assert.AreEqual(new Vector3(10, 20, 30).Normalized(), new Plane(new PlaneShape(new Vector3(10, 20, 30).Normalized(), new Vector3(11, 22, 33))).Normal);
      Assert.AreEqual(5, new Plane(new PlaneShape(new Vector3(10, 20, 30).Normalized(), 5)).DistanceFromOrigin);

      Assert.AreEqual(new Plane(new Vector3(10, 20, 30).Normalized(), new Vector3(11, 22, 33)).Normal, new PlaneShape(new Vector3(10, 20, 30).Normalized(), new Vector3(11, 22, 33)).Normal);
      Assert.AreEqual(new Plane(new Vector3(10, 20, 30).Normalized(), new Vector3(11, 22, 33)).DistanceFromOrigin, new PlaneShape(new Vector3(10, 20, 30).Normalized(), new Vector3(11, 22, 33)).DistanceFromOrigin);

      Assert.AreEqual(new Plane(new Vector3(10, 20, 30), new Vector3(11, 22, 33), new Vector3(11, 2, 3)).Normal, new PlaneShape(new Vector3(10, 20, 30), new Vector3(11, 22, 33), new Vector3(11, 2, 3)).Normal);
      Assert.AreEqual(new Plane(new Vector3(10, 20, 30), new Vector3(11, 22, 33), new Vector3(11, 2, 3)).DistanceFromOrigin, new PlaneShape(new Vector3(10, 20, 30), new Vector3(11, 22, 33), new Vector3(11, 2, 3)).DistanceFromOrigin);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorArgumentNullException()
    {
      new Plane(null);
    }


    [Test]
    public void EqualsTest()
    {
      Assert.IsTrue(new Plane().Equals(new Plane()));
      Assert.IsTrue(new Plane().Equals(new Plane(Vector3.Zero, Vector3.Zero)));
      Assert.IsTrue(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).Equals(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6))));
      Assert.IsTrue(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).Equals((object)new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6))));
      Assert.IsFalse(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).Equals(new Plane(new Vector3(0, 2, 3), new Vector3(4, 5, 6))));
      Assert.IsFalse(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).Equals(new LineSegment(new Vector3(1, 2, 3), new Vector3(4, 5, 6))));
      Assert.IsFalse(new Plane().Equals(null));

      Assert.IsTrue(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)) == new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)));
      Assert.IsTrue(new Plane(new Vector3(1, 2, 4), new Vector3(4, 5, 6)) != new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)));
    }


    [Test]
    public void GetHashCodeTest()
    {
      Assert.AreEqual(new Plane().GetHashCode(), new Plane().GetHashCode());
      Assert.AreEqual(new Plane().GetHashCode(), new Plane(Vector3.Zero, Vector3.Zero).GetHashCode());
      Assert.AreEqual(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode(), new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode());
      Assert.AreNotEqual(new Plane(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode(), new Plane(new Vector3(0, 2, 3), new Vector3(4, 5, 6)).GetHashCode());
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new Plane().ToString().StartsWith("Plane"));
    }


    [Test]
    public void PositiveUniformScaling()
    {
      Vector3 point0 = new Vector3(1, 0.5f, 0.5f);
      Vector3 point1 = new Vector3(0.5f, 1, 0.5f);
      Vector3 point2 = new Vector3(0.5f, 0.5f, 1);
      Plane plane = new Plane(point0, point1, point2);

      Vector3 pointAbove = plane.Normal * plane.DistanceFromOrigin * 2;
      Vector3 pointBelow = plane.Normal * plane.DistanceFromOrigin * 0.5f;
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointAbove) > plane.DistanceFromOrigin);
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointBelow) < plane.DistanceFromOrigin);

      Vector3 scale = new Vector3(3.5f);
      point0 *= scale;
      point1 *= scale;
      point2 *= scale;
      pointAbove *= scale;
      pointBelow *= scale;
      plane.Scale(ref scale);

      Assert.IsTrue(plane.Normal.IsNumericallyNormalized());

      Vector3 dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point1, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point2, out dummy));
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointAbove) > plane.DistanceFromOrigin);
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointBelow) < plane.DistanceFromOrigin);
    }


    [Test]
    public void NegativeUniformScaling()
    {
      Vector3 point0 = new Vector3(1, 0.5f, 0.5f);
      Vector3 point1 = new Vector3(0.5f, 1, 0.5f);
      Vector3 point2 = new Vector3(0.5f, 0.5f, 1);
      Plane plane = new Plane(point0, point1, point2);

      Vector3 pointAbove = plane.Normal * plane.DistanceFromOrigin * 2;
      Vector3 pointBelow = plane.Normal * plane.DistanceFromOrigin * 0.5f;

      Vector3 scale = new Vector3(-3.5f);
      point0 *= scale;
      point1 *= scale;
      point2 *= scale;
      pointAbove *= scale;
      pointBelow *= scale;
      plane.Scale(ref scale);

      Assert.IsTrue(plane.Normal.IsNumericallyNormalized());

      Vector3 dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point1, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point2, out dummy));
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointAbove) > plane.DistanceFromOrigin);
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointBelow) < plane.DistanceFromOrigin);
    }


    [Test]
    public void NonuniformScaling()
    {
      Vector3 scale = new Vector3(1, 2, 3);
      Assert.That(() => new Plane().Scale(ref scale), Throws.Exception.TypeOf<NotSupportedException>());
    }


    [Test]
    public void ToWorld()
    {
      Vector3 point0 = new Vector3(1, 0.5f, 0.5f);
      Vector3 point1 = new Vector3(0.5f, 1, 0.5f);
      Vector3 point2 = new Vector3(0.5f, 0.5f, 1);
      Plane plane = new Plane(point0, point1, point2);

      Vector3 pointAbove = plane.Normal * plane.DistanceFromOrigin * 2;
      Vector3 pointBelow = plane.Normal * plane.DistanceFromOrigin * 0.5f;
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointAbove) > plane.DistanceFromOrigin);
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointBelow) < plane.DistanceFromOrigin);

      Pose pose = new Pose(new Vector3(-5, 100, -20), Matrix33F.CreateRotation(new Vector3(1, 2, 3), 0.123f));
      point0 = pose.ToWorldPosition(point0);
      point1 = pose.ToWorldPosition(point1);
      point2 = pose.ToWorldPosition(point2);
      pointAbove = pose.ToWorldPosition(pointAbove);
      pointBelow = pose.ToWorldPosition(pointBelow);
      plane.ToWorld(ref pose);

      Assert.IsTrue(plane.Normal.IsNumericallyNormalized());

      Vector3 dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point1, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point2, out dummy));
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointAbove) > plane.DistanceFromOrigin);
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointBelow) < plane.DistanceFromOrigin);
    }


    [Test]
    public void ToLocal()
    {
      Vector3 point0 = new Vector3(1, 0.5f, 0.5f);
      Vector3 point1 = new Vector3(0.5f, 1, 0.5f);
      Vector3 point2 = new Vector3(0.5f, 0.5f, 1);
      Plane plane = new Plane(point0, point1, point2);

      Vector3 pointAbove = plane.Normal * plane.DistanceFromOrigin * 2;
      Vector3 pointBelow = plane.Normal * plane.DistanceFromOrigin * 0.5f;
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointAbove) > plane.DistanceFromOrigin);
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointBelow) < plane.DistanceFromOrigin);

      Pose pose = new Pose(new Vector3(-5, 100, -20), Matrix33F.CreateRotation(new Vector3(1, 2, 3), 0.123f));
      point0 = pose.ToLocalPosition(point0);
      point1 = pose.ToLocalPosition(point1);
      point2 = pose.ToLocalPosition(point2);
      pointAbove = pose.ToLocalPosition(pointAbove);
      pointBelow = pose.ToLocalPosition(pointBelow);
      plane.ToLocal(ref pose);

      Assert.IsTrue(plane.Normal.IsNumericallyNormalized());

      Vector3 dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point1, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(plane, point2, out dummy));
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointAbove) > plane.DistanceFromOrigin);
      Assert.IsTrue(Vector3.Dot(plane.Normal, pointBelow) < plane.DistanceFromOrigin);
    }


    [Test]
    public void TryNormalize()
    {
      Plane p = new Plane(new Vector3(1, 2, 3).Normalized(), 123);
      
      // Scale all parts of the plane equation.
      Plane p2 = p;
      p2.Normal *= 3.33f;
      p2.DistanceFromOrigin *= 3.33f;

      Assert.IsTrue(p2.TryNormalize());
      Assert.IsTrue(MathHelper.AreNumericallyEqual(p.Normal, p2.Normal));
      Assert.IsTrue(Numeric.AreEqual(p.DistanceFromOrigin, p2.DistanceFromOrigin));

      // Scale all parts of the plane equation.
      p2 = p;
      p2.Normal *= 1e-8f;
      p2.DistanceFromOrigin *= 1e-8f;

      Assert.IsFalse(p2.TryNormalize());
    }
  }
}
