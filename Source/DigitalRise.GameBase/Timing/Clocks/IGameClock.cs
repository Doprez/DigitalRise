// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics.CodeAnalysis;

namespace DigitalRise.GameBase.Timing
{
  /// <summary>
  /// Measures time and raises an event when time changes.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A game clock measures elapsed time. Call <see cref="Start"/> to start or resume the time 
  /// measurement. Call <see cref="Stop"/> to pause the time measurement. <see cref="Reset"/> stops
  /// the time measurement and sets the measured time to 0. The flag <see cref="IsRunning"/>
  /// indicates whether the clock is currently measuring time.
  /// </para>
  /// <para>
  /// When time changes, the event <see cref="TimeChanged"/> is raised. This event is usually 
  /// raised at variable (or fixed) intervals to indicate the beginning of a new frame. Between
  /// <see cref="TimeChanged"/> events, the time is treated as constant.
  /// </para>
  /// <para>
  /// <see cref="TotalTime"/> indicates the total duration for which the clock is running. 
  /// <see cref="DeltaTime"/> indicates the time since the last <see cref="TimeChanged"/> event or 
  /// the time since the last <see cref="ResetDeltaTime"/> call, whichever is more recent.
  /// <see cref="GameTime"/> is the sum of all <see cref="DeltaTime"/> values. If 
  /// <see cref="ResetDeltaTime"/> is never executed, <see cref="TotalTime"/> and 
  /// <see cref="GameTime"/> are equal. If <see cref="ResetDeltaTime"/> was called, 
  /// <see cref="TotalTime"/> is greater than <see cref="GameTime"/>. After long-running operations 
  /// (e.g. file access) <see cref="ResetDeltaTime"/> can be called to reset the measurement of 
  /// <see cref="DeltaTime"/>. This avoids large jumps of the <see cref="GameTime"/>. 
  /// </para>
  /// <para>
  /// A game usually has one <see cref="IGameClock"/> that is running permanently and acts as the 
  /// central time source. Each game module can further have its own <see cref="IGameTimer"/>. The 
  /// <see cref="IGameTimer"/> controls the timing of the game module. For example: A 
  /// <see cref="FixedStepTimer"/> ensures that a game module is updated with a constant frame rate.
  /// All <see cref="IGameTimer"/>s are updated by the central <see cref="IGameClock"/>.
  /// </para>
  /// <para>
  /// The <see cref="IGameClock"/> is not running by default - <see cref="Start"/> needs to be 
  /// called explicitly. The clock should be suspended (<see cref="Stop"/> method) when the 
  /// application becomes inactive or the application windows is minimized. Call <see cref="Start"/> 
  /// to resume the clock when the application becomes active again.
  /// </para>
  /// </remarks> 
  public interface IGameClock
  {
    /// <summary>
    /// Gets a value indicating whether the clock is running.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the clock is running; otherwise, <see langword="false"/> if the 
    /// clock is paused.
    /// </value>
    bool IsRunning { get; }


    /// <summary>
    /// Gets the amount of time that has elapsed since the last <see cref="TimeChanged"/>
    /// event.
    /// </summary>
    /// <value>The elapsed time since the last <see cref="TimeChanged"/> event.</value>
    /// <remarks>
    /// This value is updated before the <see cref="TimeChanged"/> is raised. Between
    /// two <see cref="TimeChanged"/> events this value is constant.
    /// </remarks>
    TimeSpan DeltaTime { get; }


    /// <summary>
    /// Gets the max limit for <see cref="DeltaTime"/>.
    /// </summary>
    /// <value>The max limit for <see cref="DeltaTime"/>.</value>
    /// <remarks>
    /// <para>
    /// <see cref="DeltaTime"/> is clamped to <see cref="MaxDeltaTime"/>. This is useful to avoid
    /// large time jumps when the game is paused in the debugger or waiting for a blocking operation
    /// where the user cannot interact with the game.
    /// </para>
    /// </remarks>
    TimeSpan MaxDeltaTime { get; set; }


    /// <summary>
    /// Gets the game time, which is the sum of all <see cref="DeltaTime"/> values.
    /// </summary>
    /// <value>The game time, which is the sum of all <see cref="DeltaTime"/> values.</value>
    TimeSpan GameTime { get; }


    /// <summary>
    /// Gets the duration (wall clock time) for which the clock is running.
    /// </summary>
    /// <value>The duration (wall clock time) for which the clock is running.</value>
    TimeSpan TotalTime { get; }


    /// <summary>
    /// Occurs when the time changed.
    /// </summary>
    /// <remarks>
    /// This event is raised at variable or fixed intervals to indicate the beginning of the next
    /// frame of the game.
    /// </remarks>
    event EventHandler<GameClockEventArgs> TimeChanged;


    /// <summary>
    /// Starts/resumes the clock.
    /// </summary>
    void Start();


    /// <summary>
    /// Pauses the clock.
    /// </summary>
    /// <remarks>
    /// The clock is paused and <see cref="IsRunning"/> is set to <see langword="false"/>. The times
    /// are not reset. The clock can be resumed by calling <see cref="Start"/>.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
    void Stop();


    /// <summary>
    /// Stops the clock and resets all times to 0.
    /// </summary>
    /// <remarks>
    /// The clock is paused and <see cref="IsRunning"/> is set to <see langword="false"/>. The times
    /// are reset. The clock can be restarted by calling <see cref="Start"/>.
    /// </remarks>
    void Reset();


    /// <summary>
    /// Resets the time measurement for the current <see cref="DeltaTime"/>. The next 
    /// <see cref="DeltaTime"/> will be the time since <see cref="ResetDeltaTime"/> was called.
    /// </summary>
    /// <remarks>
    /// <see cref="DeltaTime"/> represents the time since the last <see cref="TimeChanged"/> event. 
    /// If there are long-running operations, it can happen that it takes a long time until the next
    /// <see cref="TimeChanged"/> event is raised, and <see cref="DeltaTime"/> is a very large value
    /// that is not useful for the game logic. For example, this could cause large jumps in the 
    /// motion of objects. To avoid this problem, call <see cref="ResetDeltaTime"/> after a 
    /// long-running operation to restart the time measurement for <see cref="DeltaTime"/>.
    /// </remarks>
    void ResetDeltaTime();
  }
}
