// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRise.Geometry;


namespace DigitalRise.Physics
{
  public partial class RigidBody
  {
    /// <inheritdoc/>
    IGeometricObject IGeometricObject.Clone()
    {
      return Clone();
    }


    /// <summary>
    /// Creates a new <see cref="RigidBody"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="RigidBody"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="RigidBody"/> derived class and <see cref="CloneCore"/> to create a copy of the
    /// current instance. Classes that derive from <see cref="RigidBody"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public RigidBody Clone()
    {
      RigidBody clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a protected method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method, 
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone shape. A class derived from <see cref="RigidBody"/> does not implement 
    /// <see cref="CreateInstanceCore"/>."
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private RigidBody CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone rigid body. The derived class {0} does not implement CreateInstanceCore().",
          GetType());

        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="RigidBody"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="RigidBody"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="RigidBody"/> derived class must
    /// implement this method. A typical implementation is to simply call the default constructor
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual RigidBody CreateInstanceCore()
    {
      return new RigidBody();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="RigidBody"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="RigidBody"/> derived class must
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(RigidBody source)
    {
      // Material and mass properties
      Material = source.Material;
      MassFrame = source.MassFrame.Clone();
      AutoUpdateMass = source.AutoUpdateMass;
      LockRotationX = source.LockRotationX;
      LockRotationY = source.LockRotationY;
      LockRotationZ = source.LockRotationZ;

      // Geometric object properties
      Pose = source.Pose;
      Shape = source.Shape.Clone();
      Scale = source.Scale;

      // Other properties
      CollisionObject.CollisionGroup = source.CollisionObject.CollisionGroup;
      CollisionObject.Type = source.CollisionObject.Type;
      CollisionObject.Enabled = source.CollisionObject.Enabled;
      CollisionResponseEnabled = source.CollisionResponseEnabled;
      MotionType = source.MotionType;
      Name = source.Name;
      UserData = source.UserData;
      BuoyancyData = source.BuoyancyData;
      CcdEnabled = source.CcdEnabled;
      LinearVelocity = source.LinearVelocity;
      AngularVelocity = source.AngularVelocity;
      CanSleep = source.CanSleep;
    }
  }
}
