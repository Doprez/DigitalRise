// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRise.GameBase.Timing
{
  /// <summary>
  /// Controls the timing of a game or game component using fixed-sized time steps.
  /// </summary>
  /// <remarks>
  /// <para>
  /// See <see cref="IGameTimer"/> for basic information about game timers.
  /// </para>
  /// <para>
  /// The <see cref="FixedStepTimer"/> triggers new time steps (<see cref="IGameTimer.TimeChanged"/>
  /// event) in fixed-sized intervals. The size of each time step is <see cref="StepSize"/> or an 
  /// integer multiple of <see cref="StepSize"/>. <see cref="MaxNumberOfSteps"/> determines the 
  /// maximal size of time step: The upper limit of a time step is 
  /// <see cref="MaxNumberOfSteps"/> ∙ <see cref="StepSize"/>. The time beyond this limit will be 
  /// ignored. (<see cref="LostTime"/> indicates the amount of time that is dropped in the current 
  /// time step.)
  /// </para>
  /// <para>
  /// The timer waits until at least <see cref="StepSize"/> seconds have passed since the last time 
  /// step. During the waiting time the <see cref="Idle"/> event is raised. <see cref="IdleTime"/> 
  /// indicates the time to wait until the next time step.
  /// </para>
  /// <para>
  /// If the time that has passed is not exactly an integer multiple of <see cref="StepSize"/> then 
  /// the remainder will be considered in the next time step (see also <see cref="AccumulatedTime"/>
  /// ). (Exception: <see cref="MaxNumberOfSteps"/> ∙ <see cref="StepSize"/> is the upper limit. The
  /// time beyond this value will be dropped.)
  /// </para>
  /// <para>
  /// If the elapsed time since the last last time step is equal to or larger than two times the 
  /// <see cref="StepSize"/>, the timer will either raise one <see cref="TimeChanged"/> event with 
  /// a <see cref="DeltaTime"/> that is an integer multiple of <see cref="StepSize"/>, or it will 
  /// raise several <see cref="TimeChanged"/> events immediately after each other where each 
  /// <see cref="DeltaTime"/> is equal to <see cref="StepSize"/>. This behavior is controlled by the
  /// property <see cref="AccumulateTimeSteps"/>. If <see cref="AccumulateTimeSteps"/> is 
  /// <see langword="true"/>, the timer will raise one <see cref="TimeChanged"/> event with the
  /// accumulated <see cref="DeltaTime"/>.
  /// </para>
  /// <para>
  /// To check if the game is running slowly, the properties <see cref="AccumulatedSteps"/> and 
  /// <see cref="PendingSteps"/> can be checked. During a time step, <see cref="AccumulatedSteps"/>
  /// should be equal to 1 and <see cref="PendingSteps"/> should be equal to 0, otherwise the game 
  /// is running slowly - the workload of one time step exceeds the desired <see cref="StepSize"/>. 
  /// To improve the frame rate of the game, it should reduce its workload and probably skip the 
  /// rendering until <see cref="AccumulatedSteps"/> and <see cref="PendingSteps"/> are back to 
  /// their normal values. When the game is running slowly and starts to omit work, the game can 
  /// also call <see cref="IGameClock.ResetDeltaTime"/> of the <see cref="Clock"/> to reset the 
  /// elapsed time of the clock and avoid large jumps of the game time.
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
  public class FixedStepTimer : IGameTimer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Event args are reused to avoid multiple allocations.
    private readonly GameTimerEventArgs _eventArgs = new GameTimerEventArgs();

    private TimeSpan _maxStepSize;
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
    /// The timer waits until at least <see cref="StepSize"/> seconds have passed before a new time 
    /// step (<see cref="IGameTimer.TimeChanged"/> event) is performed. <see cref="IdleTime"/> 
    /// indicates the time to wait until the next time step. The <see cref="Idle"/> event is raised 
    /// if <see cref="IdleTime"/> is greater than 0. The application can handle this event to 
    /// perform additional tasks instead of waiting.
    /// </para>
    /// <para>
    /// The property <see cref="IdleTime"/> is reset each time step.
    /// </para>
    /// <para>
    /// The value is also scaled by <see cref="IGameTimer.Speed"/>. <see cref="IGameTimer.Time"/>, 
    /// <see cref="IGameTimer.DeltaTime"/>, <see cref="IdleTime"/>, and <see cref="LostTime"/> have
    /// the same scale. Except that <see cref="IdleTime"/> and <see cref="LostTime"/> cannot be
    /// negative. They always return the absolute value.
    /// </para>
    /// </remarks>
    public TimeSpan IdleTime { get; private set; }


    /// <summary>
    /// Gets the amount of time dropped in the current time step.
    /// </summary>
    /// <value>The lost time.</value>
    /// <remarks>
    /// <para>
    /// If a time step takes longer than <see cref="MaxNumberOfSteps"/> ∙ <see cref="StepSize"/> 
    /// seconds the time above this value is dropped. The dropped time will not be recovered in the 
    /// next time step. The time is lost.
    /// </para>
    /// <para>
    /// <see cref="LostTime"/> is adjusted each time step and only refers to the current time step. 
    /// (The lost time of multiple time steps is not accumulated.)
    /// </para>
    /// <para>
    /// A value greater than 0 indicates that the application is running too slow: The last time 
    /// step took longer than <see cref="MaxNumberOfSteps"/> · <see cref="StepSize"/> seconds to 
    /// compute.
    /// </para>
    /// <para>
    /// The value is also scaled by <see cref="IGameTimer.Speed"/>. <see cref="IGameTimer.Time"/>, 
    /// <see cref="IGameTimer.DeltaTime"/>, <see cref="IdleTime"/>, and <see cref="LostTime"/> have
    /// the same scale. Except that <see cref="IdleTime"/> and <see cref="LostTime"/> cannot be
    /// negative. They always return the absolute value.
    /// </para>
    /// </remarks>
    public TimeSpan LostTime { get; private set; }


    /// <summary>
    /// Gets the accumulated time.
    /// </summary>
    /// <value>The accumulated time.</value>
    /// <remarks>
    /// <para>
    /// The timer accumulates the time in <see cref="AccumulatedTime"/> until at least 
    /// <see cref="StepSize"/> seconds have passed before a new time step 
    /// (<see cref="IGameTimer.TimeChanged"/> event) is performed. During a 
    /// <see cref="TimeChanged"/> event <see cref="AccumulatedTime"/> is zero if exactly
    /// <see cref="StepSize"/> seconds have passed since the last <see cref="TimeChanged"/> event. 
    /// If more time has passed, <see cref="AccumulatedTime"/> contains the exceeding time. 
    /// <see cref="AccumulatedTime"/> is always less than <see cref="StepSize"/>.
    /// </para>
    /// <para>
    /// During <see cref="Idle"/> events, <see cref="AccumulatedTime"/> is equal to the elapsed time
    /// since the last <see cref="TimeChanged"/> event, and following is true:
    /// <see cref="AccumulatedTime"/> + <see cref="IdleTime"/> = <see cref="StepSize"/>.
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
    /// Gets or sets a value indicating whether the time steps are accumulated.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if time steps are accumulated; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When this value is <see langword="true"/> (default) and a time interval greater than the 
    /// <see cref="StepSize"/> has passed then <see cref="IGameTimer.TimeChanged"/> event can be 
    /// triggered with a <see cref="DeltaTime"/> that is an integer multiple of the 
    /// <see cref="StepSize"/>. 
    /// </para>
    /// <para>
    /// In contrast: If this value is <see langword="false"/>, then the 
    /// <see cref="IGameTimer.TimeChanged"/> event is always trigged with a <see cref="DeltaTime"/> 
    /// that is equal to <see cref="StepSize"/>. If a timer interval greater than 
    /// <see cref="StepSize"/> has passed, several <see cref="IGameTimer.TimeChanged"/> events will 
    /// be triggered in a row.
    /// </para>
    /// <para>
    /// If <see cref="AccumulateTimeSteps"/> is <see langword="false"/> then the method that handles
    /// the <see cref="IGameTimer.TimeChanged"/> event can be sure that the <see cref="DeltaTime"/> 
    /// is always constant. The disadvantage is that multiple <see cref="IGameTimer.TimeChanged"/> 
    /// events can be triggered in a sequence which might not be optimal for performance (i.e. 
    /// overhead in the event handler could be executed more often than necessary).
    /// </para>
    /// </remarks>
    public bool AccumulateTimeSteps { get; set; }


    /// <summary>
    /// Gets or sets the minimal size of a time step.
    /// </summary>
    /// <value>The minimal size of a time step. The default value is 1/60 seconds.</value>
    /// <remarks>
    /// <para>
    /// <see cref="StepSize"/> defines the minimal size of a time step. The size of a time step will
    /// be <see cref="StepSize"/> or an integer multiple thereof (maximal: 
    /// <see cref="MaxNumberOfSteps"/> ∙ <see cref="StepSize"/>).
    /// </para>
    /// <para>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public TimeSpan StepSize
    {
      get { return _stepSize; }
      set
      {
        if (value <= TimeSpan.Zero)
          throw new ArgumentException("StepSize must be greater than 0.");

        _stepSize = value;
        _maxStepSize = new TimeSpan(_stepSize.Ticks * _maxNumberOfSteps);
      }
    }
    private TimeSpan _stepSize;


    /// <summary>
    /// Gets or sets the allowed step size deviation.
    /// </summary>
    /// <value>The allowed step size deviation. The default value is 0.25 ms.</value>
    /// <remarks>
    /// If the difference of the elapsed time and the target <see cref="StepSize"/> is less than
    /// <see cref="StepSizeTolerance"/>, then the deviation is ignored and the elapsed time is
    /// clamped to exactly <see cref="StepSize"/>. A small allowed deviation is important if a game
    /// with a target frame rate of 60 Hz uses vsync and runs on a monitor with a different frame
    /// rate, e.g. 59.94 Hz NTSC display.
    /// </remarks>
    public TimeSpan StepSizeTolerance { get; set; }


    /// <summary>
    /// Gets or sets the maximal number of sub-steps for one time step.
    /// </summary>
    /// <value>
    /// The maximal number of sub-steps for one time step. The default value is 8.
    /// </value>
    /// <remarks>
    /// This value implicitly defines the maximum size of a time step. A time step will be an 
    /// integer multiple of <see cref="StepSize"/>, but maximal 
    /// <see cref="MaxNumberOfSteps"/> ∙ <see cref="StepSize"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int MaxNumberOfSteps
    {
      get { return _maxNumberOfSteps; }
      set
      {
        if (value <= 0)
          throw new ArgumentException("MaxNumberOfSteps must be greater than 0.");

        _maxNumberOfSteps = value;
        _maxStepSize = new TimeSpan(_stepSize.Ticks * _maxNumberOfSteps);
      }
    }
    private int _maxNumberOfSteps;


    /// <summary>
    /// Gets the number of accumulated steps.
    /// </summary>
    /// <value>The number of accumulated steps.</value>
    /// <remarks>
    /// <para>
    /// This property is only set during a <see cref="TimeChanged"/> event. After each 
    /// <see cref="TimeChanged"/> event it is reset to 0. During a <see cref="TimeChanged"/> event 
    /// <see cref="AccumulatedSteps"/> indicates how many fixed time steps have been accumulated. In
    /// other words:
    /// </para>
    /// <para>
    /// <see cref="AccumulatedSteps"/> = |<see cref="IGameTimer.DeltaTime"/> / <see cref="StepSize"/>|
    /// </para>
    /// <para>
    /// If the game is fast enough or if <see cref="AccumulateTimeSteps"/> is <see langword="false"/>,
    /// <see cref="AccumulatedSteps"/> is exactly 1 during a <see cref="TimeChanged"/> event. Only
    /// if the game is running slowly and <see cref="AccumulateTimeSteps"/> is <see langword="true"/>,
    /// <see cref="AccumulatedSteps"/> can be greater than 1.
    /// </para>
    /// </remarks>
    public int AccumulatedSteps { get; private set; }


    /// <summary>
    /// Gets the number of pending steps.
    /// </summary>
    /// <value>The number of pending steps.</value>
    /// <remarks>
    /// <para>
    /// During a time step (when <see cref="TimeChanged"/> is raised), this property indicates how 
    /// many time steps are still pending. 
    /// </para>
    /// <para>
    /// If the game is fast enough or if <see cref="AccumulateTimeSteps"/> is <see langword="true"/>,
    /// <see cref="PendingSteps"/> is always 0 during a <see cref="TimeChanged"/> event. Only
    /// if the game is running slowly and <see cref="AccumulateTimeSteps"/> is <see langword="false"/>,
    /// <see cref="PendingSteps"/> can be greater than 1 and indicates how many <see cref="TimeChanged"/>
    /// events will be triggered immediately after the current <see cref="TimeChanged"/> event.
    /// </para>
    /// </remarks>
    public int PendingSteps { get; private set; } 
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedStepTimer"/> class.
    /// </summary>
    /// <param name="clock">The clock.</param>
    public FixedStepTimer(IGameClock clock)
    {
      Speed = 1.0;
      _clock = clock;

      StepSize = new TimeSpan(166666);
      StepSizeTolerance = new TimeSpan(2500);
      MaxNumberOfSteps = 8;
      AccumulateTimeSteps = true;
      _maxStepSize = new TimeSpan(StepSize.Ticks * MaxNumberOfSteps);
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
      AccumulatedSteps = 0;
      PendingSteps = 0;
    }


    private void OnClockTimeChanged(object sender, GameClockEventArgs eventArgs)
    {
      // Scale incoming deltaTime.
      long ticks = (long)(eventArgs.DeltaTime.Ticks * Speed);

      // Determine time since last time step
      AccumulatedTime += new TimeSpan(ticks);
      TimeSpan duration = AccumulatedTime.Duration(); // Absolute value

      // Ignore small deviations (e.g. on 59.94 Hz displays).
      if (Math.Abs(duration.Ticks - StepSize.Ticks) < StepSizeTolerance.Ticks)
        duration = StepSize;

      if (duration < StepSize)
      {
        // ----- Idle

        // Time left before next time step.
        // Set idle time and trigger Idle event.
        DeltaTime = TimeSpan.Zero;
        LostTime = TimeSpan.Zero;
        IdleTime = StepSize - duration;

        UpdateEventArgs();
        OnIdle(_eventArgs);
      }
      else
      {
        // ----- Time Step

        if (duration > _maxStepSize)
        {
          // Drop time above MaxDeltaTime
          AccumulatedSteps = _maxNumberOfSteps;
          LostTime = duration - _maxStepSize;
          DeltaTime = (ticks >= 0) ? _maxStepSize : -_maxStepSize;
          AccumulatedTime = TimeSpan.Zero;
        }
        else
        {
          // Adjust Δt to an integer multiple of StepSize
          AccumulatedSteps = (int)(duration.Ticks / StepSize.Ticks);
          duration = new TimeSpan(AccumulatedSteps * StepSize.Ticks);
          LostTime = TimeSpan.Zero;
          DeltaTime = (ticks >= 0) ? duration : -duration;
          AccumulatedTime -= DeltaTime;
        }

        IdleTime = TimeSpan.Zero;
        
        // Update time and trigger next time step
        if (AccumulateTimeSteps)
        {
          // Trigger one big time step.
          FrameCount++;
          Time += DeltaTime;

          UpdateEventArgs();
          OnTimeChanged(_eventArgs);
        }
        else
        {
          // Trigger several constant time steps.
          var accumulatedTimeBackup = AccumulatedTime;

          // DeltaTime is always equal to the step size.
          DeltaTime = StepSize;

          // AccumulatedTime is 0. Only in the last time step it is set to the actual rest time.
          AccumulatedTime = TimeSpan.Zero;

          PendingSteps = AccumulatedSteps - 1;
          AccumulatedSteps = 1;
          for (; PendingSteps >= 0; PendingSteps--)
          {
            FrameCount++;
            Time += StepSize;

            if (PendingSteps == 0)
              AccumulatedTime = accumulatedTimeBackup;
           
            UpdateEventArgs();
            OnTimeChanged(_eventArgs);
          }

          Debug.Assert(AccumulatedTime == accumulatedTimeBackup);
        }

        AccumulatedSteps = 0;
        PendingSteps = 0;
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
    /// Raises the <see cref="Idle"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="GameTimerEventArgs"/> instance containing 
    /// the event data.
    /// </param>
    protected void OnIdle(GameTimerEventArgs eventArgs)
    {
      var handler = Idle;

      if (handler != null)
        handler(this, eventArgs);
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
    #endregion
  }
}
