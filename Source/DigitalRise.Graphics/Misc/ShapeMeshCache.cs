﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics.Rendering;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;


namespace DigitalRise.Graphics
{
  /// <summary>
  /// Creates and caches <see cref="Submesh"/>es for <see cref="Shape"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Whenever you have a shape and need a submesh, you can use this class.
  /// The returned submesh must not be modified because it might be shared. Every time you use the 
  /// submesh, check if it was disposed. This happens if the shape was changed.
  /// </para>
  /// <para>
  /// This class can also keep submeshes alive (using a strong reference) for a certain amount of 
  /// time. This is useful for the <see cref="DebugRenderer"/>, which get the submeshes regularly.
  /// </para>
  /// </remarks>
  internal sealed class ShapeMeshCache : IDisposable
  {
    // Note:
    // We could also cache submeshes for ITriangleMesh - but we would not know if the
    // mesh was changed. Better to use a TriangleMeshShape because shapes have Changed
    // events.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Describes a cache entry that stores a Submesh which represent a shape.
    private class CacheEntry
    {
      public int HashCode;
      public readonly WeakReference ShapeWeak;

      // The submesh is stored either as a strong or as a weak reference.
      public Submesh Submesh;
      public readonly WeakReference SubmeshWeak;  // WeakReference.Target is set on demand.

      // An SRT matrix which has to be applied to the submesh.
      public Matrix44F Matrix;

      // Describes how long the submesh hasn't been drawn.
      // 0 if the cached submesh was used in the last frame. x if cached submesh was not
      // used in the last x frames.
      public int Age;

      public CacheEntry(Shape shape)
      {
        ShapeWeak = new WeakReference(shape);
        SubmeshWeak = new WeakReference(null);
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    private class CacheEntryComparer : Singleton<CacheEntryComparer>, IComparer<CacheEntry>
    {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
      public int Compare(CacheEntry x, CacheEntry y)
      {
        if (x.HashCode < y.HashCode)
          return -1;
        if (x.HashCode > y.HashCode)
          return +1;

        return 0;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A special submesh without vertex buffer or index buffer. Represents an "empty" submesh.
    // This allows to distinguish an empty submesh from "null" values.
    private static readonly Submesh EmptySubmesh = new Submesh();

    private readonly IGraphicsService _graphicsService;

    // For all boxes, we use one shared submesh!
    private Submesh _boxSubmesh;

    // Temporary instance used while searching the cache.
    private readonly CacheEntry _tempEntry;

    private readonly List<CacheEntry> _cache;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Gets or sets the allowed relative mesh error.
    /// </summary>
    /// <value>The allowed relative error for meshes; in the range ]0, 1[.</value>
    /// <remarks>
    /// If triangle meshes are generated to draw curved shapes, the meshes are approximated with 
    /// this relative error value. 
    /// </remarks>
    public float MeshRelativeError
    {
      get { return _meshRelativeError; }
      set { _meshRelativeError = value; }
    }
    private float _meshRelativeError = 0.01f;


    /// <summary>
    /// Gets or sets the iteration limit for approximated meshes.
    /// </summary>
    /// <value>The iteration limit for approximated meshes. (Must be greater than 0.)</value>
    /// <remarks>
    /// If the mesh is generated by an iterative algorithm, no more than 
    /// <see cref="MeshIterationLimit"/> iterations are performed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is 0 or negative.
    /// </exception>
    public int MeshIterationLimit
    {
      get { return _meshIterationLimit; }
      set
      {
        if (_meshIterationLimit <= 0)
          throw new ArgumentOutOfRangeException("value", "MeshIterationLimit must be greater than 0.");

        _meshIterationLimit = value;
      }
    }
    private int _meshIterationLimit = 4;


    /// <summary>
    /// Gets or sets the normal angle limit which determines when vertex normal vectors of neighbor
    /// triangles can be merged.
    /// </summary>
    /// <value>
    /// The normal angle limit in radians. If the angle between two normal vectors is less than this
    /// value, the normals can be merged.
    /// </value>
    public float NormalAngleLimit
    {
      get { return _normalAngleLimit; }
      set { _normalAngleLimit = value; }
    }
    private float _normalAngleLimit = MathHelper.ToRadians(70);


    /// <summary>
    /// Gets or sets the number of frames an unused submesh will be cached before it can be garbage
    /// collected.
    /// </summary>
    /// <value>
    /// The number of frames an unused submeshes will be cached before it can be garbage collected.
    /// </value>
    public int MeshCacheFrameLimit
    {
      get { return _meshCacheFrameLimit; }
      set { _meshCacheFrameLimit = value; }
    }
    private int _meshCacheFrameLimit = 50;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeMeshCache"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public ShapeMeshCache(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _graphicsService = graphicsService;
      _tempEntry = new CacheEntry(null);
      _cache = new List<CacheEntry>();
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="ShapeMeshCache"/> class.
    /// </summary>
    public void Dispose()
    {
      if (!IsDisposed)
      {
        IsDisposed = true;

        // Unregister all events and dispose meshes. 
        for (int i = 0; i < _cache.Count; i++)
        {
          var entry = _cache[i];

          var shape = (Shape)entry.ShapeWeak.Target;
          if (shape != null)
            shape.Changed -= OnCachedShapeChanged;

          DisposeMesh(entry);
        }

        _cache.Clear();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void ThrowIfDisposed()
    {
      if (IsDisposed)
        throw new ObjectDisposedException(GetType().FullName);
    }


    private static void MakeWeak(CacheEntry entry)
    {
      entry.SubmeshWeak.Target = entry.Submesh;
      entry.Submesh = null;
    }


    private void DisposeMesh(CacheEntry entry)
    {
      var submesh = entry.Submesh ?? (Submesh)entry.SubmeshWeak.Target;

      // Note: Do not dispose the special shared meshes. (Calling EmptySubmesh.Dispose() is a NOP.)
      if (submesh != null && submesh != _boxSubmesh)
        submesh.Dispose();

      entry.Submesh = null;
      entry.SubmeshWeak.Target = null;
    }

    public void MakeWeakAll()
    {
			// We release all strong references, when the resource pools are cleared. This usually
			// happens when the game loads a new level.
			for (int i = 0; i < _cache.Count; i++)
				MakeWeak(_cache[i]);
		}


    /// <summary>
    /// Updates the cache, ages all cached entries, and removes expired entries.
    /// </summary>
    public void Update()
    {
      ThrowIfDisposed();

      // Update cache:
      for (int i = _cache.Count - 1; i >= 0; i--)
      {
        var entry = _cache[i];
        var shape = (Shape)entry.ShapeWeak.Target;

        // Remove entry if shape object (key) was garbage collected.
        if (shape == null)
        {
          _cache.RemoveAt(i);
          continue;
        }

        // If age is beneath limit, then only increase the age counter.
        if (entry.Age <= MeshCacheFrameLimit)
        {
          entry.Age++;
          continue;
        }

        // ----- Age limit has been exceeded.
        if (entry.Submesh != null)
        {
          MakeWeak(entry);
          continue;
        }

        // Remove this entry if the submesh was garbage collected.
        if (!entry.SubmeshWeak.IsAlive)
        {
          shape.Changed -= OnCachedShapeChanged;
          _cache.RemoveAt(i);
        }
      }
    }


    /// <summary>
    /// Gets the submesh (vertex and index buffers) for a shape.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="submesh">
    /// The created or cached submesh. This is never <see langword="null"/>!
    /// </param>
    /// <param name="matrix">
    /// The optional matrix which has to be applied to submesh.
    /// </param>
    /// <remarks>
    /// Meshes are retrieved from a cache. If no matching submesh is cached, a new submesh is
    /// created and cached for future use. The returned submesh must not be modified! Before you use
    /// the submesh check if it was disposed - this happens if the shape was changed!
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape" /> is <see langword="null" />.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Class is for internal use only.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "submesh")]
    public void GetMesh(Shape shape, out Submesh submesh, out Matrix44F matrix)
    {
      ThrowIfDisposed();
      if (shape == null)
        throw new ArgumentNullException("shape");

      var index = GetCacheIndex(shape);
      if (index >= 0)
      {
        // Found cache entry!

        var entry = _cache[index];

        Debug.Assert(entry.ShapeWeak.Target == shape, "ShapeMeshCache.GetCacheIndex() returned wrong index.");

        // Get submesh from strong or weak reference.
        submesh = entry.Submesh ?? (Submesh)entry.SubmeshWeak.Target;
        matrix = entry.Matrix;

        if (submesh != null)
        {
          // Recreate submesh if number of triangles in TriangleMeshShape has changed.
          var triangleMeshShape = shape as TriangleMeshShape;
          if (triangleMeshShape != null && triangleMeshShape.Mesh.NumberOfTriangles != submesh.PrimitiveCount)
          {
            DisposeMesh(entry);
            submesh = null;
          }
        }

        // Recreate submesh if necessary.
        if (submesh == null)
          CreateMesh(shape, out submesh, out matrix);

        _cache[index].Submesh = submesh;
        _cache[index].Matrix = matrix;
        _cache[index].Age = 0;
      }
      else
      {
        // No cache entry found.

        // GetCacheIndex returns the bitwise complement of the next index.
        index = ~index;

        // No submesh in cache.
        CreateMesh(shape, out submesh, out matrix);
        var entry = new CacheEntry(shape)
        {
          HashCode = _tempEntry.HashCode,
          Age = 0,
          Submesh = submesh,
          Matrix = matrix,
        };
        _cache.Insert(index, entry);

        // If shape changes, we must invalidate the cache entry:
        shape.Changed += OnCachedShapeChanged;
      }
    }


    private void OnCachedShapeChanged(object sender, EventArgs eventArgs)
    {
      // Set the cached submesh to null. The cache entry can then be removed in Update().
      var shape = (Shape)sender;
      var index = GetCacheIndex(shape);

      Debug.Assert(index >= 0, "ShapeMeshCache handles Shape.Changed event but no entry is in the cache for this shape.");
      Debug.Assert(_cache[index].ShapeWeak.Target == shape, "ShapeMeshCache.GetCacheIndex returned wrong index.");

      DisposeMesh(_cache[index]);
    }


    // Returns the index of the cache entry for the given shape. If there is no 
    // matching entry then the bitwise complement of the next index is returned.
    private int GetCacheIndex(Shape shape)
    {
      int hashCode = shape.GetHashCode();

      // Use a dummy CacheEntry object for the search.
      _tempEntry.HashCode = hashCode;
      int index = _cache.BinarySearch(_tempEntry, CacheEntryComparer.Instance);
      if (index < 0)
      {
        // No matching hash code found.
        return index;
      }

      // If the hash codes are unique we have an exact match.
      if (_cache[index].ShapeWeak.Target == shape)
        return index;

      // If there are several objects with the same hash code (very unlikely!), 
      // go back to the first entry.
      while (index - 1 >= 0 && _cache[index - 1].HashCode == hashCode)
        index--;

      // Now, search forward.
      while (index < _cache.Count)
      {
        if (_cache[index].ShapeWeak.Target == shape)
          return index;
        if (_cache[index].HashCode != hashCode)
          return ~index;

        index++;
      }

      return ~index;
    }


    private void CreateMesh(Shape shape, out Submesh submesh, out Matrix44F matrix)
    {
      // Use a special shared submesh for box shapes.
      var boxShape = shape as BoxShape;
      if (boxShape != null)
      {
        if (_boxSubmesh == null)
          _boxSubmesh = MeshHelper.GetBox(_graphicsService);

        submesh = _boxSubmesh;
        matrix = Matrix44F.CreateScale(boxShape.Extent);
        return;
      }

      var transformedShape = shape as TransformedShape;
      boxShape = (transformedShape != null) ? transformedShape.Child.Shape as BoxShape : null;
      if (boxShape != null)
      {
        if (_boxSubmesh == null)
          _boxSubmesh = MeshHelper.GetBox(_graphicsService);

        submesh = _boxSubmesh;
        matrix = transformedShape.Child.Pose
                 * Matrix44F.CreateScale(transformedShape.Child.Scale * boxShape.Extent);
        return;
      }

      // Create the submesh. Return EmptySubmesh if the MeshHelper returns null.
      var newSubmesh = MeshHelper.CreateSubmesh(
        _graphicsService.GraphicsDevice,
        shape.GetMesh(MeshRelativeError, MeshIterationLimit),
        NormalAngleLimit);

      submesh = newSubmesh ?? EmptySubmesh;
      matrix = Matrix44F.Identity;
    }


    // Updates submesh and matrix if necessary.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Class is for internal use only.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "submesh")]
    public static void GetMesh(IGraphicsService graphicsService, Shape shape, out Submesh submesh, out Matrix44F matrix)
    {
      // Update submesh.
      var graphicsManager = graphicsService as GraphicsManager;
      if (graphicsManager != null)
      {
        graphicsManager.ShapeMeshCache.GetMesh(shape, out submesh, out matrix);
      }
      else
      {
        // This happens if the user has implemented his own graphics manager - 
        // which is very unlikely.
        submesh = MeshHelper.CreateSubmesh(graphicsService.GraphicsDevice, shape.GetMesh(0.05f, 4), MathHelper.ToRadians(70));
        matrix = Matrix44F.Identity;
      }
    }
    #endregion
  }
}
