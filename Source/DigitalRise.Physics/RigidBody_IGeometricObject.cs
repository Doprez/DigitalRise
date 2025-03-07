// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using Microsoft.Xna.Framework;

namespace DigitalRise.Physics
{
  public partial class RigidBody
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the axis-aligned bounding box (AABB).
    /// </summary>
    /// <value>The axis-aligned bounding box (AABB).</value>
    public Aabb Aabb
    {
      get
      {
        if (!_aabbIsValid)
        {
          _aabb = Shape.GetAabb(Scale, Pose);
          _aabbIsValid = true;

          if (IsCcdActive)
          {
            // Set temporal AABB. 
            // This AABB encloses the start and end pose. Rotational effects can be missed. 
            // We could be more precise: Use AABB of bounding sphere. The radius is the minimum of 
            // bounding sphere radius and angularVelocity.Length * dt * bounding sphere radius (the 
            // later is the projected angular velocity of conservative advancement see FAST paper 
            // and others).
            // Note: We could shrink the temporal AABB by the allowedPenetration or a separate
            // CcdSlop value. This would be faster and creates good contacts.
            // Box2D uses CcdSlop = 8 * AllowedPenetration and AllowedPenetration = 5 mm.
            var targetAabb = Shape.GetAabb(Scale, TargetPose);
            _aabb.Grow(targetAabb);
          }
        }

        return _aabb;
      }
    }
    private Aabb _aabb;
    private bool _aabbIsValid;


    /// <summary>
    /// Gets or sets the pose (position and orientation).
    /// </summary>
    /// <value>The pose (position and orientation).</value>
    /// <remarks>
    /// Changing this property raises the <see cref="PoseChanged"/> event.
    /// </remarks>
    public Pose Pose
    {
      get { return _pose; }
      set
      {
        if (_pose != value)
        {
          _pose = value;
          OnPoseChanged();
        }
      }
    }
    private Pose _pose;


    /// <summary>
    /// Gets or sets the pose (position and orientation) of the center of mass.
    /// </summary>
    /// <value>The pose (position and orientation) of the center of mass.</value>
    /// <remarks>
    /// <para>
    /// This center of mass is relative to the origin of the local space of the body and is defined
    /// in the <see cref="MassFrame"/>. The property <see cref="PoseCenterOfMass"/> describes the
    /// pose of the center of mass in world space - not in local space. Changing this property
    /// automatically updates <see cref="Pose"/> and vice versa. Changing this property moves the 
    /// whole body in the world - it does not shift the center of mass inside body.
    /// </para>
    /// <para>
    /// Changing this property raises the <see cref="PoseChanged"/> event.
    /// </para>
    /// </remarks>
    public Pose PoseCenterOfMass
    {
      get
      {
        return _poseCenterOfMass;
      }
      set
      {
        if (_poseCenterOfMass != value)
        {
          _poseCenterOfMass = value;
          OnPoseCenterOfMassChanged();
        }
      }
    }
    private Pose _poseCenterOfMass;


    /// <summary>
    /// Gets or sets the target pose for CCD objects - used during motion clamping.
    /// </summary>
    /// <value>The target pose.</value>
    internal Pose TargetPose { get; set; }


    /// <summary>
    /// Gets or sets the shape.
    /// </summary>
    /// <value>
    /// The shape. The shape must not be <see langword="null"/>, but it can be of type
    /// <see cref="EmptyShape"/> (see <see cref="DigitalRise.Geometry.Shapes.Shape.Empty"/>. 
    /// The default value is a cube with side length 1.
    /// </value>
    /// <remarks>
    /// <para>
    /// Changing this property raises the <see cref="ShapeChanged"/> event.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> An <see cref="IGeometricObject"/> instance registers event 
    /// handlers for the <see cref="DigitalRise.Geometry.Shapes.Shape.Changed"/> event of the
    /// contained <see cref="Shape"/>. Therefore, a <see cref="DigitalRise.Geometry.Shapes.Shape"/> 
    /// will have an indirect reference to the <see cref="IGeometricObject"/>. This is no problem if
    /// the geometric object exclusively owns the shape. However, this could lead to problems ("life
    /// extension bugs" a.k.a. "memory leaks") when multiple geometric objects share the same shape:
    /// One geometric object is no longer used but it cannot be collected by the garbage collector
    /// because the shape still holds a reference to the object.
    /// </para>
    /// <para>
    /// Therefore, when <see cref="DigitalRise.Geometry.Shapes.Shape"/>s are shared between multiple 
    /// <see cref="IGeometricObject"/>s: Always set the property <see cref="Shape"/> to 
    /// <see cref="DigitalRise.Geometry.Shapes.Shape.Empty"/> when the <see cref="IGeometricObject"/> 
    /// is no longer used. <see cref="DigitalRise.Geometry.Shapes.Shape.Empty"/> is a special 
    /// immutable shape that never raises any <see cref="DigitalRise.Geometry.Shapes.Shape.Changed"/> 
    /// events. Setting <see cref="Shape"/> to <see cref="DigitalRise.Geometry.Shapes.Shape.Empty"/> 
    /// ensures that the internal event handlers are unregistered and the object can be 
    /// garbage-collected properly.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Shape Shape
    {
      get { return _shape; }
      set
      {
        if (_shape == value)
          return;

        if (value == null)
          throw new ArgumentNullException("value");

        if (_shape != null)
          _shape.Changed -= OnShapeChanged;

        _shape = value;
        _shape.Changed += OnShapeChanged;
        OnShapeChanged(ShapeChangedEventArgs.Empty);
      }
    }
    private Shape _shape;


    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    /// <value>
    /// The scale factors for the dimensions x, y and z. The default value is (1, 1, 1), which means
    /// "no scaling".
    /// </value>
    /// <remarks>
    /// <para>
    /// This value is a scale factor that scales the <see cref="Shape"/> of this geometric object.
    /// The scale can even be negative to mirror an object.
    /// </para>
    /// <para>
    /// Changing this value does not actually change any values in the <see cref="Shape"/> instance.
    /// Collision algorithms and anyone who uses this geometric object must use the shape and apply
    /// the scale factor as appropriate. The scale is automatically applied in the property
    /// <see cref="Aabb"/>.
    /// </para>
    /// <para>
    /// Changing this property raises the <see cref="ShapeChanged"/> event.
    /// </para>
    /// </remarks>
    public Vector3 Scale
    {
      get { return _scale; }
      set
      {
        if (_scale != value)
        {
          _scale = value;
          OnShapeChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3 _scale;


    /// <summary>
    /// Occurs when the pose was changed.
    /// </summary>
    public event EventHandler<EventArgs> PoseChanged;


    /// <summary>
    /// Occurs when the <see cref="Shape"/> or <see cref="Scale"/> was changed.
    /// </summary>
    public event EventHandler<ShapeChangedEventArgs> ShapeChanged;
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnPoseChanged()
    {
      _aabbIsValid = false;

      if (IsSleeping) // Only wake up if already sleeping. We do not want to reset the _noMovementTime.
        WakeUp();

      UpdatePoseCenterOfMass();
      UpdateInverseMass();

      OnPoseChanged(EventArgs.Empty);
    }


    private void OnPoseCenterOfMassChanged()
    {
      _aabbIsValid = false;

      if (IsSleeping) // Only wake up if already sleeping. We do not want to reset the _noMovementTime.
        WakeUp();

      UpdatePose();
      UpdateInverseMass();

      OnPoseChanged(EventArgs.Empty);
    }


    /// <summary>
    /// Raises the <see cref="PoseChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPoseChanged(EventArgs)"/> 
    /// in a derived class, be sure to call the base class's <see cref="OnPoseChanged(EventArgs)"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnPoseChanged(EventArgs eventArgs)
    {
      var handler = PoseChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Called when the <see cref="Shape"/> property has changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      OnShapeChanged(eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="ShapeChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="ShapeChangedEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> 
    /// in a derived class, be sure to call the base class's <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnShapeChanged(ShapeChangedEventArgs eventArgs)
    {
      // Reset shape dependent stuff.
      _aabbIsValid = false;
      BuoyancyData = null;

      WakeUp();

      UpdateMassFrame();

      var handler = ShapeChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Updates <see cref="PoseCenterOfMass"/> from <see cref="Pose"/>.
    /// </summary>
    private void UpdatePoseCenterOfMass()
    {
      _poseCenterOfMass = _pose * MassFrame.Pose;
    }


    /// <summary>
    /// Updates <see cref="Pose"/> from <see cref="PoseCenterOfMass"/>.
    /// </summary>
    private void UpdatePose()
    {
      _pose = _poseCenterOfMass * MassFrame.Pose.Inverse;
    }
    #endregion
  }
}
