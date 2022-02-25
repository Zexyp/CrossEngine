using System;

using BulletSharp;
using BulletSharp.Math;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Logging;
using CrossEngine.Events;
using CrossEngine.Physics;
using CrossEngine.Rendering;
using CrossEngine.Profiling;
using CrossEngine.Utils.Bullet;
using CrossEngine.Utils;

namespace CrossEngine.ComponentSystems
{
    using BulletVector4 = BulletSharp.Math.Vector4;
    using BulletVector3 = BulletSharp.Math.Vector3;
    using Vector3 = System.Numerics.Vector3;
    using Vector4 = System.Numerics.Vector4;

    [Flags]
    enum RigidBodyPropertyFlags
    {
        None            = 0,
        Static          = 1 << 0,
        Mass            = 1 << 1,
        LinearVelocity  = 1 << 2,
        AngularVelocity = 1 << 3,
        LinearFactor    = 1 << 4,
        AngularFactor   = 1 << 5,
    }

    class PhysicsSysten : ISystem
    {
        static readonly Dictionary<Type, Func<ColliderComponent, CollisionShape>> ShapeConvertors = new Dictionary<Type, Func<ColliderComponent, CollisionShape>>()
        {
            { typeof(BoxColliderComponent), (collider) => {
                var corrcoll = (BoxColliderComponent)collider;
                return new BoxShape((corrcoll.Size / 2).ToBullet());
            } }
        };

        class SyncedRBData
        {
            Entity _entity;

            RigidBodyComponent _rigidBody;
            TransformComponent _transform;
            readonly Dictionary<ColliderComponent, (bool Dirty, CollisionShape Shape)> _colliders = new Dictionary<ColliderComponent, (bool, CollisionShape)>();

            RigidBody _body;
            RigidBodyPropertyFlags _dirtyProperties;
            MotionState _motionState;
            bool _updateMotion = true;
            CompoundShape _shape;
            bool _shapeDirty = true;

            public event Action<SyncedRBData> OnUpdateRequired;

            TransformComponent Transform
            {
                set
                {
                    if (_transform != null) _transform.OnTransformChanged -= Transform_OnTransformChanged;
                    _transform = value;
                    if (_transform != null) _transform.OnTransformChanged += Transform_OnTransformChanged;
                    Transform_OnTransformChanged(_transform);
                }
            }



            public SyncedRBData(RigidBodyComponent component)
            {
                _entity = component.Entity;
                _rigidBody = component;
            }

            public void Destroy()
            {
                _entity = null;
                _rigidBody = null;
            }

            private void Entity_OnComponentAdded(Entity sender, Component component)
            {
                if (component is RigidBodyComponent) throw new InvalidOperationException();
                if (component is TransformComponent)
                {
                    Transform = (TransformComponent)component;
                }
                if (component is ColliderComponent)
                {
                    AddCollider((ColliderComponent)component);
                }
            }

            private void Entity_OnComponentRemoved(Entity sender, Component component)
            {
                if (component is RigidBodyComponent) throw new InvalidOperationException();
                if (component is TransformComponent)
                {
                    Transform = null;
                }
                if (component is ColliderComponent)
                {
                    RemoveCollider((ColliderComponent)component);
                }
            }

            private void Collider_OnShapeChanged(ColliderComponent component)
            {
                _colliders[component] = (true, _colliders[component].Shape);

                _shapeDirty = true;
                Log.Core.Debug("TODO: add removal from overlapping pairs");

                OnUpdateRequired?.Invoke(this);
            }

            private void Transform_OnTransformChanged(TransformComponent component)
            {
                _updateMotion = true;

                Log.Core.Debug("TODO: collider scaling");

                OnUpdateRequired?.Invoke(this);
            }

            private void RigidBody_OnPropertyChanged(RigidBodyComponent component, RigidBodyPropertyFlags flags)
            {
                Debug.Assert(flags != RigidBodyPropertyFlags.None);
                _dirtyProperties |= flags;

                OnUpdateRequired?.Invoke(this);
            }

            private void AddCollider(ColliderComponent component)
            {
                component.OnShapeChanged += Collider_OnShapeChanged;
                _colliders.Add(component, (true, null));
                Collider_OnShapeChanged(component);
            }

            private void RemoveCollider(ColliderComponent component)
            {
                component.OnShapeChanged -= Collider_OnShapeChanged;

                if (_colliders[component].Shape != null) _shape.RemoveChildShape(_colliders[component].Shape);
                _colliders[component].Shape.Dispose();
                _colliders.Remove(component);
            }

            public void Activate()
            {
                _shape = new CompoundShape();

                _entity.OnComponentAdded += Entity_OnComponentAdded;
                _entity.OnComponentRemoved += Entity_OnComponentRemoved;

                _rigidBody.OnPropertyChanged += RigidBody_OnPropertyChanged;

                Transform = _entity.GetComponent<TransformComponent>();

                var collcomps = _entity.GetAllComponents<ColliderComponent>();
                for (int i = 0; i < collcomps.Length; i++) AddCollider(collcomps[i]);

                _body = CreateBody(_rigidBody, _transform, _shape);
                _motionState = _body.MotionState;

                PhysicsSysten.Instance.rigidBodyWorld.World.AddRigidBody(_body);
            }

            public void Deactivate()
            {
                PhysicsSysten.Instance.rigidBodyWorld.World.RemoveRigidBody(_body);

                _body.MotionState = null;
                _body.Dispose();
                _body = null;
                _motionState.Dispose();
                _motionState = null;

                while (_colliders.Count > 0) RemoveCollider(_colliders.First().Key);

                _shape.Dispose();
                _shape = null;
                
                Transform = null;

                _rigidBody.OnPropertyChanged -= RigidBody_OnPropertyChanged;

                _entity.OnComponentAdded -= Entity_OnComponentAdded;
                _entity.OnComponentRemoved -= Entity_OnComponentRemoved;
            }

            public void Update()
            {
                if (_shapeDirty)
                {
                    foreach (var key in _colliders.Keys)
                    {
                        if (!_colliders[key].Dirty) continue;

                        if (_colliders[key].Shape != null) _shape.RemoveChildShape(_colliders[key].Shape);
                        _colliders[key].Shape?.Dispose();
                        var newshape = ShapeConvertors[key.GetType()](key);
                        _colliders[key] = (false, newshape);
                        _shape.AddChildShape(key.LocalOffset.ToBullet(), newshape);
                    }

                    _shape.CalculateLocalInertia(_rigidBody.Static ? 0 : _rigidBody.Mass, out var inertia);
                    _body.SetMassProps(_rigidBody.Static ? 0 : _rigidBody.Mass, inertia);

                    _body.UpdateInertiaTensor();

                    _body.Activate();
                }

                if (_updateMotion)
                {
                    var worldMat = _transform.WorldTransformMatrix.ToBullet();
                    _motionState.SetWorldTransform(ref worldMat);

                    _body.Activate();
                }

                if (_dirtyProperties != RigidBodyPropertyFlags.None)
                {
                    if ((_dirtyProperties | RigidBodyPropertyFlags.LinearVelocity) != 0)
                    {
                        _body.LinearVelocity = _rigidBody.Velocity.ToBullet();
                    }
                    if ((_dirtyProperties | RigidBodyPropertyFlags.AngularVelocity) != 0)
                    {
                        _body.AngularVelocity = _rigidBody.AngularVelocity.ToBullet();
                    }
                    if ((_dirtyProperties | RigidBodyPropertyFlags.LinearFactor) != 0)
                    {
                        _body.LinearFactor = _rigidBody.LinearFactor.ToBullet();
                    }
                    if ((_dirtyProperties | RigidBodyPropertyFlags.AngularFactor) != 0)
                    {
                        _body.AngularFactor = _rigidBody.AngularFactor.ToBullet();
                    }

                    if ((_dirtyProperties | RigidBodyPropertyFlags.Mass) != 0)
                    {
                        _body.SetMassProps(_rigidBody.Mass, _shape.CalculateLocalInertia(_rigidBody.Mass));
                        _body.UpdateInertiaTensor();
                    }

                    if ((_dirtyProperties | RigidBodyPropertyFlags.Static) != 0)
                    {
                        if (!_rigidBody.Static)
                        {
                            PhysicsSysten.Instance.rigidBodyWorld.World.RemoveRigidBody(_body);

                            _body.CollisionShape.CalculateLocalInertia(_rigidBody.Mass, out var inertia);
                            _body.ActivationState = ActivationState.DisableDeactivation;
                            _body.SetMassProps(_rigidBody.Mass, inertia);
                            _body.LinearFactor = _rigidBody.LinearFactor.ToBullet();
                            _body.AngularFactor =  _rigidBody.AngularFactor.ToBullet();
                            _body.UpdateInertiaTensor();
                            _body.ClearForces();
                            //_body.WorldTransform = _transform.WorldTransformMatrix;

                            PhysicsSysten.Instance.rigidBodyWorld.World.AddRigidBody(_body);
                        }
                        else
                        {
                            PhysicsSysten.Instance.rigidBodyWorld.World.RemoveRigidBody(_body);

                            _body.CollisionShape.CalculateLocalInertia(0, out var inertia);
                            _body.CollisionFlags = CollisionFlags.StaticObject;
                            _body.SetMassProps(0, inertia);
                            _body.LinearFactor = BulletVector3.Zero;
                            _body.AngularFactor = BulletVector3.Zero;
                            _body.Gravity = BulletVector3.Zero;
                            _body.UpdateInertiaTensor();
                            _body.LinearVelocity = BulletVector3.Zero;
                            _body.AngularVelocity = BulletVector3.Zero;
                            _body.ClearForces();
                            _body.ActivationState = ActivationState.WantsDeactivation;
                            //_body.WorldTransform = _transform.WorldTransformMatrix;

                            PhysicsSysten.Instance.rigidBodyWorld.World.AddRigidBody(_body);
                        }
                    }

                    _dirtyProperties = RigidBodyPropertyFlags.None;

                    _body.Activate();
                }
            }

            public void UpdataTransform()
            {
                // TODO: consider interpolation

                if (_transform != null)
                {
                    _transform.OnTransformChanged -= Transform_OnTransformChanged;
                    _transform.SetTransform(_motionState.WorldTransform.ToNumerics());
                    _transform.OnTransformChanged += Transform_OnTransformChanged;
                }

                _rigidBody.OnPropertyChanged -= RigidBody_OnPropertyChanged;
                _rigidBody.Velocity = _body.LinearVelocity.ToNumerics();
                _rigidBody.AngularVelocity = _body.AngularVelocity.ToNumerics();
                _rigidBody.OnPropertyChanged += RigidBody_OnPropertyChanged;
            }

            private static RigidBody CreateBody(RigidBodyComponent rigidBody, TransformComponent transform, CollisionShape shape)
            {
                MotionState motion;
                if (transform != null)
                    motion = new DefaultMotionState(transform.WorldTransformMatrix.ToBullet());
                else
                    motion = new DefaultMotionState();

                RigidBodyConstructionInfo bodyctorinfo;
                if (shape != null)
                {
                    BulletVector3 localInertia = shape.CalculateLocalInertia(rigidBody.Mass);
                    bodyctorinfo = new RigidBodyConstructionInfo(rigidBody.Static ? 0 : rigidBody.Mass, motion, shape, localInertia);
                }
                else
                    bodyctorinfo = new RigidBodyConstructionInfo(rigidBody.Static ? 0 : rigidBody.Mass, motion, shape);

                var body = new RigidBody(bodyctorinfo)
                {
                    LinearVelocity = rigidBody.Velocity.ToBullet(),
                    AngularVelocity = rigidBody.AngularVelocity.ToBullet(),
                    LinearFactor = rigidBody.LinearFactor.ToBullet(),
                    AngularFactor = rigidBody.AngularFactor.ToBullet(),
                };

                bodyctorinfo.Dispose();

                return body;
            }
        }

        public static PhysicsSysten Instance;

        public static Vector4 ColliderRepresentationColor = new Vector4(1.0f, 0.4f, 0.0f, 1.0f);

        List<ColliderComponent> _colliders = new List<ColliderComponent>();

        Dictionary<RigidBodyComponent, SyncedRBData> _pairs = new Dictionary<RigidBodyComponent, SyncedRBData>();
        Queue<SyncedRBData> _changed = new Queue<SyncedRBData>();

        RigidBodyWorld rigidBodyWorld;

        public PhysicsSysten()
        {
            Debug.Assert(Instance == null);

            Instance = this;
        }

        public void Init()
        {
            rigidBodyWorld = new RigidBodyWorld();

            Physics.Physics.SetContext(rigidBodyWorld);

            foreach (var item in _pairs)
            {
                item.Value.Activate();
            }
        }

        public void Shutdown()
        {
            foreach (var item in _pairs)
            {
                item.Value.Deactivate();
            }

            rigidBodyWorld.Dispose();

            throw new NotImplementedException();
        }

        public void Update()
        {
            Profiler.BeginScope($"{nameof(PhysicsSysten)}.Update");

            while (_changed.TryDequeue(out SyncedRBData rbdata))
            {
                rbdata.Update();
            }

            rigidBodyWorld.Simulate(Time.DeltaTimeF, 5, (float)Time.FixedDeltaTime);

            foreach (var item in _pairs.Values)
            {
                item.UpdataTransform();
            }

            Profiler.EndScope();
        }

        public void RegisterRigidBody(RigidBodyComponent component)
        {
            var rbdata = CreateRBData(component);

            if (rigidBodyWorld != null)
            {
                rbdata.Activate();
            }
        }

        public void UnregisterRigidBody(RigidBodyComponent component)
        {
            if (rigidBodyWorld != null)
            {
                _pairs[component].Deactivate();
            }

            DestroyRBData(component);
        }

        public void RegisterCollider(ColliderComponent component)
        {
            _colliders.Add(component);
        }

        public void UnregisterCollider(ColliderComponent component)
        {
            _colliders.Remove(component);
        }

        private SyncedRBData CreateRBData(RigidBodyComponent rbcomponent)
        {
            var data = new SyncedRBData(rbcomponent);
            _pairs.Add(rbcomponent, data);
            data.OnUpdateRequired += RBData_OnUpdateRequired;
            return data;
        }

        private void RBData_OnUpdateRequired(SyncedRBData sender)
        {
            if (!_changed.Contains(sender))
                _changed.Enqueue(sender);
        }

        private void DestroyRBData(RigidBodyComponent rbcomponent)
        {
            var data = _pairs[rbcomponent];
            data.OnUpdateRequired -= RBData_OnUpdateRequired;
            data.Destroy();
            _pairs.Remove(rbcomponent);
        }

        private void DestroyBody(RigidBody body)
        {
            rigidBodyWorld.RemoveRigidBody(body);

            body.Dispose();
        }
    }
}
