﻿using DigitalRise.Geometry.Shapes;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Geometry.Partitioning.Tests
{
  [TestFixture]
  public class CompressedAabbTreeTest
  {
    private Aabb GetAabbForItem(int i)
    {
      switch (i)
      {
        case 0:
          return new Aabb(new Vector3(float.NegativeInfinity), new Vector3(float.PositiveInfinity));
        case 1:
          return new Aabb(new Vector3(-1), new Vector3(2));
        case 2:
          return new Aabb(new Vector3(1), new Vector3(3));
        case 3:
          return new Aabb(new Vector3(4), new Vector3(5));
        case 4:
          return new Aabb(new Vector3(0), new Vector3(1, float.NaN, 1));
        default:
          return new Aabb(new Vector3(), new Vector3());
      }
    }


    [Test]
    public void Infinite()
    {
      GlobalSettings.ValidationLevel = 0xff;

      var partition = new CompressedAabbTree
      {
        EnableSelfOverlaps = true,
        GetAabbForItem = GetAabbForItem
      };

      partition.Add(1);
      partition.Add(0);
      partition.Add(2);
      partition.Add(3);

      // Exception because CompressedAabbTree cannot handle infinite.
      Assert.Throws<GeometryException>(() => partition.Update(false));
    }


    [Test]
    public void NaN()
    {
      var partition = new CompressedAabbTree
      {
        EnableSelfOverlaps = true,
        GetAabbForItem = GetAabbForItem
      };

      partition.Add(1);
      partition.Add(4);
      partition.Add(2);
      partition.Add(3);

      // Aabb builder throws exception.
      Assert.Throws<GeometryException>(() => partition.Update(false));

      partition = new CompressedAabbTree();
      partition.EnableSelfOverlaps = true;
      partition.GetAabbForItem = GetAabbForItem;
      partition.BottomUpBuildThreshold = 0;

      partition.Add(1);
      partition.Add(4);
      partition.Add(2);
      partition.Add(3);

      // Full rebuild. CompressedAabbTree throws exception when computing quantization.
      Assert.Throws<GeometryException>(() => partition.Update(true));

      partition = new CompressedAabbTree();
      partition.EnableSelfOverlaps = true;
      partition.GetAabbForItem = GetAabbForItem;
      partition.BottomUpBuildThreshold = 0;

      partition.Add(1);
      partition.Add(2);
      partition.Add(3);
      partition.Update(true);
      partition.Add(4);

      // Partial rebuild. CompressedAabbTree throws exception when computing quantization.
      Assert.Throws<GeometryException>(() => partition.Update(false));
    }


    [Test]
    public void Clone()
    {
      CompressedAabbTree partition = new CompressedAabbTree();
      partition.GetAabbForItem = i => new Aabb();
      partition.EnableSelfOverlaps = true;
      partition.Filter = new DelegatePairFilter<int>(pair => true);
      partition.Add(0);
      partition.Add(1);
      partition.Add(2);
      partition.Add(3);

      var clone = partition.Clone();
      Assert.NotNull(clone);
      Assert.AreNotSame(clone, partition);
      Assert.AreEqual(clone.EnableSelfOverlaps, partition.EnableSelfOverlaps);
      Assert.AreEqual(clone.Filter, partition.Filter);
      Assert.AreEqual(0, clone.Count);

      clone.Add(0);
      Assert.AreEqual(4, partition.Count);
      Assert.AreEqual(1, clone.Count);
    }
  }
}
