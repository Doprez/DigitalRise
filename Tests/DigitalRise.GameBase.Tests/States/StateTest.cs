﻿using System;
using NUnit.Framework;


namespace DigitalRise.GameBase.States.Tests
{
  [TestFixture]
  public class StateTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddingTransitionTwiceThrowsException()
    {
      var s = new State();
      var t = new Transition();
      t.SourceState = s;

      s.Transitions.Add(t);   // Exception: Cannot add duplicate. t was already added before.
    }


    [Test]
    public void AddingTransitionTwiceIsIgnored2()
    {
      var s = new State();
      var t = new Transition();
      s.Transitions.Add(t);
      t.SourceState = s;          // This is ignored.

      Assert.AreEqual(1, s.Transitions.Count);
    }
  }
}
