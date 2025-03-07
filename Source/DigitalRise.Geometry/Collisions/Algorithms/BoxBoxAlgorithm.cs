// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="BoxShape"/> vs. <see cref="BoxShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class BoxBoxAlgorithm : CollisionAlgorithm
  {
    // We use a separating axis test (see Coutinho: "Dynamic Simulations of Multibody Systems",
    // Ericson: "Real-Time Collision Detection").
    // The face-face computation is based on the algorithm in Bullet (see credits below).


    /// <summary>
    /// The maximal number of contacts to keep for face-face collisions.
    /// </summary>
    private const int MaxNumberOfContacts = 4;


    /// <summary>
    /// Initializes a new instance of the <see cref="BoxBoxAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public BoxBoxAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain two <see cref="BoxShape"/>es.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <paramref name="type"/> is set to <see cref="CollisionQueryType.ClosestPoints"/>. This 
    /// collision algorithm cannot handle closest-point queries. Use <see cref="Gjk"/> instead.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Invoke GJK for closest points.
      if (type == CollisionQueryType.ClosestPoints)
        throw new GeometryException("This collision algorithm cannot handle closest-point queries. Use GJK instead.");

      CollisionObject collisionObjectA = contactSet.ObjectA;
      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      BoxShape boxA = geometricObjectA.Shape as BoxShape;
      BoxShape boxB = geometricObjectB.Shape as BoxShape;

      // Check if collision objects shapes are correct.
      if (boxA == null || boxB == null)
        throw new ArgumentException("The contact set must contain box shapes.", "contactSet");

      Vector3 scaleA = MathHelper.Absolute(geometricObjectA.Scale);
      Vector3 scaleB = MathHelper.Absolute(geometricObjectB.Scale);
      Pose poseA = geometricObjectA.Pose;
      Pose poseB = geometricObjectB.Pose;

      // We perform the separating axis test in the local space of A.
      // The following variables are in local space of A.

      // Center of box B.
      Vector3 cB = poseA.ToLocalPosition(poseB.Position);
      // Orientation matrix of box B
      Matrix33F mB = poseA.Orientation.Transposed * poseB.Orientation;
      // Absolute of mB.
      Matrix33F aMB = Matrix33F.Absolute(mB);

      // Half extent vectors of the boxes.
      Vector3 eA = 0.5f * boxA.Extent * scaleA;
      Vector3 eB = 0.5f * boxB.Extent * scaleB;

      // ----- Separating Axis tests
      // If the boxes are separated, we immediately return.
      // For the case of interpenetration, we store the smallest penetration depth.
      float smallestPenetrationDepth = float.PositiveInfinity;
      int separatingAxisNumber = 0;
      Vector3 normal = Vector3.UnitX;
      bool isNormalInverted = false;
      contactSet.HaveContact = false;         // Assume no contact.

      #region ----- Case 1: Separating Axis: (1, 0, 0) -----
      float separation = Math.Abs(cB.X) - (eA.X + eB.X * aMB.M00 + eB.Y * aMB.M01 + eB.Z * aMB.M02);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean && -separation < smallestPenetrationDepth)
      {
        normal = Vector3.UnitX;
        smallestPenetrationDepth = -separation;
        isNormalInverted = cB.X < 0;
        separatingAxisNumber = 1;
      }
      #endregion

      #region ----- Case 2: Separating Axis: (0, 1, 0) -----
      separation = Math.Abs(cB.Y) - (eA.Y + eB.X * aMB.M10 + eB.Y * aMB.M11 + eB.Z * aMB.M12);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean && -separation < smallestPenetrationDepth)
      {
        normal = Vector3.UnitY;
        smallestPenetrationDepth = -separation;
        isNormalInverted = cB.Y < 0;
        separatingAxisNumber = 2;
      }
      #endregion

      #region ----- Case 3: Separating Axis: (0, 0, 1) -----
      separation = Math.Abs(cB.Z) - (eA.Z + eB.X * aMB.M20 + eB.Y * aMB.M21 + eB.Z * aMB.M22);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean && -separation < smallestPenetrationDepth)
      {
        normal = Vector3.UnitZ;
        smallestPenetrationDepth = -separation;
        isNormalInverted = cB.Z < 0;
        separatingAxisNumber = 3;
      }
      #endregion

      #region ----- Case 4: Separating Axis: OrientationB * (1, 0, 0) -----
      float expression = cB.X * mB.M00 + cB.Y * mB.M10 + cB.Z * mB.M20;
      separation = Math.Abs(expression) - (eB.X + eA.X * aMB.M00 + eA.Y * aMB.M10 + eA.Z * aMB.M20);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean && -separation < smallestPenetrationDepth)
      {
        normal = mB.GetColumn(0);
        smallestPenetrationDepth = -separation;
        isNormalInverted = expression < 0;
        separatingAxisNumber = 4;
      }
      #endregion

      #region ----- Case 5: Separating Axis: OrientationB * (0, 1, 0) -----
      expression = cB.X * mB.M01 + cB.Y * mB.M11 + cB.Z * mB.M21;
      separation = Math.Abs(expression) - (eB.Y + eA.X * aMB.M01 + eA.Y * aMB.M11 + eA.Z * aMB.M21);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean && -separation < smallestPenetrationDepth)
      {
        normal = mB.GetColumn(1);
        smallestPenetrationDepth = -separation;
        isNormalInverted = expression < 0;
        separatingAxisNumber = 5;
      }
      #endregion

      #region ----- Case 6: Separating Axis: OrientationB * (0, 0, 1) -----
      expression = cB.X * mB.M02 + cB.Y * mB.M12 + cB.Z * mB.M22;
      separation = Math.Abs(expression) - (eB.Z + eA.X * aMB.M02 + eA.Y * aMB.M12 + eA.Z * aMB.M22);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean && -separation < smallestPenetrationDepth)
      {
        normal = mB.GetColumn(2);
        smallestPenetrationDepth = -separation;
        isNormalInverted = expression < 0;
        separatingAxisNumber = 6;
      }
      #endregion

      // The next 9 tests are edge-edge cases. The normal vector has to be normalized 
      // to get the right penetration depth.
      // normal = Normalize(edgeA x edgeB)
      Vector3 separatingAxis;
      float length;

      #region ----- Case 7: Separating Axis: (1, 0, 0) x (OrientationB * (1, 0, 0)) -----
      expression = cB.Z * mB.M10 - cB.Y * mB.M20;
      separation = Math.Abs(expression) - (eA.Y * aMB.M20 + eA.Z * aMB.M10 + eB.Y * aMB.M02 + eB.Z * aMB.M01);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(0, -mB.M20, mB.M10);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 7;
        }
      }
      #endregion

      #region ----- Case 8: Separating Axis: (1, 0, 0) x (OrientationB * (0, 1, 0)) -----
      expression = cB.Z * mB.M11 - cB.Y * mB.M21;
      separation = Math.Abs(expression) - (eA.Y * aMB.M21 + eA.Z * aMB.M11 + eB.X * aMB.M02 + eB.Z * aMB.M00);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(0, -mB.M21, mB.M11);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 8;
        }
      }
      #endregion

      #region ----- Case 9: Separating Axis: (1, 0, 0) x (OrientationB * (0, 0, 1)) -----
      expression = cB.Z * mB.M12 - cB.Y * mB.M22;
      separation = Math.Abs(expression) - (eA.Y * aMB.M22 + eA.Z * aMB.M12 + eB.X * aMB.M01 + eB.Y * aMB.M00);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(0, -mB.M22, mB.M12);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 9;
        }
      }
      #endregion

      #region ----- Case 10: Separating Axis: (0, 1, 0) x (OrientationB * (1, 0, 0)) -----
      expression = cB.X * mB.M20 - cB.Z * mB.M00;
      separation = Math.Abs(expression) - (eA.X * aMB.M20 + eA.Z * aMB.M00 + eB.Y * aMB.M12 + eB.Z * aMB.M11);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(mB.M20, 0, -mB.M00);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 10;
        }
      }
      #endregion

      #region ----- Case 11: Separating Axis: (0, 1, 0) x (OrientationB * (0, 1, 0)) -----
      expression = cB.X * mB.M21 - cB.Z * mB.M01;
      separation = Math.Abs(expression) - (eA.X * aMB.M21 + eA.Z * aMB.M01 + eB.X * aMB.M12 + eB.Z * aMB.M10);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(mB.M21, 0, -mB.M01);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 11;
        }
      }
      #endregion

      #region ----- Case 12: Separating Axis: (0, 1, 0) x (OrientationB * (0, 0, 1)) -----
      expression = cB.X * mB.M22 - cB.Z * mB.M02;
      separation = Math.Abs(expression) - (eA.X * aMB.M22 + eA.Z * aMB.M02 + eB.X * aMB.M11 + eB.Y * aMB.M10);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(mB.M22, 0, -mB.M02);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 12;
        }
      }
      #endregion

      #region ----- Case 13: Separating Axis: (0, 0, 1) x (OrientationB * (1, 0, 0)) -----
      expression = cB.Y * mB.M00 - cB.X * mB.M10;
      separation = Math.Abs(expression) - (eA.X * aMB.M10 + eA.Y * aMB.M00 + eB.Y * aMB.M22 + eB.Z * aMB.M21);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(-mB.M10, mB.M00, 0);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 13;
        }
      }
      #endregion

      #region ----- Case 14: Separating Axis: (0, 0, 1) x (OrientationB * (0, 1, 0)) -----
      expression = cB.Y * mB.M01 - cB.X * mB.M11;
      separation = Math.Abs(expression) - (eA.X * aMB.M11 + eA.Y * aMB.M01 + eB.X * aMB.M22 + eB.Z * aMB.M20);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(-mB.M11, mB.M01, 0);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 14;
        }
      }
      #endregion

      #region ----- Case 15: Separating Axis: (0, 0, 1) x (OrientationB * (0, 0, 1)) -----
      expression = cB.Y * mB.M02 - cB.X * mB.M12;
      separation = Math.Abs(expression) - (eA.X * aMB.M12 + eA.Y * aMB.M02 + eB.X * aMB.M21 + eB.Y * aMB.M20);
      if (separation > 0)
        return;

      if (type != CollisionQueryType.Boolean)
      {
        separatingAxis = new Vector3(-mB.M12, mB.M02, 0);
        length = separatingAxis.Length();
        separation /= length;
        if (-separation < smallestPenetrationDepth)
        {
          normal = separatingAxis / length;
          smallestPenetrationDepth = -separation;
          isNormalInverted = expression < 0;
          separatingAxisNumber = 15;
        }
      }
      #endregion

      // We have a contact.
      contactSet.HaveContact = true;

      // HaveContact queries can exit here.
      if (type == CollisionQueryType.Boolean)
        return;

      // Lets find the contact info.
      Debug.Assert(smallestPenetrationDepth >= 0, "The smallest penetration depth should be greater than or equal to 0.");

      if (isNormalInverted)
        normal = -normal;

      // Transform normal from local space of A to world space.
      Vector3 normalWorld = poseA.ToWorldDirection(normal);

      if (separatingAxisNumber > 6)
      {
        // The intersection was detected by an edge-edge test.
        // Get the intersecting edges.
        // Separating axes: 
        //  7 = x edge on A, x edge on B
        //  8 = x edge on A, y edge on B
        //  9 = x edge on A, Z edge on B
        // 10 = y edge on A, x edge on B
        // ...
        // 15 = z edge on A, z edge on B
        var edgeA = boxA.GetEdge((separatingAxisNumber - 7) / 3, normal, scaleA);
        var edgeB = boxB.GetEdge((separatingAxisNumber - 7) % 3, Matrix33F.MultiplyTransposed(mB, -normal), scaleB);
        edgeB.Start = mB * edgeB.Start + cB;
        edgeB.End = mB * edgeB.End + cB;

        Vector3 position;
        Vector3 dummy;
        GeometryHelper.GetClosestPoints(edgeA, edgeB, out position, out dummy);
        position = position - normal * (smallestPenetrationDepth / 2);  // Position is between the positions of the box surfaces.              

        // Convert back position from local space of A to world space;
        position = poseA.ToWorldPosition(position);

        Contact contact = ContactHelper.CreateContact(contactSet, position, normalWorld, smallestPenetrationDepth, false);
        ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
      }
      else if (1 <= separatingAxisNumber && separatingAxisNumber <= 6)
      {
        // The intersection was detected by a face vs. * test.
        // The separating axis is perpendicular to a face.

        #region ----- Credits -----
        // The face vs. * test is based on the algorithm of the Bullet Continuous Collision 
        // Detection and Physics Library. DigitalRise Geometry contains a new and improved 
        // implementation of the original algorithm.
        // 
        // The box-box detector in Bullet contains the following remarks:
        // 
        //    Box-Box collision detection re-distributed under the ZLib license with permission from Russell L. Smith
        //    Original version is from Open Dynamics Engine, Copyright (C) 2001,2002 Russell L. Smith.
        //    All rights reserved.  Email: russ@q12.org   Web: www.q12.org
        //
        //    Bullet Continuous Collision Detection and Physics Library
        //    Copyright (c) 2003-2006 Erwin Coumans  http://continuousphysics.com/Bullet/
        //
        //    This software is provided 'as-is', without any express or implied warranty.
        //    In no event will the authors be held liable for any damages arising from the use of this software.
        //    Permission is granted to anyone to use this software for any purpose, 
        //    including commercial applications, and to alter it and redistribute it freely, 
        //    subject to the following restrictions:
        //
        //    1. The origin of this software must not be misrepresented; you must not claim that you wrote the 
        //       original software. If you use this software in a product, an acknowledgment in the product 
        //       documentation would be appreciated but is not required.
        //    2. Altered source versions must be plainly marked as such, and must not be misrepresented as being 
        //       the original software.
        //    3. This notice may not be removed or altered from any source distribution.
        #endregion

        // We define the face perpendicular to the separating axis to be the "reference face".
        // The face of the other box closest to the reference face is called the "incident face".
        // Accordingly, we will call the box containing the reference face the "reference box" and
        // the box containing the incident face the "incident box".
        //
        // We will transform the incident face into the 2D space of reference face. Then we will
        // clip the incident face against the reference face. The polygon resulting from the 
        // intersection will be transformed back into world space and the points of the polygon will
        // be the candidates for the contact points.

        Pose poseR;             // Pose of reference box.
        Pose poseI;             // Pose of incident box.
        Vector3 boxExtentR;    // Half extent of reference box.
        Vector3 boxExtentI;    // Half extent of incident box.

        // Contact normal (= normal of reference face) in world space.
        if (separatingAxisNumber <= 3)
        {
          poseR = poseA;
          poseI = poseB;
          boxExtentR = eA;
          boxExtentI = eB;
          isNormalInverted = false;
        }
        else
        {
          poseR = poseB;
          poseI = poseA;
          boxExtentR = eB;
          boxExtentI = eA;
          normalWorld = -normalWorld;
          isNormalInverted = true;
        }

        // Contact normal in local space of incident box.
        Vector3 normalI = poseI.ToLocalDirection(normalWorld);

        Vector3 absNormal = MathHelper.Absolute(normalI);

        Vector3 xAxisInc, yAxisInc;  // The basis of the incident-face space.
        float absFaceOffsetI;         // The offset of the incident face to the center of the box.
        Vector2 faceExtentI;         // The half extent of the incident face.
        Vector3 faceNormal;          // The normal of the incident face in world space.
        float faceDirection;          // A value indicating the direction of the incident face.

        // Find the largest component of the normal. The largest component indicates which face is
        // the incident face.
        switch (MathHelper.Absolute(normalI).IndexOfLargestComponent())
        {
          case 0:
            faceExtentI.X = boxExtentI.Y;
            faceExtentI.Y = boxExtentI.Z;
            absFaceOffsetI = boxExtentI.X;
            faceNormal = poseI.Orientation.GetColumn(0);
            xAxisInc = poseI.Orientation.GetColumn(1);
            yAxisInc = poseI.Orientation.GetColumn(2);
            faceDirection = normalI.X;
            break;
          case 1:
            faceExtentI.X = boxExtentI.X;
            faceExtentI.Y = boxExtentI.Z;
            absFaceOffsetI = boxExtentI.Y;
            faceNormal = poseI.Orientation.GetColumn(1);
            xAxisInc = poseI.Orientation.GetColumn(0);
            yAxisInc = poseI.Orientation.GetColumn(2);
            faceDirection = normalI.Y;
            break;
          // case 2:
          default:
            faceExtentI.X = boxExtentI.X;
            faceExtentI.Y = boxExtentI.Y;
            absFaceOffsetI = boxExtentI.Z;
            faceNormal = poseI.Orientation.GetColumn(2);
            xAxisInc = poseI.Orientation.GetColumn(0);
            yAxisInc = poseI.Orientation.GetColumn(1);
            faceDirection = normalI.Z;
            break;
        }

        // Compute center of incident face relative to the center of the reference box in world space.
        float faceOffset = (faceDirection < 0) ? absFaceOffsetI : -absFaceOffsetI;
        Vector3 centerOfFaceI = faceNormal * faceOffset + poseI.Position - poseR.Position;

        // (Note: We will use the center of the incident face to compute the points of the incident
        // face and transform the points into the reference-face frame. The center of the incident
        // face is relative to the center of the reference box. We could also get center of the 
        // incident face relative to the center of the reference face. But since we are projecting 
        // the points from 3D to 2D this does not matter.)

        Vector3 xAxisR, yAxisR;    // The basis of the reference-face space.
        float faceOffsetR;          // The offset of the reference face to the center of the box.
        Vector2 faceExtentR;       // The half extent of the reference face.
        switch (separatingAxisNumber)
        {
          case 1:
          case 4:
            faceExtentR.X = boxExtentR.Y;
            faceExtentR.Y = boxExtentR.Z;
            faceOffsetR = boxExtentR.X;
            xAxisR = poseR.Orientation.GetColumn(1);
            yAxisR = poseR.Orientation.GetColumn(2);
            break;
          case 2:
          case 5:
            faceExtentR.X = boxExtentR.X;
            faceExtentR.Y = boxExtentR.Z;
            faceOffsetR = boxExtentR.Y;
            xAxisR = poseR.Orientation.GetColumn(0);
            yAxisR = poseR.Orientation.GetColumn(2);
            break;
          // case 3:
          // case 6:
          default:
            faceExtentR.X = boxExtentR.X;
            faceExtentR.Y = boxExtentR.Y;
            faceOffsetR = boxExtentR.Z;
            xAxisR = poseR.Orientation.GetColumn(0);
            yAxisR = poseR.Orientation.GetColumn(1);
            break;
        }

        // Compute the center of the incident face in the reference-face frame.
        // We can simply project centerOfFaceI onto the x- and y-axis of the reference 
        // face.
        Vector2 centerOfFaceIInR;
        
        //centerOfFaceIInR.X = Vector3.Dot(centerOfFaceI, xAxisR);
        // ----- Optimized version: 
        centerOfFaceIInR.X = centerOfFaceI.X * xAxisR.X + centerOfFaceI.Y * xAxisR.Y + centerOfFaceI.Z * xAxisR.Z;
        
        //centerOfFaceIInR.Y = Vector3.Dot(centerOfFaceI, yAxisR);
        // ----- Optimized version:
        centerOfFaceIInR.Y = centerOfFaceI.X * yAxisR.X + centerOfFaceI.Y * yAxisR.Y + centerOfFaceI.Z * yAxisR.Z;

        // Now, we have the center of the incident face in reference-face coordinates.
        // To compute the corners of the incident face in reference-face coordinates, we need 
        // transform faceExtentI (the half extent vector of the incident face) from the incident-
        // face frame to the reference-face frame to compute the corners.
        //
        // The reference-face frame has the basis 
        //   mR = (xAxisR, yAxisR, ?)
        //
        // The incident-face frame has the basis 
        //   mI = (xAxisI, yAxisI, ?)
        //
        // Rotation from incident-face frame to reference-face frame is
        //   mIToR = mR^-1 * mI
        //
        // The corner offsets in incident-face space is are vectors (x, y, 0). To transform these
        // vectors from incident-face space to reference-face space we need to calculate:
        //   mIToR * v
        //
        // Since the z-components are 0 and we are only interested in the resulting x, y coordinates
        // in reference-space when can reduce the rotation to a 2 x 2 matrix. (The other components
        // are not needed.)

        // ----- Optimized version: (Original on the right)
        Matrix22F mIToR;
        mIToR.M00 = xAxisR.X * xAxisInc.X + xAxisR.Y * xAxisInc.Y + xAxisR.Z * xAxisInc.Z;  // mIToR.M00 = Vector3.Dot(xAxisR, xAxisInc);
        mIToR.M01 = xAxisR.X * yAxisInc.X + xAxisR.Y * yAxisInc.Y + xAxisR.Z * yAxisInc.Z;  // mIToR.M01 = Vector3.Dot(xAxisR, yAxisInc);
        mIToR.M10 = yAxisR.X * xAxisInc.X + yAxisR.Y * xAxisInc.Y + yAxisR.Z * xAxisInc.Z;  // mIToR.M10 = Vector3.Dot(yAxisR, xAxisInc);
        mIToR.M11 = yAxisR.X * yAxisInc.X + yAxisR.Y * yAxisInc.Y + yAxisR.Z * yAxisInc.Z;  // mIToR.M11 = Vector3.Dot(yAxisR, yAxisInc);

        // The corner offsets in incident-face space are:
        //  (-faceExtentI.X, -faceExtentI.Y) ... left, bottom corner
        //  ( faceExtentI.X, -faceExtentI.Y) ... right, bottom corner
        //  ( faceExtentI.X,  faceExtentI.Y) ... right, top corner
        //  (-faceExtentI.X,  faceExtentI.Y) ... left, top corner
        //
        // Instead of transforming each corner offset, we can optimize the computation: Do the 
        // matrix-vector multiplication once, keep the intermediate products, apply the sign
        // of the components when adding the intermediate results.

        float k1 = mIToR.M00 * faceExtentI.X;   // Products of matrix-vector multiplication.
        float k2 = mIToR.M01 * faceExtentI.Y;
        float k3 = mIToR.M10 * faceExtentI.X;
        float k4 = mIToR.M11 * faceExtentI.Y;
        List<Vector2> quad = DigitalRise.ResourcePools<Vector2>.Lists.Obtain();
        quad.Add(new Vector2(centerOfFaceIInR.X - k1 - k2, centerOfFaceIInR.Y - k3 - k4));
        quad.Add(new Vector2(centerOfFaceIInR.X + k1 - k2, centerOfFaceIInR.Y + k3 - k4));
        quad.Add(new Vector2(centerOfFaceIInR.X + k1 + k2, centerOfFaceIInR.Y + k3 + k4));
        quad.Add(new Vector2(centerOfFaceIInR.X - k1 + k2, centerOfFaceIInR.Y - k3 + k4));

        // Clip incident face (quadrilateral) against reference face (rectangle).
        List<Vector2> contacts2D = ClipQuadrilateralAgainstRectangle(faceExtentR, quad);

        // Transform contact points back to world space and compute penetration depths.
        int numberOfContacts = contacts2D.Count;
        List<Vector3> contacts3D = DigitalRise.ResourcePools<Vector3>.Lists.Obtain();
        List<float> penetrationDepths = DigitalRise.ResourcePools<float>.Lists.Obtain();
        Matrix22F mRToI = mIToR.Inverse;
        for (int i = numberOfContacts - 1; i >= 0; i--)
        {
          Vector2 contact2DR = contacts2D[i];                            // Contact in reference-face space.
          Vector2 contact2DI = mRToI * (contact2DR - centerOfFaceIInR);  // Contact in incident-face space.

          // Transform point in incident-face space to world (relative to center of reference box).
          // contact3D = mI * (x, y, 0) + centerOfFaceI
          Vector3 contact3D;
          contact3D.X = xAxisInc.X * contact2DI.X + yAxisInc.X * contact2DI.Y + centerOfFaceI.X;
          contact3D.Y = xAxisInc.Y * contact2DI.X + yAxisInc.Y * contact2DI.Y + centerOfFaceI.Y;
          contact3D.Z = xAxisInc.Z * contact2DI.X + yAxisInc.Z * contact2DI.Y + centerOfFaceI.Z;

          // Compute penetration depth.

          //float penetrationDepth = faceOffsetR - Vector3.Dot(normalWorld, contact3D);
          // ----- Optimized version:
          float penetrationDepth = faceOffsetR - (normalWorld.X * contact3D.X + normalWorld.Y * contact3D.Y + normalWorld.Z * contact3D.Z);
          
          if (penetrationDepth >= 0)
          {
            // Valid contact.
            contacts3D.Add(contact3D);
            penetrationDepths.Add(penetrationDepth);
          }
          else
          {
            // Remove bad contacts from the 2D contacts.
            // (We might still need the 2D contacts, if we need to reduce the contacts.)
            contacts2D.RemoveAt(i);
          }
        }

        numberOfContacts = contacts3D.Count;
        if (numberOfContacts == 0)
          return; // Should never happen.

        // Revert normal back to original direction.
        normal = (isNormalInverted) ? -normalWorld : normalWorld;

        // Note: normal ........ contact normal pointing from box A to B.
        //       normalWorld ... contact normal pointing from reference box to incident box.

        if (numberOfContacts <= MaxNumberOfContacts)
        {
          // Add all contacts to contact set.
          for (int i = 0; i < numberOfContacts; i++)
          {
            float penetrationDepth = penetrationDepths[i];

            // Position is between the positions of the box surfaces.
            Vector3 position = contacts3D[i] + poseR.Position + normalWorld * (penetrationDepth / 2);

            Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
            ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
          }
        }
        else
        {
          // Reduce number of contacts, keep the contact with the max penetration depth.
          int indexOfDeepest = 0;
          float maxPenetrationDepth = penetrationDepths[0];
          for (int i = 1; i < numberOfContacts; i++)
          {
            float penetrationDepth = penetrationDepths[i];
            if (penetrationDepth > maxPenetrationDepth)
            {
              maxPenetrationDepth = penetrationDepth;
              indexOfDeepest = i;
            }
          }

          List<int> indicesOfContacts = ReduceContacts(contacts2D, indexOfDeepest, MaxNumberOfContacts);

          // Add selected contacts to contact set.
          numberOfContacts = indicesOfContacts.Count;
          for (int i = 0; i < numberOfContacts; i++)
          {
            int index = indicesOfContacts[i];
            float penetrationDepth = penetrationDepths[index];

            // Position is between the positions of the box surfaces.
            Vector3 position = contacts3D[index] + poseR.Position + normalWorld * (penetrationDepth / 2);

            Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepths[index], false);
            ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
          }

          DigitalRise.ResourcePools<int>.Lists.Recycle(indicesOfContacts);
        }

        DigitalRise.ResourcePools<Vector2>.Lists.Recycle(contacts2D);
        DigitalRise.ResourcePools<Vector3>.Lists.Recycle(contacts3D);
        DigitalRise.ResourcePools<float>.Lists.Recycle(penetrationDepths);
      }
    }


    /// <summary>
    /// Clips the given quadrilateral against a rectangle in 2D.
    /// </summary>
    /// <param name="rect">The half extent of the rectangle.</param>
    /// <param name="quad">The points of the quadrilateral.</param>
    /// <returns>
    /// A polygon which is the intersection between the rectangle and the quadrilateral.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static List<Vector2> ClipQuadrilateralAgainstRectangle(Vector2 rect, List<Vector2> quad)
    {
      List<Vector2> polygon = quad;
      List<Vector2> clippedPolygon = DigitalRise.ResourcePools<Vector2>.Lists.Obtain();
      int numberOfPoints = polygon.Count;

      // ----- Clip polygon against right side of rectangle (rect.X).
      for (int i = 0; i < numberOfPoints; i++)
      {
        float clipX = rect.X;
        Vector2 p = polygon[i];
        if (p.X <= clipX)
        {
          // Keep p.
          clippedPolygon.Add(p);
        }

        // Clip line between point i and point i + 1.
        Vector2 pNext = (i < numberOfPoints - 1) ? polygon[i + 1] : polygon[0];
        if (p.X < clipX && pNext.X > clipX || p.X > clipX && pNext.X < clipX)
        {
          // Clip the polygon edge.
          //clippedPolygon.Add(ClipLineHorizontally(p, pNext, clipX));
          
          // ----- Optimized version:
          clippedPolygon.Add(new Vector2(clipX, p.Y + (clipX - p.X) * (pNext.Y - p.Y) / (pNext.X - p.X)));
        }
      }

      Mathematics.MathHelper.Swap(ref polygon, ref clippedPolygon);
      numberOfPoints = polygon.Count;
      clippedPolygon.Clear();

      // ----- Clip polygon against left side of rectangle (-rect.X).
      for (int i = 0; i < numberOfPoints; i++)
      {
        float clipX = -rect.X;
        Vector2 p = polygon[i];
        if (clipX <= p.X)
        {
          // Keep p.
          clippedPolygon.Add(p);
        }

        // Clip line between point i and point i + 1.
        Vector2 pNext = (i < numberOfPoints - 1) ? polygon[i + 1] : polygon[0];
        if (p.X < clipX && pNext.X > clipX || p.X > clipX && pNext.X < clipX)
        {
          // Clip the polygon edge.
          //clippedPolygon.Add(ClipLineHorizontally(p, pNext, clipX));

          // ----- Optimized version:
          clippedPolygon.Add(new Vector2(clipX, p.Y + (clipX - p.X) * (pNext.Y - p.Y) / (pNext.X - p.X)));
        }
      }

			Mathematics.MathHelper.Swap(ref polygon, ref clippedPolygon);
      numberOfPoints = polygon.Count;
      clippedPolygon.Clear();

      // ----- Clip polygon against top of rectangle (rect.Y).
      for (int i = 0; i < numberOfPoints; i++)
      {
        float clipY = rect.Y;
        Vector2 p = polygon[i];
        if (p.Y <= clipY)
        {
          // Keep p.
          clippedPolygon.Add(p);
        }

        // Clip line between point i and point i + 1.
        Vector2 pNext = (i < numberOfPoints - 1) ? polygon[i + 1] : polygon[0];
        if (p.Y < clipY && pNext.Y > clipY || p.Y > clipY && pNext.Y < clipY)
        {
          // Clip the polygon edge.
          //clippedPolygon.Add(ClipLineVertically(p, pNext, clipY));

          // ----- Optimized version:
          clippedPolygon.Add(new Vector2(p.X + (clipY - p.Y) * (pNext.X - p.X) / (pNext.Y - p.Y), clipY));
        }
      }

			Mathematics.MathHelper.Swap(ref polygon, ref clippedPolygon);
      numberOfPoints = polygon.Count;
      clippedPolygon.Clear();

      // ----- Clip polygon against bottom of rectangle (-rect.Y).
      for (int i = 0; i < numberOfPoints; i++)
      {
        float clipY = -rect.Y;
        Vector2 p = polygon[i];
        if (clipY <= p.Y)
        {
          // Keep p.
          clippedPolygon.Add(p);
        }

        // Clip line between point i and point i + 1.
        Vector2 pNext = (i < numberOfPoints - 1) ? polygon[i + 1] : polygon[0];
        if (p.Y < clipY && pNext.Y > clipY || p.Y > clipY && pNext.Y < clipY)
        {
          // Clip the polygon edge.
          //clippedPolygon.Add(ClipLineVertically(p, pNext, clipY));
          
          // ----- Optimized version:
          clippedPolygon.Add(new Vector2(p.X + (clipY - p.Y) * (pNext.X - p.X) / (pNext.Y - p.Y), clipY));
        }
      }

      DigitalRise.ResourcePools<Vector2>.Lists.Recycle(polygon);
      return clippedPolygon;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    private static Vector2 ClipLineHorizontally(Vector2 p0, Vector2 p1, float clipX)
    {
      return new Vector2(
        clipX,
        p0.Y + (clipX - p0.X) * (p1.Y - p0.Y) / (p1.X - p0.X));
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    private static Vector2 ClipLineVertically(Vector2 p0, Vector2 p1, float clipY)
    {
      return new Vector2(
        p0.X + (clipY - p0.Y) * (p1.X - p0.X) / (p1.Y - p0.Y),
        clipY);
    }


    /// <summary>
    /// Selects the best contacts.
    /// </summary>
    /// <param name="points">The 2D points in reference-face space.</param>
    /// <param name="indexOfBest">The index of the point to keep.</param>
    /// <param name="maxNumberOfContacts">The max number of contact points to keep.</param>
    /// <returns>The indices of the contact points that should be kept.</returns>
    private static List<int> ReduceContacts(List<Vector2> points, int indexOfBest, int maxNumberOfContacts)
    {
      Debug.Assert(points != null);
      Debug.Assert(points.Count > 0);
      Debug.Assert(indexOfBest >= 0);
      Debug.Assert(maxNumberOfContacts > 0);

      // The centroid of the polygon.
      Vector2 centroid;
      int numberOfPoints = points.Count;
      if (numberOfPoints == 1)
      {
        // No polygon: Just a single point.
        centroid = points[0];
      }
      else if (numberOfPoints == 2)
      {
        // No polygon: Just a line.
        centroid = 0.5f * (points[0] + points[1]);
      }
      else
      {
        // Compute centroid of polygon.
        // See http://en.wikipedia.org/wiki/Centroid#Centroid_of_polygon
        centroid = new Vector2();
        float a = 0;    // The polygon's signed area * 2.
        Vector2 p;     // Point i
        Vector2 pNext; // Point i + 1
        float s;
        for (int i = 0; i < numberOfPoints - 1; i++)
        {
          p = points[i];
          pNext = points[i + 1];
          s = p.X * pNext.Y - pNext.X * p.Y;
          a += s;
          centroid += (p + pNext) * s;
        }
        p = points[numberOfPoints - 1];
        pNext = points[0];
        s = p.X * pNext.Y - pNext.X * p.Y;
        a += s;
        centroid += (p + pNext) * s;
        if (!Numeric.IsZero(a))
        {
          centroid *= 1.0f / (3.0f * a);
        }
        else
        {
          // Signed area is close to zero. 
          // Avoid division by zero: Use average of points instead.
          centroid = Vector2.Zero;
          for (int i = 0; i < numberOfPoints; i++)
            centroid += points[i];

          centroid /= numberOfPoints;
        }
      }

      // Compute the angle of each point with respect to the centroid.
      // (Angles are in the range [-π, π].)
      float[] α = ResourcePools<float>.Arrays8.Obtain();
      for (int i = 0; i < numberOfPoints; i++)
      {
        Vector2 p = points[i];
        α[i] = (float)Math.Atan2(p.Y - centroid.Y, p.X - centroid.X);
      }

      // Keep the points that have angles closest to a[indexOfBest] + i * 2 * π / maxNumberOfPoints.
      float αBest = α[indexOfBest];
      List<int> bestPoints = DigitalRise.ResourcePools<int>.Lists.Obtain(); // OPTIMIZE: Use stackalloc if unsafe code is allowed.
      bool[] available = ResourcePools<bool>.Arrays8.Obtain();
      for (int i = 0; i < numberOfPoints; i++)
        available[i] = true;

      available[indexOfBest] = false;
      bestPoints.Add(indexOfBest);

      for (int i = 1; i < maxNumberOfContacts; i++)
      {
        float αDesired = αBest + i * ConstantsF.TwoPi / maxNumberOfContacts;

        // Ensure that angle is in the range [-π, π].
        if (αDesired > ConstantsF.Pi)
          αDesired -= ConstantsF.TwoPi;

        float αMinDiff = float.MaxValue;  // Current smallest difference to the desired angle.
        int iMin = -1;                    // Index of the point with currently smallest difference.
        for (int j = 0; j < numberOfPoints; j++)
        {
          if (available[j])
          {
            float αDiff = Math.Abs(α[j] - αDesired);  // Difference to desired angle.

            // Map differences in the range (π, 2π] back to the range [0, π].
            if (αDiff > ConstantsF.Pi)
              αDiff = ConstantsF.TwoPi - αDiff;

            if (αDiff < αMinDiff)
            {
              αMinDiff = αDiff;
              iMin = j;
            }
          }
        }

        if (iMin >= 0)
        {
          available[iMin] = false;
          bestPoints.Add(iMin);
        }
        else
        {
          // No point selected. Probably numerical issues.
        }
      }

      ResourcePools<bool>.Arrays8.Recycle(available);
      ResourcePools<float>.Arrays8.Recycle(α);

      Debug.Assert(bestPoints.Count <= maxNumberOfContacts);
      return bestPoints;
    }
  }
}
