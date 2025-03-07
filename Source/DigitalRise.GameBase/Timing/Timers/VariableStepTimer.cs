// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.GameBase.Timing
{
  /// <summary>
  /// Controls the timing of a game or game component using variable time steps.
  /// </summary>
  /// <remarks>
  /// <para>
  /// See <see cref="IGameTimer"/> for basic information about game timers.
  /// </para>
  /// <para>
  /// The <see cref="VariableStepTimer"/> triggers a new time step 
  /// (<see cref="IGameTimer.TimeChanged"/> event) as soon as the last time step has finished.
  /// <see cref="IGameTimer.DeltaTime"/> returns the elapsed time since the last time step.
  /// </para>
  /// <para>
  /// <see cref="MinDeltaTime"/> (default: 0) can be used to set the minimal size of a time step. If 
  /// <see cref="MinDeltaTime"/> is greater than 0 than the timer waits until at least 
  /// <see cref="MinDeltaTime"/> has passed before the next time step is triggered. During the 
  /// waiting time the <see cref="Idle"/> event is raised. <see cref="IdleTime"/> indicates the time
  /// to wait until the next time step.
  /// </para>
  /// <para>
  /// <see cref="MaxDeltaTime"/> (default: <see cref="TimeSpan.MaxValue"/>) can be used to set the 
  /// maximal size of a time step. If the previous time step took longer than 
  /// <see cref="MaxDeltaTime"/>, then the size of the time step will be limited to 
  /// <see cref="MaxDeltaTime"/>. The time beyond this value will be ignored. <see cref="LostTime"/>
  /// indicates the time that has been dropped. 
  /// </para>
  /// <para>
  /// <strong>Memory Leaks:</strong> If a <see cref="Clock"/> is set and the timer is running, the 
  /// game timer handles the <see cref="IGameClock.TimeChanged"/> event of the clock. This means, 
  /// the clock stores a strong reference to the timer and might prevent the timer from being 
  /// garbage collected. If the timer is not needed anymore, call <see cref="Stop"/> or set the 
  /// property <see cref="Clock"/> tp <see langword="null"/> to allow the timer to be garbage 
  /// collected.
  /// </para>
  /// </remarks>
  public class VariableStepTimer : IGameTimer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Event args are reused to avoid multiple allocations.
    private readonly GameTimerEventArgs _eventArgs = new GameTimerEventArgs();
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public IGameClock Clock
    {
      get { return _clock; }
      set
      {
        if (_clock != value)
        {
          if (_clock != null && IsRunning)
            _clock.TimeChanged -= OnClockTimeChanged;

          _clock = value;

          if (_clock != null && IsRunning)
            _clock.TimeChanged += OnClockTimeChanged;
        }
      }
    }
    private IGameClock _clock;


    /// <inheritdoc/>
    public long FrameCount { get; private set; }


    /// <inheritdoc/>
    public bool IsRunning { get; private set; }


    /// <inheritdoc/>
    public TimeSpan Time { get; private set; }


    /// <inheritdoc/>
    public TimeSpan DeltaTime { get; private set; }


    /// <summary>
    /// Gets the idle time.
    /// </summary>
    /// <value>The idle time.</value>
    /// <remarks>
    /// <para>
    /// If <see cref="MinDeltaTime"/> is set the timer waits until at least this amount of time has 
    /// passed before a new time step (<see cref="IGameTimer.TimeChanged"/> event) is performed. 
    /// <see cref="IdleTime"/> indicates the waiting time until the next time step. The 
    /// <see cref="Idle"/> event is raised if <see cref="IdleTime"/> is greater than 0. The 
    /// application can handle this event to perform additional tasks instead of waiting.
    /// </para>
    /// <para>
    /// The property <see cref="IdleTime"/> is reset each time step.
    /// </para>
    /// <para>
    /// The value is also scaled by <see cref="IGameTimer.Speed"/>. <see cref="IGameTimer.Time"/>, 
    /// <see cref="IGameTimer.DeltaTime"/>, <see cref="IdleTime"/>, and <see cref="LostTime"/>
    /// have the same scale. Except that <see cref="IdleTime"/> and <see cref="LostTime"/> 
    /// cannot be negative. They always return the absolute value.
    /// </para>
    /// </remarks>
    public TimeSpan IdleTime { get; private set; }


    /// <summary>
    /// Gets the amount of time dropped in the current time step.
    /// </summary>
    /// <value>The lost time.</value>
    /// <remarks>
    /// <para>
    /// If a time step takes longer than <see cref="MaxDeltaTime"/> the time above this value is 
    /// dropped. The dropped time will not be recovered in the next time step. The time is lost.
    /// </para>
    /// <para>
    /// <see cref="LostTime"/> is adjusted each time step and only refers to the current time step. 
    /// (The lost time of multiple time steps is not accumulated.)
    /// </para>
    /// <para>
    /// A value greater than 0 indicates that the application is running too slow: The last time 
    /// step took longer than <see cref="MaxDeltaTime"/> seconds to compute.
    /// </para>
    /// <para>
    /// The value is also scaled by <see cref="IGameTimer.Speed"/>. <see cref="IGameTimer.Time"/>, 
    /// <see cref="IGameTimer.DeltaTime"/>, <see cref="IdleTime"/>, and <see cref="LostTime"/>
    /// have the same scale. Except that <see cref="IdleTime"/> and <see cref="LostTime"/> 
    /// cannot be negative. They always return the absolute value.
    /// </para>
    /// </remarks>
    public TimeSpan LostTime { get; private set; }


    /// <summary>
    /// Gets the accumulated time.
    /// </summary>
    /// <value>The accumulated time.</value>
    /// <remarks>
    /// <para>
    /// During <see cref="TimeChanged"/> events, <see cref="AccumulatedTime"/> is always zero.
    /// </para>
    /// <para>
    /// During <see cref="Idle"/> events, <see cref="AccumulatedTime"/> is equal to the elapsed
    /// time since the last <see cref="TimeChanged"/> event, and following is true:
    /// <see cref="AccumulatedTime"/> + <see cref="IdleTime"/> = <see cref="MinDeltaTime"/>.
    /// </para>
    /// </remarks>
    public TimeSpan AccumulatedTime { get; private set; }


    /// <inheritdoc/>
    public double Speed { get; set; }


    /// <inheritdoc/>
    public event EventHandler<GameTimerEventArgs> Idle;


    /// <inheritdoc/>
    public event EventHandler<GameTimerEventArgs> TimeChanged;


    /// <summary>
    /// Gets or sets the minimal size of a time step.
    /// </summary>
    /// <value>The minimal amount of time for time step. The default value is 0.</value>
    /// <remarks>
    /// <para>
    /// The timer waits until at least this amount of time has passed before a new 
    /// <see cref="IGameTimer.TimeChanged"/> event is raised. Use this value to limit the frame 
    /// rate. For example: <c>MinDeltaTime = new TimeSpan(166666)</c> limits the frame rate to 
    /// 60 Hz.
    /// </para>
    /// <para>
    /// <see cref="MinDeltaTime"/> causes the application to wait until the enough time has elapsed 
    /// to perform the next time step. During the waiting time the <see cref="Idle"/> event is 
    /// raised. This event can be used to perform additional work instead of waiting. 
    /// <see cref="IdleTime"/> indicates the remaining time until the next time step occurs.
    /// </para>
    /// <para>
    /// <see cref="MinDeltaTime"/> ≤ <see cref="IGameTimer.DeltaTime"/> ≤ <see cref="MaxDeltaTime"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public TimeSpan MinDeltaTime
    {
      get { return _minDeltaTime; }
      set 
      {
        if (value < TimeSpan.Zero)
          throw new ArgumentException("MinDeltaTime must be not be negative."); 

        _minDeltaTime = value;
      }
    }
    private TimeSpan _minDeltaTime;
    

    /// <summary>
    /// Gets or sets the maximal amount of time for a time step.
    /// </summary>
    /// <value>
    /// The maximal size of a time step. The default value is <see cref="TimeSpan.MaxValue"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If a time step takes longer than <see cref="MaxDeltaTime"/> the time above this value is 
    /// dropped. The dropped time will not be recovered in the next time step. The time is lost (see
    /// also: <see cref="LostTime"/>).
    /// </para>
    /// <para>
    /// <see cref="MinDeltaTime"/> ≤ <see cref="IGameTimer.DeltaTime"/> ≤ <see cref="MaxDeltaTime"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public TimeSpan MaxDeltaTime
    {
      get { return _maxDeltaTime; }
      set 
      {
        if (value <= TimeSpan.Zero)
          throw new ArgumentException("MaxDeltaTime must be greater than 0.");

        _maxDeltaTime = value; 
      }
    }
    private TimeSpan _maxDeltaTime = TimeSpan.MaxValue;

    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableStepTimer"/> class.
    /// </summary>
    /// <param name="clock">The clock.</param>
    public VariableStepTimer(IGameClock clock)
    {
      Speed = 1.0;
      _clock = clock;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Start()
    {
      if (!IsRunning)
      {
        IsRunning = true;

        if (_clock != null)
          _clock.TimeChanged += OnClockTimeChanged;
      }
    }


    /// <inheritdoc/>
    public void Stop()
    {
      if (IsRunning)
      {
        IsRunning = false;

        if (_clock != null)
          _clock.TimeChanged -= OnClockTimeChanged;
        
        DeltaTime = TimeSpan.Zero;
        IdleTime = TimeSpan.Zero;
        LostTime = TimeSpan.Zero;
      }
    }


    /// <inheritdoc/>
    public void Reset()
    {
      Stop();
      FrameCount = 0;
      Time = TimeSpan.Zero;
      AccumulatedTime = TimeSpan.Zero;
    }


    private void OnClockTimeChanged(object sender, GameClockEventArgs eventArgs)
    {
      // Scale incoming deltaTime.
      long ticks = (long)(eventArgs.DeltaTime.Ticks * Speed);

      // Determine time since last time step
      AccumulatedTime += new TimeSpan(ticks);
      TimeSpan duration = AccumulatedTime.Duration(); // Absolute value

      if (duration < MinDeltaTime)
      {
        // ----- Idle

        // Time left before next time step.
        // Set idle time and trigger Idle event.
        DeltaTime = TimeSpan.Zero;
        LostTime = TimeSpan.Zero;
        IdleTime = MinDeltaTime - duration;
        
        UpdateEventArgs();
        OnIdle(_eventArgs);
      }
      else
      {
        // ----- Time Step

        FrameCount++;

        if (duration > MaxDeltaTime)
        {
          // Drop time above MaxDeltaTime
          LostTime = duration - MaxDeltaTime;
          DeltaTime = (AccumulatedTime.Ticks >= 0) ? MaxDeltaTime : -MaxDeltaTime;
        }
        else
        {
          // Default
          LostTime = TimeSpan.Zero;
          DeltaTime = AccumulatedTime;
        }

        AccumulatedTime = TimeSpan.Zero;
        IdleTime = TimeSpan.Zero;

        // Update time and trigger next time step
        Time += DeltaTime;

        UpdateEventArgs();
        OnTimeChanged(_eventArgs);
      }      
    }


    private void UpdateEventArgs()
    {
      _eventArgs.FrameCount = FrameCount;
      _eventArgs.Time = Time;
      _eventArgs.DeltaTime = DeltaTime;
      _eventArgs.IdleTime = IdleTime;
      _eventArgs.LostTime = LostTime;
      _eventArgs.AccumulatedTime = AccumulatedTime;
    }


    /// <summary>
    /// Raises the <see cref="TimeChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="GameTimerEventArgs"/> instance containing the event data.
    /// </param>
    protected void OnTimeChanged(GameTimerEventArgs eventArgs)
    {
      var handler = TimeChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="Idle"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="GameTimerEventArgs"/> instance containing the event data.
    /// </param>
    protected void OnIdle(GameTimerEventArgs eventArgs)
    {
      var handler = Idle;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
