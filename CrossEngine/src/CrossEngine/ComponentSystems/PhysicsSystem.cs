using System;
using BulletSharp;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

using CrossEngine.Components;
using CrossEngine.ECS;
using CrossEngine.Physics;
using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Utils.Bullet;
using CrossEngine.Utils;
using System.Collections;

namespace CrossEngine.ComponentSystems
{
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

    [Flags]
    enum ColliderPropertyFlags
    {
        None = 0,
        Shape = 1 << 0,
        LocalOffsets = 1 << 1,
    }

    class PhysicsSystem : IComponentSystem, IRenderableComponentSystem
    {
        static readonly Dictionary<Type, Func<ColliderComponent, CollisionShape>> ShapeConvertors = new Dictionary<Type, Func<ColliderComponent, CollisionShape>>()
        {
            { typeof(BoxColliderComponent), (collider) => {
                var corrcoll = (BoxColliderComponent)collider;
                return new BoxShape((corrcoll.Size / 2).ToBullet());
            } },
            { typeof(Box2DColliderComponent), (collider) => {
                var corrcoll = (Box2DColliderComponent)collider;
                return new Box2DShape(new Vector3(corrcoll.Size / 2, 1).ToBullet());
            } },
            { typeof(SphereColliderComponent), (collider) => {
                var corrcoll = (SphereColliderComponent)collider;
                return new SphereShape(corrcoll.Radius);
            } },
            { typeof(CapsuleColliderComponent), (collider) => {
                var corrcoll = (CapsuleColliderComponent)collider;
                switch (corrcoll.Direction)
                {
                    case ColliderDirection.X: return new CapsuleShapeX(corrcoll.Radius, corrcoll.Length);
                    case ColliderDirection.Y: return new CapsuleShape(corrcoll.Radius, corrcoll.Length);
                    case ColliderDirection.Z: return new CapsuleShapeZ(corrcoll.Radius, corrcoll.Length);
                    default: Debug.Assert(false); break;
                }
                return null;
            } },
        };

        class SyncedRBData
        {
            class ColliderData
            {
                public CollisionShape Shape;
                public ColliderPropertyFlags Dirt;
            }

            Entity _entity;

            RigidBodyComponent _rigidBody;
            TransformComponent _transform;
            readonly Dictionary<ColliderComponent, ColliderData> _colliders = new Dictionary<ColliderComponent, ColliderData>();

            RigidBody _body;
            RigidBodyPropertyFlags _dirtyProperties;
            MotionState _motionState;
            CompoundShape _shape;
            bool _updateMotion = true;
            bool _shapeDirty = true;
            public bool Active { get; private set; }

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

            readonly PhysicsSystem _ps;



            public SyncedRBData(RigidBodyComponent component, PhysicsSystem system)
            {
                _entity = component.Entity;
                _rigidBody = component;

                _ps = system;
            }

            public void Destroy()
            {
                _entity = null;
                _rigidBody = null;
            }

            private void Entity_OnComponentAdded(Entity sender, Component component)
            {
                //if (component is RigidBodyComponent) throw new InvalidOperationException();
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
                //if (component is RigidBodyComponent) throw new InvalidOperationException();
                if (component is TransformComponent)
                {
                    Transform = null;
                }
                if (component is ColliderComponent)
                {
                    RemoveCollider((ColliderComponent)component);
                }
            }

            private void Collider_OnPropertyChanged(ColliderComponent component, ColliderPropertyFlags flags)
            {
                Debug.Assert(flags != ColliderPropertyFlags.None);
                _colliders[component].Dirt |= flags;
                _shapeDirty = true;

                OnUpdateRequired?.Invoke(this);
            }

            private void Collider_OnEnabledChanged(Component component)
            {
                _colliders[(ColliderComponent)component].Dirt |= ColliderPropertyFlags.LocalOffsets;
                _shapeDirty = true;
                OnUpdateRequired?.Invoke(this);
            }

            private void Transform_OnTransformChanged(TransformComponent component)
            {
                _updateMotion = true;

                OnUpdateRequired?.Invoke(this);
            }

            private void RigidBody_OnPropertyChanged(RigidBodyComponent component, RigidBodyPropertyFlags flags)
            {
                Debug.Assert(flags != RigidBodyPropertyFlags.None);
                _dirtyProperties |= flags;

                OnUpdateRequired?.Invoke(this);
            }

            private void RigidBody_OnEnabledChanged(Component component)
            {
                if (!component.Enabled) _ps.rigidBodyWorld.RemoveRigidBody(_body);
                else _ps.rigidBodyWorld.AddRigidBody(_body);
            }

            private void AddCollider(ColliderComponent component)
            {
                component.OnPropertyChanged += Collider_OnPropertyChanged;
                component.OnEnabledChanged += Collider_OnEnabledChanged;

                _colliders.Add(component, new ColliderData());
                Collider_OnPropertyChanged(component,
                    ColliderPropertyFlags.Shape |
                    ColliderPropertyFlags.LocalOffsets);
            }

            private void RemoveCollider(ColliderComponent component)
            {
                component.OnEnabledChanged -= Collider_OnEnabledChanged;
                component.OnPropertyChanged -= Collider_OnPropertyChanged;

                if (_colliders[component].Shape != null) _shape.RemoveChildShape(_colliders[component].Shape);
                _colliders[component].Shape?.Dispose();
                _colliders.Remove(component);
            }

            public void Activate()
            {
                Active = true;

                _shape = new CompoundShape();

                _entity.OnComponentAdded += Entity_OnComponentAdded;
                _entity.OnComponentRemoved += Entity_OnComponentRemoved;

                _rigidBody.OnPropertyChanged += RigidBody_OnPropertyChanged;
                _rigidBody.OnEnabledChanged += RigidBody_OnEnabledChanged;

                Transform = _entity.GetComponent<TransformComponent>();

                var collcomps = _entity.GetAllComponents<ColliderComponent>();
                for (int i = 0; i < collcomps.Length; i++) AddCollider(collcomps[i]);

                _body = CreateBody(_rigidBody, _transform, _shape);
                _motionState = _body.MotionState;

                if (_rigidBody.Enabled) _ps.rigidBodyWorld.World.AddRigidBody(_body);
            }

            public void Deactivate()
            {
                if (_rigidBody.Enabled) _ps.rigidBodyWorld.World.RemoveRigidBody(_body);

                _body.MotionState = null;
                _body.Dispose();
                _body = null;
                _motionState.Dispose();
                _motionState = null;

                while (_colliders.Count > 0) RemoveCollider(_colliders.First().Key);

                _shape.Dispose();
                _shape = null;
                
                Transform = null;

                _rigidBody.OnEnabledChanged -= RigidBody_OnEnabledChanged;
                _rigidBody.OnPropertyChanged -= RigidBody_OnPropertyChanged;

                _entity.OnComponentAdded -= Entity_OnComponentAdded;
                _entity.OnComponentRemoved -= Entity_OnComponentRemoved;

                Active = false;
            }

            public void Update()
            {
                #region Shape
                if (_shapeDirty)
                {
                    //PhysicsSysten.Instance.rigidBodyWorld.World.RemoveRigidBody(_body);

                    foreach (var item in _colliders)
                    {
                        var colliderData = item.Value;
                        var colliderComponent = item.Key;

                        if (!((colliderData.Dirt & ColliderPropertyFlags.Shape) != 0))
                        {
                            if ((colliderData.Dirt & ColliderPropertyFlags.LocalOffsets) != 0)
                            {
                                _shape.RemoveChildShape(colliderData.Shape);
                                // ! this is odd
                                if (colliderComponent.Enabled) _shape.AddChildShape(colliderComponent.OffsetMatrix.ToBullet(), colliderData.Shape);
                            }

                            colliderData.Dirt = ColliderPropertyFlags.None;
                            continue;
                        }

                        // remove old
                        if (colliderData.Shape != null) _shape.RemoveChildShape(colliderData.Shape);
                        colliderData.Shape?.Dispose();

                        Debug.Assert(ShapeConvertors.ContainsKey(colliderComponent.GetType()));

                        // add new
                        colliderData.Shape = CreateShape(colliderComponent);

                        _shape.AddChildShape(colliderComponent.OffsetMatrix.ToBullet(), colliderData.Shape);

                        colliderData.Dirt = ColliderPropertyFlags.None;
                    }

                    _shape.CalculateLocalInertia(_rigidBody.Static ? 0 : _rigidBody.Mass, out var inertia);
                    _body.SetMassProps(_rigidBody.Static ? 0 : _rigidBody.Mass, inertia);

                    _body.UpdateInertiaTensor();

                    //Instance.rigidBodyWorld.CleanFromProxyPairs(_body);
                    //PhysicsSysten.Instance.rigidBodyWorld.World.AddRigidBody(_body);

                    _shapeDirty = false;
                }
                #endregion

                #region Motion
                if (_updateMotion)
                {
                    var worldMat = (Matrix4x4.CreateFromQuaternion(_transform.WorldRotation) * Matrix4x4.CreateTranslation(_transform.WorldPosition)).ToBullet();
                    
                    _motionState.WorldTransform = worldMat;
                    _body.WorldTransform = worldMat;
                    //                    sus fix
                    _shape.LocalScaling = _transform.Scale.ToBullet();

                    if (_rigidBody.Enabled) _ps.rigidBodyWorld.CleanFromProxyPairs(_body);

                    _updateMotion = false;
                }
                #endregion

                #region Body
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

                    // change of static state
                    if ((_dirtyProperties | RigidBodyPropertyFlags.Static) != 0)
                    {
                        _ps.rigidBodyWorld.World.RemoveRigidBody(_body);

                        if (!_rigidBody.Static)
                        {
                            _body.CollisionShape.CalculateLocalInertia(_rigidBody.Mass, out var inertia);
                            _body.ActivationState = ActivationState.DisableDeactivation;
                            _body.SetMassProps(_rigidBody.Mass, inertia);
                            _body.LinearFactor = _rigidBody.LinearFactor.ToBullet();
                            _body.AngularFactor =  _rigidBody.AngularFactor.ToBullet();
                            _body.UpdateInertiaTensor();
                            _body.ClearForces();
                            _body.WorldTransform = _transform.WorldTransformMatrix.ToBullet();
                        }
                        else
                        {
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
                            _body.WorldTransform = _transform.WorldTransformMatrix.ToBullet();
                        }

                        _ps.rigidBodyWorld.World.AddRigidBody(_body);
                    }

                    _dirtyProperties = RigidBodyPropertyFlags.None;
                }
                #endregion

                _body.Activate();
            }

            public void UpdataTransform()
            {
                if (!_rigidBody.Enabled) return;

                // TODO: consider interpolation

                if (_transform != null)
                {
                    _transform.OnTransformChanged -= Transform_OnTransformChanged;
                    _transform.SetWorldTranslationRotation(_motionState.WorldTransform.ToNumerics());
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

                body.ActivationState = ActivationState.DisableDeactivation;

                return body;
            }

            private static CollisionShape CreateShape(ColliderComponent collider)
            {
                var shape = ShapeConvertors[collider.GetType()](collider);

                return shape;
            }
        }

        public SystemThreadMode ThreadMode => SystemThreadMode.Sync;

        public static Vector4 ColliderRepresentationColor = new Vector4(1.0f, 0.4f, 0.0f, 1.0f);

        public (IRenderable Renderable, IList Objects) RenderData { get; private set; }

        List<ColliderComponent> _colliders = new List<ColliderComponent>();

        Dictionary<RigidBodyComponent, SyncedRBData> _pairs = new Dictionary<RigidBodyComponent, SyncedRBData>();
        Queue<SyncedRBData> _changed = new Queue<SyncedRBData>();

        internal RigidBodyWorld rigidBodyWorld;

        // note: 2d colliders are kinda not drawing...
        class RigidBodyWorldDebugRenderable : Renderable<RigidBodyWorld>
        {
            // TODO: add drawer context

            public override void Submit(RigidBodyWorld data)
            {
                for (int i = 0; i < data.World.CollisionObjectArray.Count; i++)
                {
                    data.World.DebugDrawObject(data.World.CollisionObjectArray[i].WorldTransform, data.World.CollisionObjectArray[i].CollisionShape, new Vector3(0, 1, 1).ToBullet());
                }
            }
        }
        List<RigidBodyWorld> _debugDrawData = new List<RigidBodyWorld>();

        public PhysicsSystem()
        {
            RenderData = (new RigidBodyWorldDebugRenderable(), _debugDrawData);
        }

        public void Init()
        {
            rigidBodyWorld = new RigidBodyWorld();

            PhysicsInterface.SetContext(rigidBodyWorld);

            _debugDrawData.Add(rigidBodyWorld);

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

            _debugDrawData.Remove(rigidBodyWorld);

            rigidBodyWorld.Dispose();

            //throw new NotImplementedException();
        }

        public void Update()
        {
            Profiler.BeginScope($"{nameof(PhysicsSystem)}.Update");

            while (_changed.TryDequeue(out SyncedRBData rbdata))
            {
                if (rbdata.Active) rbdata.Update();
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
            var data = new SyncedRBData(rbcomponent, this);
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
