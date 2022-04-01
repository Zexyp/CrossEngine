﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.ECS;
using CrossEngine.ComponentSystems;

namespace CrossEngine.Components
{
    [AllowSingleComponentPerEntity]
    public class TransformComponent : Component
    {
        // TODO: consider adding rotation mode

        // local
        private Vector3 _translation = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;

        // world
        private Vector3 _worldTranslation;
        private Quaternion _worldRotation;
        private Vector3 _worldScale;

        // matrices
        //private Matrix4x4 _transformMatrix;
        private Matrix4x4 _worldTransformMatrix;

        bool _dirty = true;

        private readonly List<TransformComponent> _children = new List<TransformComponent>();
        private TransformComponent _parent;
        
        // util
        private Vector3 _eulerAngles;

        // event
        public event Action<TransformComponent> OnTransformChanged;

        #region Properties
        [EditorDrag]
        public Vector3 Position
        {
            get
            {
                return _translation;
            }
            set
            {
                if (_translation == value) return;

                _translation = value;
                MarkForUpdate();
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                if (_rotation == value) return;

                _rotation = value;
                _eulerAngles = QuaternionExtension.ToEuler(value);
                MarkForUpdate();
            }
        }
        [EditorDrag]
        public Vector3 EulerRotation
        {
            get
            {
                return _eulerAngles;
            }
            set
            {
                if (_eulerAngles == value) return;
                
                _eulerAngles = value;
                _rotation = QuaternionExtension.CreateFromXYZRotation(_eulerAngles.X, _eulerAngles.Y, _eulerAngles.Z);
                MarkForUpdate();
            }
        }
        [EditorDrag]
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (_scale == value) return;

                _scale = value;
                MarkForUpdate();
            }
        }

        public Matrix4x4 WorldTransformMatrix
        {
            get
            {
                if (_dirty)
                {
                    UpdateWorldTransform();
                }
                return _worldTransformMatrix;
            }
        }

        public Vector3 WorldPosition
        {
            get
            {
                if (_dirty)
                {
                    UpdateWorldTransform();
                }

                return _worldTranslation;
            }
            set
            {
                if (_parent == null)
                {
                    Position = value;
                }
                else
                {
                    Position = Vector3.Transform(value, Matrix4x4Extension.Invert(_parent.WorldTransformMatrix));
                }
            }
        }
        public Quaternion WorldRotation
        {
            get
            {
                if (_dirty)
                {
                    UpdateWorldTransform();
                }

                return _worldRotation;
            }
            set
            {
                if (_parent == null)
                {
                    Rotation = value;
                }
                else
                {
                    Rotation = Quaternion.Inverse(_parent.WorldRotation) * value;
                }
            }
        }
        public Vector3 WorldScale
        {
            get
            {
                if (_dirty)
                {
                    UpdateWorldTransform();
                }

                return _worldScale;
            }
            set
            {
                if (_parent == null)
                {
                    Scale = value;
                }
                else
                {
                    Scale = value / _parent.WorldScale;
                }
            }
        }
        #endregion

        public TransformComponent()
        {
            Children = _children.AsReadOnly();
        }

        #region Transform things
        private void MarkForUpdate()
        {
            //// ! this might break things by not calling OnTransformChanged every time
            //if (_dirty)
            //{
            //    return;
            //}

            _dirty = true;
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].MarkForUpdate();
            }

            OnTransformChanged?.Invoke(this);
        }

        private void UpdateWorldTransform()
        {
            Matrix4x4 transform = CalculateLocalTransform();

            if (_parent == null)
            {
                _worldTransformMatrix = transform;
                _worldTranslation = _translation;
                _worldRotation = _rotation;
                _worldScale = _scale;
            }
            else
            {
                _worldTransformMatrix = transform * _parent.WorldTransformMatrix;
                _worldTranslation = new Vector3(_worldTransformMatrix.M41, _worldTransformMatrix.M42, _worldTransformMatrix.M43);
                _worldRotation = _parent.WorldRotation * _rotation;
                _worldScale = _parent.WorldScale * _scale;
            }

            _dirty = false;
        }

        private Matrix4x4 CalculateLocalTransform()
        {
            return Matrix4x4.CreateScale(_scale) * Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_translation);
        }

        public void SetTransformUseEuler(Matrix4x4 matrix)
        {
            Matrix4x4Extension.EulerDecompose(out Vector3 translation, out Vector3 rotation, out Vector3 scale, matrix);

            _translation = translation;
            EulerRotation = rotation; // ! TODO: this causes two dirty updates
            _scale = scale;

            //Log.Core.Debug("tr: {0}; rt: {1}; sc: {2}", _position, _eulerAngles, _scale);
            MarkForUpdate();
        }

        public void SetTransform(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

            _scale = scale;
            Rotation = rotation; // ! TODO: this causes two dirty updates
            _translation = translation;

            MarkForUpdate();
        }

        public void SetWorldTransform(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

            // ! TODO: also causes multiple dirty updates
            WorldScale = scale;
            WorldRotation = rotation;
            WorldPosition = translation;

            MarkForUpdate();
        }

        public void SetTranslationRotation(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out _, out Quaternion rotation, out Vector3 translation);

            Rotation = rotation; // ! TODO: this causes two dirty updates
            _translation = translation;

            MarkForUpdate();
        }

        public void SetWorldTranslationRotation(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out _, out Quaternion rotation, out Vector3 translation);

            // ! TODO: also causes multiple dirty updates
            WorldRotation = rotation;
            WorldPosition = translation;

            MarkForUpdate();
        }

        #region Utils
        public Vector3 GetRightDirection()
        {
            return Vector3.Transform(Vector3.UnitX, Rotation);
        }
        public Vector3 GetUpDirection()
        {
            return Vector3.Transform(Vector3.UnitY, Rotation);
        }
        public Vector3 GetForwardDirection()
        {
            return Vector3.Transform(Vector3.UnitZ, Rotation);
        }

        public Vector3 GetWorldRightDirection()
        {
            return Vector3.Transform(Vector3.UnitX, WorldRotation);
        }
        public Vector3 GetWorldUpDirection()
        {
            return Vector3.Transform(Vector3.UnitY, WorldRotation);
        }
        public Vector3 GetWorldForwardDirection()
        {
            return Vector3.Transform(Vector3.UnitZ, WorldRotation);
        }
        #endregion
        #endregion

        #region Hierarchy things
        internal TransformComponent Parent
        {
            get => _parent;
            private set
            {
                if (this._parent == value) return;

                // if no parent, remove yourself from parent's child collection
                if (this._parent != null) this._parent._children.Remove(this);
                this._parent = value;
                if (this._parent != null) this._parent._children.Add(this);

                MarkForUpdate();
                OnParentChanged?.Invoke(this);
            }
        }
        internal readonly ReadOnlyCollection<TransformComponent> Children;

        internal Action<TransformComponent> OnParentChanged;

        private void Entity_OnParentChanged(Entity sender)
        {
            Debug.Assert(Entity == sender);

            this.Parent = sender.Parent?.GetComponent<TransformComponent>();

            // we don't want this
            //if (this.Parent != null)
            //{
            //    WorldPosition = Position;
            //    WorldRotation = Rotation;
            //    WorldScale = Scale;
            //}
            //else
            //{
            //    Position = WorldPosition;
            //    Rotation = WorldRotation;
            //    Scale = WorldScale; 
            //}
        }

        //private void Entity_OnChildAdded(Entity sender, Entity child)
        //{
        //    Debug.Assert(Entity == sender);
        //
        //    if (child.TryGetComponent(out TransformComponent c))
        //    {
        //        c.Parent = this;
        //    }
        //}
        //
        //private void Entity_OnChildRemoved(Entity sender, Entity child)
        //{
        //    Debug.Assert(Entity == sender);
        //
        //    if (child.TryGetComponent(out TransformComponent c))
        //    {
        //        c.Parent = null;
        //    }
        //}
        #endregion

        public override void Attach()
        {
            Debug.Assert(Entity.GetComponent<TransformComponent>() != null);

            this.Parent = Entity.Parent?.GetComponent<TransformComponent>();
            // ! should be handled by Entity_OnChildAdded
            // set all children's parent to this (insert yourself)
            for (int i = 0; i < Entity.Children.Count; i++)
            {
                if (Entity.Children[i].TryGetComponent(out TransformComponent c))
                    c.Parent = this;
            }

            Entity.OnParentChanged += Entity_OnParentChanged;
            //Entity.OnChildAdded += Entity_OnChildAdded;
            //Entity.OnChildRemoved += Entity_OnChildRemoved;

            TransformSystem.Instance.Register(this);
        }

        public override void Detach()
        {
            Entity.OnParentChanged -= Entity_OnParentChanged;
            //Entity.OnChildAdded -= Entity_OnChildAdded;
            //Entity.OnChildRemoved -= Entity_OnChildRemoved;

            // ! should be handled by Entity_OnChildRemoved
            // remove the connection
            for (int i = 0; i < this._children.Count; i++)
            {
                this._children[i].Parent = null;
            }
            this._children.Clear();

            this.Parent = null;

            TransformSystem.Instance.Unregister(this);
        }

        public override void Update()
        {
            if (_dirty)
            {
                UpdateWorldTransform();
            }
        }

        public override object Clone()
        {
            var trans = new TransformComponent();
            // questionable
            trans.Enabled = this.Enabled;

            trans.Position = this.Position;
            trans.Scale = this.Scale;
            trans.Rotation = this.Rotation;

            return trans;
        }

        //public override void OnRender(RenderEvent re)
        //{
        //    if (re is EditorDrawRenderEvent)
        //    {
        //        LineRenderer.DrawAxes(WorldTransformMatrix);
        //    }
        //}

        //public override void OnSerialize(SerializationInfo info)
        //{
        //    info.AddValue("Position", Position);
        //    info.AddValue("Rotation", Rotation);
        //    info.AddValue("Scale", Scale);
        //    //info.AddRefValue("Parent", Parent);
        //}
        //
        //public override void OnDeserialize(SerializationInfo info)
        //{
        //    Position = (Vector3)info.GetValue("Position", typeof(Vector3));
        //    Rotation = (Quaternion)info.GetValue("Rotation", typeof(Quaternion));
        //    Scale = (Vector3)info.GetValue("Scale", typeof(Vector3));
        //    // this will break things!
        //    // TODO: fix it
        //    //Parent = (TransformComponent)info.GetRefValue("Parent", typeof(TransformComponent), typeof(TransformComponent).GetMember(nameof(Parent))[0], this);
        //}
    }
}
