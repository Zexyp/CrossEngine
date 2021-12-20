using System;
using System.Collections.Generic;
using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Rendering.Lines;
using CrossEngine.Serialization;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering;

namespace CrossEngine.Entities.Components
{
    public class TransformComponent : Component
    {
        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;
        private Vector3 _worldPostiton;
        private Quaternion _worldRotation;
        private Vector3 _worldScale;
        private Matrix4x4 _worldTransformMatrix;
        private bool _dirty = true;
        private readonly List<TransformComponent> _children = new List<TransformComponent>();
        private TransformComponent _parent;
        private Vector3 _eulerAngles;

        public event Action<TransformComponent> OnTransformChanged;

        #region Properties
        [EditorVector3Value]
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    MarkForUpdate();
                }
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
                if (_rotation != value)
                {
                    _rotation = value;
                    _eulerAngles = QuaternionExtension.ToEuler(value);
                    MarkForUpdate();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 EulerAngles
        {
            get
            {
                return _eulerAngles;
            }
            set
            {
                if (_eulerAngles != value)
                {
                    _eulerAngles = value;
                    _rotation = QuaternionExtension.CreateFromXYZRotation(_eulerAngles.X, _eulerAngles.Y, _eulerAngles.Z);
                    MarkForUpdate();
                }
            }
        }
        [EditorVector3Value]
        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    MarkForUpdate();
                }
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

                return _worldPostiton;
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

        }

        #region Transform things
        private void MarkForUpdate()
        {
            if (_dirty)
            {
                return;
            }

            _dirty = true;
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].MarkForUpdate();
            }

            OnTransformChanged?.Invoke(this);
        }

        private void UpdateWorldTransform()
        {
            Matrix4x4 transform = LocalTransform();

            if (_parent == null)
            {
                _worldTransformMatrix = transform;
                _worldPostiton = _position;
                _worldRotation = _rotation;
                _worldScale = _scale;
            }
            else
            {
                _worldTransformMatrix = transform * _parent.WorldTransformMatrix;
                _worldPostiton = new Vector3(_worldTransformMatrix.M41, _worldTransformMatrix.M42, _worldTransformMatrix.M43);
                _worldRotation = _parent.WorldRotation * _rotation;
                _worldScale = _parent.WorldScale * _scale;
            }

            _dirty = false;
        }

        private Matrix4x4 LocalTransform()
        {
            return Matrix4x4.CreateScale(_scale) * Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_position);
        }

        public void SetTransformUseEuler(Matrix4x4 matrix)
        {
            Matrix4x4Extension.SimpleDecompose(out Vector3 translation, out Vector3 rotation, out Vector3 scale, matrix);

            _position = translation;
            EulerAngles = rotation;
            _scale = scale;

            //Log.Core.Debug("tr: {0}; rt: {1}; sc: {2}", _position, _eulerAngles, _scale);
            MarkForUpdate();
        }

        public void SetTransform(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

            _scale = scale;
            Rotation = rotation;
            _position = translation;

            MarkForUpdate();
        }

        public void SetWorldTransform(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

            WorldScale = scale;
            WorldRotation = rotation;
            WorldPosition = translation;

            MarkForUpdate();
        }

        public void SetTranslationRotation(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out _, out Quaternion rotation, out Vector3 translation);

            Rotation = rotation;
            _position = translation;

            MarkForUpdate();
        }

        public void SetWorldTranslationRotation(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out _, out Quaternion rotation, out Vector3 translation);

            WorldRotation = rotation;
            WorldPosition = translation;

            MarkForUpdate();
        }
        #endregion

        #region Hierarchy things
        public TransformComponent Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (this._parent == value)
                    return;

                if (_parent != null)
                    _parent._children.Remove(this);

                if (value != null)
                {
                    _parent = value;
                    _parent._children.Add(this);
                }
                else
                {
                    Matrix4x4 prevWorldTransform = WorldTransformMatrix;
                    _parent = value;
                    SetTransform(WorldTransformMatrix);
                }
            }
        }

        public override void OnAttach()
        {
            if (Entity.Parent != null)
            {
                this.Parent = Entity.Parent.Transform;
                //if(Entity.Parent.TryGetComponent(out TransformComponent tc))
                //    this.Parent = tc;
            }

            //var children = Entity.GetChildren();
            //for (int i = 0; i < children.Length; i++)
            //{
            //    if (children[i].TryGetComponent(out TransformComponent tc))
            //        tc.Parent = this;
            //}

            Entity.OnParentChanged += OnParentEntitySet;
            //Entity.OnChildAdded += Entity_OnChildAdded;
            //Entity.OnChildRemoved += Entity_OnChildRemoved;
        }

        public override void OnDetach()
        {
            this.Parent = null;

            this._children.Clear();

            Entity.OnParentChanged -= OnParentEntitySet;
            //Entity.OnChildAdded -= Entity_OnChildAdded;
            //Entity.OnChildRemoved -= Entity_OnChildRemoved;
        }

        private void OnParentEntitySet(Entity sender)
        {
            this.Parent = (sender.Parent != null) ? sender.Parent.Transform : null;

            if (this.Parent != null)
            {
                WorldPosition = Position;
                WorldRotation = Rotation;
                WorldScale = Scale;
            }
            else
            {
                Position = WorldPosition;
                Rotation = WorldRotation;
                Scale = WorldScale; 
            }
        }

        //private void Entity_OnChildAdded(Entity sender, Entity child)
        //{
        //    throw new System.NotImplementedException();
        //}
        //
        //private void Entity_OnChildRemoved(Entity arg1, Entity arg2)
        //{
        //    throw new System.NotImplementedException();
        //}
        #endregion


        public override void OnRender(RenderEvent re)
        {
            if (re is EditorDrawRenderEvent)
            {
                LineRenderer.DrawAxes(WorldTransformMatrix);
            }
        }

        public override void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Position", Position);
            info.AddValue("Rotation", Rotation);
            info.AddValue("Scale", Scale);
            //info.AddRefValue("Parent", Parent);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            Position = (Vector3)info.GetValue("Position", typeof(Vector3));
            Rotation = (Quaternion)info.GetValue("Rotation", typeof(Quaternion));
            Scale = (Vector3)info.GetValue("Scale", typeof(Vector3));
            // this will break things!
            // TODO: fix it
            //Parent = (TransformComponent)info.GetRefValue("Parent", typeof(TransformComponent), typeof(TransformComponent).GetMember(nameof(Parent))[0], this);
        }
    }
}
