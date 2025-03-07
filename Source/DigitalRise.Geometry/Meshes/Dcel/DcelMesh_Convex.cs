// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using Plane = DigitalRise.Geometry.Shapes.Plane;

namespace DigitalRise.Geometry.Meshes
{
  public partial class DcelMesh
  {
    /// <overloads>
    /// <summary>
    /// Determines whether this mesh is a convex mesh.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether this mesh is a convex mesh.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this mesh is a convex mesh; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method also checks if the mesh is closed (see <see cref="IsClosed"/>). Internally,
    /// <see cref="IsConvex(float)"/> with an epsilon tolerance of 0.001f is called.
    /// </remarks>
    public bool IsConvex()
    {
      return IsConvex(0.001f);
    }


    /// <summary>
    /// Determines whether this mesh is a convex mesh using a specific tolerance.
    /// </summary>
    /// <param name="epsilon">
    /// The epsilon tolerance value. Numerical errors within this tolerance are accepted. This value 
    /// is automatically scaled with the mesh size. Recommended values are 0.001 or 0.0001. 
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this mesh is a convex mesh; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method also checks if the mesh is closed (see <see cref="IsClosed"/>).
    /// </remarks>
    public bool IsConvex(float epsilon)
    {
      if (!IsClosed())
        return false;

      UpdateCache();

      // Nothing to check for a two-sided polygon.
      if (Faces.Count <= 2)
        return true;

      try
      {
        foreach (var face in Faces)
        {
          if (face.InternalTag == 1)
            continue;                     // Face was already checked with all neighbors.

          // Mark face as visited.
          face.InternalTag = 1;

          // Get normal vector.
          var normal = face.Normal;

          // Skip degenerate faces.
          float normalLength = normal.Length();
          if (Numeric.IsZero(normalLength))
            continue;

          // Normalize.
          normal = normal / normalLength;

          // Create a plane for the face.
          Plane plane = new Plane(normal, face.Boundary.Origin.Position);

          // Visit all adjacent faces and check whether their vertices lie in or below the plane.
          var edge = face.Boundary;
          edge.InternalTag = 1;
          do
          {
            var face2 = edge.Twin.Face; // Adjacent face.
            if (face2.InternalTag != 1)
            {
              // Visit all edges of this neighbor.
              var edge2 = face2.Boundary; // Edge of adjacent face.
              edge2.InternalTag = 1;
              do
              {
                float d = Vector3.Dot(edge2.Origin.Position, plane.Normal) - plane.DistanceFromOrigin;
                if (Numeric.IsGreater(d, 0, epsilon * (1 + normalLength + (edge2.Origin.Position - edge.Origin.Position).Length())))
                  return false;

                edge2 = edge2.Next;
              } while (edge2.InternalTag != 1);
            }

            edge = edge.Next;
          } while (edge != face.Boundary);
        }
      }
      finally
      {
        ResetInternalTags();
      }

      return true;
    }


    /// <summary>
    /// Cuts mesh with a plane.
    /// </summary>
    /// <param name="plane">The plane.</param>
    /// <returns>
    /// <see langword="true"/> if the mesh was cut; otherwise, <see langword="false"/> if the mesh
    /// was not modified.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The mesh is cut with the given plane. If any parts are in front of the plane, they are
    /// removed. Edges and faces that go through the plane are cut. The resulting mesh is closed
    /// with a new face in the cut plane.
    /// </para>
    /// <para>
    /// If the whole mesh is in front of the plane, the whole mesh is removed (<see cref="Vertex"/>
    /// is set to <see langword="null"/>). If the whole mesh is behind the plane, this method does
    /// nothing.
    /// </para>
    /// <para>
    /// <strong>Prerequisites:</strong> This operation uses <see cref="DcelVertex.UserData"/> of the 
    /// vertices and it assumes that the mesh is valid, convex and closed. All faces must be 
    /// triangles or convex polygons.
    /// </para>
    /// </remarks>
    public bool CutConvex(Plane plane)
    {
      // Cuts the mesh with the given plane. The part over the plane is removed.
      // Prerequisites:
      // - Mesh is valid, convex and closed.
      // - Faces are convex polygons.
      // - Vertex user data is null.
      // Notes:
      // - Vertex user data is used.

      // Get AABB of whole mesh.
      var aabb = GetAabb();

      // Create an epsilon relative to the mesh size.
      float epsilon = Numeric.EpsilonF * (1 + aabb.Extent.Length());

      // Store distance d to plane in all vertices. d is negative if vertex is below the plane.
      float minDistance = Single.PositiveInfinity;
      float maxDistance = Single.NegativeInfinity;
      foreach (var vertex in Vertices)
      {
        float d = Vector3.Dot(vertex.Position, plane.Normal) - plane.DistanceFromOrigin;
        vertex.UserData = d;
        minDistance = Math.Min(d, minDistance);
        maxDistance = Math.Max(d, maxDistance);
      }

      if (minDistance > -epsilon)
      {
        // All vertices are in or above the plane. - The whole mesh is cut away.
        Vertex = null;
        Dirty = true;
        return true;
      }

      if (maxDistance < epsilon)
      {
        // All vertices are in or under the plane. - The mesh is not cut.
        return false;
      }

      // Now, we know that the mesh must be cut.

      // Find a first edge that starts under/in the plane and must be cut.
      DcelEdge first = null;
      foreach (var edge in Edges)
      {
        var startDistance = (float)edge.Origin.UserData;
        var endDistance = (float)edge.Twin.Origin.UserData;
        if (startDistance < -epsilon        // The edge starts below the plane and the start vertex is definitely not on the plane.
            && endDistance > -epsilon)      // The edge ends in or above the plane.
        {
          first = edge;
          break;
        }
      }

      Debug.Assert(first != null, "The mesh is cut by the plane, but could not find an edge that is cut!?");

      // The cut will be sealed with this face.
      DcelFace cap = new DcelFace();

      var outEdge = first;  // Edge that goes out of the plane and has to be cut.

      DcelVertex newVertex = new DcelVertex();
      newVertex.Position = GetCutPosition(outEdge);
      newVertex.Edge = first.Twin;

      do
      {
        // Find inEdge (edge that goes into the plane and must be cut).
        var inEdge = outEdge.Next;
        while ((float)inEdge.Twin.Origin.UserData > -epsilon)   // Loop until the edge end is definitely under the plane.
          inEdge = inEdge.Next;

        // The face will be cut between outEdge and inEdge.

        // Two new half edges:
        var newEdge = new DcelEdge();     // Edge for the cut face.
        var capEdge = new DcelEdge();     // Twin edge on the cap.

        // Update outEdge.
        outEdge.Next = newEdge;

        // Init newEdge.
        newEdge.Face = outEdge.Face;
        newEdge.Origin = newVertex;
        newEdge.Previous = outEdge;
        newEdge.Next = inEdge;
        newEdge.Twin = capEdge;

        // Find next newVertex on inEdge.
        if (inEdge.Twin != first)
        {
          // Find cut on inEdge.
          newVertex = new DcelVertex();
          newVertex.Position = GetCutPosition(inEdge);
          newVertex.Edge = inEdge;
        }
        else
        {
          // We have come full circle. The inEdge is the twin of the first outEdge.
          newVertex = first.Next.Origin;
        }

        // Init capEdge.
        capEdge.Face = cap;
        capEdge.Origin = newVertex;
        capEdge.Twin = newEdge;
        if (cap.Boundary == null)
        {
          cap.Boundary = capEdge;
        }
        else
        {
          capEdge.Next = outEdge.Twin.Previous.Twin;
          capEdge.Next.Previous = capEdge;
        }

        // Update inEdge.
        inEdge.Origin = newVertex;
        inEdge.Previous = newEdge;

        // Make sure the cut face does not link to a removed edge.
        outEdge.Face.Boundary = outEdge;

        // The next outEdge is the twin of the inEdge.
        outEdge = inEdge.Twin;

      } while (outEdge != first);

      // We still have to link the first and the last cap edge.
      var firstCapEdge = cap.Boundary;
      var lastCapEdge = first.Twin.Previous.Twin;
      firstCapEdge.Next = lastCapEdge;
      lastCapEdge.Previous = firstCapEdge;

      // Make sure DcelMesh.Vertex is not one of the removed vertices.
      Vertex = newVertex;

      Dirty = true;

      // Clear vertex user data.
      foreach (var vertex in Vertices)
        vertex.UserData = null;

      return true;
    }


    // edge vertices must have the plane distance in DcelVertex.UserData. This method computes
    // where the edge is split.
    private static Vector3 GetCutPosition(DcelEdge edge)
    {
      var startVertex = edge.Origin;
      var startDistance = (float)startVertex.UserData;

      var endVertex = edge.Twin.Origin;
      var endDistance = (float)endVertex.UserData;

      // Get interpolation parameter. 
      var parameter = Math.Abs(startDistance) / (Math.Abs(startDistance) + Math.Abs(endDistance));

      // Get position where edge is cut.
      var cutPosition = startVertex.Position * (1 - parameter) + endVertex.Position * parameter;
      return cutPosition;
    }


    /// <overloads>
    /// <summary>
    /// Modifies a convex mesh by reducing the vertices and applying a skin width.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Modifies a convex mesh by reducing the vertices and applying a skin width.
    /// </summary>
    /// <param name="vertexLimit">The vertex limit.</param>
    /// <param name="skinWidth">
    /// The width of the skin along the x, y and z directions. 
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Prerequisites:</strong> This operation uses <see cref="DcelVertex.UserData"/> of the
    /// vertices and it assumes that the mesh is valid, convex and closed. All faces must be 
    /// triangles or convex polygons.
    /// </para>
    /// <para>
    /// The number of vertices in the convex mesh is reduced to the given 
    /// <paramref name="vertexLimit"/>. If this operation has to remove vertices, the mesh grows to
    /// a bigger convex mesh. The method tries to remove only the vertices with the least influence.
    /// </para>
    /// <para>
    /// All faces are extruded along the normal by the given skin width. The skin width causes the 
    /// convex mesh to grow (positive skin width) or shrink (negative skin width). The result is
    /// again convex.
    /// </para>
    /// </remarks>
    public void ModifyConvex(int vertexLimit, Vector3 skinWidth)
    {
      // Remember all vertices.
      var originalVertices = Vertices.Select(v => v.Position).ToArray();

      // Store plane for each face.
      var planes = new List<Plane>();
      foreach (var face in Faces)
      {
        var normal = face.Normal;
        if (!normal.TryNormalize())
          continue; // Degenerate faces are ignored.

        var plane = new Plane(normal, face.Boundary.Origin.Position);

        // Apply skinWidth. Without a unit cube transformation we would add the skinWidth
        // to the DistanceFromOrigin. With a scaled skin width, we scale the normalized normal
        // - this is the vector that a point on the face would move. 
        // We project the resulting vector back to the original normal and offset the plane..
        var offset = Vector3.Dot(plane.Normal * skinWidth, plane.Normal);
        plane.DistanceFromOrigin += offset;

        bool planeIsDuplicate = false;
        for (int i = 0; i < planes.Count; i++)
        {
          var existingPlane = planes[i];
          if (Numeric.AreEqual(Vector3.Dot(plane.Normal, existingPlane.Normal), 1))
          {
            planes[i] = new Plane(plane.Normal, Math.Max(plane.DistanceFromOrigin, existingPlane.DistanceFromOrigin));
            planeIsDuplicate = true;
            break;
          }
        }

        if (!planeIsDuplicate)
          planes.Add(plane);
      }

      var aabb = GetAabb();
      // Grow AABB by skin width.
      aabb.Minimum -= skinWidth;
      aabb.Maximum += skinWidth;

      // Optional idea: Use the OBB instead of the AABB for the initial mesh cube.
      //Vector3 extent;
      //Pose pose;
      //GeometryHelper.ComputeBoundingBox(pointList, out extent, out pose);

      // Replace this mesh with a unit cube.
      var cube = CreateCube();
      Vertex = cube.Vertex;
      Dirty = true;

      // Make unit cube represent the AABB.
      foreach (var v in Vertices)
      {
        v.Position = new Vector3(v.Position.X < 0 ? aabb.Minimum.X: aabb.Maximum.X,
                                  v.Position.Y < 0 ? aabb.Minimum.Y: aabb.Maximum.Y,
                                  v.Position.Z < 0 ? aabb.Minimum.Z: aabb.Maximum.Z);

        // Use this if the OBB should be used.
        //v.Position = new Vector3(v.Position.X < 0 ? -extent.X / 2 - skinWidthScale.X : extent.X / 2 + skinWidthScale.X,
        //                          v.Position.Y < 0 ? -extent.Y / 2 - skinWidthScale.Y : extent.Y / 2 + skinWidthScale.Y,
        //                          v.Position.Z < 0 ? -extent.Z / 2 - skinWidthScale.Z : extent.Z / 2 + skinWidthScale.Z);
        //
        //v.Position = pose.ToWorldPosition(v.Position);
      }

      // Now, cut the mesh with the planes until the vertexLimit is reached.
      for (int i = 0; i < planes.Count && Vertices.Count < vertexLimit; i++)
      {
        // Get plane with highest priority.
        var bestPlaneIndex = GetBestCutPlane(planes, this);
        if (bestPlaneIndex < 0)
          break;

        var plane = planes[bestPlaneIndex];

        // Remove plane.
        planes.RemoveAt(bestPlaneIndex);

        // Ignore the plane if it cuts away relevant vertices! This can happen in numerically
        // difficult input sets.
        bool skipPlane = false;
        foreach (var point in originalVertices)
        {
          var offset = Vector3.Dot(plane.Normal * skinWidth, plane.Normal);
          float d = Vector3.Dot(point, plane.Normal) - (plane.DistanceFromOrigin - offset);
          if (d > Numeric.EpsilonF)
          {
            skipPlane = true;
            break;
          }
        }

        // Cut mesh.
        if (!skipPlane)
          CutConvex(plane);
      }
    }

    /// <summary>
    /// Modifies a convex mesh by reducing the vertices and applying a skin width.
    /// </summary>
    /// <param name="vertexLimit">The vertex limit.</param>
    /// <param name="skinWidth">The width of the skin.</param>
    /// <inheritdoc cref="ModifyConvex(int,DigitalRise.Mathematics.Algebra.Vector3)"/>
    public void ModifyConvex(int vertexLimit, float skinWidth)
    {
      ModifyConvex(vertexLimit, new Vector3(skinWidth));
    }


    private static int GetBestCutPlane(List<Plane> planes, DcelMesh mesh)
    {
      // The plane cost is inspired by Stan Melax's convex hull implementation.
      // It can be found in John Ratcliff's Convex Decomposition library.
      float bestPlaneCost = 0;
      int bestPlane = -1;

      for (int i = 0; i < planes.Count; i++)
      {
        var plane = planes[i];

        // Get min and max distance. 
        float minDistance = 0;
        float maxDistance = 0;
        foreach (var v in mesh.Vertices)
        {
          float distance = Vector3.Dot(v.Position, plane.Normal) - plane.DistanceFromOrigin;
          minDistance = Math.Min(distance, minDistance);
          maxDistance = Math.Max(distance, maxDistance);
        }

        float diff = maxDistance - minDistance;

        // The plane cost is the normalized maxDistance.
        float planeCost = maxDistance / diff;
        if (planeCost > bestPlaneCost)
        {
          bestPlaneCost = planeCost;
          bestPlane = i;
        }
      }

      return bestPlane;
    }
  }
}
