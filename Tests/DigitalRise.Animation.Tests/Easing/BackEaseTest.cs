﻿using DigitalRise.Mathematics;
using NUnit.Framework;


namespace DigitalRise.Animation.Easing.Tests
{
  [TestFixture]
  public class BackEaseTest : BaseEasingFunctionTest<BackEase>
  {
    [SetUp]
    public void Setup()
    {
      EasingFunction = new BackEase();
    }


    [Test]
    public void EaseInTest()
    {
      EasingFunction.Mode = EasingMode.EaseIn;
      TestEase();
    }


    [Test]
    public void EaseOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseOut;
      TestEase();
    }


    [Test]
    public void EaseInOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseInOut;
      TestEase();

      // Check center.
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");
    }
  }
}

