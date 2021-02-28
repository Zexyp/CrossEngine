using System;
using BulletSharp;

using System.Numerics;

using CrossEngine.Physics;

namespace CrossEngine.ComponentSystem.Components
{
    public class RigidBodyComponent : Component
    {
        RigidBody rigidBody;
        CollisionShape collider;
        MotionState motion;
        RigidBodyConstructionInfo info;

        float mass = 0.0f;

        public RigidBodyComponent(CollisionShape collider, float mass)
        {
            this.collider = collider;
            this.mass = mass;
            RigidBodyWorld.OnRigidBodyWorldUpdate += OnRigidBodyWorldUpdate;
        }

        public override void OnAwake()
        {
            Vector3 inertia = new Vector3(0, 0, 0); // can be used to lock axis
            if (mass != 0)
                collider.CalculateLocalInertia(mass, out inertia);
            motion = new DefaultMotionState(entity.transform.TransformMatrix);
            info = new RigidBodyConstructionInfo(this.mass, motion, this.collider, inertia);
            rigidBody = new RigidBody(info);
            RigidBodyWorld.RegisterRigidBody(rigidBody);
        }

        public override void OnDie()
        {
            rigidBody.Dispose();
            collider.Dispose();
            motion.Dispose();
            info.Dispose();
        }

        private void OnRigidBodyWorldUpdate(object sender, EventArgs e)
        {
            if (motion != null)
                entity.transform.SetFromMatrix(motion.WorldTransform);
        }
    }
}
