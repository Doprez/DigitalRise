// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRise.Geometry.Collisions;
using DigitalRise.Geometry.Meshes;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;

namespace DigitalRise.Geometry.Shapes
{
  /// <summary>
  /// Represents a ray, which can be used for ray casting.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can be used if an <see cref="IGeometricObject"/> with a ray shape is needed. Use
  /// the <see cref="Ray"/> structure instead if you need a lightweight representation of a ray
  /// (avoids allocating memory on the heap).
  /// </para>
  /// <para>
  /// In contrast to a real ray, a <see cref="RayShape"/> object has a finite length! Infinite rays 
  /// should not be used because finite rays are faster and produce less numerical problems.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> Do not put rays into composite shapes (for example 
  /// <see cref="CompositeShape"/>). If a composite shape collides with another object, all contacts
  /// of this object pair are merged into a single <see cref="ContactSet"/>. Thus, ray hits of child
  /// rays of the composite shape are "merged" with normal contacts of other child shapes; the 
  /// result is undefined.
  /// </para>
  /// </remarks>
  [Serializable]
  public class RayShape : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space). 
    /// </remarks>
    public override Vector3 InnerPoint
    {
      get { return _origin + _direction * (_length / 2); }
    }


    /// <summary>
    /// Gets or sets the origin of the ray.
    /// </summary>
    /// <value>The origin of the ray.</value>
    public Vector3 Origin 
    {
      get { return _origin; }
      set
      {
        if (_origin != value)
        {
          _origin = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3 _origin;


    /// <summary>
    /// Gets or sets the direction of the ray.
    /// </summary>
    /// <value>The direction of the ray. Must be normalized.</value>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is not a normalized.
    /// </exception>
    public Vector3 Direction
    {
      get { return _direction; }
      set
      {
        if (!value.IsNumericallyNormalized())
          throw new ArgumentException("The ray direction of a must be normalized.");

        if (_direction != value)
        {
          _direction = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3 _direction = Vector3.UnitX;


    /// <summary>
    /// Gets or sets the finite length.
    /// </summary>
    /// <value>The finite length.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is ≤ 0 or infinite.
    /// </exception>
    public float Length
    {
      get { return _length; }
      set 
      {
        if (value <= 0 || Numeric.IsZero(value) || float.IsInfinity(value))
          throw new ArgumentOutOfRangeException("value", "Ray length must be in the range 0 < length < infinity.");

        if (_length != value)
        {
          _length = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _length;


    /// <summary>
    /// Gets or sets a value indicating whether the ray stops at the first (closest) object that was
    /// hit.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the ray stops at the closest hit object; otherwise, 
    /// <see langword="false"/> if the ray shoots through objects and hits all objects along the 
    /// ray.
    /// </value>
    /// <remarks>
    /// Note: This property is currently not applied if a ray is contained in a 
    /// <see cref="CompositeShape"/>.
    /// </remarks>
    public bool StopsAtFirstHit
    {
      // See remarks in CollisionDomain.
      get { return _stopsAtFirstHit; }
      set
      {
        if (_stopsAtFirstHit != value)
        {
          _stopsAtFirstHit = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private bool _stopsAtFirstHit;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="RayShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="RayShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a ray starting at the origin shooting into the positive x-axis direction and with a
    /// length of 100.
    /// </remarks>
    public RayShape()
      : this (Vector3.Zero, Vector3.UnitX, 100)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> class with the given origin, 
    /// direction and length.
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="length">The finite length.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="direction"/> is not normalized.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="length"/> is ≤ 0 or infinite.
    /// </exception>
    public RayShape(Vector3 origin, Vector3 direction, float length)
    {
      if (!direction.IsNumericallyNormalized())
        throw new ArgumentException("The ray direction must be normalized.");
      if (length <= 0 || Numeric.IsZero(length) || float.IsInfinity(length))
        throw new ArgumentOutOfRangeException("length", "Ray length must be in the range 0 < length < infinity.");

      _origin = origin;
      _direction = direction;
      _length = length;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RayShape"/> class from a <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The ray.</param>
    /// <exception cref="ArgumentException">
    /// The direction of <paramref name="ray"/> is not normalized.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The length of <paramref name="ray"/> is ≤ 0 or infinite.
    /// </exception>
    public RayShape(Ray ray)
    {
      if (!ray.Direction.IsNumericallyNormalized())
        throw new ArgumentException("The ray direction must be normalized.", "ray");
      if (ray.Length <= 0 || Numeric.IsZero(ray.Length) || float.IsInfinity(ray.Length))
        throw new ArgumentOutOfRangeException("ray", "Ray length must be in the range 0 < length < infinity.");

      _origin = ray.Origin;
      _direction = ray.Direction;
      _length = ray.Length;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new RayShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (RayShape)sourceShape;
      _origin = source.Origin;
      _direction = source.Direction;
      _length = source.Length;
      _stopsAtFirstHit = source.StopsAtFirstHit;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3 scale, Pose pose)
    {
      Vector3 scaledOrigin = _origin * scale;
      Vector3 worldStart = pose.ToWorldPosition(scaledOrigin);
      Vector3 worldEnd = pose.ToWorldPosition(scaledOrigin + scale * _direction * _length);
      Vector3 minimum = Vector3.Min(worldStart, worldEnd);
      Vector3 maximum = Vector3.Max(worldStart, worldEnd);
      return new Aabb(minimum, maximum);
    }


    /// <summary>
    /// Gets a support point for a given direction.
    /// </summary>
    /// <param name="direction">
    /// The direction for which to get the support point. The vector does not need to be normalized.
    /// The result is undefined if the vector is a zero vector.
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// </remarks>
    public override Vector3 GetSupportPoint(Vector3 direction)
    {
      Vector3 end = _origin + _length * _direction;
      if (Vector3.Dot(_origin, direction) > Vector3.Dot(end, direction))
        return _origin;
      else
        return end;
    }


    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point.
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </remarks>
    public override Vector3 GetSupportPointNormalized(Vector3 directionNormalized)
    {
      Vector3 end = _origin + _length * _direction;
      if (Vector3.Dot(_origin, directionNormalized) > Vector3.Dot(end, directionNormalized))
        return _origin;
      else
        return end;
    }


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used</param>
    /// <returns>0</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return 0;
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// This creates a mesh with a single degenerate triangle that represents the ray.
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Make a mesh with 1 degenerate triangle
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(
        new Triangle
        {
          Vertex0 = Origin,
          Vertex1 = Origin,
          Vertex2 = Origin + Direction * Length,
        }, 
        true, 
        Numeric.EpsilonF, 
        false);
      return mesh;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "RayShape {{ Origin = {0}, Direction = {1}, Length = {2}, StopsAtFirstHit = {3} }}", _origin, _direction, _length, _stopsAtFirstHit);
    }
    #endregion
  }
}
