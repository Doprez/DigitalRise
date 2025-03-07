// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Physics.Constraints;
using DigitalRise.Physics.Materials;


namespace DigitalRise.Physics.Settings
{

  /// <summary>
  /// Defines constraint-related simulation settings.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Allowed Errors:</strong> For constraints small errors are allowed (e.g. small
  /// penetrations of rigid bodies). This improves the stability of the simulation. 
  /// <see cref="ContactConstraint"/>s can have an <see cref="AllowedPenetration"/>. Other 
  /// constraints can have an <see cref="AllowedLinearDeviation"/> and an 
  /// <see cref="AllowedAngularDeviation"/>. The allowed errors should be significantly larger than 
  /// 0, but so small that the visual errors in the application are acceptable.
  /// </para>
  /// </remarks>
  public class ConstraintSettings
  {
    /// <summary>
    /// Gets or sets the allowed penetration of <see cref="ContactConstraint"/>s.
    /// </summary>
    /// <value>The maximal allowed penetration. The default is 0.01.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float AllowedPenetration
    {
      get { return _allowedPenetration; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "AllowedPenetration must be greater than or equal to 0.");
        
        _allowedPenetration = value;
      }
    }
    private float _allowedPenetration;


    /// <summary>
    /// Gets or sets the allowed linear error of constraints.
    /// </summary>
    /// <value>The absolute allowed linear error of constraints. The default is 0.01.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float AllowedLinearDeviation
    {
      get { return _allowedLinearDeviation; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "AllowedLinearDeviation must be greater than or equal to 0.");
        
        _allowedLinearDeviation = value;
      }
    }
    private float _allowedLinearDeviation;


    /// <summary>
    /// Gets or sets the allowed angular error of constraints.
    /// </summary>
    /// <value>The absolute allowed angular error of constraints. The default is 0.01.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float AllowedAngularDeviation
    {
      get { return _allowedAngularDeviation; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "AllowedAngularDeviation must be greater than or equal to 0.");
        
        _allowedAngularDeviation = value;
      }
    }
    private float _allowedAngularDeviation;


    /// <summary>
    /// Gets or sets the number of constraint iterations per time step.
    /// </summary>
    /// <value>The number of constraint iterations. Must be positive. The default value is 10.
    /// </value>
    /// <remarks>
    /// The constraint solver uses an iterative algorithm. Increasing the number of iterations
    /// improves the simulation stability but makes the simulation slower.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int NumberOfConstraintIterations
    {
      get { return _numberOfConstraintIterations; }
      set 
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "NumberOfConstraintIterations must be greater than 0.");

        _numberOfConstraintIterations = value; 
      }
    }
    private int _numberOfConstraintIterations;


    /// <summary>
    /// Gets or sets the minimal constraint impulse.
    /// </summary>
    /// <value>
    /// The minimal constraint impulse in the range [0, ∞[. The default value is 0.000001.
    /// </value>
    /// <remarks>
    /// If a all constraint impulses in a <see cref="SimulationIsland"/> are below this limit,
    /// the constraint solving for this island is aborted. Setting a large value improves 
    /// simulation performance but reduces the stability of the simulation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MinConstraintImpulse
    {
      get { return _minConstraintImpulse; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MinConstraintImpulse must be greater than or equal to 0.");

        _minConstraintImpulse = value;
        MinConstraintImpulseSquared = value * value;
      }
    }
    private float _minConstraintImpulse;
    internal float MinConstraintImpulseSquared { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether constraints are randomly reordered during
    /// constraint solving.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if constraints are randomly reordered during constraint solving; 
    /// otherwise, <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When the constraints of a simulation island are solved, the stability can be greatly
    /// improved by randomly reordering constraints (internally in the solver). This stability
    /// improvement is very visible in stacks of bodies. 
    /// </para>
    /// <para>
    /// Disabling this flag increases the simulation performance at the cost of stability.
    /// </para>
    /// </remarks>
    public bool RandomizeConstraints { get; set; }


    /// <summary>
    /// Gets or sets the stacking optimization factor. (Experimental)
    /// </summary>
    /// <value>
    /// The stacking optimization factor in the range [0, ∞[. The default value is 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// Stacking objects is difficult for the simulation - especially if the 
    /// <see cref="TimingSettings.FixedTimeStep"/> is large or the 
    /// <see cref="NumberOfConstraintIterations"/> is low. The stacking optimization is a trick to
    /// improve the stability of stacking by reducing the gravity acceleration of objects in a
    /// stack. If <see cref="StackingFactor"/> is 0, the optimization is disabled. Non-zero
    /// values enable the stacking optimization. Higher values use a more aggressive optimization.
    /// Recommended values are 0 - 10.
    /// </para>
    /// <para>
    /// This stacking optimization is independent from the <see cref="StackingTolerance"/>
    /// optimization.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float StackingFactor
    {
      get { return _stackingFactor; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "StackingFactor must be greater than or equal to 0.");

        _stackingFactor = value;
      }
    }
    private float _stackingFactor;


    /// <summary>
    /// Gets or sets the stacking optimization tolerance. (Experimental)
    /// </summary>
    /// <value>
    /// The stacking optimization tolerance in the range [0, 1]. The default value is 0.1.
    /// </value>
    /// <remarks>
    /// <para>
    /// Stacking objects is difficult for the simulation - especially if the 
    /// <see cref="TimingSettings.FixedTimeStep"/> is large or the 
    /// <see cref="NumberOfConstraintIterations"/> is low. The stacking optimization is a trick to
    /// improve the stability of stacking by stabilizing contacts. A non-zero value improves the
    /// stability of high stacks or walls and it improves the simulation performance when there are 
    /// many contacts.
    /// </para>
    /// <para>
    /// Use a value of 0 to disable this optimization. This will make stacks less stable and 
    /// the simulation will actually be a bit slower. But any unnatural behaviors that could be
    /// caused by this stacking optimization are removed.
    /// </para>
    /// <para>
    /// Use a value of 1 to fully enable this optimization. This will make stakes more stable and 
    /// the simulation performance is improved. In very rare cases, the stacking optimization can 
    /// cause unnatural behavior. 
    /// </para>
    /// <para>
    /// Any value between 0 and 1 can be used. Already very small values (0.05 - 0.1) are enough to
    /// greatly improve the stability of stacks.
    /// </para>
    /// <para>
    /// This stacking optimization is independent from the <see cref="StackingFactor"/>
    /// optimization.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 0 or greater than 1.
    /// </exception>
    public float StackingTolerance
    {
      get { return _stackingTolerance; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "StackingTolerance must be in the range [0, 1].");

        _stackingTolerance = value;
      }
    }
    private float _stackingTolerance;


    /// <summary>
    /// Gets or sets the contact error reduction parameter.
    /// </summary>
    /// <value>
    /// The contact error reduction parameter in the range [0, 1]. The default value is 0.2.
    /// </value>
    /// <remarks>
    /// This value defines the speed by which penetration errors of contact constraints are 
    /// removed. If the value is 0, penetration errors are not corrected. If the value is set to 1,
    /// the simulation tries to remove 100% of the error in each time step - this is not stable and
    /// smaller values are recommended.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float ContactErrorReduction
    {
      get { return _contactErrorReduction; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "ContactErrorReduction must be greater than or equal to 0.");

        _contactErrorReduction = value;
      }
    }
    private float _contactErrorReduction;


    /// <summary>
    /// Gets or sets the restitution threshold.
    /// </summary>
    /// <value>The restitution threshold in the range [0, 1]. The default value 0.05.</value>
    /// <remarks>
    /// Restitution is also called bounciness. Rigid body materials (see <see cref="IMaterial"/>)
    /// define a coefficient of restitution for each body. If two bodies collide, a combined
    /// coefficient of restitution is computed to determine if the bodies will bounce or if the
    /// collision will be inelastic. To increase stability the simulation will clamp restitution
    /// values below <see cref="RestitutionThreshold"/> to 0. Increasing this value improves the
    /// simulation stability but reduces bounciness of objects with small bounciness values.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float RestitutionThreshold
    {
      get { return _restitutionThreshold; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "RestitutionThreshold must be greater than or equal to 0.");

        _restitutionThreshold = value;
      }
    }
    private float _restitutionThreshold;


    /// <summary>
    /// Gets or sets the resting velocity limit.
    /// </summary>
    /// <value>The resting velocity limit.</value>
    /// <remarks>
    /// Rigid bodies will not bounce off of each other if their relative collision velocity is less
    /// than <see cref="RestingVelocityLimit"/>. Increasing this value improves simulation stability
    /// and reduces the bounciness of slow objects.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float RestingVelocityLimit
    {
      get { return _restingVelocityLimit; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "RestingVelocityLimit must be greater than or equal to 0.");

        _restingVelocityLimit = value;
      }
    }
    private float _restingVelocityLimit;
      

    /// <summary>
    /// Gets or sets the Baumgarte error correction ratio. (Experimental)
    /// </summary>
    /// <value>The Baumgarte ratio in the range [0, 1]. The default is 1.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 1.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float BaumgarteRatio
    {
      // If 1: all error is corrected with Baumgarte. 
      // If 0: all errors are corrected with pseudo velocities.

      get { return _baumgarteRatio; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "BaumgarteRatio must be in the range [0, 1].");

        _baumgarteRatio = value;
      }
    }
    private float _baumgarteRatio;


    /// <summary>
    /// Gets or sets the maximal error correction velocity for general constraint errors.
    /// </summary>
    /// <value>
    /// The maximal error correction velocity. Must be greater than or equal to 0. The default value 
    /// is 100.
    /// </value>
    /// <remarks>
    /// The simulation will move bodies to remove constraint errors (e.g. interpenetrations of 
    /// rigid bodies or violated joint limits). The error correction velocities for constraint 
    /// errors are cut off above <see cref="MaxErrorCorrectionVelocity"/>. For penetration errors 
    /// <see cref="MaxPenetrationCorrectionVelocity"/> is used, not 
    /// <see cref="MaxErrorCorrectionVelocity"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MaxErrorCorrectionVelocity
    {
      get { return _maxErrorCorrectionVelocity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MaxErrorCorrectionVelocity must be greater than or equal to 0.");

        _maxErrorCorrectionVelocity = value;
        MaxErrorCorrectionVelocitySquared = value * value;
      }
    }
    private float _maxErrorCorrectionVelocity;
    internal float MaxErrorCorrectionVelocitySquared { get; private set; }


    /// <summary>
    /// Gets or sets the maximal error correction velocity for correcting rigid body 
    /// interpenetrations.
    /// </summary>
    /// <value>
    /// The maximal error correction velocity for penetration errors. Must be greater than or equal 
    /// to 0. The default value is 2.
    /// </value>
    /// <remarks>
    /// The simulation will move bodies to remove constraint errors (e.g. interpenetrations of 
    /// rigid bodies or violated joint limits). The error correction velocities for penetration
    /// errors are cut off above <see cref="MaxPenetrationCorrectionVelocity"/>. 
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MaxPenetrationCorrectionVelocity
    {
      get { return _maxPenetrationCorrectionVelocity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MaxPenetrationCorrectionVelocity must be greater than or equal to 0.");

        _maxPenetrationCorrectionVelocity = value;
      }
    }
    private float _maxPenetrationCorrectionVelocity;


    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintSettings"/> class.
    /// </summary>
    public ConstraintSettings()
    {
      AllowedPenetration = 0.01f;
      AllowedLinearDeviation = 0.01f;
      AllowedAngularDeviation = 0.04f;

      BaumgarteRatio = 1;
      ContactErrorReduction = 0.2f;
      MaxErrorCorrectionVelocity = 100;
      MaxPenetrationCorrectionVelocity = 2;
      MinConstraintImpulse = 0.000001f;
      NumberOfConstraintIterations = 10;
      RandomizeConstraints = true;
      RestingVelocityLimit = 1;
      RestitutionThreshold = 0.05f;
      StackingFactor = 0;
      StackingTolerance = 0.1f;
    }
  }
}
