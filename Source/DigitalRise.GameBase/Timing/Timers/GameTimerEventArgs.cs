// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.GameBase.Timing
{
  /// <summary>
  /// Provides arguments for a game timer's <see cref="IGameTimer.TimeChanged"/> event.
  /// </summary>
  [Serializable]
  public class GameTimerEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the number of frames since the start of the timer (= the number of 
    /// <see cref="IGameTimer.TimeChanged"/> events).
    /// </summary>
    /// <value>The number of frames.</value>
    public long FrameCount { get; set; }


    /// <summary>
    /// Gets the game time.
    /// </summary>
    /// <value>The game time.</value>
    public TimeSpan Time { get; set; }


    /// <summary>
    /// Gets the elapsed game time since the last time step.
    /// </summary>
    /// <value>
    /// The elapsed game time since the last time step.
    /// </value>
    public TimeSpan DeltaTime { get; set; }


    /// <summary>
    /// Gets the idle time.
    /// </summary>
    /// <value>The idle time.</value>
    /// <remarks>
    /// See <see cref="IGameTimer.IdleTime"/> for more information.
    /// </remarks>
    public TimeSpan IdleTime { get; set; }


    /// <summary>
    /// Gets the lost time.
    /// </summary>
    /// <value>The lost time.</value>
    /// <remarks>
    /// See <see cref="IGameTimer.LostTime"/> for more information.
    /// </remarks>
    public TimeSpan LostTime { get; set; }


    /// <summary>
    /// Gets the accumulated time.
    /// </summary>
    /// <value>The accumulated time.</value>
    /// <remarks>
    /// See <see cref="IGameTimer.AccumulatedTime"/> for more information.
    /// </remarks>
    public TimeSpan AccumulatedTime { get; set; }
  }
}
