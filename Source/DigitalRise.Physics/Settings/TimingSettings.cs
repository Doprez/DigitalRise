// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Mathematics;


namespace DigitalRise.Physics.Settings
{
  /// <summary>
  /// Defines timing-related simulation settings.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each call of <see cref="Simulation"/>.<see cref="Simulation.Update(TimeSpan)"/> advances the 
  /// simulation state by a given time. The time by which the simulation should be advanced is 
  /// called a "time step". Internally the time step specified in the parameter of 
  /// <see cref="Simulation.Update(TimeSpan)"/> is divided into sub time steps (internal time steps)
  /// of constant size. This constant size is <see cref="FixedTimeStep"/>. That means, if the user 
  /// calls <c>Simulation.Update(1.0f / 30.0f)</c> and <see cref="FixedTimeStep"/> is 1/60 (default),
  /// the simulation will perform to time steps with 1/60 s. 
  /// </para>
  /// <para>
  /// If the user calls <c>Simulation.Update(1.0f / 120.0f)</c>, the simulation will perform no 
  /// simulation update because the time step is less than <see cref="FixedTimeStep"/>. But the 
  /// time is not "lost". When the user calls <c>Simulation.Update(1.0f / 120.0f)</c> the next time,
  /// the simulation will make a single time step with 1/60 s. The simulation class makes sure that 
  /// all simulation updates use fixed time steps internally and that overall no simulation time is 
  /// lost.
  /// </para>
  /// <para>
  /// The number of time steps per <see cref="Simulation.Update(TimeSpan)"/> call is actually 
  /// limited by <see cref="MaxNumberOfSteps"/>. This is necessary to avoid a typical problem in 
  /// game physics: Example: There are too many physical objects in the game. One frame of the game
  /// took 0.016 ms to compute. The simulation is called with <c>Simulation.Update(0.016)</c> and it
  /// performs 1 time step internally. Because there are too many objects, this frame took 0.034 ms 
  /// to compute. In the next frame <c>Simulation.Update(0.034)</c> is called and the simulation 
  /// performs 2 time steps internally - which takes even more time... Each frame the simulation 
  /// must make more internal time steps to keep up with the elapsed time, and eventually the frame 
  /// rate of the game will go towards 0. To avoid this scenario the maximal number of allowed sub 
  /// time steps is limited - practically this means that time is lost and the simulation moves 
  /// objects in slow motion. Slow motion at a low frame rate is better than real-time at 0 frames 
  /// per second.
  /// </para>
  /// <para>
  /// <strong>Simulation Quality and Timing:</strong> Per default the simulation makes time steps 
  /// of 1/60 seconds (60 Hz). Increasing the time step size makes the simulation faster and less
  /// stable. Decreasing the time step size makes the simulation slower and more accurate and 
  /// stable - higher box stacks are possible, less jittering, less interpenetrations of objects.
  /// </para>
  /// </remarks>
  public class TimingSettings
  {
    // Notes:
    // We do not support a dynamic time step because we don't see any advantages to fixed time 
    // steps - only disadvantages and support problems.
    // If a dynamic time step should be supported, all time dependent values must be scaled in each 
    // frame (cached impulses, ERP, CFM, ...).


    /// <summary>
    /// Gets or sets the time step size for fixed time steps in seconds.
    /// </summary>
    /// <value>
    /// The time step size for fixed time steps in seconds. The default is 1/60 s (60 Hz).
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float FixedTimeStep
    {
      get { return _fixedTimeStep; }
      set
      {
        if (Numeric.IsLessOrEqual(value, 0))
          throw new ArgumentOutOfRangeException("value", "FixedTimeStep must be greater than 0.");

        _fixedTimeStep = value;
      }
    }
    private float _fixedTimeStep;


    //public bool UseFixedTimeStep { get; set; }


    /// <summary>
    /// Gets or sets the maximal number of sub-steps performed during one time step.
    /// </summary>
    /// <value>
    /// The maximal number of sub-steps performed during one time step. The default value is 
    /// <c>8</c>.
    /// </value>
    /// <remarks>
    /// This value implicitly defines the maximum size of a time step. A time step will be an
    /// integer multiple of <see cref="FixedTimeStep"/>, but maximal
    /// <see cref="MaxNumberOfSteps"/> ∙ <see cref="FixedTimeStep"/>. If <see cref="Simulation"/>.
    /// <see cref="Simulation.Update(TimeSpan)"/> is called with a larger time step, time is lost.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int MaxNumberOfSteps
    {
      get { return _maxNumberOfSteps; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "MaxNumberOfSteps must be greater than 0.");

        _maxNumberOfSteps = value;
      }
    }
    private int _maxNumberOfSteps;


    /// <summary>
    /// Initializes a new instance of the <see cref="TimingSettings"/> class.
    /// </summary>
    public TimingSettings()
    {
      FixedTimeStep = 1.0f / 60.0f;
      MaxNumberOfSteps = 8;
      //UseFixedTimeStep = true;
    }
  }
}
