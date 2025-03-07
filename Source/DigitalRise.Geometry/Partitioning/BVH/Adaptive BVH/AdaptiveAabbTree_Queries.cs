﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise.Geometry.Shapes;

#if !POOL_ENUMERABLES
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Ray = DigitalRise.Geometry.Shapes.Ray;
#endif


namespace DigitalRise.Geometry.Partitioning
{
  partial class AdaptiveAabbTree<T>
  {
    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Aabb aabb)
    {
      UpdateInternal();

#if !POOL_ENUMERABLES
      if (_root == null)
        yield break;

      var stack = DigitalRise.ResourcePools<Node>.Stacks.Obtain();
      stack.Push(_root);
      while (stack.Count > 0)
      {
        Node node = stack.Pop();
        node.IsActive = true;

        if (GeometryHelper.HaveContact(node.Aabb, aabb))
        {
          if (node.IsLeaf)
          {
            yield return node.Item;
          }
          else
          {
            SplitIfNecessary(node);
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
          }
        }
      }

      DigitalRise.ResourcePools<Node>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetOverlapsWork.Create(this, ref aabb);
#endif
    }


    ///// <summary>
    ///// Gets the items that touch the given leaf node.
    ///// </summary>
    ///// <param name="leaf">The axis-aligned bounding box.</param>
    ///// <returns>All items that touch the given AABB.</returns>
    ///// <remarks>
    ///// Filtering (see <see cref="BasePartition{T}.Filter"/>) is not applied.
    ///// </remarks>
    //private IEnumerable<T> GetOverlaps(Node leaf)
    //{
    //  // Note: This method is the same as GetOverlaps(Aabb), except that before checking 
    //  // the AABBs we compare the nodes. This removes some unnecessary AABB checks when 
    //  // computing self-overlaps.

    //  Debug.Assert(leaf.IsLeaf);

    //  Update(false);

    //  if (_root == null)
    //    yield break;

    //  var stack = DigitalRise.ResourcePools<Node>.Stacks.Obtain();
    //  stack.Push(_root);
    //  while (stack.Count > 0)
    //  {
    //    Node node = stack.Pop();

    //    node.IsActive = true;

    //    if (node != leaf && GeometryHelper.HaveContact(node.Aabb, leaf.Aabb))
    //    {
    //      if (node.IsLeaf)
    //      {
    //        yield return node.Item;
    //      }
    //      else
    //      {
    //        SplitIfNecessary(node);
    //        stack.Push(node.RightChild);
    //        stack.Push(node.LeftChild);
    //      }
    //    }
    //  }

    //  DigitalRise.ResourcePools<Node>.Stacks.Recycle(stack);
    //}


    /// <summary>
    /// Gets the leaf nodes that touch the given AABB. (Same as <see cref="GetOverlaps(Aabb)"/>
    /// except we directly return the AABB tree node.)
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box.</param>
    /// <returns>All leaf nodes that touch the given AABB.</returns>
    /// <remarks>
    /// Filtering (see <see cref="BasePartition{T}.Filter"/>) is not applied.
    /// </remarks>
    private IEnumerable<Node> GetLeafNodes(Aabb aabb)
    {
      // Note: This methods is the same as GetOverlaps(Aabb), but instead of returning items we 
      // return the nodes directly. This is used in tree vs. tree tests, so we do not have to 
      // recompute the AABBs of each leaf node.

      UpdateInternal();

#if !POOL_ENUMERABLES
      if (_root == null)
        yield break;

      var stack = DigitalRise.ResourcePools<Node>.Stacks.Obtain();
      stack.Push(_root);
      while (stack.Count > 0)
      {
        Node node = stack.Pop();
        node.IsActive = true;

        if (GeometryHelper.HaveContact(node.Aabb, aabb))
        {
          if (node.IsLeaf)
          {
            yield return node;
          }
          else
          {
            SplitIfNecessary(node);
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
          }
        }
      }

      DigitalRise.ResourcePools<Node>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetLeafNodesWork.Create(this, ref aabb);
#endif
    }


    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Ray ray)
    {
      UpdateInternal();

#if !POOL_ENUMERABLES
      if (_root == null)
        yield break;

      var rayDirectionInverse = new Vector3(
            1 / ray.Direction.X,
            1 / ray.Direction.Y,
            1 / ray.Direction.Z);

      float epsilon = Numeric.EpsilonF * (1 + Aabb.Extent.Length());

      var stack = DigitalRise.ResourcePools<Node>.Stacks.Obtain();
      stack.Push(_root);
      while (stack.Count > 0)
      {
        var node = stack.Pop();
        node.IsActive = true;

        if (GeometryHelper.HaveContact(node.Aabb, ray.Origin, rayDirectionInverse, ray.Length, epsilon))
        {
          if (node.IsLeaf)
          {
            yield return node.Item;
          }
          else
          {
            SplitIfNecessary(node);
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
          }
        }
      }

      DigitalRise.ResourcePools<Node>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetOverlapsWithRayWork.Create(this, ref ray);
#endif
    }


    /// <inheritdoc/>
    public float GetClosestPointCandidates(Aabb aabb, float maxDistanceSquared, Func<T, float> callback)
    {
      if (callback == null)
        throw new ArgumentNullException("callback");

      UpdateInternal();

      if (_root == null)
        return -1;

      float closestPointDistanceSquared = maxDistanceSquared;
      GetClosestPointCandidatesImpl(_root, aabb, callback, ref closestPointDistanceSquared);
      return closestPointDistanceSquared;
    }


    private static void GetClosestPointCandidatesImpl(Node node, Aabb aabb, Func<T, float> callback, ref float closestPointDistanceSquared)
    {
      // closestPointDistanceSquared == -1 indicates early exit.
      if (closestPointDistanceSquared < 0)
      {
        // Abort.
        return;
      }

      node.IsActive = true;

      // If we have a contact, it is not necessary to examine nodes with no AABB contact
      // because they cannot give a closer point pair.
      if (closestPointDistanceSquared == 0 && !GeometryHelper.HaveContact(aabb, node.Aabb))
        return;

      if (node.IsLeaf)
      {
        // Node is leaf - call callback and updated closest-point distance.
        var leafDistanceSquared = callback(node.Item);
        closestPointDistanceSquared = Math.Min(leafDistanceSquared, closestPointDistanceSquared);
        return;
      }

      SplitIfNecessary(node);
      Node leftChild = node.LeftChild;
      Node rightChild = node.RightChild;

      if (closestPointDistanceSquared == 0)
      {
        // We have contact, so we must examine all children.
        GetClosestPointCandidatesImpl(leftChild, aabb, callback, ref closestPointDistanceSquared);
        GetClosestPointCandidatesImpl(rightChild, aabb, callback, ref closestPointDistanceSquared);
        return;
      }

      // No contact. Use lower bound estimates to search the best nodes first.
      float minDistanceLeft = GeometryHelper.GetDistanceSquared(aabb, leftChild.Aabb);
      float minDistanceRight = GeometryHelper.GetDistanceSquared(aabb, rightChild.Aabb);

      if (minDistanceLeft < minDistanceRight)
      {
        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
        if (minDistanceLeft > closestPointDistanceSquared)
          return;

        // Handle left first.
        GetClosestPointCandidatesImpl(leftChild, aabb, callback, ref closestPointDistanceSquared);

        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
        if (minDistanceRight > closestPointDistanceSquared)
          return;

        GetClosestPointCandidatesImpl(rightChild, aabb, callback, ref closestPointDistanceSquared);
      }
      else
      {
        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
        if (minDistanceRight > closestPointDistanceSquared)
          return;

        // Handle right first.
        GetClosestPointCandidatesImpl(rightChild, aabb, callback, ref closestPointDistanceSquared);

        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
        if (minDistanceLeft > closestPointDistanceSquared)
          return;

        GetClosestPointCandidatesImpl(leftChild, aabb, callback, ref closestPointDistanceSquared);
      }
    }
  }
}
