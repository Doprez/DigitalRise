// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using DigitalRise.Mathematics;

namespace DigitalRise.Geometry.Shapes
{
  /// <summary>
  /// Defines a triangle.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a lightweight structure. To define a triangle shape for <see cref="IGeometricObject"/>
  /// use <see cref="TriangleShape"/>.
  /// </para>
  /// <para>
  /// Two <see cref="Triangle"/>s are considered as equal if they contain the same vertices in the
  /// same order.
  /// </para>
  /// </remarks>
  [Serializable]
  public struct Triangle : IEquatable<Triangle>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The first vertex.
    /// </summary>
    /// <value>The first vertex.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3 Vertex0;


    /// <summary>
    /// The second vertex.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3 Vertex1;


    /// <summary>
    /// The third vertex.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3 Vertex2;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the axis-aligned bounding box (AABB) for this triangle.
    /// </summary>
    /// <value>
    /// The AABB of the triangle.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Aabb Aabb
    {
      get
      {
        Vector3 minimum = Vector3.Min(Vertex0, Vector3.Min(Vertex1, Vertex2));
        Vector3 maximum = Vector3.Max(Vertex0, Vector3.Max(Vertex1, Vertex2));
        return new Aabb(minimum, maximum); 
      }
    }

    
    /// <summary>
    /// Gets the normal.
    /// </summary>
    /// <value>The normal.</value>
    /// <remarks>
    /// If the triangle is degenerate, an arbitrary normalized vector is returned.
    /// </remarks>
    public Vector3 Normal
    {
      get
      {
        Vector3 normal = Vector3.Cross(Vertex1 - Vertex0, Vertex2 - Vertex0);
        if (!normal.TryNormalize())
          normal = Vector3.UnitY;

        return normal;
      }
    }


    /// <summary>
    /// Gets or sets the vertex at the specified index.
    /// </summary>
    /// <param name="index">The index of the triangle point.</param>
    /// <value>The vertex with the given index.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is out of range.
    /// </exception>
    public Vector3 this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return Vertex0;
          case 1: return Vertex1;
          case 2: return Vertex2;
          default:
            throw new ArgumentOutOfRangeException("index");
        }
      }
      set
      {
        switch (index)
        {
          case 0: Vertex0 = value; break;
          case 1: Vertex1 = value; break;
          case 2: Vertex2 = value; break;
          default:
            throw new ArgumentOutOfRangeException("index");
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of <see cref="Triangle"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of <see cref="Triangle"/> from three points.
    /// </summary>
    /// <param name="vertex0">The first vertex.</param>
    /// <param name="vertex1">The second vertex.</param>
    /// <param name="vertex2">The third vertex.</param>
    public Triangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
    {
      Vertex0 = vertex0;
      Vertex1 = vertex1;
      Vertex2 = vertex2;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Triangle"/> from a <see cref="TriangleShape"/>.
    /// </summary>
    /// <param name="triangleShape">
    /// The <see cref="TriangleShape"/> from which vertices are copied.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="triangleShape"/> is <see langword="null"/>.
    /// </exception>
    public Triangle(TriangleShape triangleShape)
    {
      if (triangleShape == null)
        throw new ArgumentNullException("triangleShape");

      Vertex0 = triangleShape.Vertex0;
      Vertex1 = triangleShape.Vertex1;
      Vertex2 = triangleShape.Vertex2;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //-------------------------------------------------------------- 

    #region ----- Equality -----

    /// <overloads>
    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="Object"/> to compare with the current <see cref="Object"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is Triangle && Equals((Triangle)obj);
    }


    /// <summary>
    /// Determines whether the specified <see cref="Triangle"/> is equal to the current 
    /// <see cref="Triangle"/>.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Triangle other)
    {
      return Vertex0 == other.Vertex0 && Vertex1 == other.Vertex1 && Vertex2 == other.Vertex2;
    }


    /// <summary>
    /// Tests if two <see cref="Triangle"/>s are equal.
    /// </summary>
    /// <param name="triangle1">The first <see cref="Triangle"/>.</param>
    /// <param name="triangle2">The second <see cref="Triangle"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Triangle"/>s are equal; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Triangle triangle1, Triangle triangle2)
    {
      return triangle1.Vertex0 == triangle2.Vertex0
             && triangle1.Vertex1 == triangle2.Vertex1
             && triangle1.Vertex2 == triangle2.Vertex2;
    }


    /// <summary>
    /// Tests if two <see cref="Triangle"/>s are different.
    /// </summary>
    /// <param name="triangle1">The first <see cref="Triangle"/>.</param>
    /// <param name="triangle2">The second <see cref="Triangle"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Triangle"/>s are different; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Triangle triangle1, Triangle triangle2)
    {
      return triangle1.Vertex0 != triangle2.Vertex0
             || triangle1.Vertex1 != triangle2.Vertex1
             || triangle1.Vertex2 != triangle2.Vertex2;
    }
    #endregion


    /// <summary>
    /// Computes the axis-aligned bounding box (AABB) for this triangle positioned in world space
    /// using the given <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">
    /// The <see cref="Pose"/> of the shape. This pose defines how the shape should be positioned in
    /// world space.
    /// </param>
    /// <returns>The AABB of the shape positioned in world space.</returns>
    /// <remarks>
    /// <para>
    /// The AABB is axis-aligned to the axes of the world space (or the parent coordinate space).
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Aabb GetAabb(Pose pose)
    {
      // Note: Compute AABB in world space
      // This code should be the same as TriangleShape.GetAabb().
      Vector3 vertex0 = pose.ToWorldPosition(Vertex0);
      Vector3 vertex1 = pose.ToWorldPosition(Vertex1);
      Vector3 vertex2 = pose.ToWorldPosition(Vertex2);
      Vector3 minimum = Vector3.Min(vertex0, Vector3.Min(vertex1, vertex2));
      Vector3 maximum = Vector3.Max(vertex0, Vector3.Max(vertex1, vertex2));
      return new Aabb(minimum, maximum);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = Vertex0.GetHashCode();
        hashCode = (hashCode * 397) ^ Vertex1.GetHashCode();
        hashCode = (hashCode * 397) ^ Vertex2.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "Triangle {{ Vertex0 = {0}, Vertex1 = {1}, Vertex2 = {2} }}", Vertex0, Vertex1, Vertex2);
    }
    #endregion
  }
}
