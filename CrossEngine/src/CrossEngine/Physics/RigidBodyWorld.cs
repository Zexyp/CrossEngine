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

namespace CrossEngine.Physics
{
    public class RigidBodyWorldUpdateEvent : Event
    {

    }

    public class RigidBodyWorld
    {
        CollisionConfiguration collisionConfiguration;
        CollisionDispatcher Dispatcher;
        BroadphaseInterface Broadphase;
        public DiscreteDynamicsWorld dynamicsWorld; // TODO: fix access

        private readonly VoronoiSimplexSolver _simplexSolver;
        private readonly MinkowskiPenetrationDepthSolver _penetrationDepthSolver;

        private readonly Convex2DConvex2DAlgorithm.CreateFunc _convexAlgo2D;
        private readonly Box2DBox2DCollisionAlgorithm.CreateFunc _boxAlgo2D;

        List<RigidBody> rigidBodies = new List<RigidBody>();

        public Vector3 Gravity
        {
            get => dynamicsWorld.Gravity;
            set
            {
                if (dynamicsWorld.Gravity == value) return;
                dynamicsWorld.Gravity = value;
                for (int i = 0; i < rigidBodies.Count; i++)
                {
                    if (!rigidBodies[i].IsActive) rigidBodies[i].Activate();
                }
            }
        }

        public RigidBodyWorld()
        {
            // 2D rn
            collisionConfiguration = new DefaultCollisionConfiguration();

            // Use the default collision dispatcher. For parallel processing you can use a diffent dispatcher.
            Dispatcher = new CollisionDispatcher(collisionConfiguration);

            _simplexSolver = new VoronoiSimplexSolver();
            _penetrationDepthSolver = new MinkowskiPenetrationDepthSolver();

            _convexAlgo2D = new Convex2DConvex2DAlgorithm.CreateFunc(_simplexSolver, _penetrationDepthSolver);
            _boxAlgo2D = new Box2DBox2DCollisionAlgorithm.CreateFunc();

            Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Convex2DShape, BroadphaseNativeType.Convex2DShape, _convexAlgo2D);
            Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Box2DShape, BroadphaseNativeType.Convex2DShape, _convexAlgo2D);
            Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Convex2DShape, BroadphaseNativeType.Box2DShape, _convexAlgo2D);
            Dispatcher.RegisterCollisionCreateFunc(BroadphaseNativeType.Box2DShape, BroadphaseNativeType.Box2DShape, _boxAlgo2D);

            Broadphase = new DbvtBroadphase();

            dynamicsWorld = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, collisionConfiguration);

            dynamicsWorld.DebugDrawer = new RigidBodyWorldDebugDraw();
        }

        public void Update(float step)
        {
            // TODO: look into StepSimulation(float timeStep, int maxSubSteps, float fixedTimeStep)
            dynamicsWorld.StepSimulation(step/*, 10*/);
        }

        internal void AddRigidBody(RigidBody body)
        {
            dynamicsWorld.AddRigidBody(body);
            rigidBodies.Add(body);
        }

        internal void RemoveRigidBody(RigidBody body)
        {
            dynamicsWorld.RemoveRigidBody(body);
            rigidBodies.Remove(body);
        }

        internal void CleanProxyFromPairs(RigidBody body)
        {
            Broadphase.OverlappingPairCache.CleanProxyFromPairs(body.BroadphaseProxy, Dispatcher);
        }
    }

    class RigidBodyWorldDebugDraw : IDebugDraw
    {
        DebugDrawModes _debugMode;
        public DebugDrawModes DebugMode { get => _debugMode; set => _debugMode = value; }

        public void Draw3dText(ref Vector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public void DrawAabb(ref Vector3 from, ref Vector3 to, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Color color, bool drawSect)
        {
            throw new NotImplementedException();
        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Color color, bool drawSect, float stepDegrees)
        {
            throw new NotImplementedException();
        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ref Matrix4x4 trans, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix4x4 transform, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawCone(float radius, float height, int upAxis, ref Matrix4x4 transform, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix4x4 transform, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, Color fromColor, Color toColor)
        {
            throw new NotImplementedException();
        }

        public void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix4x4 transform, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawSphere(float radius, ref Matrix4x4 transform, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawSphere(ref Vector3 p, float radius, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color, float stepDegrees)
        {
            throw new NotImplementedException();
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color, float stepDegrees, bool drawCenter)
        {
            throw new NotImplementedException();
        }

        public void DrawTransform(ref Matrix4x4 transform, float orthoLen)
        {
            throw new NotImplementedException();
        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, Color color, float alpha)
        {
            throw new NotImplementedException();
        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed3, ref Vector3 __unnamed4, ref Vector3 __unnamed5, Color color, float alpha)
        {
            throw new NotImplementedException();
        }

        public void FlushLines()
        {
            throw new NotImplementedException();
        }

        public void ReportErrorWarning(string warningString)
        {
            Log.Core.Warn($"[Bullet] {warningString}");
        }
    }
}
