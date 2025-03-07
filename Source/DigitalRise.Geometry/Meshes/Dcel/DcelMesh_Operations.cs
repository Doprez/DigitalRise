// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;

namespace DigitalRise.Geometry.Meshes
{
  public partial class DcelMesh
  {
    /// <summary>
    /// Creates a mesh for unit cube.
    /// </summary>
    /// <returns>The DCEL mesh that represent a unit cube.</returns>
    /// <remarks>
    /// The cube is centered at the origin and has 6 faces. The edge length is 2.
    /// </remarks>
    public static DcelMesh CreateCube()
    {
      #region ----- Vertices -----
      var vertex0 = new DcelVertex(new Vector3(-1, -1, -1), null);
      var vertex1 = new DcelVertex(new Vector3( 1, -1, -1), null);
      var vertex2 = new DcelVertex(new Vector3(-1,  1, -1), null);
      var vertex3 = new DcelVertex(new Vector3( 1,  1, -1), null);
      var vertex4 = new DcelVertex(new Vector3(-1, -1,  1), null);
      var vertex5 = new DcelVertex(new Vector3( 1, -1,  1), null);
      var vertex6 = new DcelVertex(new Vector3(-1,  1,  1), null);
      var vertex7 = new DcelVertex(new Vector3( 1,  1,  1), null);
      #endregion

      #region ----- Faces -----
      var near = new DcelFace();    // +z face
      var far = new DcelFace();     // -z face
      var top = new DcelFace();     // +y face
      var bottom = new DcelFace();  // -y face
      var left = new DcelFace();    // -x face
      var right = new DcelFace();   // +x face
      #endregion

      #region ----- Edges -----
      var edge01 = new DcelEdge { Origin = vertex0, Face = bottom };
      var edge10 = new DcelEdge { Origin = vertex1, Face = far };
      var edge13 = new DcelEdge { Origin = vertex1, Face = right };
      var edge31 = new DcelEdge { Origin = vertex3, Face = far };
      var edge23 = new DcelEdge { Origin = vertex2, Face = far };
      var edge32 = new DcelEdge { Origin = vertex3, Face = top };
      var edge02 = new DcelEdge { Origin = vertex0, Face = far };
      var edge20 = new DcelEdge { Origin = vertex2, Face = left };
      var edge26 = new DcelEdge { Origin = vertex2, Face = top };
      var edge62 = new DcelEdge { Origin = vertex6, Face = left };
      var edge37 = new DcelEdge { Origin = vertex3, Face = right };
      var edge73 = new DcelEdge { Origin = vertex7, Face = top };
      var edge04 = new DcelEdge { Origin = vertex0, Face = left };
      var edge40 = new DcelEdge { Origin = vertex4, Face = bottom };
      var edge15 = new DcelEdge { Origin = vertex1, Face = bottom };
      var edge51 = new DcelEdge { Origin = vertex5, Face = right };
      var edge45 = new DcelEdge { Origin = vertex4, Face = near };
      var edge54 = new DcelEdge { Origin = vertex5, Face = bottom };
      var edge57 = new DcelEdge { Origin = vertex5, Face = near };
      var edge75 = new DcelEdge { Origin = vertex7, Face = right };
      var edge46 = new DcelEdge { Origin = vertex4, Face = left };
      var edge64 = new DcelEdge { Origin = vertex6, Face = near };
      var edge67 = new DcelEdge { Origin = vertex6, Face = top };
      var edge76 = new DcelEdge { Origin = vertex7, Face = near };
      #endregion

      #region ----- Set DcelVertex.Edge -----
      vertex0.Edge = edge01;
      vertex1.Edge = edge10;
      vertex2.Edge = edge20;
      vertex3.Edge = edge31;
      vertex4.Edge = edge40;
      vertex5.Edge = edge57;
      vertex6.Edge = edge67;
      vertex7.Edge = edge76;
      #endregion

      #region ----- Set DcelFace.Boundary -----
      near.Boundary = edge57;
      far.Boundary = edge10;
      left.Boundary = edge62;
      right.Boundary = edge51;
      top.Boundary = edge67;
      bottom.Boundary = edge01;
      #endregion

      #region ----- Set DcelEdge.Twin -----
      edge01.Twin = edge10; edge10.Twin = edge01;
      edge13.Twin = edge31; edge31.Twin = edge13;
      edge23.Twin = edge32; edge32.Twin = edge23;
      edge02.Twin = edge20; edge20.Twin = edge02;
      edge26.Twin = edge62; edge62.Twin = edge26;
      edge37.Twin = edge73; edge73.Twin = edge37;
      edge04.Twin = edge40; edge40.Twin = edge04;
      edge15.Twin = edge51; edge51.Twin = edge15;
      edge45.Twin = edge54; edge54.Twin = edge45;
      edge57.Twin = edge75; edge75.Twin = edge57;
      edge46.Twin = edge64; edge64.Twin = edge46;
      edge67.Twin = edge76; edge76.Twin = edge67;
      #endregion

      #region ----- Set DcelEdge.Next/Previous -----
      edge10.Next = edge02; edge02.Next = edge23; edge23.Next = edge31; edge31.Next = edge10; // far
      edge02.Previous = edge10; edge23.Previous = edge02; edge31.Previous = edge23; edge10.Previous = edge31; 

      edge45.Next = edge57; edge57.Next = edge76; edge76.Next = edge64; edge64.Next = edge45; // near
      edge57.Previous = edge45; edge76.Previous = edge57; edge64.Previous = edge76; edge45.Previous = edge64; 

      edge62.Next = edge20; edge20.Next = edge04; edge04.Next = edge46; edge46.Next = edge62; // left
      edge20.Previous = edge62; edge04.Previous = edge20; edge46.Previous = edge04; edge62.Previous = edge46; 

      edge51.Next = edge13; edge13.Next = edge37; edge37.Next = edge75; edge75.Next = edge51; // right
      edge13.Previous = edge51; edge37.Previous = edge13; edge75.Previous = edge37; edge51.Previous = edge75; 

      edge67.Next = edge73; edge73.Next = edge32; edge32.Next = edge26; edge26.Next = edge67; // top
      edge73.Previous = edge67; edge32.Previous = edge73; edge26.Previous = edge32; edge67.Previous = edge26; 

      edge01.Next = edge15; edge15.Next = edge54; edge54.Next = edge40; edge40.Next = edge01; // bottom
      edge15.Previous = edge01; edge54.Previous = edge15; edge40.Previous = edge54; edge01.Previous = edge40; 
      #endregion

      return new DcelMesh { Vertex = vertex0 };
    }

    /// <summary>
    /// Gets the axis-aligned bounding box of this mesh.
    /// </summary>
    /// <returns>
    /// The AABB of this mesh.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Aabb GetAabb()
    {
      if (Vertex == null)
        return new Aabb();

      Aabb aabb = new Aabb(Vertex.Position, Vertex.Position);
      foreach(var v in Vertices)
        aabb.Grow(v.Position);

      return aabb;
    }


    internal void MergeCoplanarFaces()
    {
      UpdateCache();
      foreach (DcelEdge edge in _edges)
      {
        if (edge.InternalTag == 1)
        {
          // Edge has already been visited.
          continue;
        }

        DcelEdge twin = edge.Twin;
        DcelFace face0 = edge.Face;
        DcelFace face1 = twin.Face;

        if (face0 != null && face1 != null)
        {
          // Compare face normals.
          Vector3 normal0 = face0.Normal;
          Vector3 normal1 = face1.Normal;
          float cosα = Vector3.Dot(normal0, normal1) / (normal0.Length() * normal1.Length());
          if (Numeric.AreEqual(cosα, 1))
          {
            // Faces are coplanar:
            // --> Merge faces and remove edge.

            // Get vertices and edges.
            DcelVertex vertex0 = edge.Origin;         // Start vertex of edge.
            DcelVertex vertex1 = twin.Origin;         // End vertex of edge.
            DcelEdge vertex0Incoming = edge.Previous;
            DcelEdge vertex0Outgoing = twin.Next;
            DcelEdge vertex1Incoming = twin.Previous;
            DcelEdge vertex1Outgoing = edge.Next;

            // Update vertices.
            vertex0.Edge = vertex0Outgoing;
            vertex1.Edge = vertex1Outgoing;

            // Update edges.
            vertex0Incoming.Next = vertex0Outgoing;
            vertex0Outgoing.Previous = vertex0Incoming;
            vertex1Incoming.Next = vertex1Outgoing;
            vertex1Outgoing.Previous = vertex1Incoming;

            // Throw away face1. Make sure that all edges point to face0.
            DcelEdge current = vertex0Outgoing;
            do
            {
              current.Face = face0;
              current = current.Next;
            } while (current != vertex0Outgoing);

            Dirty = true;
          }

          // Mark edges as visited.
          edge.InternalTag = 1;
          twin.InternalTag = 1;
        }
      }

      ResetTags();
    }
  }
}
