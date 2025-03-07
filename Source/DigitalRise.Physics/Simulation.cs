// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//#define STOPWATCH            // Use this define to activate time measurement for profiling.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DigitalRise.Collections;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Collisions;
using DigitalRise.Geometry.Partitioning;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Physics.Constraints;
using DigitalRise.Physics.ForceEffects;
using DigitalRise.Physics.Settings;
using DigitalRise.Threading;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

#if STOPWATCH
using System.Diagnostics;
#endif


namespace DigitalRise.Physics
{
#if STOPWATCH
  /// <summary>
  /// Stores measured execution times.
  /// </summary>
  public class SimulationDiagnostics
  {
    public float TimeUpdateContacts { get; set; }
    public float TimeEvaluateForces { get; set; }
    public float TimeUpdateVelocity { get; set; }
    public float TimeIslandManager { get; set; }
    public float TimeConstraintSolver { get; set; }
    public float TimeUpdatePose { get; set; }
    public float TimeCcd { get; set; }
    public float TimeCleanUp { get; set; }

    public float TimeTotal
    {
      get
      {
        return TimeUpdateContacts + TimeEvaluateForces + TimeUpdateVelocity + TimeIslandManager
               + TimeConstraintSolver + TimeUpdatePose + TimeCcd + TimeCleanUp;
      }
    }
  }
#endif


  /// <summary>
  /// Manages a physics simulation.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="Simulation"/> owns collections of <see cref="RigidBodies"/>, 
  /// <see cref="Constraints"/> and <see cref="ForceEffects"/>. All <see cref="RigidBody"/>, 
  /// <see cref="Constraint"/>, and <see cref="ForceEffect"/> objects need to be added to these
  /// collections. All objects in these collections take part in the simulation. Object that are not
  /// added to these collections are not simulated.
  /// </para>
  /// <para>
  /// <strong>Advancing the Simulation:</strong> To advance the simulation 
  /// <see cref="Update(TimeSpan)"/> must be called with the time span by which the simulation time 
  /// should advance. In <see cref="Update(TimeSpan)"/> the simulation computes forces and moves the 
  /// objects to new positions. One step of the simulation is called a "time step". In some cases 
  /// the simulation will internally subdivide a time step into "sub time steps" or "internal time 
  /// steps". In most games the <see cref="Update(TimeSpan)"/> will be called with 1/60 s (60 frames
  /// per second) and the simulation will make exactly one time step. See 
  /// <see cref="TimingSettings"/> for more information regarding timing.
  /// </para>
  /// <para>
  /// <strong>Collision Detection:</strong> The <see cref="Simulation"/> owns a 
  /// <see cref="CollisionDomain"/>. Each <see cref="RigidBody"/> has a 
  /// <see cref="RigidBody.CollisionObject"/> that represents the collision information of the body
  /// and is put into the collision domain. You are free to use the <see cref="CollisionDomain"/> to
  /// perform collision queries (e.g. 
  /// <see cref="Geometry.Collisions.CollisionDomain.GetContacts(CollisionObject)"/>). You can also
  /// add custom <see cref="CollisionObject"/>s to the collision domain. For example, you can add a
  /// collision object to check if rigid bodies or other collision objects enter a certain area. 
  /// </para>
  /// <para>
  /// The collision filter of the 
  /// <see cref="Geometry.Collisions.CollisionDomain.CollisionDetection"/> is set to an instance of
  /// type <see cref="CollisionFilter"/>. The filter rules in the <see cref="CollisionFilter"/> can
  /// freely be changed as required by the application. The whole filter can be replaced too, but
  /// the new filter should implement the interface <see cref="ICollisionFilter"/>. If the new 
  /// collision filter does not implement <see cref="ICollisionFilter"/>, then automatic collision
  /// filtering for constraints does not work (see property <see cref="Constraint.CollisionEnabled"/>).
  /// Advanced physics modules like ragdoll physics and vehicle physics might also need a collision
  /// filter that implements <see cref="ICollisionFilter"/>.
  /// </para>
  /// <para>
  /// The <see cref="Contact.UserData"/> of <see cref="Contact"/>s in this collision domain are used
  /// to store references to <see cref="ContactConstraint"/>s. That means, <see cref="Contact"/>.
  /// <see cref="Contact.UserData"/> must not be changed.
  /// </para>
  /// <para>
  /// <strong>The "World" Rigid Body:</strong> The simulation owns one special rigid body that 
  /// represents the "world" of the simulation: <see cref="World"/>. This rigid body is not 
  /// contained in the <see cref="RigidBodies"/> collection and other bodies do not collide with 
  /// this body. This body is only used to define the space in which the simulation takes place.
  /// Rigid bodies that leave the space of this rigid body are automatically removed from the 
  /// simulation. Per default the <see cref="Shape"/> of the <see cref="World"/> body is a box that
  /// is 20,000 units long and centered at the world space origin. - If object leave this area, they
  /// should be removed. A typical scenario is that an explosion shoots objects into nirvana. When
  /// they leave the 20,000 units area, they are removed to safe simulation time. You can adjust the
  /// <see cref="RigidBody.Shape"/> of the world body to the size of the level or the "area of 
  /// interest".
  /// </para>
  /// <para>
  /// The second function of the <see cref="World"/> body is to act as an anchor for 
  /// <see cref="Constraint"/>s. All constraints are two-body constraints and, for example, if you
  /// want to fix a rigid body at a certain position in world space you create a constraint between
  /// the rigid body and the world body as the other body.
  /// </para>
  /// </remarks>
  public partial class Simulation
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Represents the simulation "world".
    /// </summary>
    /// <remarks>
    /// <para>
    /// This abstract rigid bodies represents the simulation world. Rigid bodies that do not touch
    /// this body anymore are removed from the simulation. This body can also be used if a rigid
    /// body should be attached with a constraint to a fixed position in the "air". In such cases 
    /// <see cref="World"/> can be used to represent the empty space. The local space of 
    /// <see cref="World"/> is equal to the world space.
    /// </para>
    /// <para>
    /// The default value is a <see cref="MotionType.Static"/> rigid body with a 
    /// <see cref="BoxShape"/> (20,000 units side length). This body is not part of the 
    /// <see cref="RigidBodies"/> collection.
    /// </para>
    /// <para>
    /// In general, the <see cref="RigidBody.Shape"/> of this body can be changed. Other properties
    /// should not be altered. See <see cref="Simulation"/> for more information.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public readonly RigidBody World;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Some lists that will be re-used each frame. Cached to avoid garbage.
    private readonly List<Vector3> _tempForceList = new List<Vector3>();
    //private readonly List<Vector3> _tempTorqueList = new List<Vector3>();
    private readonly List<ForceEffect> _tempForceEffects = new List<ForceEffect>();

    // The size of the current sub-time step. (Only valid during Update(). Used to avoid garbage
    // when multithreading.)
    private float _fixedTimeStep;

    // Delegates stored to avoid garbage when multithreading is enabled.
    private readonly Action<int> _updateVelocityMethod;
    private readonly Action<int> _solveIslandMethod;
    private readonly Action<int> _updatePoseMethod;
    private readonly Action<int> _computeTimeOfImpactMethod;
    private readonly Action<int> _moveToTimeOfImpactMethod;

#if STOPWATCH
    private readonly Stopwatch _stopwatch = new Stopwatch();
#endif
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the collision domain.
    /// </summary>
    /// <value>The collision domain.</value>
    /// <remarks>
    /// <para>
    /// This collision domain computes collisions for the rigid bodies.
    /// </para>
    /// <para>
    /// This collision domain can be used to make collision queries (e.g. 
    /// <see cref="Geometry.Collisions.CollisionDomain.GetContacts(CollisionObject)"/>) and it is
    /// allowed to add custom <see cref="CollisionObject"/>s to this collision domain.
    /// </para>
    /// <para>
    /// The collision domain is automatically updated by the simulation. (See also 
    /// <see cref="SimulationSettings.SynchronizeCollisionDomain"/>.)
    /// </para>
    /// </remarks>
    public CollisionDomain CollisionDomain { get; private set; }


#if STOPWATCH
    public SimulationDiagnostics Diagnostics { get; private set; }
#endif


    /// <summary>
    /// Gets the contact constraints.
    /// </summary>
    /// <value>The contact constraints.</value>
    /// <remarks>
    /// For all bodies that have contact and where collision response is enabled, the simulation
    /// creates a <see cref="ContactConstraint"/> for each <see cref="Contact"/>. This list is
    /// updated in each time step. The <see cref="ContactConstraint"/> instances in this collection
    /// should not be changed, but properties of the <see cref="ContactConstraints"/> can be read.
    /// For example, <see cref="ContactConstraint.LinearConstraintImpulse"/> can used to check which
    /// impulse was applied at this contact. This is useful if you want to "destroy" an object after
    /// a severe collision.
    /// </remarks>
    public ReadOnlyCollection<ContactConstraint> ContactConstraints
    {
      get
      {
        if (_contactConstraints == null)
          _contactConstraints = new ReadOnlyCollection<ContactConstraint>(ContactConstraintsInternal);

        return _contactConstraints;
      }
    }
    private ReadOnlyCollection<ContactConstraint> _contactConstraints;


    /// <summary>
    /// Gets the contact constraints. (For internal use only.)
    /// </summary>
    /// <value>The contact constraints.</value>
    internal List<ContactConstraint> ContactConstraintsInternal; //{ get; private set; }


    /// <summary>
    /// Gets the constraints.
    /// </summary>
    /// <value>The constraints.</value>
    public ConstraintCollection Constraints { get; private set; }


    /// <summary>
    /// Gets or sets the constraint solver.
    /// </summary>
    /// <value>The constraint solver.</value>
    private ConstraintSolver ConstraintSolver { get; set; }


    /// <summary>
    /// Gets the force effects that act on the rigid bodies.
    /// </summary>
    /// <value>The force effects.</value>
    public ForceEffectCollection ForceEffects { get; private set; }


    /// <summary>
    /// Gets the rigid bodies.
    /// </summary>
    /// <value>The rigid bodies.</value>
    public RigidBodyCollection RigidBodies { get; private set; }


    /// <summary>
    /// Gets or sets the collision response filter.
    /// </summary>
    /// <value>
    /// The collision response filter. The default is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// Per default, this property is <see langword="null"/> which means that collision response is
    /// globally enabled. A filter can be set if it is necessary to disable collision response for
    /// certain pairs of rigid bodies. The default filter implementation is 
    /// <see cref="CollisionResponseFilter"/>. The response filter should return 
    /// <see langword="true"/> if collision response is enabled.
    /// </remarks>
    public IPairFilter<RigidBody> ResponseFilter { get; set; }

    // If possible disable collisions, this is more efficient. 
    // Internal Example: FineSkills drill machine: Drill vs. Hand = normal contact response.
    // Drill vs. Werkstück = only collision no response, special handling.
    // Filter returns true if response enabled.


    /// <summary>
    /// Gets the current simulation time.
    /// </summary>
    /// <value>The current simulation time.</value>
    /// <remarks>
    /// <para>
    /// This value shows for which time the simulation state is valid. The simulation will never 
    /// backtrack before this time. Each <see cref="Update(TimeSpan)"/> call usually advances this 
    /// time.
    /// </para>
    /// <para>
    /// See also <see cref="TargetTime"/>.
    /// </para>
    /// </remarks>
    public TimeSpan Time { get; private set; }


    /// <summary>
    /// Gets the target time to which the simulation <see cref="Time"/> should advance.
    /// </summary>
    /// <value>The target simulation time.</value>
    /// <remarks>
    /// <see cref="TargetTime"/> is the time to which the simulation should advance. 
    /// <see cref="Time"/> is the time to which the simulation has actually advanced. 
    /// <seealso cref="TargetTime"/> is updated at the beginning of <see cref="Update(TimeSpan)"/>. 
    /// Then <see cref="Update(TimeSpan)"/> advances the simulation in fixed time steps ("sub time 
    /// steps"). Each step increases <see cref="Time"/>. <see cref="Update(TimeSpan)"/> ends when 
    /// the difference between <see cref="TargetTime"/> and <see cref="Time"/> is less than one 
    /// fixed time step size.
    /// </remarks>
    public TimeSpan TargetTime { get; private set; }


    //// The actual time step. This is usually equal to Settings.FixedTimeStep.
    //public float TimeStep { get; set; }


    /// <summary>
    /// Gets or sets the time scaling.
    /// </summary>
    /// <value>The time scaling. The default value is <c>1.0</c>.</value>
    /// <remarks>
    /// <para>
    /// This is a factor which speeds up or slows down the simulation. Use a value less than 1 to
    /// create slow motion effects.
    /// </para>
    /// <para>
    /// The fixed time step size (<see cref="TimingSettings.FixedTimeStep"/>) is not automatically
    /// scaled to keep the simulation results consistent. That means, if the time scaling is set to
    /// 2, the simulation makes twice as much steps as usual when <see cref="Update(TimeSpan)"/> is 
    /// called. If the time scaling is set to 0.1 to create a slow motion effect, the simulation 
    /// performs only 1 / 10 of the usual simulation steps. - This standard behavior keeps the 
    /// simulation accuracy constant. 
    /// </para>
    /// <para>
    /// If constant accuracy is not required, you might want to scale
    /// <see cref="TimingSettings.FixedTimeStep"/> manually with this scaling factor. Then the 
    /// simulation accuracy will change with the time scaling factor. Especially, when a time 
    /// scaling less than 1 ("slow motion") is used, it might be necessary to make the fixed time
    /// step smaller to keep the simulation results fluid.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float TimeScaling
    {
      get { return _timeScaling; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "TimeScaling must be greater than or equal to 0.");

        _timeScaling = value;
      }
    }
    private float _timeScaling = 1;


    /// <summary>
    /// Gets or sets the simulation settings.
    /// </summary>
    /// <value>The simulation settings.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public SimulationSettings Settings
    {
      get { return _settings; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _settings = value;
      }
    }
    private SimulationSettings _settings;


    /// <summary>
    /// Occurs when an internal time step has finished.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Depending on the size of the current time step the simulation performs a number of sub-steps 
    /// in <see cref="Update(TimeSpan)"/>. This event can be used to execute code between sub-steps.
    /// However, the simulations objects or settings should not be modified between sub-steps. The 
    /// whole simulation should be treated as read-only. The event handler should be only used to 
    /// monitor the simulation.
    /// </para>
    /// </remarks>
    /// <example>
    /// An event handler could be used to register collisions that happen between full time steps. 
    /// (Some collision only appear in sub time steps - they are not visible before or after 
    /// <see cref="Update(TimeSpan)"/>. For example, a rigid body might collide with the floor in 
    /// one sub time step. The collision might be resolved in the next sub time step: The rigid body 
    /// bounces off the floor. The event handler could check for contacts that appear between sub 
    /// time steps and, for example, play a sound effect. To check for collisions check the 
    /// <see cref="Geometry.Collisions.CollisionDomain.ContactSets"/> in the 
    /// <see cref="CollisionDomain"/>.)
    /// </example>
    public event EventHandler SubTimeStepFinished;


    // This flag will be set by rigid bodies that need CCD.
    internal bool CcdRequested { get; set; }


    /// <summary>
    /// Gets the <see cref="SimulationIslandManager"/>.
    /// </summary>
    /// <value>The <see cref="SimulationIslandManager"/>.</value>
    public SimulationIslandManager IslandManager { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Simulation"/> class.
    /// </summary>
    public Simulation()
    {
      Settings = new SimulationSettings();

      var collisionDetection = new CollisionDetection
      {
        CollisionFilter = new CollisionFilter(),
        FullContactSetPerFrame = true
      };
      CollisionDomain = new CollisionDomain(collisionDetection);

      ConstraintSolver = new SequentialImpulseBasedSolver(this);

      Constraints = new ConstraintCollection();
      Constraints.CollectionChanged += OnConstraintsChanged;

      ForceEffects = new ForceEffectCollection();
      ForceEffects.CollectionChanged += OnForceEffectsChanged;

      RigidBodies = new RigidBodyCollection();
      RigidBodies.CollectionChanged += OnRigidBodiesChanged;

      ContactConstraintsInternal = new List<ContactConstraint>();

      IslandManager = new SimulationIslandManager(this);

      // Define the "World" as a rigid body.
      //   - World is an imaginary body that is used to define the space of the simulation.
      //   - The user can use World in constraints e.g. to anchor objects in the world.
      //   - No contacts are computed for World.
      World = new RigidBody(new BoxShape(20000, 20000, 20000))
      {
        CollisionResponseEnabled = false,
        CollisionObject = { Type = CollisionObjectType.Trigger },
        MotionType = MotionType.Static,
        Name = "World",
        Simulation = this,
      };

      // Remove "World" from the collision domain. 
      CollisionDomain.CollisionObjects.Remove(World.CollisionObject);

#if STOPWATCH
      Diagnostics = new SimulationDiagnostics();
#endif

      // Store delegate methods to avoid garbage when multithreading.
      _updateVelocityMethod = i =>
      {
        var body = RigidBodies[i];
        body.UpdateVelocity(_fixedTimeStep);
      };
      _solveIslandMethod = SolveIsland;
      _updatePoseMethod = i =>
      {
        var body = RigidBodies[i];
        body.UpdatePose(_fixedTimeStep);
      };
      _computeTimeOfImpactMethod = ComputeTimeOfImpact_Multithreaded;
      _moveToTimeOfImpactMethod = MoveToTimeOfImpact;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the force effect collection was changed.
    /// </summary>
    private void OnForceEffectsChanged(object sender, CollectionChangedEventArgs<ForceEffect> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      int numberOfOldItems = eventArgs.OldItems.Count;
      for (int i = 0; i < numberOfOldItems; i++)
      {
        var oldEffect = eventArgs.OldItems[i];
        oldEffect.Simulation = null;
      }

      int numberOfNewItems = eventArgs.NewItems.Count;
      for (int i = 0; i < numberOfNewItems; i++)
      {
        var newEffect = eventArgs.NewItems[i];
        newEffect.Simulation = this;
      }
    }


    /// <summary>
    /// Called when the rigid body collection was changed.
    /// </summary>
    private void OnRigidBodiesChanged(object sender, CollectionChangedEventArgs<RigidBody> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      int numberOfOldItems = eventArgs.OldItems.Count;
      for (int i = 0; i < numberOfOldItems; i++)
      {
        var oldBody = eventArgs.OldItems[i];
        oldBody.Simulation = null;
      }

      int numberOfNewItems = eventArgs.NewItems.Count;
      for (int i = 0; i < numberOfNewItems; i++)
      {
        var newBody = eventArgs.NewItems[i];
        if (newBody != World)
          newBody.Simulation = this;
      }
    }


    /// <summary>
    /// Called when the constraint collection was changed.
    /// </summary>
    private void OnConstraintsChanged(object sender, CollectionChangedEventArgs<Constraint> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      int numberOfOldItems = eventArgs.OldItems.Count;
      for (int i = 0; i < numberOfOldItems; i++)
      {
        var oldConstraint = eventArgs.OldItems[i];
        oldConstraint.Simulation = null;
      }

      int numberOfNewItems = eventArgs.NewItems.Count;
      for (int i = 0; i < numberOfNewItems; i++)
      {
        var newConstraint = eventArgs.NewItems[i];
        newConstraint.Simulation = this;
      }
    }


    /// <summary>
    /// Gets the size of the next simulation time step.
    /// </summary>
    /// <param name="deltaTime">The current time step size.</param>
    /// <param name="totalTimeStepSize">Total size of the next time step.</param>
    /// <param name="numberOfSubTimeSteps">The number of sub time steps.</param>
    /// <remarks>
    /// This method computes how much the next <see cref="Update(System.TimeSpan)"/> call will
    /// advance the simulation <see cref="Time"/> and returns the result in 
    /// <paramref name="totalTimeStepSize"/>. <paramref name="numberOfSubTimeSteps"/> returns
    /// the number of internal time steps which will be performed. Since the simulation is 
    /// always updated in fixed time steps, <paramref name="totalTimeStepSize"/> is
    /// equal to <paramref name="numberOfSubTimeSteps"/> * <see cref="TimingSettings.FixedTimeStep"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public void GetNextTimeStep(TimeSpan deltaTime, out TimeSpan totalTimeStepSize, out int numberOfSubTimeSteps)
    {
      // This method does the same time operations as the Update() method to get exactly the 
      // same result.

      totalTimeStepSize = TimeSpan.Zero;
      numberOfSubTimeSteps = 0;

      if (deltaTime == TimeSpan.Zero)
        return;

      // Apply speedup factor.
      deltaTime = new TimeSpan((long)(deltaTime.Ticks * TimeScaling));

      TimeSpan fixedTimeStep = new TimeSpan((long)(Settings.Timing.FixedTimeStep * TimeSpan.TicksPerSecond));

      // Negative time steps are not allowed. MaxNumberOfSteps limits the max allowed time step. 
      // If deltaTime is larger, then some time is lost.
      TimeSpan minTimeStep = TimeSpan.Zero;
      TimeSpan maxTimeStep = new TimeSpan(fixedTimeStep.Ticks * Settings.Timing.MaxNumberOfSteps);
      deltaTime = MathHelper.Clamp(deltaTime, minTimeStep, maxTimeStep);

      TimeSpan targetTime = TargetTime + deltaTime;
      TimeSpan time = Time;

      // Loop until target time is reached or the difference to target time is less than 
      // the time step size.
      while (targetTime - time >= fixedTimeStep)
      {
        totalTimeStepSize += fixedTimeStep;
        numberOfSubTimeSteps++;
        time += fixedTimeStep;
      }
    }

    
    /// <overloads>
    /// <summary>
    /// Advances the simulation by the given time.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Advances the simulation by the given time.
    /// </summary>
    /// <param name="deltaTime">The size of the time step.</param>
    /// <remarks>
    /// See <see cref="Simulation"/> for more information.
    /// </remarks>
    public void Update(TimeSpan deltaTime)
    {
      if (deltaTime == TimeSpan.Zero)
        return;

      // Apply speedup factor.
      deltaTime = new TimeSpan((long)(deltaTime.Ticks * TimeScaling));

      _fixedTimeStep = Settings.Timing.FixedTimeStep;
      TimeSpan fixedTimeStep = new TimeSpan((long)(_fixedTimeStep * TimeSpan.TicksPerSecond));

      // Negative time steps are not allowed. MaxNumberOfSteps limits the max allowed time step. 
      // If deltaTime is larger, then some time is lost.
      TimeSpan minTimeStep = TimeSpan.Zero;
      TimeSpan maxTimeStep = new TimeSpan(fixedTimeStep.Ticks * Settings.Timing.MaxNumberOfSteps);
      deltaTime = MathHelper.Clamp(deltaTime, minTimeStep, maxTimeStep);

      // Update target time.
      TargetTime += deltaTime;

      //if (Settings.Timing.UseFixedTimeStep)
      //  TimeStep = Settings.Timing.FixedTimeStep;
      //else
      //  TimeStep = deltaTime;

      // Loop until target time is reached or the difference to target time is less than 
      // the time step size.
      while (TargetTime - Time >= fixedTimeStep)
      {
        if (Settings.EnableMultithreading)
        {
          SubTimeStep_Multithreaded(fixedTimeStep);
        }
        else
        {
          SubTimeStep_Singlethreaded(fixedTimeStep);
        }
      }

      if (Settings.SynchronizeCollisionDomain)
      {
        // Update collision domain so that user sees new contacts. But don't recycle
        // the old contact sets because they are still referenced by contact constraints.
        CollisionDomain.Update(0, false);
      }

      // Reset user forces. 
      {
        int numberOfRigidBodies = RigidBodies.Count;
        for (int i = 0; i < numberOfRigidBodies; i++)
        {
          RigidBody body = RigidBodies[i];
          body.ClearForces();
        }
      }
    }


    /// <summary>
    /// Advances the simulation by the given time.
    /// </summary>
    /// <param name="deltaTime">The size of the time step in seconds.</param>
    /// <remarks>
    /// See <see cref="Simulation"/> for more information.
    /// </remarks>
    public void Update(float deltaTime)
    {
      Update(new TimeSpan((long)(deltaTime * TimeSpan.TicksPerSecond)));
    }


    private void SubTimeStep_Singlethreaded(TimeSpan fixedTimeStep)
    {
#if STOPWATCH
      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Detect collisions and create contact constraints.
      UpdateContacts();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeUpdateContacts = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Compute forces and torques.
      EvaluateForces();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeEvaluateForces = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Update velocities.
      int numberOfRigidBodies = RigidBodies.Count;
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        var body = RigidBodies[i];
        body.UpdateVelocity(_fixedTimeStep);
      }
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeUpdateVelocity = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Find/Update simulation islands.
      IslandManager.Update();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeIslandManager = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      int numberOfIslands = IslandManager.IslandsInternal.Count;
      for (int i = 0; i < numberOfIslands; i++)
      {
        SolveIsland(i);
      }
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeConstraintSolver = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Update positions.
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        var body = RigidBodies[i];
        body.UpdatePose(_fixedTimeStep);
      }
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeUpdatePose = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // CCD motion clamping.
      DoCcdMotionClamping_Singlethreaded();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeCcd = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Remove bodies that have left the world AABB.
      RemoveBodiesOutsideWorld();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeCleanUp = (float)_stopwatch.Elapsed.TotalSeconds;
#endif

      // Advance simulation time.
      Time += fixedTimeStep;

      // Raise SubTimeStepFinished event.
      OnSubTimeStepFinished(EventArgs.Empty);
    }


    private void SubTimeStep_Multithreaded(TimeSpan fixedTimeStep)
    {
#if STOPWATCH
      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      UpdateContacts();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeUpdateContacts = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Compute forces and torques.
      EvaluateForces();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeEvaluateForces = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Update velocities.
      int numberOfRigidBodies = RigidBodies.Count;
      Parallel.For(0, numberOfRigidBodies, _updateVelocityMethod);
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeUpdateVelocity = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Find/Update simulation islands.
      IslandManager.Update();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeIslandManager = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      Parallel.For(0, IslandManager.IslandsInternal.Count, _solveIslandMethod);
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeConstraintSolver = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Update positions.
      Parallel.For(0, numberOfRigidBodies, _updatePoseMethod);
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeUpdatePose = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // CCD motion clamping.
      DoCcdMotionClamping_Multithreaded();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeCcd = (float)_stopwatch.Elapsed.TotalSeconds;

      _stopwatch.Reset();
      _stopwatch.Start();
#endif
      // Remove bodies that have left the world AABB.
      RemoveBodiesOutsideWorld();
#if STOPWATCH
      _stopwatch.Stop();
      Diagnostics.TimeCleanUp = (float)_stopwatch.Elapsed.TotalSeconds;
#endif

      // Advance simulation time.
      Time += fixedTimeStep;

      // Raise SubTimeStepFinished event.
      OnSubTimeStepFinished(EventArgs.Empty);
    }


    private void UpdateContacts()
    {
      CollisionDomain.Update(_fixedTimeStep);

      IslandManager.ContactSetLinks.Clear();

      // Go through contacts and add contact constraints.
      foreach (ContactSet contactSet in CollisionDomain.ContactSets)
      {
        RigidBody bodyA = contactSet.ObjectA.GeometricObject as RigidBody;
        RigidBody bodyB = contactSet.ObjectB.GeometricObject as RigidBody;
        if (bodyA != null && bodyB != null)
        {
          // Check if a dynamic body is involved and if collision response is enabled.
          bool responseEnabled = (bodyA.MotionType == MotionType.Dynamic || bodyB.MotionType == MotionType.Dynamic)
                                 && bodyA.CollisionResponseEnabled
                                 && bodyB.CollisionResponseEnabled
                                 && (ResponseFilter == null || ResponseFilter.Filter(new Pair<RigidBody>(bodyA, bodyB)));

          if (responseEnabled)
            IslandManager.ContactSetLinks.Add(new Pair<RigidBody>(bodyA, bodyB));

          int numberOfContacts = contactSet.Count;
          for (int i = 0; i < numberOfContacts; i++)
          {
            var contact = contactSet[i];
            ContactConstraint constraint = contact.UserData as ContactConstraint;

            if (constraint != null)
            {
              if (responseEnabled)
              {
                // Contact constraint is still in use. 
                // --> Mark contact constraint as active.
                constraint.Used = true;
              }
              else
              {
                // The response was disabled.
                // --> Remove an old constact constraint. 
                contact.UserData = null;
              }
            }
            else if (responseEnabled)
            {
              // Create a new contact constraint.
              constraint = ContactConstraint.Create(bodyA, bodyB, contact);
              contact.UserData = constraint;
              ContactConstraintsInternal.Add(constraint);
              constraint.Used = true;
            }
          }
        }
      }

      // ----- Recycle old contact constraints.
      int numberOfConstraints = ContactConstraintsInternal.Count;
      int numberOfUsedConstraints = numberOfConstraints;
      for (int i = numberOfConstraints - 1; i >= 0; i--)
      {
        var constraint = ContactConstraintsInternal[i];
        if (constraint.Used)
        {
          // The contact constraint is still in use.
          // Keep constraint and reset flag.
          constraint.Used = false;
        }
        else
        {
          numberOfUsedConstraints--;

          // Recycle contact constraint.
          constraint.Recycle();

          // The contact constraint is no longer in use.
          // Swap a used constraint to this index.
          ContactConstraintsInternal[i] = ContactConstraintsInternal[numberOfUsedConstraints];
          // Not needed because we call List.RemoveRange for the end of the list.
          //ContactConstraintsInternal[numberOfUsedConstraints] = constraint;
        }
      }

      // Remove recycled contacts at end of list.
      int numberOfUnusedConstraints = numberOfConstraints - numberOfUsedConstraints;
      if (numberOfUnusedConstraints > 0)
      {
        ContactConstraintsInternal.RemoveRange(numberOfUsedConstraints, numberOfUnusedConstraints);
      }
    }


    private void EvaluateForces()
    {
      // Store the old forces in this list.
      _tempForceList.Clear();
      //_tempTorqueList.Clear();

      // Set accumulated force/torque to force of user.
      int numberOfRigidBodies = RigidBodies.Count;
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        RigidBody body = RigidBodies[i];
        _tempForceList.Add(body.AccumulatedForce);
        //_tempTorqueList.Add(body.AccumulatedTorque);

        body.AccumulatedForce = body.UserForce;
        body.AccumulatedTorque = body.UserTorque;
      }

      // ----- Apply force effects.
      try
      {
        // Copy force effects into a temporary list, so that force effects can remove themselves 
        // from the force effects collection if they want to.
        foreach (var forceEffect in ForceEffects)
          _tempForceEffects.Add(forceEffect);

        int numberOfForceEffects = _tempForceEffects.Count;
        for (int i = 0; i < numberOfForceEffects; i++)
        {
          ForceEffect force = _tempForceEffects[i];
          force.Apply();
        }
      }
      finally
      {
        _tempForceEffects.Clear();
      }

      // Wake up bodies when there is a significant force change.
      // TODO: Torque changes do not yet wake up bodies!
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        RigidBody body = RigidBodies[i];

        // Wake body up if the force change would cause an acceleration equal to the sleep velocity
        // threshold.
        if ((_tempForceList[i] - body.AccumulatedForce).LengthSquared() > Settings.Sleeping.LinearVelocityThresholdSquared * body.MassFrame.Mass * body.MassFrame.Mass)
        {
          body.WakeUp();
        }
      }
    }


    private void SolveIsland(int i)
    {
      var island = IslandManager.IslandsInternal[i];
      if (!island.IsSleeping())
      {
        // Constraint handling.
        ConstraintSolver.Solve(island, _fixedTimeStep);
      }
    }


    private void DoCcdMotionClamping_Singlethreaded()
    {
      if (CcdRequested)
      {
        // (This method uses the InternalBroadPhase. Customers cannot use this.)

        // Update broad phase. 
        CollisionDomain.InternalBroadPhase.Update();

        // Compute final position of all CCD bodies. 
        foreach (var contactSet in CollisionDomain.InternalBroadPhase.CandidatePairs)
          ComputeTimeOfImpact_Singlethreaded(contactSet);

        // Set bodies to their time of impact.
        int numberOfRigidBodies = RigidBodies.Count;
        for (int i = 0; i < numberOfRigidBodies; i++)
          MoveToTimeOfImpact(i);

        CcdRequested = false;
      }
    }


    private void DoCcdMotionClamping_Multithreaded()
    {
      if (CcdRequested)
      {
        CollisionDomain.InternalBroadPhase.Update();
        Parallel.For(0, CollisionDomain.InternalBroadPhase.CandidatePairs.InternalCount, _computeTimeOfImpactMethod);
        Parallel.For(0, RigidBodies.Count, _moveToTimeOfImpactMethod);

        CcdRequested = false;
      }
    }


    private void ComputeTimeOfImpact_Singlethreaded(ContactSet contactSet)
    {
      RigidBody bodyA = contactSet.ObjectA.GeometricObject as RigidBody;
      RigidBody bodyB = contactSet.ObjectB.GeometricObject as RigidBody;

      // Note: We do CCD only between rigid bodies. 
      // TODO: Support CCD between RigidBody and general CollisionObject of the user.
      if (bodyA != null && bodyB != null)
      {
        // Check if at least one object needs CCD.
        if (bodyA.IsCcdActive || bodyB.IsCcdActive)
        {
          // Check CCD filter. (We check this filter first because it hopefully filters out a lot
          // of unnecessary checks (e.g. body against debris).)
          Func<RigidBody, RigidBody, bool> ccdFilter = Settings.Motion.CcdFilter;
          if (ccdFilter != null && !ccdFilter(bodyA, bodyB))
            return;

          // Check collision filter. (Using the internal method CanCollide that uses a cache.)
          if (!CollisionDomain.CanCollide(contactSet))
            return;

          // If convex bodies are touching we do not need to compute TOI because they are either
          // moving more toward each other (TOI = 0) or separating (TOI = 1). If they are moving
          // toward each other, then the contact constraint has failed and CCD is not the problem.
          // For objects vs. concave objects we still have to make a check because the bullet
          // could be separating and colliding with another part of the concave object. 
          // To avoid that objects stick: GetTimeOfImpact() should not return 0 for separating 
          // objects and we use 2 * AllowedPenetration so that the current contact does not 
          // count.

          if (!(bodyA.Shape is ConvexShape)
              || !(bodyB.Shape is ConvexShape)
              || !CollisionDomain.HaveContact(contactSet.ObjectA, contactSet.ObjectB))
          {
            // Get time of impact.
            float timeOfImpact = CollisionDomain.CollisionDetection.GetTimeOfImpact(
              contactSet.ObjectA,
              bodyA.IsCcdActive ? bodyA.TargetPose : bodyA.Pose,
              contactSet.ObjectB,
              bodyB.IsCcdActive ? bodyB.TargetPose : bodyB.Pose,
              Settings.Constraints.AllowedPenetration * 2f);

            // Store minimal time of impact.
            if (timeOfImpact < bodyA.TimeOfImpact)
              bodyA.TimeOfImpact = timeOfImpact;

            if (timeOfImpact < bodyB.TimeOfImpact)
              bodyB.TimeOfImpact = timeOfImpact;
          }
        }
      }
    }


    private void ComputeTimeOfImpact_Multithreaded(int i)
    {
      var contactSet = CollisionDomain.InternalBroadPhase.CandidatePairs[i];
      if (contactSet == null)
        return;

      RigidBody bodyA = contactSet.ObjectA.GeometricObject as RigidBody;
      RigidBody bodyB = contactSet.ObjectB.GeometricObject as RigidBody;

      if (bodyA != null && bodyB != null)
      {
        if (bodyA.IsCcdActive || bodyB.IsCcdActive)
        {
          Func<RigidBody, RigidBody, bool> ccdFilter = Settings.Motion.CcdFilter;
          if (ccdFilter != null && !ccdFilter(bodyA, bodyB))
            return;

          if (!CollisionDomain.CanCollide(contactSet))
            return;

          if (!(bodyA.Shape is ConvexShape)
              || !(bodyB.Shape is ConvexShape)
              || !CollisionDomain.HaveContact(contactSet.ObjectA, contactSet.ObjectB))
          {
            float timeOfImpact = CollisionDomain.CollisionDetection.GetTimeOfImpact(
              contactSet.ObjectA,
              bodyA.IsCcdActive ? bodyA.TargetPose : bodyA.Pose,
              contactSet.ObjectB,
              bodyB.IsCcdActive ? bodyB.TargetPose : bodyB.Pose,
              Settings.Constraints.AllowedPenetration * 2f);

            float initialToi;
            do
            {
              initialToi = bodyA.TimeOfImpact;
            } while (timeOfImpact < initialToi && Interlocked.CompareExchange(ref bodyA.TimeOfImpact, timeOfImpact, initialToi) != initialToi);

            do
            {
              initialToi = bodyB.TimeOfImpact;
            } while (timeOfImpact < initialToi && Interlocked.CompareExchange(ref bodyB.TimeOfImpact, timeOfImpact, initialToi) != initialToi);
          }
        }
      }
    }


    private void MoveToTimeOfImpact(int i)
    {
      var body = RigidBodies[i];
      if (body.IsCcdActive)
      {
        if (body.TimeOfImpact == 1)
        {
          body.Pose = body.TargetPose;
        }
        else
        {
          // Note: We could replace the interpolation with a UpdatePose(toi * dt) call?
          body.Pose = Pose.Interpolate(body.Pose, body.TargetPose, body.TimeOfImpact);
        }

        body.IsCcdActive = false;
      }
    }


    /// <summary>
    /// Removes the bodies that have left the simulation boundaries.
    /// </summary>
    private void RemoveBodiesOutsideWorld()
    {
      if (Settings.Motion.RemoveBodiesOutsideWorld)
      {
        int numberOfRigidBodies = RigidBodies.Count;
        for (int i = numberOfRigidBodies - 1; i >= 0; i--)
        {
          var body = RigidBodies[i];
          if (body.IsSleeping)
            continue;

          var pose = body.Pose;
          if (pose.Position.IsNaN()
              || pose.Orientation.IsNaN
              || !GeometryHelper.HaveContact(body.Aabb, World.Aabb))
          {
            RigidBodies.Remove(body);
          }
        }
      }
    }


    /// <summary>
    /// Raises the <see cref="SubTimeStepFinished"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnSubTimeStepFinished"/> in 
    /// a derived class, be sure to call the base class's <see cref="OnSubTimeStepFinished"/> method 
    /// so that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnSubTimeStepFinished(EventArgs eventArgs)
    {
      var handler = SubTimeStepFinished;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
