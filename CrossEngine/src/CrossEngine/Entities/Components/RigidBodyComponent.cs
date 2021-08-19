using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Physics;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils;

namespace CrossEngine.Entities.Components
{
    public class RigidBodyComponent : Component
    {
        RigidBody rigidBody;
        private ColliderComponent _collider;

        private float _mass = 1.0f;
        private bool _static = false;

        [EditorSingleValue]
        public float Mass
        {
            get => _mass;
            set
            {
                _mass = value;

                if (rigidBody != null)
                {
                    rigidBody.SetMassProps(_mass, rigidBody.LocalInertia);
                }
            }
        }

        [EditorVector3Value]
        public Vector3 Velocity
        {
            get => rigidBody.LinearVelocity;
            set => rigidBody.LinearVelocity = value;
        }

        [EditorBooleanValue]
        //public bool Static
        //{
        //    get => _static;
        //    set
        //    {
        //        _static = value;
        //
        //        if (rigidBody != null)
        //        {
        //            if (_static)
        //                rigidBody.CollisionFlags |= CollisionFlags.StaticObject;
        //            else
        //                rigidBody.CollisionFlags &= ~CollisionFlags.StaticObject;
        //        }
        //    }
        //}

        public ColliderComponent Collider
        {
            get => _collider;
            set
            {
                if (_collider != null) _collider.OnShapeChanged -= Collider_OnShapeChanged;
                _collider = value;
                if (_collider != null) _collider.OnShapeChanged += Collider_OnShapeChanged;
            }
        }

        public RigidBodyComponent()
        {
            
        }

        public override void OnAttach()
        {
            using (RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(_mass, new DefaultMotionState(Entity.Transform.WorldTransformMatrix), null))
            {
                rigidBody = new RigidBody(info);
            }

            if (Entity.TryGetComponent(out ColliderComponent cc, true))
            {
                Collider = cc;
                rigidBody.CollisionShape = cc.Shape;
                rigidBody.UpdateInertiaTensor();
            }
            rigidBody.CollisionFlags = CollisionFlags.None;
        }

        public override void OnDetach()
        {
            rigidBody.Dispose();
            rigidBody = null;
        }

        public override void OnEnable()
        {
            Entity.Scene.RigidBodyWorld.AddRigidBody(rigidBody);
        }

        public override void OnDisable()
        {
            Entity.Scene.RigidBodyWorld.RemoveRigidBody(rigidBody);
        }

        private void Collider_OnShapeChanged(ColliderComponent collider)
        {
            if (rigidBody != null)
            {
                rigidBody.CollisionShape = collider.Shape;
                rigidBody.UpdateInertiaTensor();
            }
        }

        public override void OnEvent(Event e)
        {
            if (e is RigidBodyWorldUpdateEvent)
            {
                Entity.Transform.SetTranslationRotation(rigidBody.WorldTransform);
            }
        }
    }
}
