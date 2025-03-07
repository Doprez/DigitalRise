using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class TriangleShapeTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3(), new TriangleShape().Vertex0);
      Assert.AreEqual(new Vector3(), new TriangleShape().Vertex1);
      Assert.AreEqual(new Vector3(), new TriangleShape().Vertex2);
      Assert.AreEqual(new Vector3(), new TriangleShape(Vector3.Zero, Vector3.Zero, Vector3.Zero).Vertex0);
      Assert.AreEqual(new Vector3(), new TriangleShape(Vector3.Zero, Vector3.Zero, Vector3.Zero).Vertex1);
      Assert.AreEqual(new Vector3(), new TriangleShape(Vector3.Zero, Vector3.Zero, Vector3.Zero).Vertex2);
      Assert.AreEqual(new Vector3(1, 2, 3), new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)).Vertex0);
      Assert.AreEqual(new Vector3(4, 5, 6), new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)).Vertex1);
      Assert.AreEqual(new Vector3(7, 8, 9), new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)).Vertex2);

      Assert.AreEqual(new Vector3(1, 2, 3), new TriangleShape(new Triangle(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9))).Vertex0);
      Assert.AreEqual(new Vector3(4, 5, 6), new TriangleShape(new Triangle(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9))).Vertex1);
      Assert.AreEqual(new Vector3(7, 8, 9), new TriangleShape(new Triangle(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9))).Vertex2);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual((new Vector3(1, 2, 3) + new Vector3(4, 5, 6) + new Vector3(7, 8, 9))/3,
                      new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)).InnerPoint);
    }


    [Test]
    public void TestProperties()
    {
      TriangleShape t = new TriangleShape();
      Assert.AreEqual(new Vector3(), t.Vertex0);
      Assert.AreEqual(new Vector3(), t.Vertex1);
      Assert.AreEqual(new Vector3(), t.Vertex2);
      
      t.Vertex0 = new Vector3(1, 2, 3);
      Assert.AreEqual(new Vector3(1, 2, 3), t.Vertex0);
      Assert.AreEqual(new Vector3(), t.Vertex1);
      Assert.AreEqual(new Vector3(), t.Vertex2);

      t.Vertex1 = new Vector3(4, 5, 6);
      Assert.AreEqual(new Vector3(1, 2, 3), t.Vertex0);
      Assert.AreEqual(new Vector3(4, 5, 6), t.Vertex1);
      Assert.AreEqual(new Vector3(), t.Vertex2);

      t.Vertex2 = new Vector3(9, 7, 8);
      Assert.AreEqual(new Vector3(1, 2, 3), t.Vertex0);
      Assert.AreEqual(new Vector3(4, 5, 6), t.Vertex1);
      Assert.AreEqual(new Vector3(9, 7, 8), t.Vertex2);

      Assert.IsTrue(MathHelper.AreNumericallyEqual(Vector3.Cross(new Vector3(3, 3, 3), new Vector3(8, 5, 5)).Normalized(), t.Normal));

      // Degenerate triangles can have any normal.
      Assert.IsTrue(Numeric.AreEqual(1, new TriangleShape().Normal.Length()));
    }


    [Test]
    public void TestIndexer()
    {
      TriangleShape t = new TriangleShape();
      Assert.AreEqual(new Vector3(), t[0]);
      Assert.AreEqual(new Vector3(), t[1]);
      Assert.AreEqual(new Vector3(), t[2]);

      t[0] = new Vector3(1, 2, 3);
      Assert.AreEqual(new Vector3(1, 2, 3), t[0]);
      Assert.AreEqual(new Vector3(), t[1]);
      Assert.AreEqual(new Vector3(), t[2]);

      t[1] = new Vector3(4, 5, 6);
      Assert.AreEqual(new Vector3(1, 2, 3), t[0]);
      Assert.AreEqual(new Vector3(4, 5, 6), t[1]);
      Assert.AreEqual(new Vector3(), t[2]);

      t[2] = new Vector3(7, 8, 9);
      Assert.AreEqual(new Vector3(1, 2, 3), t[0]);
      Assert.AreEqual(new Vector3(4, 5, 6), t[1]);
      Assert.AreEqual(new Vector3(7, 8, 9), t[2]);

      Assert.AreEqual(t.Vertex0, t[0]);
      Assert.AreEqual(t.Vertex1, t[1]);
      Assert.AreEqual(t.Vertex2, t[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException0()
    {
      new TriangleShape()[3] = Vector3.Zero;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException1()
    {
      new TriangleShape()[-1] = Vector3.Zero;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException2()
    {
      Vector3 v = new TriangleShape()[3];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException3()
    {
      Vector3 v = new TriangleShape()[-1];
    }


    //[Test]
    //public void GetAabb()
    //{
    //  Assert.AreEqual(new Aabb(), new TriangleShape().GetAabb(Pose.Identity));
    //  Assert.AreEqual(new Aabb(new Vector3(10, 100, -13), new Vector3(10, 100, -13)),
    //                 new TriangleShape().GetAabb(new Pose(new Vector3(10, 100, -13),
    //                                                                     MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f))));
    //  Assert.AreEqual(new Aabb(new Vector3(11, 102, 1003), new Vector3(11, 102, 1003)),
    //                 new TriangleShape(new Vector3(1, 2, 3)).GetAabb(new Pose(new Vector3(10, 100, 1000),
    //                                                                     Quaternion.Identity)));
    //  Quaternion rotation = MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f);
    //  Vector3 worldPos = rotation.Rotate(new Vector3(1, 2, 3)) + new Vector3(10, 100, 1000);
    //  Assert.IsTrue(MathHelper.AreNumericallyEqual(worldPos, new TriangleShape(new Vector3(1, 2, 3)).GetAabb(new Pose(new Vector3(10, 100, 1000), rotation)).Minimum));
    //  Assert.IsTrue(MathHelper.AreNumericallyEqual(worldPos, new TriangleShape(new Vector3(1, 2, 3)).GetAabb(new Pose(new Vector3(10, 100, 1000), rotation)).Maximum));
    //}


    [Test]
    public void GetMesh()
    {
      var t = new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(8, 9, -1));
      
      var m = t.GetMesh(0, 1);
      Assert.AreEqual(1, m.NumberOfTriangles);

      Triangle t2 = m.GetTriangle(0);

      Assert.AreEqual(t.Vertex0, t2.Vertex0);
      Assert.AreEqual(t.Vertex1, t2.Vertex1);
      Assert.AreEqual(t.Vertex2, t2.Vertex2);
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3(0, 0, 0), new TriangleShape().GetSupportPoint(new Vector3(1, 0, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new TriangleShape().GetSupportPoint(new Vector3(0, 1, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new TriangleShape().GetSupportPoint(new Vector3(0, 0, 1)));
      Assert.AreEqual(new Vector3(0, 0, 0), new TriangleShape().GetSupportPoint(new Vector3(1, 1, 1)));

      Vector3 p0 = new Vector3(2, 0, 0);
      Vector3 p1 = new Vector3(-1, -1, -2);
      Vector3 p2 = new Vector3(0, 2, -3);
      Assert.AreEqual(p0, new TriangleShape(p0, p1, p2).GetSupportPoint(new Vector3(1, 0, 0)));
      Assert.AreEqual(p2, new TriangleShape(p0, p1, p2).GetSupportPoint(new Vector3(0, 1, 0)));
      Assert.AreEqual(p2, new TriangleShape(p0, p1, p2).GetSupportPoint(new Vector3(0, 0, -1)));
      Assert.AreEqual(p1, new TriangleShape(p0, p1, p2).GetSupportPoint(new Vector3(-1, 0, 1)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new TriangleShape().GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(0, new TriangleShape().GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(0, new TriangleShape().GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(0, new TriangleShape().GetSupportPointDistance(new Vector3(1, 1, 1)));

    //  Assert.AreEqual(1, new TriangleShape(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(2, new TriangleShape(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(3, new TriangleShape(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  Assert.IsTrue(Numeric.AreEqual(MathHelper.ProjectTo(new Vector3(1, 2, 3), new Vector3(1, 1, 1)).Length, new TriangleShape(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(1, 1, 1))));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual(
        "TriangleShape { Vertex0 = (1; 2; 3), Vertex1 = (4; 5; 6), Vertex2 = (7; 8; 9) }", 
        new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)).ToString());
    }


    [Test]
    public void GetClosestPoints()
    {
      var t = new Triangle(new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(0, 10, 0));

      float u, v, w;
      GeometryHelper.GetClosestPoint(t, new Vector3(10, 0, 0), out u, out v, out w);
      Assert.AreEqual(0, u);
      Assert.AreEqual(1, v);
      Assert.AreEqual(0, w);

      GeometryHelper.GetClosestPoint(t, new Vector3(-1, -1, 0), out u, out v, out w);
      Assert.AreEqual(1, u);
      Assert.AreEqual(0, v);
      Assert.AreEqual(0, w);

      GeometryHelper.GetClosestPoint(t, new Vector3(5, 0, 0), out u, out v, out w);
      Assert.AreEqual(0.5f, u);
      Assert.AreEqual(0.5f, v);
      Assert.AreEqual(0, w);

      GeometryHelper.GetClosestPoint(t, new Vector3(0, 100, 3), out u, out v, out w);
      Assert.AreEqual(0, u);
      Assert.AreEqual(0, v);
      Assert.AreEqual(1, w);

      GeometryHelper.GetClosestPoint(t, new Vector3(-1, 9, 0), out u, out v, out w);
      Assert.IsTrue(Numeric.AreEqual(0.1f, u));
      Assert.IsTrue(Numeric.AreEqual(0f, v));
      Assert.IsTrue(Numeric.AreEqual(0.9f, w));

      GeometryHelper.GetClosestPoint(t, new Vector3(100, 100, -2), out u, out v, out w);
      Assert.IsTrue(Numeric.AreEqual(0f, u));
      Assert.IsTrue(Numeric.AreEqual(0.5f, v));
      Assert.IsTrue(Numeric.AreEqual(0.5f, w));

      GeometryHelper.GetClosestPoint(t, new Vector3(2, 4, 5), out u, out v, out w);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Vector3(2, 4, 0), GeometryHelper.GetPointFromBarycentric(t, u, v, w)));

      // Two identical vertices in the triangle.
      var degeneratedTriangle = new Triangle(new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0));
      GeometryHelper.GetClosestPoint(degeneratedTriangle, new Vector3(), out u, out v, out w);
      var closestPoint = u * degeneratedTriangle.Vertex0 + v * degeneratedTriangle.Vertex1 + w * degeneratedTriangle.Vertex2;
      Assert.AreEqual(new Vector3(0, 1, 0), closestPoint);
    }


    [Test]
    public void IsInFront()
    {
      Triangle t = new Triangle(new Vector3(-1, 0, -2), new Vector3(1, 0, 10), new Vector3(100, 0, -1));
      Assert.IsTrue(GeometryHelper.IsInFront(t, new Vector3(10, 20, 1)) > 0);
      Assert.IsTrue(GeometryHelper.IsInFront(t, new Vector3(10, -20, 1)) < 0);
      Assert.IsTrue(GeometryHelper.IsInFront(t, new Vector3(10, 0, 1)) == 0);
    }


    [Test]
    public void BarycentricTest()
    {
      var t = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0));

      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 0, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(1, 0, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 1, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 0.3f, 2)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0.1f, 0.1f, -1)));
      Assert.AreEqual(false, GeometryHelper.IsOver(t, new Vector3(0, 2, 0)));
      Assert.AreEqual(false, GeometryHelper.IsOver(t, new Vector3(1, 1, 0)));

      t = new Triangle(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));

      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 0, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 1, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 0, 1)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(2, 0.3f, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(-1, 0.1f, 0.1f)));
      Assert.AreEqual(false, GeometryHelper.IsOver(t, new Vector3(0, 2, 0)));
      Assert.AreEqual(false, GeometryHelper.IsOver(t, new Vector3(1, 2, 1)));

      t = new Triangle(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1));

      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 0, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(1, 0, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0, 0, 1)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0.3f, 2, 0)));
      Assert.AreEqual(true, GeometryHelper.IsOver(t, new Vector3(0.1f, -1, 0.1f)));
      Assert.AreEqual(false, GeometryHelper.IsOver(t, new Vector3(0, 0, 2)));
      Assert.AreEqual(false, GeometryHelper.IsOver(t, new Vector3(1, 0, 1)));
    }


    [Test]
    public void Clone()
    {
      TriangleShape triangle = new TriangleShape(new Vector3(1, 2, 3), new Vector3(2, 3, 4), new Vector3(4, 5, 6));
      TriangleShape clone = triangle.Clone() as TriangleShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(triangle.Vertex0, clone.Vertex0);
      Assert.AreEqual(triangle.Vertex1, clone.Vertex1);
      Assert.AreEqual(triangle.Vertex2, clone.Vertex2);
      Assert.AreEqual(triangle.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(triangle.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9));

      // Serialize object.
      var stream = new MemoryStream();
      var serializer = new XmlSerializer(typeof(Shape));
      serializer.Serialize(stream, a);

      // Output generated xml. Can be manually checked in output window.
      stream.Position = 0;
      var xml = new StreamReader(stream).ReadToEnd();
      Trace.WriteLine("Serialized Object:\n" + xml);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new XmlSerializer(typeof(Shape));
      var b = (TriangleShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Vertex0, b.Vertex0);
      Assert.AreEqual(a.Vertex1, b.Vertex1);
      Assert.AreEqual(a.Vertex2, b.Vertex2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new TriangleShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9));

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (TriangleShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Vertex0, b.Vertex0);
      Assert.AreEqual(a.Vertex1, b.Vertex1);
      Assert.AreEqual(a.Vertex2, b.Vertex2);
    }
  }
}
