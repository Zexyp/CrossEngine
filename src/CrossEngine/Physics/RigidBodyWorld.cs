using System;
using BulletSharp;

using System.Collections.Generic;
using System.Threading;
using System.Numerics;

namespace CrossEngine.Physics
{
    public static class RigidBodyWorld
    {
        static DynamicsWorld dynamicsWorld;
        static Dispatcher dispatcher;
        static CollisionConfiguration collisionConfig;
        static BroadphaseInterface broadphase;
        static ConstraintSolver solver;

        static List<RigidBody> bodies = new List<RigidBody> { };
        public static float step = 1.0f / 60;

        public static event EventHandler<EventArgs> OnRigidBodyWorldUpdate;

        public static void Init()
        {
            collisionConfig = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfig);
            broadphase = new DbvtBroadphase();
            solver = new SequentialImpulseConstraintSolver();
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);

            dynamicsWorld.Gravity = new Vector3(0, -9.81f, 0);
        }

        public static void Dispose()
        {
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfig.Dispose();
            broadphase.Dispose();
            solver.Dispose();
        }

        public static void Update()
        {
            dynamicsWorld.StepSimulation(step);
            OnRigidBodyWorldUpdate?.Invoke(null, EventArgs.Empty);
        }

        public static void RegisterRigidBody(RigidBody body)
        {
            dynamicsWorld.AddRigidBody(body);
            bodies.Add(body);
        }
    }
}
