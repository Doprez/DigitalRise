// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using DigitalRise.Collections;
using DigitalRise.Geometry.Partitioning;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Linq;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Ray = DigitalRise.Geometry.Shapes.Ray;

namespace DigitalRise.Geometry.Collisions
{
  /// <summary>
  /// A collision domain that manages collision objects.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="CollisionDetection"/> can be used for ad hoc collision queries between any two
  /// objects. The <see cref="CollisionDomain"/> is designed to manage multiple collision objects at
  /// once and allows faster contact queries. 
  /// </para>
  /// <para>
  /// The method <see cref="Update(float)"/> must be called in each frame (time step) to update the 
  /// collision domain. <see cref="Update(float)"/> computes all collisions between all objects 
  /// inside the domain. The resulting contacts are stored in <see cref="ContactSets"/>. The 
  /// collision domain reuses collision data from the last frame. Additionally, if the property
  /// <see cref="EnableMultithreading"/> is set, the workload is distributed across multiple CPU 
  /// cores. Therefore the collision computation is much faster in comparison to ad hoc queries (as 
  /// in <see cref="CollisionDetection"/>).
  /// </para>
  /// <para>
  /// A <see cref="CollisionDomain"/> can only compute real contacts (geometric objects are touching
  /// or intersecting), but it does not calculate closest-point queries for separated objects. Use 
  /// <see cref="CollisionDetection"/> to calculate the closest-point queries.
  /// </para>
  /// </remarks>
  public class CollisionDomain
  {
    // Handling Ray.StopsAtFirstHit:
    // Currently, we compute all contact sets but do not add blocked ray contact sets to the
    // ContactSets collection. This brings no performance improvement. But the usability is improved 
    // because the user can create a room with many lasers and these lasers are automatically 
    // blocked. Thus, CollisionDomain.GetContacts(hero) will not return any contacts for blocked 
    // lasers.
    // For a performance improvement we could stop checking object pairs if the contact point cannot 
    // be nearer than the current nearest ray contact. The problem with this approach is: If a 
    // former blocking object is removed, we have to test the ray against the other objects again
    // even if the ray and the other objects did not move. - In other words the new nearest contact 
    // is not contained in the cached contact sets.
    //
    // Note: We could add the properties AlgorithmMatrix, CollisionFilter, ContactFilter if we want 
    // different CollisionDomains to use different settings. Normally, this is not required, 
    // therefore these properties can be set only per CollisionDetection.
    //
    // BroadPhase:
    // In Update(): The narrow phase is done for all overlaps of the BroadPhase.
    // This is necessary to update the contact lifetimes. If the contacts store a creation-time,
    // then these contact set do not need to be touched every frame. Then we would only need
    // to iterate over contact sets where one object has moved or one object is new. 
    // (Iterate over all contact sets if the collision filter was changed.)


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A clone of the CollisionDetection.CollisionAlgorithmMatrix. We can call
    // optimize on this clone without causing multi-threading problems.
    private CollisionAlgorithmMatrix _algorithmMatrix;
    // When CollisionDetection.CollisionAlgorithmMatrix changes, we must update the clone.
    private int _algorithmMatrixVersion;

    // A collection of all known rays with StopsAtFirstHit enabled and their first hit.
    private readonly Dictionary<CollisionObject, ContactSet> _rayCache = new Dictionary<CollisionObject, ContactSet>();
    private KeyValuePair<CollisionObject, ContactSet>[] _tempRayCache = new KeyValuePair<CollisionObject, ContactSet>[4];

    // Delegate stored to avoid garbage when multithreading is enabled.
    private readonly Action<int> _narrowPhaseMethod;

    // The following members are only set temporarily within Update() and used in NarrowPhase().
    // (Necessary to avoid garbage when multithreading.)

    // The size of the current sub-time step in seconds.
    private float _deltaTime;

    // The collision object that needs to be updated, null to update everything.
    private CollisionObject _collisionObject;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    // Gets the internal matrix if it is up-to-date; otherwise the matrix from CollisionDetection.
    private CollisionAlgorithmMatrix AlgorithmMatrix
    {
      get
      {
        if (_algorithmMatrixVersion != CollisionDetection.AlgorithmMatrix._version)
          return CollisionDetection.AlgorithmMatrix;

        return _algorithmMatrix;
      }
    }


    /// <summary>
    /// Gets the collision detection service.
    /// </summary>
    /// <value>The collision detection service.</value>
    public CollisionDetection CollisionDetection { get; private set; }


    /// <summary>
    /// Gets a collection of collision objects that are managed in this collision domain.
    /// </summary>
    /// <value>The collision objects of the collision domain.</value>
    public CollisionObjectCollection CollisionObjects { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether multithreading is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="true"/> if the current system has more than one CPU cores.
    /// </value>
    /// <remarks>
    /// <para>
    /// When multithreading is enabled the collision domain will distribute the workload across
    /// multiple processors (CPU cores) to improve the performance. 
    /// </para>
    /// <para>
    /// Multithreading adds an additional overhead, therefore it should only be enabled if the 
    /// current system has more than one CPU core and if the other cores are not fully utilized by
    /// the application. Multithreading should be disabled if the system has only one CPU core or
    /// if all other CPU cores are busy. In some cases it might be necessary to run a benchmark of
    /// the application and compare the performance with and without multithreading to decide
    /// whether multithreading should be enabled or not.
    /// </para>
    /// <para>
    /// The collision domain internally uses the class <see cref="Parallel"/> for parallelization.
    /// <see cref="Parallel"/> is a static class that defines how many worker threads are created, 
    /// how the workload is distributed among the worker threads and more. (See 
    /// <see cref="Parallel"/> to find out more on how to configure parallelization.)
    /// </para>
    /// </remarks>
    /// <seealso cref="Parallel"/>
    public bool EnableMultithreading { get; set; }


    /// <summary>
    /// Gets or sets the <see cref="ISpatialPartition{T}"/> that is used for the broad phase of
    /// the collision detection.
    /// </summary>
    /// <value>
    /// The broad phase spatial partitioning method. The default value is an instance of 
    /// <see cref="SweepAndPruneSpace{T}"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Collision detection with a <see cref="CollisionDomain"/> works in two phases: 
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// In the broad phase object pairs which cannot collide are sorted out using a spatial
    /// partitioning method.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// In the narrow phase the collision info (contact positions, normal vectors, etc.) is
    /// computed.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public ISpatialPartition<CollisionObject> BroadPhase
    {
      get { return InternalBroadPhase.SpatialPartition; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        InternalBroadPhase.SpatialPartition = value;
      }
    }


    /// <summary>
    /// Gets a collection with all contacts found in the collision domain.
    /// </summary>
    /// <value>The collection of all contacts found in the collision domain.</value>
    public ContactSetCollection ContactSets { get; private set; }


    /// <summary>
    /// Gets the number of AABB overlaps in the broad phase.
    /// </summary>
    /// <value>The number of AABB overlaps in the broad phase.</value>
    public int NumberOfBroadPhaseOverlaps
    {
      get { return InternalBroadPhase.CandidatePairs.Count; }
    }


    internal CollisionDetectionBroadPhase InternalBroadPhase { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionDomain"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionDomain"/> class.
    /// </summary>
    public CollisionDomain()
      : this(new CollisionDetection())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionDomain"/> class.
    /// </summary>
    /// <param name="collisionDetection">
    /// The collision detection instance that defines the settings (tolerance values,
    /// collision algorithm matrix, etc.) that this collision domain should use.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
    public CollisionDomain(CollisionDetection collisionDetection)
    {
      if (collisionDetection == null)
        collisionDetection = new CollisionDetection();

      CollisionDetection = collisionDetection;
      CollisionDetection.CollisionFilterChanged += OnCollisionFilterChanged;

      _algorithmMatrix = new CollisionAlgorithmMatrix(CollisionDetection.AlgorithmMatrix);
      _algorithmMatrixVersion = collisionDetection.AlgorithmMatrix._version;

      CollisionObjects = new CollisionObjectCollection();
      
      if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelUserHighExpensive) != 0)
        CollisionObjects.CollectionChanged += OnCollisionObjectsChangedValidation;
      
      CollisionObjects.CollectionChanged += OnCollisionObjectsChanged;

      ContactSets = new ContactSetCollection(this);

      InternalBroadPhase = new CollisionDetectionBroadPhase(this);

      // Enable multithreading by default if the current system has multiple processors.
      EnableMultithreading = Environment.ProcessorCount > 1;

       // Multithreading works but Parallel.For of Xamarin.Android/iOS is very inefficient.
#if ANDROID || IOS
        EnableMultithreading = false;
#endif

            // Store narrow phase method as Action<int> to avoid garbage when multithreading is enabled.
            _narrowPhaseMethod = NarrowPhase;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines whether the pose or shape of the involved collision objects were modified.
    /// </summary>
    /// <param name="pair">The pair of collision objects.</param>
    /// <returns>
    /// <see langword="true"/> if the pose or shape of the involved collision objects were modified; 
    /// otherwise, <see langword="false"/>.</returns>
    private static bool AreCollisionObjectsModified(ContactSet pair)
    {
      return pair.ObjectA.Changed || pair.ObjectB.Changed;
    }


    /// <summary>
    /// Called when the collision filter of the collision detection changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    private void OnCollisionFilterChanged(object sender, EventArgs eventArgs)
    {
      // Reset all cached filter results.
      foreach (var contactSet in InternalBroadPhase.CandidatePairs)
        contactSet.CanCollide = -1;
    }


    /// <summary>
    /// Called when collision objects were added or removed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="CollectionChangedEventArgs{T}"/> instance containing the event data.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// <see cref="CollisionObject.GeometricObject"/> of a newly added <see cref="CollisionObject"/> 
    /// is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void OnCollisionObjectsChanged(object sender, CollectionChangedEventArgs<CollisionObject> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Handle removed objects.
      var oldItems = eventArgs.OldItems;
      int numberOfOldItems = oldItems.Count;
      for (int i = 0; i < numberOfOldItems; i++)
      {
        CollisionObject oldItem = oldItems[i];

        // Reset the Domain property.
        oldItem.Domain = null;

        // Remove related contact sets.
        ContactSets.Remove(oldItem);

        // Remove from ray cache.
        _rayCache.Remove(oldItem);
      }

      // Handle new objects.
      var newItems = eventArgs.NewItems;
      int numberOfNewItems = newItems.Count;
      for (int i = 0; i < numberOfNewItems; i++)
      {
        CollisionObject newItem = newItems[i];

        // Set Domain property.
        newItem.Domain = this;

        // Ensure that CollisionObject has a valid geometric object.
        if (newItem.GeometricObject == null)
          throw new InvalidOperationException("CollisionObject.GeometricObject must not be null when the CollisionObject is added to a CollisionDomain.");

        // Add entry in ray cache.
        if (newItem.IsRayThatStopsAtFirstHit)
          _rayCache.Add(newItem, null);
      }
    }


    private static void OnCollisionObjectsChangedValidation(object sender, CollectionChangedEventArgs<CollisionObject> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Handle removed objects.
      var oldItems = eventArgs.OldItems;
      int numberOfOldItems = oldItems.Count;
      for (int i = 0; i < numberOfOldItems; i++)
      {
        var oldItem = oldItems[i];
        var geometricObject = oldItem.GeometricObject;
        if (geometricObject != null)
        {
          geometricObject.ShapeChanged -= ValidateGeometricObjectShape;
          geometricObject.PoseChanged -= ValidateGeometricObjectPose;
        }
      }

      // Handle new objects.
      var newItems = eventArgs.NewItems;
      int numberOfNewItems = newItems.Count;
      for (int i = 0; i < numberOfNewItems; i++)
      {
        var newItem = newItems[i];
        var geometricObject = newItem.GeometricObject;
        if (geometricObject != null)
        {
          geometricObject.ShapeChanged += ValidateGeometricObjectShape;
          geometricObject.PoseChanged += ValidateGeometricObjectPose;

          // Validate immediately.
          ValidateShape(geometricObject);
          ValidatePose(geometricObject);
        }
      }
    }


    private static void ValidateGeometricObjectShape(object sender, ShapeChangedEventArgs eventArgs)
    {
      ValidateShape(sender as GeometricObject);
    }


    private static void ValidateGeometricObjectPose(object sender, EventArgs eventArgs)
    {
      ValidatePose(sender as GeometricObject);
    }


    private static void ValidateShape(IGeometricObject geometricObject)
    {
      if (geometricObject == null)
        return;

      // Check for NaN.
      Vector3 scale = geometricObject.Scale;
      if (Numeric.IsNaN(scale.X) || Numeric.IsNaN(scale.Y) || Numeric.IsNaN(scale.Z))
        throw new GeometryException("Invalid scale! If a geometric object is part of a collision domain, the scale must not contain invalid values, e.g. NaN.");

      // Check for NaN.
      Aabb aabb = geometricObject.Shape.GetAabb();
      if (Numeric.IsNaN(aabb.Extent.X) || Numeric.IsNaN(aabb.Extent.Y) || Numeric.IsNaN(aabb.Extent.Z))
        throw new GeometryException("Invalid shape! If a geometric object is part of a collision domain, the shape must not contain invalid values, e.g. NaN.");
    }


    private static void ValidatePose(IGeometricObject geometricObject)
    {
      if (geometricObject == null)
        return;

      Pose pose = geometricObject.Pose;

      // Check if pose is valid. (User might have set pose to 'new Pose()' which is invalid.)
      if (pose.Orientation == Matrix33F.Zero)
        throw new GeometryException("Invalid pose! If a geometric object is part of a collision domain, the orientation must not be a zero matrix.");

      // Check for NaN.
      float value = pose.Position.X + pose.Position.Y + pose.Position.Z
                    + pose.Orientation.M00 + pose.Orientation.M01 + pose.Orientation.M02
                    + pose.Orientation.M10 + pose.Orientation.M11 + pose.Orientation.M12
                    + pose.Orientation.M20 + pose.Orientation.M21 + pose.Orientation.M22;
      if (!Numeric.IsFinite(value))
        throw new GeometryException("Invalid pose! If a geometric object is part of a collision domain, the pose must not contain invalid values, e.g. NaN or infinity.");
    }


    /// <summary>
    /// Performs collision filtering and determines whether the given collision objects can collide.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// <see langword="true"/> if the collision objects can collide; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    private bool CanCollide(CollisionObject objectA, CollisionObject objectB)
    {
      return objectA.Enabled && objectB.Enabled
             && (CollisionDetection.CollisionFilter == null
                 || CollisionDetection.CollisionFilter.Filter(new Pair<CollisionObject>(objectA, objectB)));
    }


    /// <summary>
    /// Performs collision filtering and determines whether the collision objects in the given
    /// contact set can collide.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <returns>
    /// <see langword="true"/> if the objects in the contact set can collide; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Similar to <see cref="CanCollide(CollisionObject,CollisionObject)"/>, but the filter result 
    /// is cached in the contact set (see <see cref="ContactSet.CanCollide"/> and reused.
    /// </remarks>
    internal bool CanCollide(ContactSet contactSet)
    {
      // Only the CanCollide flag of broad-phase contact sets is correctly reset in 
      // OnCollisionFilterChanged. This method should not be called for contact sets 
      // that are not in stored in the broad phase.
      Debug.Assert(
        InternalBroadPhase.CandidatePairs.Contains(contactSet),
        "Internal CanCollide(ContactSet) must only be used for contact sets of the broad-phase.");

      var objectA = contactSet.ObjectA;
      if (!objectA.Enabled)
        return false;

      var objectB = contactSet.ObjectB;
      if (!objectB.Enabled)
        return false;

      var canCollide = contactSet.CanCollide;
      if (canCollide == 1)
        return true;

      if (canCollide == 0)
        return false;

      Debug.Assert(canCollide == -1);

      // CanCollide is not cached: Check collision filter.
      bool result = CollisionDetection.CollisionFilter == null
                    || CollisionDetection.CollisionFilter.Filter(new Pair<CollisionObject>(objectA, objectB));

      // Cache result in contact set.
      contactSet.CanCollide = result ? 1 : 0;

      return result;
    }


    /// <overloads>
    /// <summary>
    /// Gets contact information from the collision domain.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets all contacts of the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <returns>
    /// All <see cref="ContactSet"/>s where <paramref name="collisionObject"/> is involved.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Each returned <see cref="ContactSet"/> contains a pair of collision objects  (
    /// <paramref name="collisionObject"/> and another object) that describes the contact between 
    /// the objects. The collision objects in the returned <see cref="ContactSet"/> can be swapped! 
    /// See <see cref="ContactSet"/> for more information on <i>swapped contact sets</i>.
    /// </para>
    /// <para>
    /// If <paramref name="collisionObject"/> is part of this <see cref="CollisionDomain"/>,
    /// then this method returns the currently cached contacts sets (which are stored in
    /// <see cref="ContactSets"/>). The contact sets are only updated, when
    /// <see cref="Update(System.TimeSpan)"/> is called. If the collision object has moved since the
    /// collision domain was updated last, the contact information will not be up-to-date. In this
    /// case you need to call <see cref="Update(System.TimeSpan)"/> again before calling
    /// <see cref="GetContacts(DigitalRise.Geometry.Collisions.CollisionObject)"/> to get
    /// up-to-date results.
    /// </para>
    /// <para>
    /// If <paramref name="collisionObject"/> is not part of this <see cref="CollisionDomain"/>,
    /// then this method automatically calculates the new contact information.
    /// </para>
    /// <para>
    /// If <paramref name="collisionObject"/> is part of this <see cref="CollisionDomain"/>, then
    /// the returned contact sets are managed by the domain. You must not modify or recycle the
    /// contact sets! However, if <paramref name="collisionObject"/> is not part of this
    /// <see cref="CollisionDomain"/>, the returned contact sets are not managed by the domain.
    /// The contact sets can be modified. When they are no longer needed, they should be recycled to
    /// avoid unnecessary memory allocations. For example:
    /// </para>
    /// <code lang="csharp" title="Recycling contact sets">
    /// <![CDATA[
    /// foreach (ContactSet contactSet in myCollisionDomain.GetContacts(myCollisionObject))
    /// {
    ///   // Check contact set.
    ///   ...
    ///   
    ///   Debug.Assert(myCollisionObject.Domain != myCollisionDomain);
    ///   foreach(var contact in contactSet)
    ///     contact.Recycle();
    ///   contactSet.Recycle();
    /// }
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collisionObject"/> is <see langword="null"/>.
    /// </exception>
    public IEnumerable<ContactSet> GetContacts(CollisionObject collisionObject)
    {
      if (collisionObject == null)
        throw new ArgumentNullException("collisionObject");

      if (collisionObject.Domain == this)
      {
        // ----- Object is in domain.
        return ContactSets.GetContacts(collisionObject);
      }
      else
      {
        // ----- Object is not in domain.

        // Early exit if collision object is disabled.
        if (collisionObject.Enabled == false)
          return LinqHelper.Empty<ContactSet>();

        var contactSets = DigitalRise.ResourcePools<ContactSet>.HashSets.Obtain();

        IEnumerable<CollisionObject> overlaps;
        if (collisionObject.IsRay)
        {
          RayShape rayShape = (RayShape)collisionObject.GeometricObject.Shape;
          Vector3 rayScale = collisionObject.GeometricObject.Scale;
          Pose rayPose = collisionObject.GeometricObject.Pose;

          Ray rayWorld = new Ray(rayShape);
          rayWorld.Scale(ref rayScale);
          rayWorld.ToWorld(ref rayPose);

          overlaps = BroadPhase.GetOverlaps(rayWorld);
        }
        else
        {
          overlaps = BroadPhase.GetOverlaps(collisionObject); 
        }

        // For all collision candidates with the collision object run the narrow phase.
        var algorithmMatrix = AlgorithmMatrix;
        foreach (var touchedObject in overlaps)
        {
          if (CanCollide(collisionObject, touchedObject))
          {
            var collisionAlgorithm = algorithmMatrix[collisionObject, touchedObject];
            var pair = collisionAlgorithm.GetContacts(collisionObject, touchedObject);
            
            // If the pair is not returned to the user, we have to recycle it.
            var recyclePair = true;

            if (pair.HaveContact)
            {
              #region ----- Handle Ray.StopsAtFirstHit and add contact set to collection. -----

              // If we have a contact, then we add this contact pair.
              // For rays with StopsAtFirstHit we add only closest contacts and update the _rayCache.
              // Note: A candidate pairs where HaveContact is true can only contain 1 ray because 
              // 2 rays do not collide.
              CollisionObject ray = null;
              if (pair.ObjectA.IsRayThatStopsAtFirstHit)
                ray = pair.ObjectA;
              else if (pair.ObjectB.IsRayThatStopsAtFirstHit)
                ray = pair.ObjectB;

              if (ray != null)
              {
                // Ray hit: Ray could be blocked by another object.
                // Only add the new contact if it is closer than any other contact.
                ContactSet cachedClosestContact;
                _rayCache.TryGetValue(ray, out cachedClosestContact);
                if (cachedClosestContact == null            // The ray is not blocked by another object.
                    || cachedClosestContact.Count == 0      // There is a potential blocker, but it has no useful contact info.
                    || (pair.Count > 0 && pair[0].PenetrationDepth < cachedClosestContact[0].PenetrationDepth) // There is another ray contact but the current is closer.
                   )
                {
                  // Update ray cache, but only if our collisionObject is the ray. Otherwise, do 
                  // not update the ray cache because the collision object is only temporarily 
                  // added to the collision domain.
                  if (ray == collisionObject)
                    _rayCache[ray] = pair;

                  // Remove previously closest hit, if any.
                  contactSets.Remove(cachedClosestContact);

                  // Add new closest hit with ray.
                  contactSets.Add(pair);
                  recyclePair = false;
                }
              }
              else
              {
                // Default case: Normal contact, no rays.
                // Add contact set.
                contactSets.Add(pair);
                recyclePair = false;
              }
              #endregion
            }

            if (recyclePair)
              pair.Recycle(true);
          }
        }

        // Above loop could have created rayCache entries. Remove them.
        if (collisionObject.IsRayThatStopsAtFirstHit)
          _rayCache.Remove(collisionObject);

#if !POOL_ENUMERABLES
        return contactSets;
#else
        return GetContactsWork.Create(contactSets);
#endif
      }
    }


#if POOL_ENUMERABLES
    private sealed class GetContactsWork : PooledEnumerable<ContactSet>
    {
      private static readonly ResourcePool<GetContactsWork> Pool = new ResourcePool<GetContactsWork>(() => new GetContactsWork(), x => x.Initialize(), null);
      private HashSet<ContactSet> _contactSets;
      private HashSet<ContactSet>.Enumerator _enumerator;

      public static IEnumerable<ContactSet> Create(HashSet<ContactSet> contactSets)
      {
        var enumerable = Pool.Obtain();
        enumerable._contactSets = contactSets;
        enumerable._enumerator = contactSets.GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out ContactSet current)
      {
        while (_enumerator.MoveNext())
        {
          current = _enumerator.Current;
          return true;
        }

        current = null;
        return false;
      }

      protected override void OnRecycle()
      {
        DigitalRise.ResourcePools<ContactSet>.HashSets.Recycle(_contactSets);
        _contactSets = null;
        Pool.Recycle(this);
      }
    }
#endif


    /// <summary>
    /// Gets the contacts for the given <see cref="CollisionObject"/> pair.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// A <see cref="ContactSet"/> describing the contact information if <paramref name="objectA"/>
    /// and <paramref name="objectB"/> are intersecting; otherwise, <see langword="null"/> if the
    /// objects are separated.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If both collision objects are part of this <see cref="CollisionDomain"/>,
    /// then this method returns the currently cached contacts sets (which are stored in
    /// <see cref="ContactSets"/>). The contact sets are only updated, when
    /// <see cref="Update(System.TimeSpan)"/> is called. If the objects have moved since the
    /// collision domain was updated last, the contact information will not be up-to-date. In this
    /// case you need to call <see cref="Update(System.TimeSpan)"/> again before calling
    /// <see cref="GetContacts(DigitalRise.Geometry.Collisions.CollisionObject)"/> to get
    /// up-to-date results.
    /// </para>
    /// <para>
    /// If one collision object is not part of this <see cref="CollisionDomain"/>,
    /// then this method automatically calculates the new contact information.
    /// </para>
    /// <para>
    /// If both collision objects are part of this <see cref="CollisionDomain"/>, then
    /// the returned contact sets are managed by the domain. You must not modify or recycle the
    /// contact sets! However, if any collision object is not part of this
    /// <see cref="CollisionDomain"/>, the returned contact sets are not managed by the domain.
    /// The contact sets can be modified. When they are no longer needed, they should be recycled to
    /// avoid unnecessary memory allocations. For example:
    /// </para>
    /// <code lang="csharp" title="Recycling contact sets">
    /// <![CDATA[
    /// foreach (ContactSet contactSet in myCollisionDomain.GetContacts(objectA, objectB))
    /// {
    ///   // Check contact set.
    ///   ...
    ///   
    ///   Debug.Assert(objectA.Domain != myCollisionDomain || objectB.Domain != myCollisionDomain);
    ///   foreach(var contact in contactSet)
    ///     contact.Recycle();
    ///   contactSet.Recycle();
    /// }
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public ContactSet GetContacts(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      if (objectA.Domain == this && objectB.Domain == this)
      {
        // ----- Both objects are in domain.
        return ContactSets.GetContacts(objectA, objectB);
      }
      else
      {
        // ----- At least one object is not in domain.

        // Make pairwise check.
        ContactSet pair = null;
        if (CanCollide(objectA, objectB) && HaveAabbContact(objectA, objectB) && HaveRayAabbContact(objectA, objectB))
        {
          pair = AlgorithmMatrix[objectA, objectB].GetContacts(objectA, objectB);
        }

        // If there is no contact, we can return immediately.
        if (pair == null)
          return null;

        if (!pair.HaveContact)
        {
          pair.Recycle();
          return null;
        }

        // There is a contact. 

        #region ----- Handle Ray.StopsAtFirstHit -----

        // If one object is a ray with StopsAtFirstHit, we have to check
        // the other objects in the domain because they could block the ray.
        // Only objectA or objectB can be a ray - not both because two rays
        // do not collide.
        CollisionObject ray = null;
        if (objectA.IsRayThatStopsAtFirstHit)
          ray = objectA;
        else if (objectB.IsRayThatStopsAtFirstHit)
          ray = objectB;

        if (ray != null)
        {
          // The contact set returned by GetContacts(x) must be manually recycled
          // if x is not in the domain. 
          bool recycle = (ray.Domain != this);

          bool isBlocked = false;

          foreach (var rayContactSet in GetContacts(ray))
          {
            if (!isBlocked && rayContactSet.Count > 0)
            {
              if (pair.Count == 0 || rayContactSet[0].PenetrationDepth < pair[0].PenetrationDepth)
              {
                // The ray is blocked. --> No contact between objectA and objectB.
                isBlocked = true;
              }
            }

            if (recycle)
              rayContactSet.Recycle(true);
          }

          if (isBlocked)
          {
            pair.Recycle(true);
            return null;
          }
        }
        #endregion

        return pair;
      }
    }


    /// <summary>
    /// Gets all <see cref="CollisionObject"/>s that have contact with the given object.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <returns>
    /// All <see cref="CollisionObject"/>s that have contact with 
    /// <paramref name="collisionObject"/>.
    /// </returns>
    public IEnumerable<CollisionObject> GetContactObjects(CollisionObject collisionObject)
    {
#if !POOL_ENUMERABLES
      foreach (ContactSet contactSet in GetContacts(collisionObject))
      {
        Debug.Assert(contactSet.HaveContact);

        CollisionObject otherCollisionObject = (contactSet.ObjectA == collisionObject) ? contactSet.ObjectB : contactSet.ObjectA;
        yield return otherCollisionObject;
      }
#else
      return GetContactObjectsWork.Create(collisionObject, GetContacts(collisionObject));
#endif
    }


#if POOL_ENUMERABLES
    private sealed class GetContactObjectsWork : PooledEnumerable<CollisionObject>
    {
      private static readonly ResourcePool<GetContactObjectsWork> Pool = new ResourcePool<GetContactObjectsWork>(() => new GetContactObjectsWork(), x => x.Initialize(), null);
      private CollisionObject _collisionObject;
      private IEnumerator<ContactSet> _enumerator;

      public static IEnumerable<CollisionObject> Create(CollisionObject collisionObject, IEnumerable<ContactSet> contactSets)
      {
        var enumerable = Pool.Obtain();
        enumerable._collisionObject = collisionObject;
        enumerable._enumerator = contactSets.GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out CollisionObject current)
      {
        if (_enumerator.MoveNext())
        {
          var contactSet = _enumerator.Current;
          current = (contactSet.ObjectA == _collisionObject) ? contactSet.ObjectB : contactSet.ObjectA;
          return true;
        }
        current = null;
        return false;
      }

      protected override void OnRecycle()
      {
        _collisionObject = null;
        _enumerator.Dispose();
        _enumerator = null;
        Pool.Recycle(this);
      }
    }
#endif


    /// <summary>
    /// Tests if the AABBs of two objects which are not in the domain overlap. (Can be used as ad
    /// hoc broad phase.)
    /// </summary>
    private static bool HaveAabbContact(CollisionObject objectA, CollisionObject objectB)
    {
      return GeometryHelper.HaveContact(objectA.GeometricObject.Aabb, objectB.GeometricObject.Aabb);
    }



    /// <summary>
    /// Returns true if the no object is a ray. If an object is a ray, it only returns true if the
    /// ray hits the AABB of the other object. If both objects are rays, false is returned.
    /// </summary>
    internal static bool HaveRayAabbContact(CollisionObject objectA, CollisionObject objectB)
    {
      IGeometricObject rayObject, otherObject;

      if (objectA.IsRay)
      {
        if (objectB.IsRay)
          return false;            // Two rays do not collide.

        rayObject = objectA.GeometricObject;
        otherObject = objectB.GeometricObject;
      }
      else if (objectB.IsRay)
      {
        rayObject = objectB.GeometricObject;
        otherObject = objectA.GeometricObject;
      }
      else
      {
        // No ray. 
        return true;
      }

      RayShape rayShape = (RayShape)rayObject.Shape;
      Vector3 rayScale = rayObject.Scale;
      Pose rayPose = rayObject.Pose;

      Ray rayWorld = new Ray(rayShape);
      rayWorld.Scale(ref rayScale);
      rayWorld.ToWorld(ref rayPose);

      var rayDirectionInverse = new Vector3(
        1 / rayWorld.Direction.X,
        1 / rayWorld.Direction.Y,
        1 / rayWorld.Direction.Z);

      return GeometryHelper.HaveContactFast(otherObject.Aabb, rayWorld.Origin, rayDirectionInverse, rayWorld.Length);
    }


    /// <summary>
    /// Determines whether the specified collision object has contact with any other object in the 
    /// domain.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <returns>
    /// <see langword="true"/> if the specified collision object touches or penetrates another 
    /// object in the collision domain; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If <paramref name="collisionObject"/> is part of this <see cref="CollisionDomain"/>,
    /// then this method checks the currently cached contacts sets (which are stored in
    /// <see cref="ContactSets"/>). The contact sets are only updated, when
    /// <see cref="Update(System.TimeSpan)"/> is called. If the collision object has moved since the
    /// collision domain was updated last, the contact information will not be up-to-date. In this
    /// case you need to call <see cref="Update(System.TimeSpan)"/> again before calling
    /// <see cref="GetContacts(DigitalRise.Geometry.Collisions.CollisionObject)"/> to get
    /// up-to-date results.
    /// </para>
    /// <para>
    /// If <paramref name="collisionObject"/> is not part of this <see cref="CollisionDomain"/>,
    /// then this method automatically calculates the new contact information.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collisionObject"/> is <see langword="null"/>.
    /// </exception>
    public bool HasContact(CollisionObject collisionObject)
    {
      if (collisionObject == null)
        throw new ArgumentNullException("collisionObject");

      if (collisionObject.Domain == this)
      {
        #region ----- Object is in domain. -----
        return ContactSets.Contains(collisionObject);
        #endregion
      }
      else
      {
        #region ----- Object is not in domain. -----
        // Early exit if collision object is disabled.
        if (collisionObject.Enabled == false)
          return false;

        bool hasContact = false;

        IEnumerable<CollisionObject> overlaps;
        if (collisionObject.IsRay)
        {
          RayShape rayShape = (RayShape)collisionObject.GeometricObject.Shape;
          Vector3 rayScale = collisionObject.GeometricObject.Scale;
          Pose rayPose = collisionObject.GeometricObject.Pose;

          Ray rayWorld = new Ray(rayShape);
          rayWorld.Scale(ref rayScale);
          rayWorld.ToWorld(ref rayPose);

          overlaps = BroadPhase.GetOverlaps(rayWorld);
        }
        else
        {
          overlaps = BroadPhase.GetOverlaps(collisionObject);
        }

        // For all collision candidates with the object run narrow phase.
        var algorithmMatrix = AlgorithmMatrix;
        foreach (var touchedObject in overlaps)
        {
          // Collision filtering 
          if (CanCollide(collisionObject, touchedObject))
          {
            Debug.Assert(hasContact == false);

            hasContact = algorithmMatrix[collisionObject, touchedObject].HaveContact(collisionObject, touchedObject);

            #region ----- Handle Ray.StopsAtFirstHit (other object is ray) -----
            if (hasContact)
            {
              ContactSet cachedClosestContact;
              _rayCache.TryGetValue(touchedObject, out cachedClosestContact);
              if (cachedClosestContact != null && cachedClosestContact.Count > 0)
              {
                // If the other object is a cached ray that StopsAtFirstHit, get detailed contact 
                // information with penetrationDepth.
                ContactSet rayVsObject = algorithmMatrix[collisionObject, touchedObject]
                  .GetContacts(collisionObject, touchedObject);

                if (rayVsObject.Count == 0                                                          // Ray vs. object has no useful contact information.
                    || cachedClosestContact[0].PenetrationDepth < rayVsObject[0].PenetrationDepth)  // Ray is blocked by another object.
                {
                  // No contact for this object pair.
                  hasContact = false;
                }

                rayVsObject.Recycle(true);
              }
            }
            #endregion

            if (hasContact)
              break;
          }
        }

        return hasContact;
        #endregion
      }
    }


    /// <summary>
    /// Determines whether two <see cref="CollisionObject"/>s have contact.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are touching or penetrating; otherwise 
    /// <see langword="false"/> if the objects are separated.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If both collision objects are part of this <see cref="CollisionDomain"/>,
    /// then this method checks the currently cached contacts sets (which are stored in
    /// <see cref="ContactSets"/>). The contact sets are only updated, when
    /// <see cref="Update(System.TimeSpan)"/> is called. If the objects have moved since the
    /// collision domain was updated last, the contact information will not be up-to-date. In this
    /// case you need to call <see cref="Update(System.TimeSpan)"/> again before calling
    /// <see cref="GetContacts(DigitalRise.Geometry.Collisions.CollisionObject)"/> to get
    /// up-to-date results.
    /// </para>
    /// <para>
    /// If one collision object is not part of this <see cref="CollisionDomain"/>,
    /// then this method automatically calculates the new contact information.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public bool HaveContact(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      if (objectA.Domain == this && objectB.Domain == this)
      {
        // ----- Both objects are in domain.
        ContactSet contactSet = ContactSets.GetContacts(objectA, objectB);
        Debug.Assert(contactSet == null || contactSet.HaveContact, "ContactSet should either be null or a valid contact.");
        return (contactSet != null);
      }
      else
      {
        // ----- At least one object is not in the domain.
        if (objectA.IsRayThatStopsAtFirstHit || objectB.IsRayThatStopsAtFirstHit)
        {
          // At least one object is a ray that StopsAtFirstHit.
          // We have to check the other objects in the domain because they could block the ray.
          // This is correctly handled in GetContacts.
          ContactSet contactSet = GetContacts(objectA, objectB);
          Debug.Assert(contactSet == null || contactSet.HaveContact, "ContactSet should either be null or a valid contact.");

          if (contactSet != null)
          {
            contactSet.Recycle(true);
            return true;
          }
        }
        else
        {
          // 2 normal objects: Make pairwise check.
          if (CanCollide(objectA, objectB) && HaveAabbContact(objectA, objectB) && HaveRayAabbContact(objectA, objectB))
          {
            return AlgorithmMatrix[objectA, objectB].HaveContact(objectA, objectB);
          }
        }

        return false;
      }
    }


    /// <overloads>
    /// <summary>
    /// Updates the collision domain and computes new contact information.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Updates the collision domain and computes the new contact information.
    /// </summary>
    /// <param name="deltaTime">
    /// The simulation time that has elapsed since the last update. (The size of the time step.)
    /// </param>
    /// <remarks>
    /// This method has to be called in each frame. The computed collision data will be updated for 
    /// the collision objects which were modified since the last <see cref="Update(float)"/> call.
    /// </remarks>
    public void Update(TimeSpan deltaTime)
    {
      Update((float)deltaTime.TotalSeconds, true);
    }


    /// <summary>
    /// Updates the collision domain and computes the new contact information.
    /// </summary>
    /// <param name="deltaTime">
    /// The simulation time that has elapsed since the last update in seconds. (The size of the time
    /// step.)
    /// </param>
    /// <remarks>
    /// This method has to be called in each frame. The computed collision data will be updated for 
    /// the collision objects which were modified since the last <see cref="Update(float)"/> call.
    /// </remarks>
    public void Update(float deltaTime)
    {
      Update(deltaTime, true);
    }


    /// <summary>
    /// Updates the collision domain and computes the new contact information.
    /// </summary>
    /// <param name="deltaTime">
    /// The simulation time that has elapsed since the last update in seconds. (The size of the time
    /// step.)</param>
    /// <param name="recycleContactSets">
    /// If set to <see langword="true" />, obsolete contact sets from the last frames will be 
    /// recycled - which is the default behavior. If, for some reason, obsolete contact sets might
    /// still be referenced and used, use <see langword="false" />; the contact sets are kept
    /// and recycled in future <see cref="Update(System.TimeSpan)"/> calls. <see langword="false" /> 
    /// should only be used in special cases.
    /// </param>
    /// <remarks>
    /// This method has to be called in each frame. The computed collision data will be updated for
    /// the collision objects which were modified since the last <see cref="Update(float)" /> call.
    /// </remarks>
    public void Update(float deltaTime, bool recycleContactSets)
    {
      // ----- Remove all old contact sets.
      // This must be done before the broad-phase because the broad-phase recycles contact sets
      // and ContactSets would contain invalid objects.
      ContactSets.Clear();

      // Make sure our copy of the algorithm matrix is up-to-date.
      if (_algorithmMatrixVersion != CollisionDetection.AlgorithmMatrix._version)
      {
        _algorithmMatrix = new CollisionAlgorithmMatrix(CollisionDetection.AlgorithmMatrix);
        _algorithmMatrixVersion = CollisionDetection.AlgorithmMatrix._version;
      }

      // ----- Clean up ray cache.
      // Needs to be done before broad phase update because obsolete contact sets
      // are recycled in InternalBroadPhase.NewFrame().
      int numberOfRaysInCache = _rayCache.Count;
      if (numberOfRaysInCache > 0)
      {
        // Remove cached info if the objects have moved or if the collision has been filtered.

        // A temporary array is used when iterating all rays in the cache (to avoid garbage).
        if (_tempRayCache.Length < numberOfRaysInCache)
        {
          // Resize temporary array if necessary.
          _tempRayCache = new KeyValuePair<CollisionObject, ContactSet>[numberOfRaysInCache];
        }

        ((ICollection<KeyValuePair<CollisionObject, ContactSet>>)_rayCache).CopyTo(_tempRayCache, 0);

        // Invalidate entry in ray cache if the collision objects or the collision filter has changed.
        for (int i = 0; i < numberOfRaysInCache; i++)
        {
          var entry = _tempRayCache[i];
          ContactSet contactSet = entry.Value;
          if (contactSet != null                          // The entry is valid.
              && (AreCollisionObjectsModified(contactSet) // The collision objects have changed.
                 || !CanCollide(contactSet)))             // The collision filter has changed.
          {
            // Invalidate entry.
            _rayCache[entry.Key] = null;
          }
        }

        // Clear temporary array to avoid memory leaks.
        Array.Clear(_tempRayCache, 0, numberOfRaysInCache);
      }

      // ----- Run broad phase.
      // Recycle old contact sets. In rare occasions, we must keep the obsolete contact
      // sets for one more frame; e.g. if this Update is called more than once per frame.
      // This is needed for Simulation.SynchronizeCollisionDomain.
      if (recycleContactSets)
        InternalBroadPhase.NewFrame();

      InternalBroadPhase.Update();

      // ----- Run narrow phase for all collision candidates.
      // Prepare parameters for narrow phase.
      Debug.Assert(_deltaTime == 0, "_deltaTime should have been reset.");
      _deltaTime = deltaTime;

      // Run narrow phase.
      NarrowPhase();

      // Reset parameters.
      _deltaTime = 0;

      // TODO: We could avoid enumerating ALL objects if Changed CollisionObjects register
      // in HashSet.
      // Reset modified flags.
      int numberOfCollisionObjects = CollisionObjects.Count;
      for (int i = 0; i < numberOfCollisionObjects; i++)
      {
        CollisionObject collisionObject = CollisionObjects[i];
        collisionObject.Changed = false;
        collisionObject.ShapeTypeChanged = false;
      }

      // Let the algorithm matrix optimize itself.
      _algorithmMatrix.Optimize();
    }


    /// <summary>
    /// Updates the collision domain and computes the new contact information for a given collision
    /// object.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <remarks>
    /// <para>
    /// This method acts like <see cref="Update(float)"/>, except that only the contact info for the
    /// specified collision object is updated. The update is done with a time step of 0, which means
    /// that any timestamps or similar properties (e.g. <see cref="Contact.Lifetime"/>) are not 
    /// modified.
    /// </para>
    /// <para>
    /// This method should only be used in special cases - whenever possible 
    /// <see cref="Update(float)"/> should be used instead. The collision detection caches a lot of
    /// information to improve performance. By updating only a single collision object the collision
    /// detection might remain in an inconsistent state. That is, the collision objects might have
    /// contact, but the contact information is missing.
    /// </para>
    /// <para>
    /// Example 1 of an inconsistent state: <paramref name="collisionObject"/> was the first hit of
    /// a ray that stops at first hits (see <see cref="RayShape.StopsAtFirstHit"/>). Now 
    /// <paramref name="collisionObject"/> is moved and does not touch the ray anymore. When 
    /// <see cref="Update(CollisionObject)"/> is called in this situation, the collision detection 
    /// might remain in an inconsistent state. When <see cref="Update(float)"/> is called the 
    /// contact information of <paramref name="collisionObject"/> is updated and the 
    /// <see cref="ContactSet"/> between the <paramref name="collisionObject"/> and the ray will be 
    /// removed. Since the ray is now no longer blocked by the <paramref name="collisionObject"/> it 
    /// might hit another object. However, the contact information of the ray is not updated.
    /// </para>
    /// <para>
    /// Example 2 of an inconsistent state: Some collision objects have moved since the last call of 
    /// <see cref="Update(float)"/>. <paramref name="collisionObject"/> is moved into contact with
    /// one of the moved objects. Now <see cref="Update(CollisionObject)"/> for 
    /// <paramref name="collisionObject"/> is called. The broad phase information of 
    /// <paramref name="collisionObject"/> is updated. But - depending on the type of broad phase 
    /// algorithm - the broad phase information of the other objects might not be updated. The 
    /// collision detection uses the cached broad phase information from the previous 
    /// <see cref="Update(float)"/>. So the collision detection might not find all contacts for
    /// <paramref name="collisionObject"/>.
    /// </para>
    /// <para>
    /// To sum up: Use this method carefully. Try to use <see cref="Update(float)"/> instead. Call
    /// this method only if <paramref name="collisionObject"/> is the only object that has moved 
    /// since the last call of <see cref="Update(float)"/>.
    /// </para> 
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collisionObject"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="collisionObject"/> is not in this <see cref="CollisionDomain"/>.
    /// </exception>
    public void Update(CollisionObject collisionObject)
    {
      // Note: To be really consistent, we would have to cache the old states of all bodies and 
      // use these in the broad phase and the narrow phase. Currently, the SweepAndPrune phase 
      // caches the old states. But the new states are always used in the AabbBroadPhase and
      // all narrow phase algorithms.
      // All involved rays with StopsAtFirstHit must also be updated, but with their OLD STATES!

      if (collisionObject == null)
        throw new ArgumentNullException("collisionObject");
      if (!CollisionObjects.Contains(collisionObject))
        throw new ArgumentException("The given collision object is not part of this CollisionDomain.", "collisionObject");

      // Make sure our copy of the algorithm matrix is up-to-date.
      if (_algorithmMatrixVersion != CollisionDetection.AlgorithmMatrix._version)
      {
        _algorithmMatrix = new CollisionAlgorithmMatrix(CollisionDetection.AlgorithmMatrix);
        _algorithmMatrixVersion = CollisionDetection.AlgorithmMatrix._version;
      }

      // Remove all old contact sets involving collisionObject.
      // This must be done before the broad-phase because the broad-phase recycles contact sets
      // and ContactSets would contain invalid objects.
      ContactSets.Remove(collisionObject);

      // ----- Run broad phase.
      InternalBroadPhase.Update(collisionObject);

      // ----- Prepare ray cache before running narrow phase.
      int numberOfRaysInCache = _rayCache.Count;
      if (numberOfRaysInCache > 0)
      {
        // Remove cached info if the objects have moved or if the collision has been filtered.

        // A temporary array is used when iterating all rays in the cache (to avoid garbage).
        if (_tempRayCache.Length < numberOfRaysInCache)
        {
          // Resize temporary array if necessary.
          _tempRayCache = new KeyValuePair<CollisionObject, ContactSet>[numberOfRaysInCache];
        }

        ((ICollection<KeyValuePair<CollisionObject, ContactSet>>)_rayCache).CopyTo(_tempRayCache, 0);

        // Invalidate entry in ray cache if the collision objects or the collision filter has changed.
        for (int i = 0; i < numberOfRaysInCache; i++)
        {
          var entry = _tempRayCache[i];
          ContactSet contactSet = entry.Value;
          if (contactSet != null                            // The entry is valid.
              && (contactSet.ObjectA == collisionObject     // Only if collisionObject is involved.
                 || contactSet.ObjectB == collisionObject)
              && (AreCollisionObjectsModified(contactSet)   // The collision objects have changed.
                 || !CanCollide(contactSet)))               // The collision filter has changed.
          {
            // Invalidate entry.
            _rayCache[entry.Key] = null;
          }
        }

        // Clear temporary array to avoid memory leaks.
        Array.Clear(_tempRayCache, 0, numberOfRaysInCache);
      }

      // ----- Run narrow phase for all collision candidates.
      // Prepare parameters for narrow phase.
      Debug.Assert(_deltaTime == 0, "_deltaTime should have been reset.");
      Debug.Assert(_collisionObject == null, "_collisionObject should have been reset.");
      _collisionObject = collisionObject;

      // Run narrow phase.
      NarrowPhase();

      // Reset parameters.
      _collisionObject = null;

      // Reset modified flags.
      collisionObject.Changed = false;
      collisionObject.ShapeTypeChanged = false;
    }


    /// <summary>
    /// Performs the collision detection narrow phase for all candidate pairs.
    /// </summary>
    private void NarrowPhase()
    {
      // Preconditions: 
      //   _deltaTime must be set.
      //   _collisionObject must be set, if only certain contact sets should be updated.
      if (EnableMultithreading && InternalBroadPhase.CandidatePairs.Count > 1)
      {
        // Multi-threaded update
        Parallel.For(0, InternalBroadPhase.CandidatePairs.InternalCount, 
          _narrowPhaseMethod);

        foreach (var contactSet in InternalBroadPhase.CandidatePairs)
          AddToContactSets(contactSet);
      }
      else
      {
        // Single-threaded update
        foreach (var contactSet in InternalBroadPhase.CandidatePairs)
        {
          NarrowPhase(contactSet);
          AddToContactSets(contactSet);
        }
      }
    }


    private void NarrowPhase(int i)
    {
      var pair = InternalBroadPhase.CandidatePairs[i];
      if (pair != null)
        NarrowPhase(pair);
    }


    /// <summary>
    /// Performs the collision detection narrow phase for the candidate pair with the given index.
    /// </summary>
    private void NarrowPhase(ContactSet pair)
    {
      // Preconditions: 
      //   _deltaTime must be set.
      //   _collisionObject must be set, if only a certain object should be updated.

      if (_collisionObject != null && pair.ObjectA != _collisionObject && pair.ObjectB != _collisionObject)
        return;

      if (pair.ObjectA.ShapeTypeChanged || pair.ObjectB.ShapeTypeChanged)
        pair.CollisionAlgorithm = null;

      // Make ray vs AABB test. (If no object is a ray, HaveRayAabbContact returns true.)
      if (!HaveRayAabbContact(pair.ObjectA, pair.ObjectB))
      {
        pair.HaveContact = false;
        foreach (var contact in pair)
          contact.Recycle();
        pair.Clear();
        return;
      }

      if (CanCollide(pair))
      {
        // ----- Update collision data.

        // Check if at least one of the objects has moved or if the contact set was marked as 
        // invalid. If no object has moved, then we assume the contact was computed in a 
        // previous frame and is still up to date.
        if (AreCollisionObjectsModified(pair) || pair.IsValid == false)
        {
          // Updated contact set and compute collision.
          if (pair.CollisionAlgorithm == null)
            pair.CollisionAlgorithm = _algorithmMatrix[pair];

          pair.CollisionAlgorithm.UpdateContacts(pair, _deltaTime);
        }
        else
        {
          // Only call helper method to update contact lifetime.
          // Optimization: If deltaTime is 0, there is nothing to do.
          // We could further optimize the whole method: If No Object Changed && CollisionFilter.Changed == false --> Do nothing...
          // That would require a Changed flag/event for the collision filter.
          if (_deltaTime > 0)
            ContactHelper.UpdateContacts(pair, _deltaTime, CollisionDetection.ContactPositionTolerance);
        }

        Debug.Assert(pair.Count == 0 || pair[0].PenetrationDepth >= 0 || Numeric.IsNaN(pair[0].PenetrationDepth));
      }
      else
      {
        // ------ Objects cannot collide (collision disabled).

        // If the objects are modified, the cached info gets invalid.
        if (AreCollisionObjectsModified(pair))
        {
          foreach (var contact in pair)
            contact.Recycle();

          pair.Clear();

          // Set contact set to invalid: If the filtering of the two pairs is disabled,
          // then the collision should be computed even if the collision objects have not
          // moved since the last frame.
          pair.IsValid = false;
        }

        // If the objects have not changed, we keep their last valid contact info.
        // The contact set remains internally in BroadPhase.CandidatePairs, but it is not copied
        // into ContactSets.
      }
    }


    private void AddToContactSets(ContactSet pair)
    {
      // Preconditions: 
      //   _deltaTime must be set.
      //   _collisionObject must be set, if only a certain object should be updated.

      if (_collisionObject != null && pair.ObjectA != _collisionObject && pair.ObjectB != _collisionObject)
        return;

      if (pair.HaveContact && CanCollide(pair))
      {
        #region ----- Handle Ray.StopsAtFirstHit and add contact set to collection. -----

        // If we have a contact, then we add this contact pair.
        // For rays with StopsAtFirstHit we add only closest contacts and update the _rayCache.
        // Note: Candidate pairs where HaveContact is true can only contain 1 ray because 
        // 2 rays do not collide.
        CollisionObject ray = null;
        if (pair.ObjectA.IsRayThatStopsAtFirstHit)
          ray = pair.ObjectA;
        else if (pair.ObjectB.IsRayThatStopsAtFirstHit)
          ray = pair.ObjectB;

        if (ray != null)
        {
          // Ray hit: Ray could be blocked by another object.
          // Only add the new contact if it is closer than any other contact.
          ContactSet cachedClosestContact;
          _rayCache.TryGetValue(ray, out cachedClosestContact);
          if (cachedClosestContact == null          // The ray is not blocked by another object.
              || cachedClosestContact == pair       // This is the closest contact anyway.
              || cachedClosestContact.Count == 0    // There is a potential blocker, but it has no useful contact info.
              || (pair.Count > 0 && pair[0].PenetrationDepth < cachedClosestContact[0].PenetrationDepth)  // There is another ray contact but the current is closer.
             )
          {
            _rayCache[ray] = pair;

            // Remove previously closest hit, if any.
            ContactSets.Remove(cachedClosestContact);

            // Add new closest hit with ray.
            ContactSets.Add(pair);
          }
        }
        else
        {
          // Default case: Normal contact, no rays.
          // Add contact set.
          ContactSets.Add(pair);
        }
        #endregion
      }
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "CollisionDomain {{ Count = {0} }}", CollisionObjects.Count);
    }


    //public void Validate()
    //{
    //  foreach (var contactSet in InternalBroadPhase.CandidatePairs)
    //  {
    //    if (contactSet.CollisionAlgorithm != null && contactSet.ObjectA.Domain != null
    //        && contactSet.ObjectB.Domain != null)
    //    {
    //      if (_algorithmMatrix[contactSet] != contactSet.CollisionAlgorithm
    //          && !contactSet.ObjectA.ShapeTypeChanged && !contactSet.ObjectB.ShapeTypeChanged)
    //        Debugger.Break();
    //    }
    //  }
    //}
    #endregion
  }
}
