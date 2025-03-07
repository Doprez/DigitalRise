// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.GameBase.Timing
{
  /// <summary>
  /// Provides arguments for a game clock's <see cref="IGameClock.TimeChanged"/> event.
  /// </summary>
  [Serializable]
  public class GameClockEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the amount of time that has elapsed since the last <see cref="IGameClock.TimeChanged"/>
    /// event.
    /// </summary>
    /// <value>The elapsed time since the last <see cref="IGameClock.TimeChanged"/> event.</value>
    public TimeSpan DeltaTime { get; set; }


    /// <summary>
    /// Gets the game time, which is the sum of all <see cref="DeltaTime"/> values.
    /// </summary>
    /// <value>The game time, which is the sum of all <see cref="DeltaTime"/> values.</value>
    public TimeSpan GameTime { get; set; }


    /// <summary>
    /// Gets the duration (wall clock time) for which the clock is running.
    /// </summary>
    /// <value>The duration (wall clock time) for which the clock is running.</value>
    public TimeSpan TotalTime { get; set; }
  }
}
