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
using CrossEngine.Serialization.Json;

namespace CrossEngine.Entities.Components
{
    [RequireComponent(typeof(TransformComponent))]
    [RequireComponent(typeof(ColliderComponent))]
    public class RigidBodyComponent : Component, ISerializable
    {
        RigidBody rigidBody;

        private TransformComponent _transform;
        private ColliderComponent _collider;
        public TransformComponent Transform
        {
            get => _transform;
            private set
            {
                if (_transform != null) _transform.OnTransformChanged -= OnTransformChanged;
                _transform = value;
                if (_transform != null) _transform.OnTransformChanged += OnTransformChanged;
            }
        }
        public ColliderComponent Collider
        {
            get => _collider;
            private set
            {
                if (_collider != null) _collider.OnShapeChanged -= OnColliderShapeChanged;
                _collider = value;
                if (_collider != null)
                {
                    _collider.OnShapeChanged += OnColliderShapeChanged;
                    if (rigidBody != null)
                    {
                        rigidBody.CollisionShape = _collider.Shape;

                        rigidBody.UpdateInertiaTensor();

                        if (rigidBody.BroadphaseProxy != null)
                        {
                            Entity.Scene.RigidBodyWorld.CleanProxyFromPairs(rigidBody);
                        }

                        ActivateBody();
                    }
                }
            }
        }



        private bool _static;
        [EditorBooleanValue]
        public bool Static
        {
            get => _static;
            set
            {
                if (_static == value) return;
                _static = value;

                if (rigidBody != null)
                {
                    Entity.Scene.RigidBodyWorld.RemoveRigidBody(rigidBody);

                    // that's it i just recreate it
                    RecreateRigidBody();

                    if (_static)
                    {
                        rigidBody.LinearVelocity = Vector3.Zero;
                        rigidBody.AngularVelocity = Vector3.Zero;
                    }

                    Entity.Scene.RigidBodyWorld.AddRigidBody(rigidBody);
                }
            }
        }
        #region RB Fields
        private float _mass = 1.0f;

        private Vector3 _velocity = Vector3.Zero;
        private Vector3 _angularVelocity = Vector3.Zero;
        private Vector3 _linearFactor = Vector3.One;
        private Vector3 _angularFactor = Vector3.One;
        #region Properties
        [EditorSingleValue]
        public float Mass
        {
            get => _static ? 0.0f : _mass;
            set
            {
                //if (_mass == value) return;
                if (value <= 0.0f) return;
                _mass = value;
                if (rigidBody != null && !_static)
                {
                    UpdateMassProps(_mass);
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
        public Vector3 AngularVelocity
        {
            get => _angularVelocity = (rigidBody != null) ? rigidBody.AngularVelocity : _angularVelocity;
            set
            {
                if (_angularVelocity == value) return;
                _angularVelocity = value;
                if (rigidBody != null && !_static)
                {
                    rigidBody.AngularVelocity = _angularVelocity;
                    ActivateBody();
                }
            }
        }
        public Vector3 LinearFactor
        {
            get => _linearFactor = (rigidBody != null) ? rigidBody.LinearFactor : _linearFactor;
            set
            {
                if (_linearFactor == value) return;
                _linearFactor = value;
                if (rigidBody != null && !_static)
                {
                    rigidBody.LinearFactor = _linearFactor;
                    ActivateBody();
                }
            }
        }
        public Vector3 AngularFactor
        {
            get => _angularFactor = (rigidBody != null) ? rigidBody.AngularFactor : _angularFactor;
            set
            {
                if (_angularFactor == value) return;
                _angularFactor = value;
                if (rigidBody != null && !_static)
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

        private void RecreateRigidBody()
        {
            if (rigidBody != null) rigidBody.Dispose();
            rigidBody = null;

            using (RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(Mass, new DefaultMotionState(_transform.WorldTransformMatrix), (_collider != null) ? _collider.Shape : null))
            {
                rigidBody = new RigidBody(info)
                {
                    LinearVelocity = _velocity,
                    AngularVelocity = _angularVelocity,
                    LinearFactor = _linearFactor,
                    AngularFactor = _angularFactor,
                };
            }

            if (_collider != null)
            {
                UpdateMassProps(Mass);
            }

            rigidBody.CollisionFlags = CollisionFlags.None;
        }

        public override void OnAttach()
        {
            Transform = Entity.GetComponent<TransformComponent>();
            
            if (Entity.TryGetComponent(out ColliderComponent cc, true))
            {
                Collider = cc;
            }

            RecreateRigidBody();

            if (_static)
            {
                rigidBody.LinearVelocity = Vector3.Zero;
                rigidBody.AngularVelocity = Vector3.Zero;
            }

            //Entity.OnComponentAdded += OnComponentAdded;
            //Entity.OnComponentRemoved += OnComponentRemoved;
        }

        public override void OnDetach()
        {
            rigidBody.Dispose();
            rigidBody = null;

            //Entity.OnComponentAdded -= OnComponentAdded;
            //Entity.OnComponentRemoved -= OnComponentRemoved;
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

        #region Events
        //private void OnComponentAdded(Entity sender, Component component)
        //{
        //    
        //}
        //
        //private void OnComponentRemoved(Entity sender, Component component)
        //{
        //
        //}

        #region Component Events
        private void OnTransformChanged(TransformComponent sender)
        {
            if (rigidBody != null)
            {
                rigidBody.WorldTransform = Matrix4x4.CreateFromQuaternion(_transform.WorldRotation) * Matrix4x4.CreateTranslation(_transform.WorldPosition);
                ActivateBody();
            }
            _collider.Shape.LocalScaling = _transform.LocalScale;
        }

        private void OnColliderShapeChanged(ColliderComponent sender)
        {
            if (rigidBody != null)
            {
                rigidBody.CollisionShape = _collider.Shape;

                if (_collider.Shape == null) Enabled = false;

                rigidBody.UpdateInertiaTensor();

                if (rigidBody.BroadphaseProxy != null)
                {
                    Entity.Scene.RigidBodyWorld.CleanProxyFromPairs(rigidBody);
                }

                ActivateBody();
            }
        }
        #endregion
        #endregion

        private void ActivateBody()
        {
            if (!rigidBody.IsActive)
                rigidBody.Activate();
        }

        private void UpdateMassProps(float mass)
        {
            rigidBody.SetMassProps(mass, _collider.Shape.CalculateLocalInertia(mass));
            rigidBody.UpdateInertiaTensor();
        }

        #region ISerializable
        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue("Static", Static);
            info.AddValue("Mass", Mass);
        }

        public RigidBodyComponent(DeserializationInfo info)
        {
            Static = (bool)info.GetValue("Static", typeof(bool));
            Mass = (float)info.GetValue("Mass", typeof(float));
        }
        #endregion
    }
}
