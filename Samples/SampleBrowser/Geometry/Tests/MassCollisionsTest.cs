﻿//#define PROFILE

using System;
using System.Diagnostics;
using System.Linq;
using DigitalRise.Diagnostics;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Collisions;
using DigitalRise.Geometry.Partitioning;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Graphics;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRise.Mathematics.MathHelper;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    "",
    "Hold <Space> to enable multithreading.",
    1000)]
  public class MassCollisionsTest : BasicSample
  {
    // Create a custom GeometricObject class which stores a LinearVelocity.
    private class MovingGeometricObject : GeometricObject
    {
      public Vector3 LinearVelocity { get; set; }
      public Vector3 AngularVelocity { get; set; }
    }

    private float _defaultPlaneShapeMeshSize;

    // A few constants.
    private const bool ClosestPointQueriesEnabled = false;      // true if closest point queries are performed for all object pairs.
    private const int ObjectsPerType = 10;
    private const float BoxSize = 5f;                           // The size of the box where objects can move.
    private const float ObjectSize = 0.5f;                      // The preferred size of a single object.
    private const float MaxLinearVelocity = 5;                  // The maximal linear speed of an object.
    private const float MaxAngularVelocity = ConstantsF.TwoPi;  // The maximal angular speed of an object.

    // The collision domain that manages collision objects.
    private readonly CollisionDomain _domain;


    public MassCollisionsTest(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3(0, 0, 20), 0, 0);

      // We use one collision domain that manages all objects.
      _domain = new CollisionDomain { EnableMultithreading = false };
      //_domain.BroadPhase = new AabbTree<CollisionObject>();
      //_domain.BroadPhase = new DynamicAabbTree<CollisionObject> { EnableMotionPrediction = true, OptimizationPerFrame = 0.01f };
      //_domain.BroadPhase = new DualPartition<CollisionObject>();
      //_domain.BroadPhase = new DualPartition<CollisionObject>(new AdaptiveAabbTree<CollisionObject>(), new AdaptiveAabbTree<CollisionObject>());
      //_domain.BroadPhase = new DualPartition<CollisionObject>(new AdaptiveAabbTree<CollisionObject>(), new DynamicAabbTree<CollisionObject> { EnableMotionPrediction = true, OptimizationPerFrame = 0.01f });
      //_domain.BroadPhase = new DebugSpatialPartition<CollisionObject>();
      //_domain.BroadPhase = new AdaptiveAabbTree<CollisionObject>();

      // Create the 6 box planes.
      CreateBoundaryPlanes();

      // Create a lot of random objects.
      CreateRandomObjects();
    }


    // Creates 6 planes that form a box. Objects can move inside the box.
    private void CreateBoundaryPlanes()
    {
      // Planes (class DigitalRise.Geometry.Shapes.PlaneShape) are infinite planes that 
      // divide the world into two half-spaces. The space into which the plane normal
      // is pointing is "empty" and the other space is considered "solid".

      // Planes are infinite, but to draw a visual representation of the plane a mesh 
      // is generated with plane.GetMesh() in the DebugRenderer. Rectangle meshes
      // are automatically generated and the size of these rectangles is defined with
      // PlaneShape.MeshSize. 
      // Here we set MeshSize such that rectangles for each plane have the size of 
      // a box face - but don't forget that planes are infinite for the collision detection.
      _defaultPlaneShapeMeshSize = PlaneShape.MeshSize;
      PlaneShape.MeshSize = BoxSize * 2;

      // Left plane.
      var leftPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(Vector3.UnitX, 0),
        Pose = new Pose(new Vector3(-BoxSize, 0, 0)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(leftPlane));

      // Right plane.
      var rightPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(-Vector3.UnitX, 0),
        Pose = new Pose(new Vector3(BoxSize, 0, 0)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(rightPlane));

      // Top plane.
      var topPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(-Vector3.UnitY, 0),
        Pose = new Pose(new Vector3(0, BoxSize, 0)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(topPlane));

      // Bottom plane.
      //var bottomPlane = new MovingGeometricObject
      //{
      //  Shape = new PlaneShape(Vector3.UnitY, 0),
      //  Pose = new Pose(new Vector3(0, -BoxSize, 0)),
      //};
      //_domain.CollisionObjects.Add(new CollisionObject(bottomPlane));

      // Front plane.
      var frontPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(-Vector3.UnitZ, 0),
        Pose = new Pose(new Vector3(0, 0, BoxSize)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(frontPlane));

      // Back plane.
      var backPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(Vector3.UnitZ, 0),
        Pose = new Pose(new Vector3(0, 0, -BoxSize)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(backPlane));
    }


    // Creates a lot of random objects.
    private void CreateRandomObjects()
    {
      var random = new Random();

      var isFirstHeightField = true;

      int currentShape = 0;
      int numberOfObjects = 0;
      while (true)
      {
        numberOfObjects++;
        if (numberOfObjects > ObjectsPerType)
        {
          currentShape++;
          numberOfObjects = 0;
        }

        Shape shape;
        switch (currentShape)
        {
          case 0:
            // Box
            shape = new BoxShape(ObjectSize, ObjectSize * 2, ObjectSize * 3);
            break;
          case 1:
            // Capsule
            shape = new CapsuleShape(0.3f * ObjectSize, 2 * ObjectSize);
            break;
          case 2:
            // Cone
            shape = new ConeShape(1 * ObjectSize, 2 * ObjectSize);
            break;
          case 3:
            // Cylinder
            shape = new CylinderShape(0.4f * ObjectSize, 2 * ObjectSize);
            break;
          case 4:
            // Sphere
            shape = new SphereShape(ObjectSize);
            break;
          case 5:
            // Convex hull of several points.
            ConvexHullOfPoints hull = new ConvexHullOfPoints();
            hull.Points.Add(new Vector3(-1 * ObjectSize, -2 * ObjectSize, -1 * ObjectSize));
            hull.Points.Add(new Vector3(2 * ObjectSize, -1 * ObjectSize, -0.5f * ObjectSize));
            hull.Points.Add(new Vector3(1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize));
            hull.Points.Add(new Vector3(-1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize));
            hull.Points.Add(new Vector3(-1 * ObjectSize, 0.7f * ObjectSize, -0.6f * ObjectSize));
            shape = hull;
            break;
          case 6:
            // A composite shape: two boxes that form a "T" shape.
            var composite = new CompositeShape();
            composite.Children.Add(
              new GeometricObject(
                new BoxShape(ObjectSize, 3 * ObjectSize, ObjectSize),
                new Pose(new Vector3(0, 0, 0))));
            composite.Children.Add(
              new GeometricObject(
                new BoxShape(2 * ObjectSize, ObjectSize, ObjectSize),
                new Pose(new Vector3(0, 2 * ObjectSize, 0))));
            shape = composite;
            break;
          case 7:
            shape = new CircleShape(ObjectSize);
            break;
          case 8:
            {
              var compBvh = new CompositeShape();
              compBvh.Children.Add(new GeometricObject(new BoxShape(0.5f, 1, 0.5f), new Pose(new Vector3(0, 0.5f, 0), Matrix33F.Identity)));
              compBvh.Children.Add(new GeometricObject(new BoxShape(0.8f, 0.5f, 0.5f), new Pose(new Vector3(0.5f, 0.7f, 0), Matrix33F.CreateRotationZ(-MathHelper.ToRadians(15)))));
              compBvh.Children.Add(new GeometricObject(new SphereShape(0.3f), new Pose(new Vector3(0, 1.15f, 0), Matrix33F.Identity)));
              compBvh.Children.Add(new GeometricObject(new CapsuleShape(0.2f, 1), new Pose(new Vector3(0.6f, 1.15f, 0), Matrix33F.CreateRotationX(0.3f))));
              compBvh.Partition = new AabbTree<int>();
              shape = compBvh;
              break;
            }
          case 9:
            CompositeShape comp = new CompositeShape();
            comp.Children.Add(new GeometricObject(new BoxShape(0.5f * ObjectSize, 1 * ObjectSize, 0.5f * ObjectSize), new Pose(new Vector3(0, 0.5f * ObjectSize, 0), Quaternion.Identity)));
            comp.Children.Add(new GeometricObject(new BoxShape(0.8f * ObjectSize, 0.5f * ObjectSize, 0.5f * ObjectSize), new Pose(new Vector3(0.3f * ObjectSize, 0.7f * ObjectSize, 0), MathHelper.CreateRotationZ(-MathHelper.ToRadians(45)))));
            comp.Children.Add(new GeometricObject(new SphereShape(0.3f * ObjectSize), new Pose(new Vector3(0, 1.15f * ObjectSize, 0), Quaternion.Identity)));
            shape = comp;
            break;
          case 10:
            shape = new ConvexHullOfPoints(new[]
            {
              new Vector3(-1 * ObjectSize, -2 * ObjectSize, -1 * ObjectSize),
              new Vector3(2 * ObjectSize, -1 * ObjectSize, -0.5f * ObjectSize),
              new Vector3(1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize),
              new Vector3(-1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize),
              new Vector3(-1 * ObjectSize, 0.7f * ObjectSize, -0.6f * ObjectSize)
            });
            break;
          case 11:
            ConvexHullOfShapes shapeHull = new ConvexHullOfShapes();
            shapeHull.Children.Add(new GeometricObject(new SphereShape(0.3f * ObjectSize), new Pose(new Vector3(0, 2 * ObjectSize, 0), Matrix33F.Identity)));
            shapeHull.Children.Add(new GeometricObject(new BoxShape(1 * ObjectSize, 2 * ObjectSize, 3 * ObjectSize), Pose.Identity));
            shape = shapeHull;
            break;
          case 12:
            shape = Shape.Empty;
            break;
          case 13:
            var numberOfSamplesX = 10;
            var numberOfSamplesZ = 10;
            var samples = new float[numberOfSamplesX * numberOfSamplesZ];
            for (int z = 0; z < numberOfSamplesZ; z++)
              for (int x = 0; x < numberOfSamplesX; x++)
                samples[z * numberOfSamplesX + x] = (float)(Math.Cos(z / 3f) * Math.Sin(x / 2f) * BoxSize / 6);
            HeightField heightField = new HeightField(0, 0, 2 * BoxSize, 2 * BoxSize, samples, numberOfSamplesX, numberOfSamplesZ);
            shape = heightField;
            break;
          //case 14:
          //shape = new LineShape(new Vector3(0.1f, 0.2f, 0.3f), new Vector3(0.1f, 0.2f, -0.3f).Normalized());
          //break;            
          case 15:
            shape = new LineSegmentShape(
              new Vector3(0.1f, 0.2f, 0.3f), new Vector3(0.1f, 0.2f, 0.3f) + 3 * ObjectSize * new Vector3(0.1f, 0.2f, -0.3f));
            break;
          case 16:
            shape = new MinkowskiDifferenceShape
            {
              ObjectA = new GeometricObject(new SphereShape(0.1f * ObjectSize)),
              ObjectB = new GeometricObject(new BoxShape(1 * ObjectSize, 2 * ObjectSize, 3 * ObjectSize))
            };
            break;
          case 17:
            shape = new MinkowskiSumShape
            {
              ObjectA = new GeometricObject(new SphereShape(0.1f * ObjectSize)),
              ObjectB = new GeometricObject(new BoxShape(1 * ObjectSize, 2 * ObjectSize, 3 * ObjectSize)),
            };
            break;
          case 18:
            shape = new OrthographicViewVolume(0, ObjectSize, 0, ObjectSize, ObjectSize / 2, ObjectSize * 2);
            break;
          case 19:
            shape = new PerspectiveViewVolume(MathHelper.ToRadians(60f), 16f / 10, ObjectSize / 2, ObjectSize * 3);
            break;
          case 20:
            shape = new PointShape(0.1f, 0.3f, 0.2f);
            break;
          case 21:
            shape = new RayShape(new Vector3(0.2f, 0, -0.12f), new Vector3(1, 2, 3).Normalized(), ObjectSize * 2);
            break;
          case 22:
            shape = new RayShape(new Vector3(0.2f, 0, -0.12f), new Vector3(1, 2, 3).Normalized(), ObjectSize * 2)
            {
              StopsAtFirstHit = true
            };
            break;
          case 23:
            shape = new RectangleShape(ObjectSize, ObjectSize * 2);
            break;
          case 24:
            shape = new TransformedShape(
              new GeometricObject(
                new BoxShape(1 * ObjectSize, 2 * ObjectSize, 3 * ObjectSize),
                new Pose(new Vector3(0.1f, 1, -0.2f))));
            break;
          case 25:
            shape = new TriangleShape(
              new Vector3(ObjectSize, 0, 0), new Vector3(0, ObjectSize, 0), new Vector3(ObjectSize, ObjectSize, ObjectSize));
            break;
          //case 26:
          //  {
          //    // Create a composite object from which we get the mesh.
          //    CompositeShape compBvh = new CompositeShape();
          //    compBvh.Children.Add(new GeometricObject(new BoxShape(0.5f, 1, 0.5f), new Pose(new Vector3(0, 0.5f, 0), Matrix33F.Identity)));
          //    compBvh.Children.Add(
          //      new GeometricObject(
          //        new BoxShape(0.8f, 0.5f, 0.5f),
          //        new Pose(new Vector3(0.5f, 0.7f, 0), Matrix33F.CreateRotationZ(-(float)MathHelper.ToRadians(15)))));
          //    compBvh.Children.Add(new GeometricObject(new SphereShape(0.3f), new Pose(new Vector3(0, 1.15f, 0), Matrix33F.Identity)));
          //    compBvh.Children.Add(
          //      new GeometricObject(new CapsuleShape(0.2f, 1), new Pose(new Vector3(0.6f, 1.15f, 0), Matrix33F.CreateRotationX(0.3f))));

          //    TriangleMeshShape meshBvhShape = new TriangleMeshShape { Mesh = compBvh.GetMesh(0.01f, 3) };
          //    meshBvhShape.Partition = new AabbTree<int>();
          //    shape = meshBvhShape;
          //    break;
          //  }
          //case 27:
          //  {
          //    // Create a composite object from which we get the mesh.
          //    CompositeShape compBvh = new CompositeShape();
          //    compBvh.Children.Add(new GeometricObject(new BoxShape(0.5f, 1, 0.5f), new Pose(new Vector3(0, 0.5f, 0), Quaternion.Identity)));
          //    compBvh.Children.Add(
          //      new GeometricObject(
          //        new BoxShape(0.8f, 0.5f, 0.5f),
          //        new Pose(new Vector3(0.5f, 0.7f, 0), MathHelper.CreateRotationZ(-(float)MathHelper.ToRadians(15)))));
          //    compBvh.Children.Add(new GeometricObject(new SphereShape(0.3f), new Pose(new Vector3(0, 1.15f, 0), Quaternion.Identity)));
          //    compBvh.Children.Add(
          //      new GeometricObject(new CapsuleShape(0.2f, 1), new Pose(new Vector3(0.6f, 1.15f, 0), MathHelper.CreateRotationX(0.3f))));

          //    TriangleMeshShape meshBvhShape = new TriangleMeshShape { Mesh = compBvh.GetMesh(0.01f, 3) };
          //    meshBvhShape.Partition = new AabbTree<int>();
          //    shape = meshBvhShape;
          //    break;
          //  }
          case 28:
            shape = new ConvexPolyhedron(new[]
            {
              new Vector3(-1 * ObjectSize, -2 * ObjectSize, -1 * ObjectSize),
              new Vector3(2 * ObjectSize, -1 * ObjectSize, -0.5f * ObjectSize),
              new Vector3(1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize),
              new Vector3(-1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize),
              new Vector3(-1 * ObjectSize, 0.7f * ObjectSize, -0.6f * ObjectSize)
            });
            break;
          case 29:
            return;
          default:
            currentShape++;
            continue;
        }

        // Create an object with the random shape, pose, color and velocity.
        Pose randomPose = new Pose(
          random.NextVector3(-BoxSize + ObjectSize * 2, BoxSize - ObjectSize * 2),
          random.NextQuaternion());
        var newObject = new MovingGeometricObject
        {
          Pose = randomPose,
          Shape = shape,
          LinearVelocity = random.NextQuaternion().Rotate(new Vector3(MaxLinearVelocity, 0, 0)),
          AngularVelocity = random.NextQuaternion().Rotate(Vector3.Forward)
                            * RandomHelper.Random.NextFloat(0, MaxAngularVelocity),
        };

        if (RandomHelper.Random.NextBool())
          newObject.LinearVelocity = Vector3.Zero;
        if (RandomHelper.Random.NextBool())
          newObject.AngularVelocity = Vector3.Zero;

        if (shape is LineShape || shape is HeightField)
        {
          // Do not move lines or the height field.
          newObject.LinearVelocity = Vector3.Zero;
          newObject.AngularVelocity = Vector3.Zero;
        }

        // Create only 1 heightField!
        if (shape is HeightField)
        {
          if (isFirstHeightField)
          {
            isFirstHeightField = true;
            newObject.Pose = new Pose(new Vector3(-BoxSize, -BoxSize, -BoxSize));
          }
          else
          {
            currentShape++;
            numberOfObjects = 0;
            continue;
          }
        }

        // Add collision object to collision domain.
        _domain.CollisionObjects.Add(new CollisionObject(newObject));

        //co.Type = CollisionObjectType.Trigger;
        //co.Name = "Object" + shape.GetType().Name + "_" + i;
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Reset default values.
        PlaneShape.MeshSize = _defaultPlaneShapeMeshSize;
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      _domain.EnableMultithreading = InputService.IsDown(Keys.Space);

#if PROFILE
      MessageBox.Show("Start");
      _deltaTime = 1 / 60f;
      for (int bla = 0; bla < 10000; bla++)
      {
#endif
      if (ClosestPointQueriesEnabled)
      {
        // Here we run closest point queries on all object pairs.
        // We compare the results with the contact queries.

        for (int i = 0; i < _domain.CollisionObjects.Count; i++)
        {
          for (int j = i + 1; j < _domain.CollisionObjects.Count; j++)
          {
            CollisionObject a = _domain.CollisionObjects[i];
            CollisionObject b = _domain.CollisionObjects[j];

            ContactSet closestPointQueryResult = _domain.CollisionDetection.GetClosestPoints(a, b);
            ContactSet contactSet = _domain.GetContacts(a, b);

            // Ignore height fields and rays.
            if (a.GeometricObject.Shape is HeightField || b.GeometricObject.Shape is HeightField)
              break;
            if (a.GeometricObject.Shape is RayShape || b.GeometricObject.Shape is RayShape)
              break;

            if (contactSet == null || !contactSet.HaveContact)
            {
              // No contact in contactSet
              if (closestPointQueryResult.HaveContact)
              {
                // Contact in closest point query. Results are inconsistent.
                if (closestPointQueryResult.Count > 0
                    && closestPointQueryResult[0].PenetrationDepth > 0.001f)
                  Debugger.Break();
              }
            }
            else if (!closestPointQueryResult.HaveContact)
            {
              // contact in contact query, but no contact in closest point query.
              // We allow a deviation within a small tolerance.
              if (closestPointQueryResult.Count > 0 && contactSet.Count > 0
                  && closestPointQueryResult[0].PenetrationDepth + contactSet[0].PenetrationDepth > 0.001f)
                Debugger.Break();
            }
          }
        }
      }

      // Reflect velocity if objects collide:
      // The collision domain contains a ContactSet for each pair of touching objects.
      foreach (var contactSet in _domain.ContactSets)
      {
        // Get the touching objects.
        var moA = (MovingGeometricObject)contactSet.ObjectA.GeometricObject;
        var moB = (MovingGeometricObject)contactSet.ObjectB.GeometricObject;

        // Reflect only at boundary objects.
        if (!(moA.Shape is PlaneShape) && !(moB.Shape is PlaneShape)
            && !(moA.Shape is HeightField) && !(moB.Shape is HeightField))
          continue;

        // Get normal vector. If objects are sensors, the contact set does not tell us
        // the right normal.
        Vector3 normal = Vector3.Zero;
        if (contactSet.Count > 0)
        {
          // Take normal from contact set.
          normal = contactSet[0].Normal;
        }
        else
        {
          // If we use Trigger CollisionObjects we do not have contacts. --> Reflect at
          // bounding planes.
          if (moA.Shape is PlaneShape)
            normal = ((PlaneShape)moA.Shape).Normal;
          else if (moB.Shape is PlaneShape)
            normal = -((PlaneShape)moB.Shape).Normal;
          else if (moA.Shape is HeightField)
            normal = Vector3.UnitY;
          else
            normal = -Vector3.UnitY;
        }
        //else if (moA.Shape is Plane || moB.Shape is Plane                       )
        //{
        //  // Use plane normal.
        //  IGeometricObject plane = moA.Shape is Plane ? moA : moB;
        //  normal = plane.Pose.ToWorldDirection(((Plane)plane.Shape).Normal);
        //  if (moB == plane)
        //    normal = -normal;
        //}
        //else if (moA.Shape is HeightField || moB.Shape is HeightField)
        //{
        //  // Use up-vector for height field contacts.
        //  normal = Vector3.UnitY;
        //  if (moB.Shape is HeightField)
        //    normal = -normal;
        //}
        //else
        //{
        //  // Use random normal.
        //  normal = RandomHelper.NextVector3(-1, 1).Normalized();
        //}

        // Check if the objects move towards or away from each other in the direction of the normal.
        if (normal != Vector3.Zero && Vector3.Dot(moB.LinearVelocity - moA.LinearVelocity, normal) <= 0)
        {
          // Objects move towards each other. --> Reflect their velocities.
          moA.LinearVelocity -= 2 * MathHelper.ProjectTo(moA.LinearVelocity, normal);
          moB.LinearVelocity -= 2 * MathHelper.ProjectTo(moB.LinearVelocity, normal);
          moA.AngularVelocity = -moA.AngularVelocity;
          moB.AngularVelocity = -moB.AngularVelocity;
        }
      }

      // Get the size of the current time step.
      float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Move objects.
      var objects = _domain.CollisionObjects.Select(co => co.GeometricObject).OfType<MovingGeometricObject>();
      foreach (var obj in objects)
      {
        // Update position.
        Vector3 position = obj.Pose.Position + obj.LinearVelocity * timeStep;

        // Update rotation.
        Vector3 rotationAxis = obj.AngularVelocity;
        float angularSpeed = obj.AngularVelocity.Length();
        Matrix33F rotation = (Numeric.IsZero(angularSpeed))
          ? Matrix33F.Identity
          : Matrix33F.CreateRotation(rotationAxis, angularSpeed * timeStep);
        var orientation = rotation * obj.Pose.Orientation;

        // Incrementally updating the rotation matrix will eventually create a 
        // matrix which is not a rotation matrix anymore because of numerical 
        // problems. Re-othogonalization make sure that the matrix represents a
        // rotation.
        orientation.Orthogonalize();

        obj.Pose = new Pose(position, orientation);
      }




      // Update collision domain. This computes new contact information.      
      _domain.Update(timeStep);

#if PROFILE
      MessageBox.Show("Finished");
      Exit();
#endif

      // Record some statistics.
      int numberOfObjects = _domain.CollisionObjects.Count;
      Profiler.SetFormat("NumObjects", 1, "The total number of objects.");
      Profiler.AddValue("NumObjects", numberOfObjects);
      // If there are n objects, we can have max. n * (n - 1) / 2 collisions.
      Profiler.SetFormat("NumObjectPairs", 1, "The number of objects pairs, which have to be checked.");
      Profiler.AddValue("NumObjectPairs", numberOfObjects * (numberOfObjects - 1f) / 2f);
      // The first part of the collision detection is the "broad-phase" which
      // filters out objects that cannot collide (e.g. using a fast bounding box test).
      Profiler.SetFormat("BroadPhasePairs", 1, "The number of overlaps reported by the broad phase.");
      Profiler.AddValue("BroadPhasePairs", _domain.NumberOfBroadPhaseOverlaps);
      // Finally, the collision detection computes the exact contact information and creates
      // a ContactSet with the Contacts for each pair of colliding objects.
      Profiler.SetFormat("ContactSetCount", 1, "The number of actual collisions.");
      Profiler.AddValue("ContactSetCount", _domain.ContactSets.Count);

      // Draw objects using the DebugRenderer of the graphics screen.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var collisionObject in _domain.CollisionObjects)
        debugRenderer.DrawObject(collisionObject.GeometricObject, GraphicsHelper.GetUniqueColor(collisionObject), false, false);
    }
  }
}
