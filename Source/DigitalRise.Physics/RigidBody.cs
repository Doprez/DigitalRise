// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Collisions;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Physics.Constraints;
using DigitalRise.Physics.ForceEffects;
using DigitalRise.Physics.Materials;
using DigitalRise.Physics.Settings;
using Microsoft.Xna.Framework;

namespace DigitalRise.Physics
{
  /// <summary>
  /// Represents a rigid body.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A rigid body is a body that is simulated by rigid body dynamics. The body has a 
  /// <see cref="Shape"/> that is constant for the simulation (although the shape can be exchanged
  /// by the user). No deformations are computed, therefore this body cannot model a soft body like
  /// cloth or fluids.
  /// </para>
  /// <para>
  /// The body also has a mass properties defined in <see cref="MassFrame"/> and a 
  /// <see cref="Material"/> that defines friction, bounciness and other material properties.
  /// </para>
  /// <para>
  /// <strong>Center Of Mass:</strong> The body has a <see cref="Pose"/> that defines its position
  /// and orientation in world space. The origin of the local space of the body can be different
  /// from its center of mass. The body has a <see cref="PoseCenterOfMass"/> that defines the pose
  /// of the center of mass in world space. The center of mass is automatically computed from the
  /// shape of the rigid body when the body's <see cref="MassFrame"/> is created. To move the rigid
  /// body to a new pose the properties <see cref="Pose"/> or <see cref="PoseCenterOfMass"/> can be 
  /// used; the two properties are synchronized automatically.
  /// </para>
  /// <para>
  /// <strong>Collision Detection:</strong> The rigid body class implements 
  /// <see cref="IGeometricObject"/> and it automatically creates a <see cref="CollisionObject"/>
  /// for the rigid body. This collision object is automatically added to the
  /// <see cref="Physics.Simulation.CollisionDomain"/> of the simulation.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> An <see cref="IGeometricObject"/> instance registers event
  /// handlers for the <see cref="Geometry.Shapes.Shape.Changed"/> event of the contained
  /// <see cref="Shape"/>. Therefore, a <see cref="Geometry.Shapes.Shape"/> will have an indirect
  /// reference to the <see cref="IGeometricObject"/>. This is no problem if the geometric object
  /// exclusively owns the shape. However, this could lead to problems ("life extension bugs" a.k.a.
  /// "memory leaks") when multiple geometric objects share the same shape: One geometric object is
  /// no longer used but it cannot be collected by the garbage collector because the shape still
  /// holds a reference to the object.
  /// </para>
  /// <para>
  /// Therefore, when <see cref="Geometry.Shapes.Shape"/>s are shared between multiple
  /// <see cref="IGeometricObject"/>s: Always set the property <see cref="Shape"/> to
  /// <see cref="DigitalRise.Geometry.Shapes.Shape.Empty"/> when the <see cref="IGeometricObject"/>
  /// is no longer used. <see cref="DigitalRise.Geometry.Shapes.Shape.Empty"/> is a special
  /// immutable shape that never raises any <see cref="Geometry.Shapes.Shape.Changed"/> events.
  /// Setting <see cref="Shape"/> to <see cref="Geometry.Shapes.Shape.Empty"/> ensures that the
  /// internal event handlers are unregistered and the object can be garbage-collected properly.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> The rigid body can be cloned using the <see cref="Clone"/> method.
  /// Cloning creates a deep copy of the object. The <see cref="UserData"/> and the
  /// <see cref="Material"/> are copied and not cloned, so the clone will refer to the
  /// same <see cref="UserData"/> and <see cref="Material"/> instances. All other properties are
  /// properly cloned.
  /// </para>
  /// <para>
  /// <strong>Sleeping:</strong> See also <see cref="SleepingSettings"/>. The rigid body 
  /// automatically wakes up if important properties are changed - for example, if the 
  /// <see cref="Shape"/> is changed, the body wakes up.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public partial class RigidBody : IGeometricObject, INamedObject
  {
    // All quantities are in world space unless noted otherwise.
    // Changing important properties (shape, mass) automatically wakes the body up.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the collision object.
    /// </summary>
    /// <value>The collision object.</value>
    /// <remarks>
    /// This collision object is automatically created and added to the 
    /// <see cref="Physics.Simulation.CollisionDomain"/> of the simulation when the rigid body is
    /// added to a simulation. 
    /// <see cref="CollisionObject"/>.<see cref="Geometry.Collisions.CollisionObject.GeometricObject"/> 
    /// refers to this rigid body instance.
    /// </remarks>
    public CollisionObject CollisionObject { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether collision response is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the collision response is enabled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// If collision response is disabled, no <see cref="ContactConstraint"/>s are created for this
    /// rigid body, and the simulation will not stop the body from moving through other rigid
    /// bodies. Disabling collision response only make sense if there is no <see cref="Gravity"/> in
    /// the simulated world or if the body is held in place by constraints or other forces. Without 
    /// constraints or forces the body will just fall through the floor and nothing will stop it.
    /// </remarks>
    public bool CollisionResponseEnabled { get; set; }


    /// <summary>
    /// Gets or sets the material.
    /// </summary>
    /// <value>
    /// The material. The default value is a new instance of <see cref="UniformMaterial"/>.
    /// </value>
    /// <remarks>
    /// The material defines the surface friction, bounciness and other properties. See
    /// <see cref="IMaterial"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public IMaterial Material
    {
      get { return _material; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _material = value;
      }
    }
    private IMaterial _material;


    /// <summary>
    /// Gets or sets the motion type.
    /// </summary>
    /// <value>The motion type. The default is <see cref="Physics.MotionType.Dynamic"/>.</value>
    public MotionType MotionType
    {
      get { return _motionType; }
      set 
      {
        if (_motionType != value)
        {
          _motionType = value;

          UpdateInverseMass();

          if (_motionType != MotionType.Static)
            WakeUp();
          else
            Sleep();  // Static bodies are sleeping bodies.
        }
      }
    }
    private MotionType _motionType;


    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name of this rigid body. The default is "Unnamed".</value>
    /// <remarks>
    /// This property can be used for debugging or other purposes. It is not used by the physics 
    /// simulation.
    /// </remarks>
    public string Name { get; set; }


    /// <summary>
    /// Gets the simulation.
    /// </summary>
    /// <value>The simulation.</value>
    /// <remarks>
    /// This property is set if this body is in a <see cref="Physics.Simulation.RigidBodies"/> 
    /// collection of a simulation. Otherwise it is <see langword="null"/>.
    /// </remarks>
    public Simulation Simulation
    {
      get { return _simulation; }
      internal set
      {
        if (_simulation != value)
        {
          if (_simulation != null)
            OnRemoveFromSimulation();

          _simulation = value;

          if (_simulation != null)
            OnAddToSimulation();
        }
      }
    }
    private Simulation _simulation;


    /// <summary>
    /// Gets or sets the user data.
    /// </summary>
    /// <value>The user data.</value>
    /// <remarks>
    /// <para>
    /// This property can store end-user data. This property is not used by the physics simulation.
    /// </para>
    /// </remarks>
    public object UserData { get; set; }


    /// <summary>
    /// Gets or sets the island ID.
    /// </summary>
    /// <value>The island ID.</value>
    /// <remarks>
    /// Kinematic/static bodies have ID -1.
    /// </remarks>
    internal int IslandId { get; set; }


    /// <summary>
    /// Gets or sets the buoyancy data.
    /// </summary>
    /// <value>The buoyancy data.</value>
    /// <remarks>
    /// This is normally not set. The data is initialized when the body touches the water surface of
    /// a <see cref="Buoyancy"/> effect or when <see cref="Buoyancy.Prepare"/> is called. The data
    /// is invalidated when the shape of the rigid body is changed.
    /// </remarks>
    internal BuoyancyData BuoyancyData { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody"/> class.
    /// </summary>
    public RigidBody()
      : this(null, null, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody"/> class.
    /// </summary>
    /// <param name="shape">
    /// The shape. Can be <see langword="null"/> to use the default <see cref="Shape"/>.
    /// </param>
    public RigidBody(Shape shape)
      : this (shape, null, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody"/> class.
    /// </summary>
    /// <param name="shape">
    /// The shape. Can be <see langword="null"/> to use the default <see cref="Shape"/>.
    /// </param>
    /// <param name="massFrame">
    /// The mass frame. Can be <see langword="null"/> in which case the mass properties for a
    /// density of 1000 are used.
    /// </param>
    /// <param name="material">
    /// The material. Can be <see langword="null"/> to use the default <see cref="Material"/>.
    /// </param>
    public RigidBody(Shape shape, MassFrame massFrame, IMaterial material)
    {
      AutoUpdateMass = true;
      IslandId = -1;
      Name = "Unnamed";

      _pose = Pose.Identity;
      _shape = shape ?? new BoxShape(1, 1, 1);
      _shape.Changed += OnShapeChanged;
      _scale = Vector3.One;

      Material = material ?? new UniformMaterial();

      if (massFrame != null)
        MassFrame = massFrame;
      else
        UpdateMassFrame();

      CollisionResponseEnabled = true;
      MotionType = MotionType.Dynamic;
      
      CollisionObject = new CollisionObject(this);
      
      CanSleep = true;
      IsSleeping = false;

      TimeOfImpact = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when this rigid body is added to a simulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The simulation to which the rigid body is added is set in the property 
    /// <see cref="Simulation"/>.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnAddToSimulation"/> in a 
    /// derived class, be sure to call the base class's <see cref="OnAddToSimulation"/> method.
    /// </para>
    /// </remarks>
    protected virtual void OnAddToSimulation()
    {
      Simulation.CollisionDomain.CollisionObjects.Add(CollisionObject);
    }


    /// <summary>
    /// Called when this rigid body is removed from a simulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The simulation from which the rigid body is removed is set in the property 
    /// <see cref="Simulation"/>. After <see cref="OnRemoveFromSimulation"/> the property 
    /// <see cref="Simulation"/> will be reset to <see langword="null"/>.
    /// <para>
    /// </para>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnRemoveFromSimulation"/>
    /// in a derived class, be sure to call the base class's <see cref="OnRemoveFromSimulation"/>
    /// method.
    /// </para>
    /// </remarks>
    protected virtual void OnRemoveFromSimulation()
    {
      Simulation.CollisionDomain.CollisionObjects.Remove(CollisionObject);

      // Remove related contacts.
      for (int i = Simulation.ContactConstraintsInternal.Count - 1; i >= 0; i--)
      {
        var contact = Simulation.ContactConstraintsInternal[i];
        if (contact.BodyA == this || contact.BodyB == this)
        {
          contact.BodyA.WakeUp();
          contact.BodyB.WakeUp();
          Simulation.ContactConstraintsInternal.RemoveAt(i);
        }
      }

      // Remove related constraints.
      for (int i = Simulation.Constraints.Count - 1; i >= 0; i--)
      {
        var constraint = Simulation.Constraints[i];
        if (constraint.BodyA == this || constraint.BodyB == this)
          Simulation.Constraints.RemoveAt(i);
      }

      IslandId = -1;
    }
    #endregion
  }
}
