using System;
using NUnit.Framework;


namespace DigitalRise.GameBase.Timing.Tests
{
  [TestFixture]
  public class FixedStepTimerTest
  {
    bool idleEventOccured;
    bool timeChangedEventOccured;
    GameTimerEventArgs idleEventArgs;
    GameTimerEventArgs timeEventArgs;


    [SetUp]
    public void Setup()
    {
      idleEventOccured = false;
      timeChangedEventOccured = false;
      idleEventArgs = null;
      timeEventArgs = null;
    }

    [Test]
    public void Constructor()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock);
      Assert.AreEqual(8, timer.MaxNumberOfSteps);
      Assert.AreEqual(TimeSpan.FromTicks(166666), timer.StepSize);

      timer = new FixedStepTimer(null);
      Assert.AreEqual(8, timer.MaxNumberOfSteps);
      Assert.AreEqual(TimeSpan.FromTicks(166666), timer.StepSize);
    }


    [Test]
    public void NormalRun()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;

      // Clock is running. Timer is stopped.
      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckNoTimeChangedEvent();

      // Start
      timer.Start();
      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(10), timer.DeltaTime);

      // Pause
      timer.Stop();
      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.Time);

      // Resume
      timer.Start();
      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.FromMilliseconds(30), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(10), timer.DeltaTime);
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      // Time step = 3 ∙ step size
      clock.Update(TimeSpan.FromMilliseconds(34));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(60), TimeSpan.FromMilliseconds(30));
      Assert.AreEqual(TimeSpan.FromMilliseconds(60), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(30), timer.DeltaTime);
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      // Time step = 3 ∙ step size
      // (27 ms + 4 ms from previous step = 31 ms)
      clock.Update(TimeSpan.FromMilliseconds(27));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(90), TimeSpan.FromMilliseconds(30));
      Assert.AreEqual(TimeSpan.FromMilliseconds(90), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(30), timer.DeltaTime);
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      // Time step = step size
      // (9 ms + 1 ms from previous step = 10 ms)
      clock.Update(TimeSpan.FromMilliseconds(9));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.FromMilliseconds(100), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(10), timer.DeltaTime);
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);
    }


    [Test]
    public void SwitchClocks()
    {
      ManualClock clock1 = new ManualClock();
      ManualClock clock2 = new ManualClock();
      clock1.Start();
      clock2.Start();

      IGameTimer timer = new FixedStepTimer(clock1)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
      };

      timer.TimeChanged += timer_TimeChanged;
      timer.Start();

      clock1.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));

      timer.Clock = clock2;
      Assert.AreSame(clock2, timer.Clock);
      clock1.Update(TimeSpan.FromMilliseconds(10));
      CheckNoTimeChangedEvent();

      clock2.Update(TimeSpan.FromMilliseconds(20));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(20));

      timer.Clock = null;
      Assert.IsNull(timer.Clock);
      clock1.Update(TimeSpan.FromMilliseconds(10));
      clock2.Update(TimeSpan.FromMilliseconds(20));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(30), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.DeltaTime);
    }

    [Test]
    public void TimerReset()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Reset();
      timer.Start();

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));

      timer.Reset();
      Assert.IsFalse(timer.IsRunning);
      Assert.AreEqual(TimeSpan.Zero, timer.Time);
      Assert.AreEqual(TimeSpan.Zero, timer.DeltaTime);

      timer.Start();
      clock.Update(TimeSpan.FromMilliseconds(10));
      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(10));

      timer.Stop();
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.Time);
      Assert.AreEqual(TimeSpan.Zero, timer.DeltaTime);

      timer.Reset();
      Assert.AreEqual(TimeSpan.Zero, timer.Time);
      Assert.AreEqual(TimeSpan.Zero, timer.DeltaTime);
    }

    [Test]
    public void IdleTime()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Start();

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.Zero, timer.IdleTime);

      clock.Update(TimeSpan.FromMilliseconds(3));
      CheckIdleEvent(TimeSpan.FromMilliseconds(7));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(7), timer.IdleTime);

      clock.Update(TimeSpan.FromMilliseconds(6));
      CheckIdleEvent(TimeSpan.FromMilliseconds(1));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(TimeSpan.FromMilliseconds(1), timer.IdleTime);

      clock.Update(TimeSpan.FromMilliseconds(12));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(20));
      Assert.AreEqual(TimeSpan.Zero, timer.IdleTime);
    }

    [Test]
    public void LostTime()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Start();

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(50));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(60), TimeSpan.FromMilliseconds(50));
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(51));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(110), TimeSpan.FromMilliseconds(50));
      Assert.AreEqual(TimeSpan.FromMilliseconds(1), timer.LostTime);

      clock.Update(TimeSpan.FromMilliseconds(49));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(150), TimeSpan.FromMilliseconds(40));
      Assert.AreEqual(TimeSpan.Zero, timer.LostTime);
    }

    [Test]
    public void AccumulatedTime()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Start();

      clock.Update(TimeSpan.FromMilliseconds(10));
      Assert.AreEqual(TimeSpan.Zero, timer.AccumulatedTime);

      clock.Update(TimeSpan.FromMilliseconds(3));
      Assert.AreEqual(TimeSpan.FromMilliseconds(3), timer.AccumulatedTime);

      clock.Update(TimeSpan.FromMilliseconds(6));
      Assert.AreEqual(TimeSpan.FromMilliseconds(9), timer.AccumulatedTime);

      clock.Update(TimeSpan.FromMilliseconds(12));
      Assert.AreEqual(TimeSpan.FromMilliseconds(1), timer.AccumulatedTime);

      // MaxNumberOfSteps exceeded:
      clock.Update(TimeSpan.FromMilliseconds(1000000));
      Assert.AreEqual(TimeSpan.Zero, timer.AccumulatedTime);

      // Accumulated steps.
      timer.AccumulateTimeSteps = true;
      clock.Update(TimeSpan.FromMilliseconds(22));
      Assert.AreEqual(TimeSpan.FromMilliseconds(2), timer.AccumulatedTime);

      timer.AccumulateTimeSteps = false;
      clock.Update(TimeSpan.FromMilliseconds(22));
      timer.TimeChanged += (s, e) =>
      {
        if (timer.PendingSteps > 0)
          Assert.AreEqual(TimeSpan.Zero, timer.AccumulatedTime);
        else
          Assert.AreEqual(TimeSpan.FromMilliseconds(4), timer.AccumulatedTime);
      };
    }

    [Test]
    public void AccumulatedStepsAndPendingSteps()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Start();

      timer.AccumulateTimeSteps = true;

      int desiredPendingSteps = 0;
      int desiredAccumlatedSteps = 2;

      timer.TimeChanged += (s, e) =>
      {
        Assert.AreEqual(desiredPendingSteps, timer.PendingSteps);
        Assert.AreEqual(desiredAccumlatedSteps, timer.AccumulatedSteps);
        desiredPendingSteps--;
      };

      clock.Update(TimeSpan.FromMilliseconds(22));

      timer.Reset();
      timer.Start();

      timer.AccumulateTimeSteps = false;

      desiredPendingSteps = 3;
      desiredAccumlatedSteps = 1;
      clock.Update(TimeSpan.FromMilliseconds(42));
      
      Assert.AreEqual(-1, desiredPendingSteps);
    }

    [Test]
    public void Scale()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.TimeChanged += timer_TimeChanged;
      timer.Start();
      Assert.AreEqual(1.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(10));

      timer.Speed = 0.5;
      Assert.AreEqual(0.5, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoTimeChangedEvent();

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(10));

      timer.Speed = 2.0;
      Assert.AreEqual(2.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(20));

      timer.Speed = -3.0;
      Assert.AreEqual(-3.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(-30));
      Assert.AreEqual(TimeSpan.FromMilliseconds(20), timer.Time);
      Assert.AreEqual(TimeSpan.FromMilliseconds(-30), timer.DeltaTime);
    }

    [Test]
    public void NegativeScale()
    {
      ManualClock clock = new ManualClock();
      clock.Start();

      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = TimeSpan.FromMilliseconds(10),
        MaxNumberOfSteps = 5,
      };
      timer.Idle += timer_Idle;
      timer.TimeChanged += timer_TimeChanged;
      timer.Speed = -2.0;
      timer.Start();
      Assert.AreEqual(-2.0, timer.Speed);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-20), TimeSpan.FromMilliseconds(-20));
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      clock.Update(TimeSpan.FromMilliseconds(10));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-40), TimeSpan.FromMilliseconds(-20));
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      clock.Update(TimeSpan.FromMilliseconds(21));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-80), TimeSpan.FromMilliseconds(-40));
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      clock.Update(TimeSpan.FromMilliseconds(2));
      CheckIdleEvent(TimeSpan.FromMilliseconds(4));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      clock.Update(TimeSpan.FromMilliseconds(4));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-90), TimeSpan.FromMilliseconds(-10));
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      clock.Update(TimeSpan.FromMilliseconds(1));
      CheckIdleEvent(TimeSpan.FromMilliseconds(4));
      CheckNoTimeChangedEvent();
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      clock.Update(TimeSpan.FromMilliseconds(13));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-120), TimeSpan.FromMilliseconds(-30));
      Assert.AreEqual(0, timer.AccumulatedSteps);
      Assert.AreEqual(0, timer.PendingSteps);

      // 1 ms wall clock time remaining from previous step.

      clock.Update(TimeSpan.FromMilliseconds(25));
      CheckNoIdleEvent();
      CheckTimeChangedEvent(TimeSpan.FromMilliseconds(-170), TimeSpan.FromMilliseconds(-50));
      Assert.AreEqual(TimeSpan.FromMilliseconds(2), timer.LostTime);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidStepSize()
    {
      FixedStepTimer timer = new FixedStepTimer(null);
      timer.StepSize = -TimeSpan.FromMilliseconds(10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidMaxNumberOfSteps()
    {
      FixedStepTimer timer = new FixedStepTimer(null);
      timer.MaxNumberOfSteps = -8;
    }


    [Test]
    public void AccumlateTimeSteps()
    {
      int handleTimeStepCallCount = 0;

      ManualClock clock = new ManualClock();
      clock.Start();

      var timeIncrement = new TimeSpan(166666);
      FixedStepTimer timer = new FixedStepTimer(clock)
      {
        StepSize = timeIncrement,
        MaxNumberOfSteps = 8,
      };
      timer.TimeChanged += (obj, timeEventArgs) =>
                             {
                               Assert.AreEqual(timeIncrement, timeEventArgs.DeltaTime);
                               handleTimeStepCallCount++;
                             };

      Assert.AreEqual(true, timer.AccumulateTimeSteps);
      timer.AccumulateTimeSteps = false;
      Assert.AreEqual(false, timer.AccumulateTimeSteps);

      timer.Start();
      handleTimeStepCallCount = 0;
      clock.Update(new TimeSpan(timeIncrement.Ticks * 6));
      Assert.AreEqual(6, handleTimeStepCallCount);

      handleTimeStepCallCount = 0;
      clock.Update(timeIncrement);
      Assert.AreEqual(1, handleTimeStepCallCount);

      handleTimeStepCallCount = 0;
      clock.Update(new TimeSpan(timeIncrement.Ticks / 2));
      Assert.AreEqual(0, handleTimeStepCallCount);
      clock.Update(new TimeSpan(timeIncrement.Ticks / 2));
      Assert.AreEqual(1, handleTimeStepCallCount);
    }


    //--------------------------------------------------------------
    #region Helpers for Events
    //--------------------------------------------------------------

    void timer_Idle(object sender, GameTimerEventArgs eventArgs)
    {
      idleEventOccured = true;
      idleEventArgs = eventArgs;
    }

    void timer_TimeChanged(object sender, GameTimerEventArgs eventArgs)
    {
      timeChangedEventOccured = true;
      timeEventArgs = eventArgs;
    }

    void CheckNoIdleEvent()
    {
      Assert.IsFalse(idleEventOccured);
    }

    void CheckNoTimeChangedEvent()
    {
      Assert.IsFalse(timeChangedEventOccured);
    }

    void CheckIdleEvent(TimeSpan expectedIdleTime)
    {
      Assert.IsTrue(idleEventOccured);
      Assert.AreEqual(expectedIdleTime, idleEventArgs.IdleTime);
      idleEventOccured = false;
    }

    void CheckTimeChangedEvent(TimeSpan expectedTime, TimeSpan expectedDeltaTime)
    {
      Assert.IsTrue(timeChangedEventOccured);
      Assert.AreEqual(expectedTime, timeEventArgs.Time);
      Assert.AreEqual(expectedDeltaTime, timeEventArgs.DeltaTime);
      timeChangedEventOccured = false;
    }
    #endregion
  }
}
