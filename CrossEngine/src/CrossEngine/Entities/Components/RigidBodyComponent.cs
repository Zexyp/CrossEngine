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
    [RequireComponent(typeof(TransformComponent))]
    [RequireComponent(typeof(ColliderComponent))]
    public class RigidBodyComponent : Component
    {
        RigidBody rigidBody;

        private TransformComponent _transform;
        private ColliderComponent _collider;
        TransformComponent Transform
        {
            get => _transform;
            set
            {
                if (_transform != null) _transform.OnTransformChanged -= OnTransformChanged;
                _transform = value;
                if (_transform != null) _transform.OnTransformChanged += OnTransformChanged;
            }
        }
        ColliderComponent Collider
        {
            get => _collider;
            set
            {
                if (_collider != null) _collider.OnShapeChanged -= OnColliderShapeChanged;
                _collider = value;
                if (_collider != null)
                {
                    _collider.OnShapeChanged += OnColliderShapeChanged;
                    if (rigidBody != null)
                    {

                        
                    }
                }
            }
        }


        #region RB Fields
        private float _mass = 1.0f;

        private Vector3 _linearFactor = Vector3.One;
        private Vector3 _angularFactor = Vector3.One;
        private Vector3 _velocity = Vector3.Zero;
        #region Properties
        [EditorSingleValue]
        public float Mass
        {
            get => _mass;
            set
            {
                if (_mass == value) return;
                _mass = value;
                if (rigidBody != null)
                {
                    rigidBody.SetMassProps(_mass, rigidBody.LocalInertia);
                    Console.WriteLine((int)rigidBody.CollisionFlags);
                    ActivateBody();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 Velocity
        {
            get => _velocity = (rigidBody != null) ? rigidBody.LinearVelocity : _velocity;
            set
            {
                if (_velocity == value) return;
                _velocity = value;
                if (rigidBody != null)
                {
                    rigidBody.LinearVelocity = _velocity;
                    ActivateBody();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 LinearFactor
        {
            get => _linearFactor = (rigidBody != null) ? rigidBody.LinearFactor : _linearFactor;
            set
            {
                if (_linearFactor == value) return;
                _linearFactor = value;
                if (rigidBody != null)
                {
                    rigidBody.LinearFactor = _linearFactor;
                    ActivateBody();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 AngularFactor
        {
            get => _angularFactor = (rigidBody != null) ? rigidBody.AngularFactor : _angularFactor;
            set
            {
                if (_angularFactor == value) return;
                _angularFactor = value;
                if (rigidBody != null)
                {
                    rigidBody.AngularFactor = _angularFactor;
                    ActivateBody();
                }
            }
        }
        #endregion
        #endregion

        bool rbadded = false;

        public RigidBodyComponent()
        {

        }

        public override void OnAttach()
        {
            Transform = Entity.GetComponent<TransformComponent>();
            
            if (Entity.TryGetComponent(out ColliderComponent cc, true))
            {
                Collider = cc;
            }

            using (RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(_mass, new DefaultMotionState(_transform.WorldTransformMatrix), (_collider != null) ? _collider.Shape : null))
            {
                rigidBody = new RigidBody(info)
                {
                    LinearVelocity = _velocity,
                    LinearFactor = _linearFactor,
                    AngularFactor = _angularFactor,
                };
            }

            if (_collider != null)
            {
                UpdateLocalInertia();
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
            rbadded = true;
        }

        public override void OnDisable()
        {
            Entity.Scene.RigidBodyWorld.RemoveRigidBody(rigidBody);
            rbadded = false;
        }

        private void OnTransformChanged(TransformComponent sender)
        {
            if (rigidBody != null)
            {
                rigidBody.WorldTransform = _transform.WorldTransformMatrix;
                ActivateBody();
            }
            _collider.Shape.LocalScaling = _transform.LocalScale;
        }

        private void OnColliderShapeChanged(ColliderComponent sender)
        {
            if (rigidBody != null)
            {
                rigidBody.CollisionShape = _collider.Shape;

                if (rigidBody.BroadphaseProxy != null)
                {
                    Entity.Scene.RigidBodyWorld.CleanProxyFromPairs(rigidBody);
                }

                //UpdateLocalInertia();
                ActivateBody();
            }
        }

        private void UpdateLocalInertia()
        {
            if (rbadded) Entity.Scene.RigidBodyWorld.AddRigidBody(rigidBody);
            rigidBody.SetMassProps(_mass, rigidBody.CollisionShape.CalculateLocalInertia(_mass));
            if (rbadded) Entity.Scene.RigidBodyWorld.RemoveRigidBody(rigidBody);
        }

        private void ActivateBody()
        {
            if (!rigidBody.IsActive)
                rigidBody.Activate();
        }

        public override void OnEvent(Event e)
        {
            if (e is RigidBodyWorldUpdateEvent)
            {
                _transform.OnTransformChanged -= OnTransformChanged;
                if (_transform.WorldTransformMatrix != rigidBody.MotionState.WorldTransform)
                    _transform.SetTranslationRotation(rigidBody.MotionState.WorldTransform);
                _transform.OnTransformChanged += OnTransformChanged;
            }
        }
    }
}
