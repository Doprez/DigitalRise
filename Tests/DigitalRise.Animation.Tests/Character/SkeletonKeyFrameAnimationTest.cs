﻿using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Animation.Character.Tests
{
  [TestFixture]
  public class SkeletonKeyFrameAnimationTest
  {
    [Test]
    public void MissingWeights()
    {
      var animation = new SkeletonKeyFrameAnimation();

      animation.AddKeyFrame(0, TimeSpan.FromSeconds(0), new SrtTransform(Quaternion.Identity));
      animation.AddKeyFrame(1, TimeSpan.FromSeconds(0), new SrtTransform(Quaternion.Identity));

      Assert.AreEqual(1.0f, animation.GetWeight(0));
      Assert.AreEqual(1.0f, animation.GetWeight(1));

      animation.Freeze();

      Assert.AreEqual(1.0f, animation.GetWeight(0));
    }


    [Test]
    public void PartiallyMissingWeight()
    {
      var animation = new SkeletonKeyFrameAnimation();

      animation.AddKeyFrame(0, TimeSpan.FromSeconds(0), new SrtTransform(Quaternion.Identity));
      animation.AddKeyFrame(1, TimeSpan.FromSeconds(0), new SrtTransform(Quaternion.Identity));
      animation.SetWeight(0, 0.5f);

      Assert.AreEqual(0.5f, animation.GetWeight(0));
      Assert.AreEqual(1.0f, animation.GetWeight(1));

      animation.Freeze();

      Assert.AreEqual(0.5f, animation.GetWeight(0));
      Assert.AreEqual(1.0f, animation.GetWeight(1));
    }
  }
}
