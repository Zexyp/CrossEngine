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
using CrossEngine.Rendering;
using CrossEngine.Rendering.Lines;
using CrossEngine.Serialization;

namespace CrossEngine.Entities.Components
{
    using BulletVector3 = BulletSharp.Math.Vector3;

    [RequireComponent(typeof(TransformComponent))]
    [RequireComponent(typeof(ColliderComponent))]
    public class RigidBodyComponent : Component
    {
        RigidBody nativeRigidBody;

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
                    if (nativeRigidBody != null)
                    {
                        nativeRigidBody.CollisionShape = _collider.NativeShape;

                        nativeRigidBody.UpdateInertiaTensor();

                        CleanProxyFromOverlapingPairsCache();

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

                if (nativeRigidBody != null)
                {
                    Entity.Scene.RigidBodyWorld.RemoveRigidBody(nativeRigidBody);

                    // that's it i just recreate it
                    RecreateRigidBody();

                    if (_static)
                    {
                        Velocity = Vector3.Zero;
                        AngularVelocity = Vector3.Zero;
                        //nativeRigidBody.LinearVelocity = BulletVector3.Zero;
                        //nativeRigidBody.AngularVelocity = BulletVector3.Zero;
                    }

                    Entity.Scene.RigidBodyWorld.AddRigidBody(nativeRigidBody);
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
                if (nativeRigidBody != null && !_static)
                {
                    UpdateMassProps(_mass);
                    ActivateBody();
                }
            }
        }

        [EditorVector3Value]
        public Vector3 Velocity
        {
            get => _velocity = (nativeRigidBody != null) ? nativeRigidBody.LinearVelocity.ToNumerics() : _velocity;
            set
            {
                if (Velocity == value) return;
                _velocity = value;
                if (nativeRigidBody != null)
                {
                    nativeRigidBody.LinearVelocity = _velocity.ToBullet();
                    ActivateBody();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 AngularVelocity
        {
            get => _angularVelocity = (nativeRigidBody != null) ? nativeRigidBody.AngularVelocity.ToNumerics() : _angularVelocity;
            set
            {
                if (AngularVelocity == value) return;
                _angularVelocity = value;
                if (nativeRigidBody != null && !_static)
                {
                    nativeRigidBody.AngularVelocity = _angularVelocity.ToBullet();
                    ActivateBody();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 LinearFactor
        {
            get => _linearFactor = (nativeRigidBody != null) ? nativeRigidBody.LinearFactor.ToNumerics() : _linearFactor;
            set
            {
                if (LinearFactor == value) return;
                _linearFactor = value;
                if (nativeRigidBody != null && !_static)
                {
                    nativeRigidBody.LinearFactor = _linearFactor.ToBullet();
                    ActivateBody();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 AngularFactor
        {
            get => _angularFactor = (nativeRigidBody != null) ? nativeRigidBody.AngularFactor.ToNumerics() : _angularFactor;
            set
            {
                if (AngularFactor == value) return;
                _angularFactor = value;
                if (nativeRigidBody != null && !_static)
                {
                    nativeRigidBody.AngularFactor = _angularFactor.ToBullet();
                    ActivateBody();
                }
            }
        }
        #endregion
        #endregion

        public RigidBodyComponent()
        {

        }

        private void RecreateRigidBody()
        {
            nativeRigidBody?.MotionState?.Dispose();
            nativeRigidBody?.Dispose();
            nativeRigidBody = null;

            CreateRigidBody();

            //nativeRigidBody.UserIndex = Entity.UID;
        }

        private void CreateRigidBody()
        {
            BulletVector3 localInertia = BulletVector3.Zero;

            if (!_static && _collider != null)
                localInertia = _collider.NativeShape.CalculateLocalInertia(Mass);

            MotionState motionState = new DefaultMotionState(_transform.WorldTransformMatrix.ToBullet());
            using (RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(!Static ? Mass : 0.0f, motionState, _collider.NativeShape, localInertia))
            {
                nativeRigidBody = new RigidBody(info)
                {
                    LinearVelocity = _velocity.ToBullet(),
                    AngularVelocity = _angularVelocity.ToBullet(),
                    LinearFactor = _linearFactor.ToBullet(),
                    AngularFactor = _angularFactor.ToBullet(),
                };
            }

            nativeRigidBody.CollisionFlags = CollisionFlags.None;

            //if (!_static && _collider != null)
            //{
            //    UpdateMassProps(Mass);
            //}

            nativeRigidBody.UserObject = this;
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
                nativeRigidBody.LinearVelocity = BulletVector3.Zero;
                nativeRigidBody.AngularVelocity = BulletVector3.Zero;
            }

            Entity.OnComponentAdded += OnComponentAdded;
            Entity.OnComponentRemoved += OnComponentRemoved;
        }

        public override void OnDetach()
        {
            nativeRigidBody.Dispose();
            nativeRigidBody = null;

            Entity.OnComponentAdded -= OnComponentAdded;
            Entity.OnComponentRemoved -= OnComponentRemoved;
        }

        public override void OnEnable()
        {
            Entity.Scene.RigidBodyWorld.AddRigidBody(nativeRigidBody);
        }

        public override void OnDisable()
        {
            Entity.Scene.RigidBodyWorld.RemoveRigidBody(nativeRigidBody);
        }

        public override void OnEvent(Event e)
        {
            if (e is RigidBodyWorldUpdateEvent)
            {
                _transform.OnTransformChanged -= OnTransformChanged;
                if (_transform.WorldTransformMatrix != nativeRigidBody.MotionState.WorldTransform.ToNumerics())
                    _transform.SetTranslationRotation(nativeRigidBody.MotionState.WorldTransform.ToNumerics());
                _transform.OnTransformChanged += OnTransformChanged;
            }
        }

        #region Events
        private void OnComponentAdded(Entity sender, Component component)
        {
            if (component.GetType().IsSubclassOf(typeof(ColliderComponent))) Collider = Entity.GetComponent<ColliderComponent>(true);
            if (component.GetType() == typeof(TransformComponent)) Transform = Entity.Transform;
        }
        
        private void OnComponentRemoved(Entity sender, Component component)
        {
            if (component.GetType().IsSubclassOf(typeof(ColliderComponent))) Collider = Entity.GetComponent<ColliderComponent>(true);
            if (component.GetType() == typeof(TransformComponent)) Transform = Entity.Transform;
        }

        #region Component Events
        private void OnTransformChanged(TransformComponent sender)
        {
            if (nativeRigidBody != null)
            {
                nativeRigidBody.WorldTransform = (Matrix4x4.CreateFromQuaternion(_transform.WorldRotation) * Matrix4x4.CreateTranslation(_transform.WorldPosition)).ToBullet();

                CleanProxyFromOverlapingPairsCache();

                ActivateBody();
            }
            //_collider.NativeShape.LocalScaling = _transform.Scale.ToBullet();
        }

        private void OnColliderShapeChanged(ColliderComponent sender)
        {
            if (nativeRigidBody != null)
            {
                nativeRigidBody.CollisionShape = _collider.NativeShape;

                nativeRigidBody.UpdateInertiaTensor();

                CleanProxyFromOverlapingPairsCache();

                ActivateBody();
            }
        }
        #endregion
        #endregion

        private void ActivateBody()
        {
            if (!nativeRigidBody.IsActive)
                nativeRigidBody.Activate();
        }

        private void UpdateMassProps(float mass)
        {
            nativeRigidBody.SetMassProps(mass, _collider.NativeShape.CalculateLocalInertia(mass));
            nativeRigidBody.UpdateInertiaTensor();
        }

        private void CleanProxyFromOverlapingPairsCache()
        {
            if (nativeRigidBody.BroadphaseProxy != null)
            {
                var rbw = Entity.Scene.RigidBodyWorld;
                rbw.Broadphase.OverlappingPairCache.CleanProxyFromPairs(nativeRigidBody.BroadphaseProxy, rbw.Dispatcher);
            }
        }

        public override void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Static", Static);
            info.AddValue("Mass", Mass);

            info.AddValue("Velocity", Velocity);
            info.AddValue("AngularVelocity", AngularVelocity);
            
            info.AddValue("LinearFactor", LinearFactor);
            info.AddValue("AngularFactor", AngularFactor);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            Static = (bool)info.GetValue("Static", typeof(bool));
            Mass = (float)info.GetValue("Mass", typeof(float));

            Velocity = (Vector3)info.GetValue("Velocity", typeof(Vector3));
            AngularVelocity = (Vector3)info.GetValue("AngularVelocity", typeof(Vector3));
            
            LinearFactor = (Vector3)info.GetValue("LinearFactor", typeof(Vector3));
            AngularFactor = (Vector3)info.GetValue("AngularFactor", typeof(Vector3));
        }
    }
}
