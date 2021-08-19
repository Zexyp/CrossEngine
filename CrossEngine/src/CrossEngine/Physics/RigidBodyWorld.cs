using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Events;

namespace CrossEngine.Physics
{
    public class RigidBodyWorldUpdateEvent : Event
    {

    }

    public class RigidBodyWorld
    {
        CollisionConfiguration collisionConfiguration;
        CollisionDispatcher collisionDispatcher;
        DynamicsWorld dynamicsWorld;
        DbvtBroadphase dbvtBroadphase;

        public RigidBodyWorld()
        {
            collisionConfiguration = new DefaultCollisionConfiguration();
            collisionDispatcher = new CollisionDispatcher(collisionConfiguration);
            dbvtBroadphase = new DbvtBroadphase();
            dynamicsWorld = new DiscreteDynamicsWorld(collisionDispatcher, dbvtBroadphase, null, collisionConfiguration);
        }

        public void Update(float step)
        {
            // TODO: look into StepSimulation(float timeStep, int maxSubSteps, float fixedTimeStep)
            dynamicsWorld.StepSimulation(step/*, 10*/);
        }

        // TODO: fix!!...
        public void AddRigidBody(RigidBody body)
        {
            dynamicsWorld.AddRigidBody(body);
        }

        public void RemoveRigidBody(RigidBody body)
        {
            dynamicsWorld.RemoveRigidBody(body);
        }
    }
}
