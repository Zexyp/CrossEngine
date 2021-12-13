using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Entities.Components;
using CrossEngine.Logging;
using CrossEngine.Utils;

namespace CrossEngine.Physics
{
    using BulletVector3 = BulletSharp.Math.Vector3;

    public class RigidBodyWorldUpdateEvent : Event
    {

    }

    public class RigidBodyWorld
    {
        private CollisionConfiguration _collisionConfiguration;
        private CollisionDispatcher _dispatcher;
        private BroadphaseInterface _overlappingPairCache;
        private DiscreteDynamicsWorld _world;

        internal CollisionDispatcher Dispatcher { get => _dispatcher; }
        internal BroadphaseInterface Broadphase { get => _overlappingPairCache; }

        public int MaxSubSteps = 10;
        public float FixedTimeStep = 1.0f / 60;

        //private readonly VoronoiSimplexSolver _simplexSolver;
        //private readonly MinkowskiPenetrationDepthSolver _penetrationDepthSolver;
        //
        //private readonly Convex2DConvex2DAlgorithm.CreateFunc _convexAlgo2D;
        //private readonly Box2DBox2DCollisionAlgorithm.CreateFunc _boxAlgo2D;

        public Vector3 Gravity
        {
            get => _world.Gravity.ToNumerics();
            set
            {
                var bulletval = value.ToBullet();
                if (_world.Gravity == bulletval) return;
                _world.Gravity = bulletval;
                CollisionObject co;
                for (int i = 0; i < _world.CollisionObjectArray.Count; i++)
                {
                    co = _world.CollisionObjectArray[i];
                    if (!co.IsActive) co.Activate();
                }
            }
        }

        internal object GetWorld() => _world;

        public RigidBodyWorld()
        {
            _collisionConfiguration = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collisionConfiguration);
            _overlappingPairCache = new DbvtBroadphase();
            _world = new DiscreteDynamicsWorld(_dispatcher, _overlappingPairCache, null, _collisionConfiguration);

            //_world.DebugDrawer = new RigidBodyWorldDebugDraw();
            
            // 2D rn!
            //
            //collisionConfiguration = new DefaultCollisionConfiguration();
            //
            //// Use the default collision dispatcher. For parallel processing you can use a diffent dispatcher.
            //Dispatcher = new CollisionDispatcher(collisionConfiguration);
            //
            ////_simplexSolver = new VoronoiSimplexSolver();
            ////_penetrationDepthSolver = new MinkowskiPenetrationDepthSolver();
            ////
            ////_convexAlgo2D = new Convex2DConvex2DAlgorithm.CreateFunc(_simplexSolver, _penetrationDepthSolver);
            ////_boxAlgo2D = new Box2DBox2DCollisionAlgorithm.CreateFunc();
            ////
            ////Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Convex2DShape, BroadphaseNativeType.Convex2DShape, _convexAlgo2D);
            ////Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Box2DShape, BroadphaseNativeType.Convex2DShape, _convexAlgo2D);
            ////Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Convex2DShape, BroadphaseNativeType.Box2DShape, _convexAlgo2D);
            ////Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Box2DShape, BroadphaseNativeType.Box2DShape, _boxAlgo2D);
            //
            //Broadphase = new DbvtBroadphase();
            //
            //dynamicsWorld = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, collisionConfiguration);
            //
        }

        #region Cleanup
        public void Cleanup()
        {
            CleanupConstraints(_world);
            CleanupBodiesAndShapes(_world);

            _world.Dispose();
            _overlappingPairCache.Dispose();
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

        public void Update(float step)
        {
            // TODO: look into StepSimulation(float timeStep, int maxSubSteps, float fixedTimeStep)
            _world.StepSimulation(step/*, MaxSubSteps, FixedTimeStep*/);
        }

        internal void AddRigidBody(RigidBody body)
        {
            _world.AddRigidBody(body);
        }

        internal void RemoveRigidBody(RigidBody body)
        {
            _world.RemoveRigidBody(body);
        }
    }

    class RigidBodyWorldDebugDraw : DebugDraw
    {
        public override DebugDrawModes DebugMode { get; set; }

        public override void Draw3DText(ref BulletSharp.Math.Vector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public override void DrawLine(ref BulletSharp.Math.Vector3 from, ref BulletSharp.Math.Vector3 to, ref BulletSharp.Math.Vector3 color)
        {
            throw new NotImplementedException();
        }

        public override void ReportErrorWarning(string warningString)
        {
            Log.Core.Warn($"[Bullet] {warningString}");
        }
    }
}
