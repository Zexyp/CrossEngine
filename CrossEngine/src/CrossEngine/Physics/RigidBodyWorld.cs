using System;
using BulletSharp;

using System.Collections.Generic;

using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Utils.Bullet;

namespace CrossEngine.Physics
{
    using BulletVector4 = BulletSharp.Math.Vector4;
    using BulletVector3 = BulletSharp.Math.Vector3;
    using Vector3 = System.Numerics.Vector3;
    using Vector4 = System.Numerics.Vector4;

    class RigidBodyWorld : IDisposable, IObjectRenderData
    {
        public Vector3 Gravity
        {
            get => World.Gravity.ToNumerics();
            set
            {
                World.Gravity = value.ToBullet();
                for (int i = 0; i < World.CollisionObjectArray.Count; i++)
                {
                    var collisionObject = World.CollisionObjectArray[i];
                    if (!collisionObject.IsStaticObject)
                        collisionObject.Activate();
                }
            }
        }

        CollisionConfiguration _collisionConfiguration;
        CollisionDispatcher _dispatcher;
        internal BroadphaseInterface Broadphase { get; private set; }
        internal DiscreteDynamicsWorld World { get; private set; }

        public RigidBodyWorld()
        {
            _collisionConfiguration = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collisionConfiguration);
            Broadphase = new DbvtBroadphase();
            World = new DiscreteDynamicsWorld(_dispatcher, Broadphase, null, _collisionConfiguration);
            World.LatencyMotionStateInterpolation = false;

            World.DebugDrawer = new RigidBodyWorldDebugDraw();
        }

        #region Cleanup
        public void Dispose()
        {
            CleanupConstraints(World);
            CleanupBodiesAndShapes(World);

            World.Dispose();
            Broadphase.Dispose();
            _dispatcher.Dispose();
            _collisionConfiguration.Dispose();
        }

        private static void CleanupConstraints(DynamicsWorld world)
        {
            var nonWorldObjects = new HashSet<CollisionObject>();

            for (int i = world.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = world.GetConstraint(i);
                world.RemoveConstraint(constraint);
                if (constraint.RigidBodyA.BroadphaseHandle == null)
                {
                    nonWorldObjects.Add(constraint.RigidBodyA);
                }
                if (constraint.RigidBodyB.BroadphaseHandle == null)
                {
                    nonWorldObjects.Add(constraint.RigidBodyB);
                }
                constraint.Dispose();
            }

            foreach (var obj in nonWorldObjects)
            {
                obj.Dispose();
            }
        }

        private static void CleanupBodiesAndShapes(DynamicsWorld world)
        {
            var shapes = new HashSet<CollisionShape>();

            for (int i = world.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = world.CollisionObjectArray[i];

                var rigidBody = obj as RigidBody;
                if (rigidBody != null && rigidBody.MotionState != null)
                {
                    rigidBody.MotionState.Dispose();
                }
                world.RemoveCollisionObject(obj);
                GetShapeWithChildShapes(obj.CollisionShape, shapes);

                obj.Dispose();
            }

            foreach (var shape in shapes)
            {
                shape.Dispose();
            }
        }

        private static void GetShapeWithChildShapes(CollisionShape shape, HashSet<CollisionShape> shapes)
        {
            shapes.Add(shape);

            var convex2DShape = shape as Convex2DShape;
            if (convex2DShape != null)
            {
                GetShapeWithChildShapes(convex2DShape.ChildShape, shapes);
                return;
            }

            var compoundShape = shape as CompoundShape;
            if (compoundShape != null)
            {
                foreach (var childShape in compoundShape.ChildList)
                {
                    GetShapeWithChildShapes(childShape.ChildShape, shapes);
                }
                return;
            }

            var scaledTriangleMeshShape = shape as ScaledBvhTriangleMeshShape;
            if (scaledTriangleMeshShape != null)
            {
                GetShapeWithChildShapes(scaledTriangleMeshShape.ChildShape, shapes);
                return;
            }

            var uniformScalingShape = shape as UniformScalingShape;
            if (uniformScalingShape != null)
            {
                GetShapeWithChildShapes(uniformScalingShape.ChildShape, shapes);
                return;
            }
        }
        #endregion

        public void AddRigidBody(RigidBody body)
        {
            World.AddRigidBody(body);
        }

        public void RemoveRigidBody(RigidBody body)
        {
            World.RemoveRigidBody(body);
        }

        public void CleanFromProxyPairs(RigidBody body)
        {
            Broadphase.OverlappingPairCache.CleanProxyFromPairs(body.BroadphaseProxy, _dispatcher);
        }

        public void Simulate(float timeStep, int maxSubSteps, float fixedTimeStep)
        {
            World.StepSimulation(timeStep, maxSubSteps, fixedTimeStep);
        }
    }

    class RigidBodyWorldDebugDraw : DebugDraw
    {
        public override DebugDrawModes DebugMode { get; set; }
        Logger Logger = new Logger("Bullet");

        public override void Draw3DText(ref BulletVector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public override void DrawLine(ref BulletVector3 from, ref BulletVector3 to, ref BulletVector3 color)
        {
            LineRenderer.DrawLine(from.ToNumerics(), to.ToNumerics(), new Vector4(color.ToNumerics(), 1.0f));
        }

        public override void ReportErrorWarning(string warningString)
        {
            Logger.Warn(warningString);
        }
    }
}
