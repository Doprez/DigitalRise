﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Meshes
{
  /// <summary>
  /// Describes a triangle during the convex decomposition process.
  /// </summary>
  [DebuggerDisplay("Triangle: Island = {Island.Id}")]
  internal sealed class CDTriangle
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A unique number
    public int Id;

    // The three vertices.
    public Vector3[] Vertices;

    // The triangle face normal.
    public Vector3 Normal;

    // The averaged vertex normals.
    // TODO: If this info is stored per triangle it is stored duplicated in all triangles that share this vertex. 
    public Vector3[] VertexNormals = new Vector3[3];

    // The neighbor triangles.
    // Neighbor[i] contains the neighbor on the edge that does NOT involve vertex i.
    // Example: Vertices V0, V1, V2. Neighbor[2] is the neighbor on the V0-V1 edge.
    // A neighbor link can be null for triangles on a perimeter of an open meshes.
    public CDTriangle[] Neighbors = new CDTriangle[3];

    // The island to which this triangle belongs. 
    public CDIsland Island;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Creates CDTriangle.Neighbor references if the triangles are edge neighbors.
    public static void FindNeighbors(CDTriangle triangleA, CDTriangle triangleB)
    {
      // Loop through all edges of A.
      for (int i = 0; i < 3; i++)
      {
        if (triangleA.Neighbors[i] != null)
          continue;

        // Get edge opposite of vertex i.
        Vector3 startEdgeA = triangleA.Vertices[(i + 1) % 3];
        Vector3 endEdgeA = triangleA.Vertices[(i + 2) % 3];

        // Loop through all edges of B.
        for (int j = 0; j < 3; j++)
        {
          if (triangleB.Neighbors[j] != null)
            continue;

          // Get edge opposite of vertex j.
          Vector3 startEdgeB = triangleB.Vertices[(j + 1) % 3];
          Vector3 endEdgeB = triangleB.Vertices[(j + 2) % 3];

          // Check if edges use the same vertices like to DCEL half edges.
          if (MathHelper.AreNumericallyEqual(startEdgeA, endEdgeB)
              && MathHelper.AreNumericallyEqual(endEdgeA, startEdgeB))
          {
            // Store neighbor links.
            triangleA.Neighbors[i] = triangleB;
            triangleB.Neighbors[j] = triangleA;
          }
        }
      }
    }
    #endregion
  }
}
