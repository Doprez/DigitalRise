// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics.CodeAnalysis;

namespace DigitalRise.GameBase.Timing
{
  /// <summary>
  /// Controls the timing of a game or a game component.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="IGameTimer"/>s must be attached to a <see cref="IGameClock"/>. They are updated 
  /// automatically by handling the clock's <see cref="IGameClock.TimeChanged"/> event.
  /// </para>
  /// <para>
  /// A game usually has one <see cref="IGameClock"/> that is running permanently and acts as the 
  /// central time source. Each game or game component can further have its own 
  /// <see cref="IGameTimer"/>. The <see cref="IGameTimer"/> controls the timing of the game 
  /// component. For example: A <see cref="FixedStepTimer"/> ensures that a game component is 
  /// updated with a constant frame rate. All <see cref="IGameTimer"/>s are updated by the central 
  /// <see cref="IGameClock"/>.
  /// </para>
  /// <para>
  /// The terms 'time step' and 'frame' in this documentation refer to the <see cref="TimeChanged"/> 
  /// event. The <see cref="TimeChanged"/> event triggers a new time step. (In a game the 
  /// <see cref="TimeChanged"/> event usually triggers the "Update" method of a game module which 
  /// computes and renders the next frame.)
  /// </para>
  /// </remarks>
  public interface IGameTimer
  {
    /// <summary>
    /// Gets or sets the clock.
    /// </summary>
    /// <value>
    /// The clock. Can be <see langword="null"/>. Without a valid clock, the timer is paused.
    /// </value>
    IGameClock Clock { get; set; }


    /// <summary>
    /// Gets the number of frames since the start of the timer (= the number of 
    /// <see cref="TimeChanged"/> events).
    /// </summary>
    /// <value>The number of frames.</value>
    long FrameCount { get; }


    /// <summary>
    /// Gets a value indicating whether the timer is running.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the timer is running; otherwise, <see langword="false"/> if the 
    /// timer is paused. The default value is <see langword="false"/>.
    /// </value>
    bool IsRunning { get; }


    /// <summary>
    /// Gets the game time.
    /// </summary>
    /// <value>The game time.</value>
    TimeSpan Time { get; }


    /// <summary>
    /// Gets the elapsed game time since the last time step.
    /// </summary>
    /// <value>The elapsed game time since the last time step.</value>
    TimeSpan DeltaTime { get; }


    /// <summary>
    /// Gets the idle time.
    /// </summary>
    /// <value>The idle time.</value>
    /// <remarks>
    /// <para>
    /// Most game timers can be configured to start a new time step only at certain intervals. For
    /// example, the <see cref="FixedStepTimer"/> only performs time steps at a fixed time interval 
    /// such as 1/60 seconds (see <see cref="FixedStepTimer.StepSize"/>). Or, the 
    /// <see cref="VariableStepTimer"/> waits until at least a minimal amount of time has passed 
    /// since the last time step (see <see cref="VariableStepTimer.MinDeltaTime"/>).
    /// </para>
    /// <para>
    /// <see cref="IdleTime"/> indicates the time that needs to pass until the next time step will
    /// be performed (see event <see cref="TimeChanged"/>).The <see cref="Idle"/> event is raised 
    /// if <see cref="IdleTime"/> is greater than 0. The application can handle this event to 
    /// perform additional tasks instead of waiting.
    /// </para>
    /// <para>
    /// The property <see cref="IdleTime"/> is reset each time step.
    /// </para>
    /// <para>
    /// The value is also scaled by <see cref="Speed"/>. <see cref="Time"/>, 
    /// <see cref="DeltaTime"/>, <see cref="IdleTime"/>, and <see cref="LostTime"/> have the same
    /// scale. Except that <see cref="IdleTime"/> and <see cref="LostTime"/> cannot be negative.
    /// They always return the absolute value.
    /// </para>
    /// </remarks>
    TimeSpan IdleTime { get; }


    /// <summary>
    /// Gets the amount of time dropped in the current time step.
    /// </summary>
    /// <value>The lost time.</value>
    /// <remarks>
    /// <para>
    /// In most game timers the maximal time of time step can be limited. For example, see property 
    /// <see cref="FixedStepTimer.MaxNumberOfSteps"/> of <see cref="FixedStepTimer"/> or property 
    /// <see cref="VariableStepTimer.MaxDeltaTime"/> of <see cref="VariableStepTimer"/>. If a time
    /// step takes longer than the maximal allowed amount of time, <see cref="DeltaTime"/> is set to
    /// the maximal allowed amount of time, the time above this value is dropped. The dropped time
    /// will not be recovered in the next time steps. The time is lost.
    /// </para>
    /// <para>
    /// This mechanism is necessary to preserve stability in physics simulations or other game 
    /// components. As a result the game is running slower than real-time (wall clock time), but the
    /// simulation stays stable.
    /// </para>
    /// <para>
    /// <see cref="LostTime"/> is adjusted each time step and only refers to the current time step. 
    /// (The lost time of multiple time steps is not accumulated.)
    /// </para>
    /// <para>
    /// The value is also scaled by <see cref="Speed"/>. <see cref="Time"/>, 
    /// <see cref="DeltaTime"/>, <see cref="IdleTime"/>, and <see cref="LostTime"/> have the same 
    /// scale. Except that <see cref="IdleTime"/> and <see cref="LostTime"/> cannot be negative.
    /// They always return the absolute value.
    /// </para>
    /// </remarks>
    TimeSpan LostTime { get; }


    /// <summary>
    /// Gets the accumulated time.
    /// </summary>
    /// <value>The accumulated time.</value>
    /// <remarks>
    /// <para>
    /// Some game timers can be configured to start a new time step only at certain intervals. For
    /// example, the <see cref="FixedStepTimer"/> only performs time steps at a fixed time interval 
    /// such as 1/60 seconds (see <see cref="FixedStepTimer.StepSize"/>). When a time step is
    /// preformed (<see cref="TimeChanged"/> is raised), <see cref="AccumulatedTime"/> indicates
    /// the current elapsed time that exceeds the interval size. When an <see cref="Idle"/> event
    /// is raised, <see cref="AccumulatedTime"/> indicates the elapsed since the last 
    /// <see cref="TimeChanged"/> event. <see cref="AccumulatedTime"/> is always less than the 
    /// configured time interval for time steps. 
    /// </para>
    /// <para>
    /// For example: A fixed step timer is configured to perform time steps at 1 second intervals.
    /// If the timer is updated by the clock and exactly 1 second has passed, 
    /// <see cref="TimeChanged"/> is raised and <see cref="AccumulatedTime"/> is 0. If 1.3 seconds 
    /// have passed since the last time step, <see cref="TimeChanged"/> is raised and 
    /// <see cref="AccumulatedTime"/> is 0.3 seconds. During <see cref="Idle"/> events, 
    /// <see cref="AccumulatedTime"/> is equal to the time step interval minus the 
    /// <see cref="IdleTime"/>.
    /// </para>
    /// </remarks>
    TimeSpan AccumulatedTime { get; }

    
    /// <summary>
    /// Gets or sets the speed ratio at which the game time progresses.
    /// </summary>
    /// <value>The speed ratio. The default value is 1.0.</value>
    /// <remarks>
    /// <para>
    /// Each timer is bound to a <see cref="IGameClock"/>. <see cref="Speed"/> defines how fast the 
    /// time progresses in comparison to the <see cref="IGameClock"/>. A value of 2.0 means that 
    /// this timer advances twice as fast as the real time. A value less than 1.0 creates a slow 
    /// motion effect. Negative values should be handled with caution: The game logic needs to be 
    /// able to deal with negative values.
    /// </para>
    /// <para>
    /// Changing <see cref="Speed"/> only effects future time changes, i.e. it does not affect the 
    /// current <see cref="Time"/> or <see cref="DeltaTime"/>.
    /// </para>
    /// </remarks>
    double Speed { get; set; }


    /// <summary>
    /// Occurs when application is idle.
    /// </summary>
    /// <remarks>
    /// This event is raised when the timer is updated, but there is time left before the next time 
    /// step should be performed. See <see cref="IdleTime"/> for more details.
    /// </remarks>
    event EventHandler<GameTimerEventArgs> Idle;


    /// <summary>
    /// Occurs when the game time has advanced.
    /// </summary>
    /// <remarks>
    /// This event triggers the next time step (frame).
    /// </remarks>
    event EventHandler<GameTimerEventArgs> TimeChanged;


    /// <summary>
    /// Stops the timer and resets the time to zero.
    /// </summary>
    /// <remarks>
    /// This method stops the timer and sets the <see cref="FrameCount"/>, <see cref="Time"/>,
    /// <see cref="DeltaTime"/>, <see cref="LostTime"/>, <see cref="IdleTime"/> to zero. 
    /// </remarks>
    void Reset();

    
    /// <summary>
    /// Starts/resumes the timer.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="Start"/> for an already running timer does nothing. This method does not 
    /// reset <see cref="FrameCount"/> or <see cref="Time"/>.
    /// </remarks>
    void Start();


    /// <summary>
    /// Pauses the timer.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="Stop"/> does nothing if the timer is not running. This method does not 
    /// reset <see cref="FrameCount"/> or <see cref="Time"/>.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
    void Stop();
  }
}
