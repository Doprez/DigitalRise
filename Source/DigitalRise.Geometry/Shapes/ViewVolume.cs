// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.Geometry.Shapes
{
  /// <summary>
  /// Represents a view volume (base implementation).
  /// </summary>
  /// <para>
  /// The <see cref="ViewVolume"/> class is designed to model the view volume of a camera: The 
  /// observer is looking from the origin along the negative z-axis. The x-axis points to the right 
  /// and the y-axis points upwards. <see cref="ViewVolume.Near"/> and <see cref="ViewVolume.Far"/> 
  /// specify the distance from the origin (observer) to the near and far clip planes 
  /// (<see cref="ViewVolume.Near"/> &lt; <see cref="ViewVolume.Far"/>).
  /// </para>
  [Serializable]
  public abstract class ViewVolume : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the minimum x-value of the view volume at the near clip plane.
    /// </summary>
    /// <value>The minimum x-value of the view volume at the near clip plane.</value>
    public float Left
    {
      get { return _left; }
      set
      {
        if (_left != value)
        {
          _left = value;
          Update();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _left;


    /// <summary>
    /// Gets or sets the maximum x-value of the view volume at the near clip plane.
    /// </summary>
    /// <value>The maximum x-value of the view volume at the near clip plane.</value>
    public float Right
    {
      get { return _right; }
      set
      {
        if (_right != value)
        {
          _right = value;
          Update();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _right;


    /// <summary>
    /// Gets or sets the minimum y-value of the view volume at the near clip plane.
    /// </summary>
    /// <value>The minimum y-value of the view volume at the near clip plane.</value>
    public float Bottom
    {
      get { return _bottom; }
      set
      {
        if (_bottom != value)
        {
          _bottom = value;
          Update();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _bottom;


    /// <summary>
    /// Gets or sets the maximum y-value of the view volume at the near clip plane.
    /// </summary>
    /// <value>The maximum y-value of the view volume at the near clip plane.</value>
    public float Top
    {
      get { return _top; }
      set
      {
        if (_top != value)
        {
          _top = value;
          Update();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _top;


    /// <summary>
    /// Gets or sets the distance to the near clip plane. 
    /// </summary>
    /// <value>The distance to the near clip plane.</value>
    public float Near
    {
      get { return _near; }
      set
      {
        if (_near != value)
        {
          _near = value;
          Update();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _near;


    /// <summary>
    /// Gets or sets the distance to the far clip plane. 
    /// </summary>
    /// <value>The distance to the far clip plane.</value>
    public float Far
    {
      get { return _far; }
      set
      {
        if (_far != value)
        {
          _far = value;
          Update();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _far;


    /// <summary>
    /// Gets the width of the view volume at the near clip plane.
    /// </summary>
    /// <value>The width of the view volume at the near clip plane.</value>
    public float Width
    {
      get { return Math.Abs(Right - Left); }
    }


    /// <summary>
    /// Gets the height of the view volume at the near clip plane.
    /// </summary>
    /// <value>The height of the view volume at the near clip plane.</value>
    public float Height
    {
      get { return Math.Abs(Top - Bottom); }
    }


    /// <summary>
    /// Gets the depth of the view volume (= <see cref="Far"/> - <see cref="Near"/>).
    /// </summary>
    /// <value>The depth of the view volume (= <see cref="Far"/> - <see cref="Near"/>).</value>
    public float Depth
    {
      get { return Math.Abs(Far - Near); }
    }


    /// <summary>
    /// Gets the aspect ratio (width / height).
    /// </summary>
    /// <value>The aspect ratio (<see cref="Width"/> / <see cref="Height"/>).</value>
    public float AspectRatio
    {
      get { return Width / Height; }
    }


    /// <summary>
    /// Gets the horizontal field of view.
    /// </summary>
    /// <value>
    /// The horizontal field of view, or <see cref="Single.NaN"/> if this is a orthographic view 
    /// volume.
    /// </value>
    public abstract float FieldOfViewX { get; }


    /// <summary>
    /// Gets the vertical field of view.
    /// </summary>
    /// <value>
    /// The vertical field of view, or <see cref="Single.NaN"/> if this is a orthographic view 
    /// volume.
    /// </value>
    public abstract float FieldOfViewY { get; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Sets the width and height of the view volume to the specified values.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the width and height of the view volume to the specified size and depth.
    /// </summary>
    /// <param name="width">The width of the view volume at the near clip plane.</param>
    /// <param name="height">The height of the view volume at the near clip plane.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    public void SetWidthAndHeight(float width, float height, float near, float far)
    {
      if (near >= far)
        throw new ArgumentException("The near plane distance of a view volume needs to be less than the far plane distance.");

      _near = near;
      _far = far;
      SetWidthAndHeight(width, height);
    }


    /// <summary>
    /// Sets the width and height of the view volume to the specified size.
    /// </summary>
    /// <param name="width">The width of the view volume at the near clip plane.</param>
    /// <param name="height">The height of the view volume at the near clip plane.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
    /// </exception>
    public void SetWidthAndHeight(float width, float height)
    {
      if (width <= 0)
        throw new ArgumentOutOfRangeException("width", "The width of the view volume must be greater than 0.");
      if (height <= 0)
        throw new ArgumentOutOfRangeException("height", "The height of the view volume must be greater than 0.");

      float halfWidth = width / 2.0f;
      float halfHeight = height / 2.0f;
      _left = -halfWidth;
      _right = halfWidth;
      _bottom = -halfHeight;
      _top = halfHeight;
      Update();
    }


    /// <overloads>
    /// <summary>
    /// Sets the dimensions of the view volume.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the dimensions of the view volume (including depths).
    /// </summary>
    /// <param name="left">The minimum x-value of the view volume at the near clip plane.</param>
    /// <param name="right">The maximum x-value of the view volume at the near clip plane.</param>
    /// <param name="bottom">The minimum y-value of the view volume at the near clip plane.</param>
    /// <param name="top">The maximum y-value of the view volume at the near clip plane.</param>
    /// <param name="near">The distance to the near clip plane.</param>
    /// <param name="far">The distance to the far clip plane.</param>
    /// <remarks>
    /// This method can be used to define an asymmetric, off-center view volume.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="left"/> is greater than or equal to <paramref name="right"/>, 
    /// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>, or
    /// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
    /// </exception>
    public void Set(float left, float right, float bottom, float top, float near, float far)
    {
      if (near >= far)
        throw new ArgumentException("The near plane distance of a view volume needs to be less than the far plane distance (near < far).");

      _near = near;
      _far = far;
      Set(left, right, bottom, top);
    }


    /// <summary>
    /// Sets the dimensions of the view volume.
    /// </summary>
    /// <param name="left">The minimum x-value of the view volume at the near clip plane.</param>
    /// <param name="right">The maximum x-value of the view volume at the near clip plane.</param>
    /// <param name="bottom">The minimum y-value of the view volume at the near clip plane.</param>
    /// <param name="top">The maximum y-value of the view volume at the near clip plane.</param>
    /// <remarks>
    /// This method can be used to define an asymmetric, off-center view volume.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="left"/> is greater than or equal to <paramref name="right"/>, or
    /// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>.
    /// </exception>
    public void Set(float left, float right, float bottom, float top)
    {
      if (left >= right)
        throw new ArgumentException("Left needs to be less than right (left < right).");
      if (bottom >= top)
        throw new ArgumentException("Bottom needs to be less than top (bottom < top).");

      _left = left;
      _right = right;
      _bottom = bottom;
      _top = top;
      Update();
    }


    /// <summary>
    /// Updates the shape.
    /// </summary>
    protected abstract void Update();
    #endregion
  }
}
