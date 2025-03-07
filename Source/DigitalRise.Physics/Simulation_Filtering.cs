// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Collections;
using DigitalRise.Geometry.Collisions;
using DigitalRise.Physics.Constraints;


namespace DigitalRise.Physics
{
  // Constraints have a CollisionEnabled flag. The simulation has to modify the collision filter
  // of the collision detection to disable collisions between rigid bodies that are connected
  // by constraints where the CollisionEnabled is set.
  //
  // This works like this:
  // Each constraint calls Simulation.RegisterInCollisionFilter/UnregisterFromCollisionFilter
  // if CollisionEnabled is false and if the constraints is enabled. The simulation keeps
  // track of the registered constraints and disables collisions for a body pair when at least
  // one constraint for this pair is registered.

  public partial class Simulation
  {
    // One list of constraints for a body pair.
    // If a list contains at least one constraint, then the collisions between this body pair
    // are disabled. If an list becomes empty or is removed, the collision are re-enabled.
    private readonly Dictionary<Pair<CollisionObject>, List<Constraint>> _constraints = new Dictionary<Pair<CollisionObject>, List<Constraint>>();


    /// <summary>
    /// Adds the specified constraint to the collision filter.
    /// </summary>
    /// <param name="constraint">The constraint to be registered.</param>
    internal void RegisterInCollisionFilter(Constraint constraint)
    {
      Debug.Assert(constraint != null, "constraint is null.");
      Debug.Assert(constraint.BodyA != null, "constraint.BodyA is null.");
      Debug.Assert(constraint.BodyB != null, "constraint.BodyB is null.");
      Debug.Assert(constraint.Enabled && !constraint.CollisionEnabled, "Constraints should only be registered if it disabled collisions.");

      var pair = new Pair<CollisionObject>(constraint.BodyA.CollisionObject, constraint.BodyB.CollisionObject);
      List<Constraint> constraints;
      if (!_constraints.TryGetValue(pair, out constraints))
      {
        // Create new list of constraints for the current pair.
        constraints = ResourcePools<Constraint>.Lists.Obtain();
        _constraints[pair] = constraints;
      }

      if (!constraints.Contains(constraint))
      {
        if (constraints.Count == 0)
        {
          var filter = CollisionDomain.CollisionDetection.CollisionFilter as ICollisionFilter;
          if (filter != null)
            filter.Set(pair.First, pair.Second, false);
        }

        constraints.Add(constraint);
      }
    }


    /// <summary>
    /// Removes the specified constraint from the collision filter.
    /// </summary>
    /// <param name="constraint">The constraint to be removed.</param>
    internal void UnregisterFromCollisionFilter(Constraint constraint)
    {
      Debug.Assert(constraint != null, "constraint is null.");
      Debug.Assert(constraint.BodyA != null, "constraint.BodyA is null.");
      Debug.Assert(constraint.BodyB != null, "constraint.BodyB is null.");

      var pair = new Pair<CollisionObject>(constraint.BodyA.CollisionObject, constraint.BodyB.CollisionObject);
      Debug.Assert(_constraints.ContainsKey(pair), "No constraint is stored in the collision filter for the given pair of collision objects.");
      List<Constraint> constraints = _constraints[pair];
      Debug.Assert(constraints.Contains(constraint));
      if (constraints.Count == 1)
      {
        // The constraint is the only constraint of this pair.
        // --> Remove the entry for this pair.
        Debug.Assert(constraints[0] == constraint, "Constraint is not registered in collision filter.");
        ResourcePools<Constraint>.Lists.Recycle(constraints);
        _constraints.Remove(pair);

        // Re-enable collisions.
        var filter = CollisionDomain.CollisionDetection.CollisionFilter as ICollisionFilter;
        if (filter != null)
          filter.Set(pair.First, pair.Second, true);
      }
      else
      {
        // Remove the constraint from the list.
        constraints.Remove(constraint);
      }
    }
  }
}
