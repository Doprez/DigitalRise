using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class QuaternionTest
  {
    [Test]
    public void Zero()
    {
      Quaternion zero = MathHelper.QuaternionZero;
      Assert.AreEqual(zero.W, 0.0f);
      Assert.AreEqual(zero.X, 0.0f);
      Assert.AreEqual(zero.Y, 0.0f);
      Assert.AreEqual(zero.Z, 0.0f);
    }


    [Test]
    public void Identity()
    {
      Quaternion identity = Quaternion.Identity;
      Assert.AreEqual(identity.W, 1.0f);
      Assert.AreEqual(identity.X, 0.0f);
      Assert.AreEqual(identity.Y, 0.0f);
      Assert.AreEqual(identity.Z, 0.0f);

      Vector3 v = new Vector3(2.0f, 2.0f, 2.0f);
      Vector3 rotated = identity.ToRotationMatrix33() * v;
      Assert.IsTrue(v == rotated);
    }


    [Test]
    public void Constructor()
    {
      Quaternion q = new Quaternion(2, 3, 4, 1);
      Assert.AreEqual(1.0f, q.W);
      Assert.AreEqual(2.0f, q.X);
      Assert.AreEqual(3.0f, q.Y);
      Assert.AreEqual(4.0f, q.Z);

      // From matrix
      q = MathHelper.CreateRotation(Matrix33F.Identity);
      Assert.AreEqual(Quaternion.Identity, q);

      q = new Quaternion(new Vector3(1.0f, 2.0f, 3.0f), 0.123f);
      Assert.AreEqual(0.123f, q.W);
      Assert.AreEqual(1.0f, q.X);
      Assert.AreEqual(2.0f, q.Y);
      Assert.AreEqual(3.0f, q.Z);
    }


    [Test]
    public void QuaternionromMatrix33()
    {
      Vector3 v = Vector3.One;
      Matrix33F m = Matrix33F.Identity;
      Quaternion q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = Matrix33F.CreateRotation(Vector3.UnitX, 0.3f);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = Matrix33F.CreateRotation(Vector3.UnitY, 1.0f);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = Matrix33F.CreateRotation(Vector3.UnitZ, 4.0f);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = Matrix33F.Identity;
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = Matrix33F.CreateRotation(-Vector3.UnitX, 1.3f);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = Matrix33F.CreateRotation(-Vector3.UnitY, -1.4f);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = Matrix33F.CreateRotation(-Vector3.UnitZ, -0.1f);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = new Matrix33F(0,  0, 1,
                        0, -1, 0,
                        1,  0, 0);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));

      m = new Matrix33F(-1, 0,  0,
                         0, 1,  0,
                         0, 0, -1);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(m * v, q.Rotate(v)));
    }


    [Test]
    public void Properties()
    {
      Quaternion q = new Quaternion(1.0f, 2.0f, 3.0f, 0.123f);
      Assert.AreEqual(0.123f, q.W);
      Assert.AreEqual(1.0f, q.X);
      Assert.AreEqual(2.0f, q.Y);
      Assert.AreEqual(3.0f, q.Z);

      q.W = 1.0f;
      q.X = 2.0f;
      q.Y = 3.0f;
      q.Z = 4.0f;
      Assert.AreEqual(1.0f, q.W);
      Assert.AreEqual(2.0f, q.X);
      Assert.AreEqual(3.0f, q.Y);
      Assert.AreEqual(4.0f, q.Z);
    }


    [Test]
    public void Angle()
    {
      Vector3 axis = new Vector3(1.0f, 2.0f, 3.0f);
      Quaternion q = MathHelper.CreateRotation(axis, 0.4f);
      Assert.IsTrue(Numeric.AreEqual(0.4f, q.Angle()));
      q.SetAngle(0.9f);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q, MathHelper.CreateRotation(axis, 0.9f)));

      Assert.AreEqual(0, new Quaternion(0, 0, 0, 1.000001f).Angle());
    }


    [Test]
    public void Axis()
    {
      Vector3 axis = new Vector3(1.0f, 2.0f, 3.0f);
      float angle = 0.2f;      

      Quaternion q = MathHelper.CreateRotation(axis, angle);
      Assert.IsTrue(Numeric.AreEqual(angle, q.Angle()));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(axis.Normalized(), q.Axis()));
    }


    [Test]
    public void Equality()
    {
      Quaternion q1 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion copyOfQ1 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion q2 = new Quaternion(-1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion q3 = new Quaternion(1.0f, -2.0f, 3.0f, 4.0f);
      Quaternion q4 = new Quaternion(1.0f, 2.0f, -3.0f, 4.0f);
      Quaternion q5 = new Quaternion(1.0f, 2.0f, 3.0f, -4.0f);
      Assert.IsTrue(q1 == copyOfQ1);
      Assert.IsFalse(q1 == q2);
      Assert.IsFalse(q1 == q3);
      Assert.IsFalse(q1 == q4);
      Assert.IsFalse(q1 == q5);

      Assert.IsFalse(q1 != copyOfQ1);
      Assert.IsTrue(q1 != q2);
      Assert.IsTrue(q1 != q3);
      Assert.IsTrue(q1 != q4);
      Assert.IsTrue(q1 != q5);
    }


    [Test]
    public void AreEqual()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      Quaternion q1 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion q2 = new Quaternion(1.000001f, 2.000001f, 3.000001f, 4.000001f);
      Quaternion q3 = new Quaternion(1.00000001f, 2.00000001f, 3.00000001f, 4.00000001f);

      Assert.IsTrue(MathHelper.AreNumericallyEqual(q1, q1));
      Assert.IsFalse(MathHelper.AreNumericallyEqual(q1, q2));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q1, q3));

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void TestEquals()
    {
      Quaternion q1 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion q2 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion q3 = new Quaternion(1.0f, 2.0f, 3.0f, 0.0f);
      Assert.IsTrue(q1.Equals(q2));
      Assert.IsFalse(q1.Equals(q3));
    }


    [Test]
    public void TestEquals2()
    {
      Quaternion q = Quaternion.Identity;
      Assert.IsFalse(q.Equals(q.ToString()));
    }


    [Test]
    public void HashCode()
    {
      Quaternion q = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Assert.AreNotEqual(MathHelper.QuaternionZero.GetHashCode(), q.GetHashCode());
    }


    [Test]
    public void TestToString()
    {
      Quaternion q = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      string s = q.ToString();
      Assert.IsNotNull(s);
      Assert.Greater(s.Length, 0);
    }


    [Test]
    public void Conjugated()
    {
      Quaternion q = new Quaternion(2, 3, 4, 1);
      Quaternion conjugate = q.Conjugated();
      Assert.AreEqual(1.0f, conjugate.W);
      Assert.AreEqual(-2.0f, conjugate.X);
      Assert.AreEqual(-3.0f, conjugate.Y);
      Assert.AreEqual(-4.0f, conjugate.Z);
    }


    [Test]
    public void Conjugate()
    {
      Quaternion q = new Quaternion(2, 3, 4, 1);
      q.Conjugate();
      Assert.AreEqual(1.0f, q.W);
      Assert.AreEqual(-2.0f, q.X);
      Assert.AreEqual(-3.0f, q.Y);
      Assert.AreEqual(-4.0f, q.Z);
    }


    [Test]
    public void Inverse()
    {
      Quaternion identity = Quaternion.Identity;
      Quaternion inverseIdentity = identity.Inverse();
      Assert.AreEqual(inverseIdentity, identity);

      float angle = 0.4f;
      Vector3 axis = new Vector3(1.0f, 1.0f, 1.0f);
      axis.Normalize();
      Quaternion q = MathHelper.CreateRotation(axis, angle);
      Quaternion inverse = q.Inverse();
      Assert.IsTrue(MathHelper.AreNumericallyEqual(-axis, inverse.Axis()));

      q = new Quaternion(1, 2, 3, 4);
      inverse = q.Inverse();
      Assert.IsTrue(MathHelper.AreNumericallyEqual(Quaternion.Identity, inverse * q));
    }


    [Test]
    public void TryNormalize()
    {
      Quaternion q = MathHelper.QuaternionZero;
      bool normalized = q.TryNormalize();
      Assert.IsFalse(normalized);

      q = new Quaternion(1, 2, 3, 4);
      normalized = q.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Quaternion(1, 2, 3, 4).Normalized(), q);

      q = new Quaternion(0, -1, 0, 0);
      normalized = q.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Quaternion(0, -1, 0, 0).Normalized(), q);
    }


    [Test]
    public void DotProduct()
    {
      Quaternion q1 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion q2 = new Quaternion(5.0f, 6.0f, 7.0f, 8.0f);
      float dotProduct = Quaternion.Dot(q1, q2);
      Assert.AreEqual(70, dotProduct);
    }


    [Test]
    public void Rotate()
    {
      float angle = 0.4f;
      Vector3 axis = new Vector3(1.0f, 2.0f, 3.0f);
      Quaternion q = MathHelper.CreateRotation(axis, angle);
      Matrix33F m33 = Matrix33F.CreateRotation(axis, angle);
      Vector3 v = new Vector3(0.3f, -2.4f, 5.6f);
      Vector3 result1 = q.Rotate(v);
      Vector3 result2 = m33 * v;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void RotationMatrix33()
    {
      float angle = -1.6f;
      Vector3 axis = new Vector3(1.0f, 2.0f, -3.0f);
      Quaternion q = MathHelper.CreateRotation(axis, angle);
      Matrix33F m33 = Matrix33F.CreateRotation(axis, angle);
      Vector3 v = new Vector3(0.3f, -2.4f, 5.6f);
      Vector3 result1 = q.ToRotationMatrix33() * v;
      Vector3 result2 = m33 * v;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void RotationMatrix44()
    {
      float angle = -1.6f;
      Vector3 axis = new Vector3(1.0f, 2.0f, -3.0f);
      Quaternion q = MathHelper.CreateRotation(axis, angle);
      Matrix44F m44 = Matrix44F.CreateRotation(axis, angle);
      Assert.IsTrue(Matrix44F.AreNumericallyEqual(q.ToRotationMatrix44(), m44));
    }


    [Test]
    public void MultiplyScalarOperator()
    {
      float s = 123.456f;
      Quaternion q = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion expectedResult = new Quaternion(s * 1.0f, s * 2.0f, s * 3.0f, s * 4.0f);
      Quaternion result2 = q * s;
      Assert.AreEqual(expectedResult, result2);
    }


    [Test]
    public void MultiplyScalar()
    {
      float s = 123.456f;
      Quaternion q = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion expectedResult = new Quaternion(s * 1.0f, s * 2.0f, s * 3.0f, s * 4.0f);
      Quaternion result = Quaternion.Multiply(q, s);
      Assert.AreEqual(expectedResult, result);
    }


    [Test]
    public void MultiplyOperator()
    {
      float angle1 = 0.4f;
      Vector3 axis1 = new Vector3(1.0f, 2.0f, 3.0f);
      Quaternion q1 = MathHelper.CreateRotation(axis1, angle1);
      Matrix33F m1 = Matrix33F.CreateRotation(axis1, angle1);

      float angle2 = -1.6f;
      Vector3 axis2 = new Vector3(1.0f, -2.0f, -3.5f);
      Quaternion q2 = MathHelper.CreateRotation(axis2, angle2);
      Matrix33F m2 = Matrix33F.CreateRotation(axis2, angle2);

      Vector3 v = new Vector3(0.3f, -2.4f, 5.6f);
      Vector3 result1 = (q2 * q1).Rotate(v);
      Vector3 result2 = m2 * m1 * v;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void Multiply()
    {
      float angle1 = 0.4f;
      Vector3 axis1 = new Vector3(1.0f, 2.0f, 3.0f);
      Quaternion q1 = MathHelper.CreateRotation(axis1, angle1);
      Matrix33F m1 = Matrix33F.CreateRotation(axis1, angle1);

      float angle2 = -1.6f;
      Vector3 axis2 = new Vector3(1.0f, -2.0f, -3.5f);
      Quaternion q2 = MathHelper.CreateRotation(axis2, angle2);
      Matrix33F m2 = Matrix33F.CreateRotation(axis2, angle2);

      Vector3 v = new Vector3(0.3f, -2.4f, 5.6f);
      Vector3 result1 = Quaternion.Multiply(q2, q1).Rotate(v);
      Vector3 result2 = m2 * m1 * v;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void DivisionScalarOperator()
    {
      float s = 123.456f;
      Quaternion q = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion expectedResult = new Quaternion(1.0f / s, 2.0f / s, 3.0f / s, 4.0f / s);
      Quaternion result = q * (1.0f / s);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(expectedResult, result));
    }


    [Test]
    public void DivisionOperator()
    {
      float angle1 = 0.4f;
      Vector3 axis1 = new Vector3(1.0f, 2.0f, 3.0f);
      Quaternion q1 = MathHelper.CreateRotation(axis1, angle1);
      Matrix33F m1 = Matrix33F.CreateRotation(axis1, angle1);

      float angle2 = -1.6f;
      Vector3 axis2 = new Vector3(1.0f, -2.0f, -3.5f);
      Quaternion q2 = MathHelper.CreateRotation(axis2, angle2);
      Matrix33F m2 = Matrix33F.CreateRotation(axis2, angle2);

      Vector3 v = new Vector3(0.3f, -2.4f, 5.6f);
      Vector3 result1 = (q2 / q1).Rotate(v);
      Vector3 result2 = m2 * m1.Inverse * v;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void Division()
    {
      float angle1 = 0.4f;
      Vector3 axis1 = new Vector3(1.0f, 2.0f, 3.0f);
      Quaternion q1 = MathHelper.CreateRotation(axis1, angle1);
      Matrix33F m1 = Matrix33F.CreateRotation(axis1, angle1);

      float angle2 = -1.6f;
      Vector3 axis2 = new Vector3(1.0f, -2.0f, -3.5f);
      Quaternion q2 = MathHelper.CreateRotation(axis2, angle2);
      Matrix33F m2 = Matrix33F.CreateRotation(axis2, angle2);

      Vector3 v = new Vector3(0.3f, -2.4f, 5.6f);
      Vector3 result1 = Quaternion.Divide(q2, q1).Rotate(v);
      Vector3 result2 = m2 * m1.Inverse * v;
      Assert.IsTrue(MathHelper.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void CreateRotation()
    {
      Quaternion q;

      // From matrix vs. from angle/axis
      Matrix33F m = Matrix33F.CreateRotation(Vector3.UnitX, (float)Math.PI / 4);
      q = MathHelper.CreateRotation(m);
      Quaternion q2 = MathHelper.CreateRotation(Vector3.UnitX, (float)Math.PI / 4);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q2, q));
      m = Matrix33F.CreateRotation(Vector3.UnitY, (float)Math.PI / 4);
      q = MathHelper.CreateRotation(m);
      q2 = MathHelper.CreateRotation(Vector3.UnitY, (float)Math.PI / 4);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q2, q));
      m = Matrix33F.CreateRotation(Vector3.UnitZ, (float)Math.PI / 4);
      q = MathHelper.CreateRotation(m);
      q2 = MathHelper.CreateRotation(Vector3.UnitZ, (float)Math.PI / 4);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q2, q));

      // From vector-vector
      Vector3 start, end;
      start = Vector3.UnitX;
      end = Vector3.UnitY;
      q = MathHelper.CreateRotation(start, end);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(end, q.ToRotationMatrix33() * start));

      start = Vector3.UnitY;
      end = Vector3.UnitZ;
      q = MathHelper.CreateRotation(start, end);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(end, q.ToRotationMatrix33() * start));

      start = Vector3.UnitZ;
      end = Vector3.UnitX;
      q = MathHelper.CreateRotation(start, end);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(end, q.ToRotationMatrix33() * start));

      start = new Vector3(1, 1, 1);
      end = new Vector3(1, 1, 1);
      q = MathHelper.CreateRotation(start, end);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(end, q.ToRotationMatrix33() * start));

      start = new Vector3(1.0f, 1.0f, 1.0f);
      end = new Vector3(-1.0f, -1.0f, -1.0f);
      q = MathHelper.CreateRotation(start, end);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(end, q.ToRotationMatrix33() * start));

      start = new Vector3(-1.0f, 2.0f, 1.0f);
      end = new Vector3(-2.0f, -1.0f, -1.0f);
      q = MathHelper.CreateRotation(start, end);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(end, q.ToRotationMatrix33() * start));

      float degree45 = MathHelper.ToRadians(45);
      q = MathHelper.CreateRotation(degree45, Vector3.UnitZ, degree45, Vector3.UnitY, degree45, Vector3.UnitX, false);
      Quaternion expected = MathHelper.CreateRotation(Vector3.UnitZ, degree45) * MathHelper.CreateRotation(Vector3.UnitY, degree45)
                             * MathHelper.CreateRotation(Vector3.UnitX, degree45);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(expected, q));

      q = MathHelper.CreateRotation(degree45, Vector3.UnitZ, degree45, Vector3.UnitY, degree45, Vector3.UnitX, true);
      expected = MathHelper.CreateRotation(Vector3.UnitX, degree45) * MathHelper.CreateRotation(Vector3.UnitY, degree45)
                 * MathHelper.CreateRotation(Vector3.UnitZ, degree45);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(expected, q));
    }


    [Test]
    public void FromMatrixWithZeroTrace()
    {
      Quaternion q;
      Matrix33F m = new Matrix33F(0, 1, 0,
                                  0, 0, 1,
                                  1, 0, 0);
      q = MathHelper.CreateRotation(m);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new Quaternion(0.5f, 0.5f, 0.5f, -0.5f), q));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateRotationException1()
    {
      MathHelper.CreateRotation(Vector3.Zero, Vector3.UnitX);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateRotationException2()
    {
      MathHelper.CreateRotation(Vector3.UnitY, Vector3.Zero);
    }


    [Test]
    public void CreateRotationX()
    {
      float angle = 0.3f;
      Quaternion q = MathHelper.CreateRotation(Vector3.UnitX, angle);
      Quaternion qx = MathHelper.CreateRotationX(angle);
      Assert.AreEqual(q, qx);
    }


    [Test]
    public void CreateRotationY()
    {
      float angle = 0.3f;
      Quaternion q = MathHelper.CreateRotation(Vector3.UnitY, angle);
      Quaternion qy = MathHelper.CreateRotationY(angle);
      Assert.AreEqual(q, qy);
    }


    [Test]
    public void CreateRotationZ()
    {
      float angle = 0.3f;
      Quaternion q = MathHelper.CreateRotation(Vector3.UnitZ, angle);
      Quaternion qz = MathHelper.CreateRotationZ(angle);
      Assert.AreEqual(q, qz);
    }


    [Test]
    public void Exp()
    {
      float θ = -0.3f;
      Vector3 v = new Vector3(1.0f, 2.0f, 3.0f);
      v.Normalize();

      Quaternion q = new Quaternion(θ * v, 0.0f);
      Quaternion exp = MathHelper.Exp(q);
      Assert.IsTrue(Numeric.AreEqual((float)Math.Cos(θ), exp.W));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(v * (float)Math.Sin(θ), exp.V()));
    }


    [Test]
    public void Exp3()
    {
      float θ = 0.0f;
      Vector3 v = new Vector3(1.0f, 2.0f, 3.0f);
      v.Normalize();

      Quaternion q = new Quaternion(θ * v, 0.0f);
      Quaternion exp = MathHelper.Exp(q);
      Assert.IsTrue(Numeric.AreEqual(1, exp.W));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(Vector3.Zero, exp.V()));
    }


    [Test]
    public void Ln()
    {
      float θ = 0.3f;
      Vector3 v = new Vector3(1.0f, 2.0f, 3.0f);
      v.Normalize();
      Quaternion q = new Quaternion((float)Math.Sin(θ) * v, (float)Math.Cos(θ));

      Quaternion ln = MathHelper.Ln(q);
      Assert.IsTrue(Numeric.AreEqual(0.0f, ln.W));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(θ * v, ln.V()));
    }


    [Test]
    public void Ln2()
    {
      float θ = 0.0f;
      Vector3 v = new Vector3(1.0f, 2.0f, 3.0f);
      v.Normalize();
      Quaternion q = new Quaternion((float)Math.Sin(θ) * v, (float)Math.Cos(θ));

      Quaternion ln = MathHelper.Ln(q);
      Assert.IsTrue(Numeric.AreEqual(0.0f, ln.W));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(θ * v, ln.V()));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LnException()
    {
      Quaternion q = new Quaternion(0.0f, 0.0f, 0.0f, 1.5f);
      MathHelper.Ln(q);
    }


    [Test]
    public void AdditionOperator()
    {
      Quaternion a = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion b = new Quaternion(2.0f, 3.0f, 4.0f, 5.0f);
      Quaternion c = a + b;
      Assert.AreEqual(new Quaternion(3.0f, 5.0f, 7.0f, 9.0f), c);
    }


    [Test]
    public void Addition()
    {
      Quaternion a = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion b = new Quaternion(2.0f, 3.0f, 4.0f, 5.0f);
      Quaternion c = Quaternion.Add(a, b);
      Assert.AreEqual(new Quaternion(3.0f, 5.0f, 7.0f, 9.0f), c);
    }


    [Test]
    public void SubtractionOperator()
    {
      Quaternion a = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion b = new Quaternion(10.0f, -10.0f, 0.5f, 2.5f);
      Quaternion c = a - b;
      Assert.AreEqual(new Quaternion(-9.0f, 12.0f, 2.5f, 1.5f), c);
    }


    [Test]
    public void Subtraction()
    {
      Quaternion a = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion b = new Quaternion(10.0f, -10.0f, 0.5f, 2.5f);
      Quaternion c = Quaternion.Subtract(a, b);
      Assert.AreEqual(new Quaternion(-9.0f, 12.0f, 2.5f, 1.5f), c);
    }


    [Test]
    public void NegationOperator()
    {
      Quaternion a = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Assert.AreEqual(new Quaternion(-1.0f, -2.0f, -3.0f, -4.0f), -a);
    }


    [Test]
    public void Negation()
    {
      Quaternion a = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Assert.AreEqual(new Quaternion(-1.0f, -2.0f, -3.0f, -4.0f), Quaternion.Negate(a));
    }


    [Test]
    public void Power2()
    {
      const float θ = 0.4f;
      const float t = -1.2f;
      Vector3 v = new Vector3(2.3f, 1.0f, -2.0f);
      v.Normalize();

      Quaternion q = new Quaternion((float)Math.Sin(θ) * v, (float)Math.Cos(θ));
      q.Power(t);
      Quaternion expected = new Quaternion((float)Math.Sin(t * θ) * v, (float)Math.Cos(t * θ));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(expected, q));
    }


    [Test]
    public void Power3()
    {
      const float θ = 0.4f;
      Vector3 v = new Vector3(2.3f, 1.0f, -2.0f);
      v.Normalize();

      Quaternion q = new Quaternion((float)Math.Sin(θ) * v, (float)Math.Cos(θ));
      Quaternion q2 = q;
      q2.Power(2);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q * q, q2));
      Quaternion q3 = q;
      q3.Power(3);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q * q * q, q3));

      q2 = q;
      q2.Power(-2);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q.Inverse() * q.Inverse(), q2));

      q3 = q;
      q3.Power(-3);
      Assert.IsTrue(MathHelper.AreNumericallyEqual(q.Inverse() * q.Inverse() * q.Inverse(), q3));
    }


    [Test]
    public void GetAngleTest()
    {
      Quaternion qIdentity = Quaternion.Identity;
      Quaternion q03 = MathHelper.CreateRotation(Vector3.UnitX, 0.3f);
      Quaternion q03Plus11 = MathHelper.CreateRotation(new Vector3(1, 0.2f, -3), 1.1f) * q03;
      Quaternion q0 = MathHelper.CreateRotation(Vector3.UnitX, 0.0f);
      Quaternion qPi = MathHelper.CreateRotation(Vector3.UnitX, ConstantsF.Pi);
      Quaternion q2Pi = MathHelper.CreateRotation(Vector3.UnitX, ConstantsF.TwoPi);

      Assert.IsTrue(Numeric.AreEqual(0.0f, MathHelper.GetAngle(qIdentity, qIdentity)));
      Assert.IsTrue(Numeric.AreEqual(0.3f, MathHelper.GetAngle(qIdentity, q03)));
      Assert.IsTrue(Numeric.AreEqual(0.3f, MathHelper.GetAngle(qIdentity, -q03))); // Remember: q and -q represent the same orientation.
      Assert.IsTrue(Numeric.AreEqual(1.1f, MathHelper.GetAngle(q03, q03Plus11)));
      Assert.IsTrue(Numeric.AreEqual(1.1f, MathHelper.GetAngle(-q03, q03Plus11)));
      Assert.IsTrue(Numeric.AreEqual(1.1f, MathHelper.GetAngle(q03, -q03Plus11)));
      Assert.IsTrue(Numeric.AreEqual(1.1f, MathHelper.GetAngle(-q03, -q03Plus11)));
      Assert.IsTrue(Numeric.AreEqual(0.0f, MathHelper.GetAngle(qIdentity, q0)));
      Assert.IsTrue(Numeric.AreEqual(0.0f, MathHelper.GetAngle(qIdentity, q2Pi)));
      Assert.IsTrue(Numeric.AreEqual(0.0f, MathHelper.GetAngle(q0, q2Pi)));
      Assert.IsTrue(Numeric.AreEqual(0.3f, MathHelper.GetAngle(q03, q0)));
      Assert.IsTrue(Numeric.AreEqual(ConstantsF.Pi, MathHelper.GetAngle(q0, qPi)));
      Assert.IsTrue(Numeric.AreEqual(ConstantsF.Pi, MathHelper.GetAngle(q2Pi, qPi)));
    }


    [Test]
    public void SerializationXml()
    {
      Quaternion q1 = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f);
      Quaternion q2;
      string fileName = "SerializationQuaternion.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Quaternion));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, q1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Quaternion));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      q2 = (Quaternion)serializer.Deserialize(fileStream);
      Assert.AreEqual(q1, q2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Quaternion q1 = new Quaternion(0.1f, -0.2f, 6, 40);
      Quaternion q2;
      string fileName = "SerializationQuaternion.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, q1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      q2 = (Quaternion)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(q1, q2);
    }


    [Test]
    public void SerializationXml2()
    {
      Quaternion q1 = new Quaternion(0.1f, -0.2f, 6, 40);
      Quaternion q2;

      string fileName = "SerializationQuaternion_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Quaternion));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, q1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        q2 = (Quaternion)serializer.ReadObject(reader);

      Assert.AreEqual(q1, q2);
    }


    [Test]
    public void SerializationJson()
    {
      Quaternion q1 = new Quaternion(0.1f, -0.2f, 6, 40);
      Quaternion q2;

      string fileName = "SerializationQuaternion.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Quaternion));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, q1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        q2 = (Quaternion)serializer.ReadObject(stream);

      Assert.AreEqual(q1, q2);
    }
  }
}
