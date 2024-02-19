using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Utils;
using CrossEngine.Ecs;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    interface ITransform
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        Vector3 Scale { get; }
    }

    [AllowSinglePerEntity]
    public class TransformComponent : Component
    {
        // TODO: consider adding rotation mode
        // TODO: override to global coordinates will be needed when doin fizyks / or just disallow parents of RB children to be RB

        // event
        public event Action<TransformComponent> TransformChanged;

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
                _dirtyLocal = true;
                MarkForUpdate();
            }
        }
        //[EditorDrag]
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
                _dirtyLocal = true;

                _eulerAnglesCache = QuaternionExtension.ToEuler(value);
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
                _dirtyLocal = true;
                MarkForUpdate();
            }
        }
        [EditorDrag]
        public Vector3 EulerRotation
        {
            get
            {
                return _eulerAnglesCache;
            }
            set
            {
                if (_eulerAnglesCache == value) return;

                _eulerAnglesCache = value;
                _eulerAnglesCache = new Vector3(_eulerAnglesCache.X % (MathF.PI * 2), _eulerAnglesCache.Y % (MathF.PI * 2), _eulerAnglesCache.Z % (MathF.PI * 2));
                
                _rotation = QuaternionExtension.RotateXYZ(_eulerAnglesCache);
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
        public Matrix4x4 TransformMatrix
        {
            get
            {
                if (_dirtyLocal)
                {
                    UpdateTransform();
                }
                return _transformMatrix;
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

        #region Private Fields
        // local
        private Vector3 _translation = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;
        private Vector3 _eulerAnglesCache = Vector3.Zero;

        // world
        // they get cached
        private Vector3 _worldTranslation;
        private Quaternion _worldRotation;
        private Vector3 _worldScale;

        // matrices
        private Matrix4x4 _transformMatrix;
        private Matrix4x4 _worldTransformMatrix;

        bool _dirty = true;
        bool _dirtyLocal = true;

        // util
        //private Vector3 _eulerAngles;
        #endregion

        #region Transform things
        //public void SetTransformUseEuler(Matrix4x4 matrix)
        //{
        //    Matrix4x4Extension.EulerDecompose(out Vector3 translation, out Vector3 rotation, out Vector3 scale, matrix);
        //
        //    _translation = translation;
        //    EulerRotation = rotation; // ! TODO: this causes two dirty updates
        //    _scale = scale;
        //
        //    //Log.Core.Debug("tr: {0}; rt: {1}; sc: {2}", _position, _eulerAngles, _scale);
        //    MarkForUpdate();
        //}

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

        private void MarkForUpdate()
        {
            //// ! this might break things by not calling OnTransformChanged every time
            //if (_dirty)
            //{
            //    return;
            //}

            _dirty = true;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].MarkForUpdate();
            }

            TransformChanged?.Invoke(this);
        }

        private void UpdateWorldTransform()
        {
            if (_dirtyLocal)
                UpdateTransform();

            if (_parent == null)
            {
                _worldTransformMatrix = _transformMatrix;
                _worldTranslation = _translation;
                _worldRotation = _rotation;
                _worldScale = _scale;
            }
            else
            {
                _worldTransformMatrix = _transformMatrix * _parent.WorldTransformMatrix;
                _worldTranslation = new Vector3(_worldTransformMatrix.M41, _worldTransformMatrix.M42, _worldTransformMatrix.M43);
                _worldRotation = _parent.WorldRotation * _rotation;
                _worldScale = _parent.WorldScale * _scale;
            }

            _dirty = false;
        }

        private void UpdateTransform()
        {
            _transformMatrix = Matrix4x4.CreateScale(_scale) * Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_translation);
        }
        #endregion

        #region Hierarchy things
        internal readonly List<TransformComponent> Children = new List<TransformComponent>();
        private TransformComponent _parent;

        internal TransformComponent Parent
        {
            get => _parent;
            set
            {
                if (this._parent == value) return;

                // if no parent, remove yourself from parent's child collection
                if (this._parent != null) this._parent.Children.Remove(this);
                this._parent = value;
                if (this._parent != null) this._parent.Children.Add(this);

                MarkForUpdate();
                ParentChanged?.Invoke(this);
            }
        }
        
        internal event Action<TransformComponent> ParentChanged;
        #endregion

        internal void Update()
        {
            if (_dirty)
            {
                UpdateWorldTransform();
            }

            if (_dirtyLocal)
            {
                UpdateTransform();
            }
        }

        public override object Clone()
        {
            var trans = new TransformComponent();

            trans.Position = this.Position;
            trans.Scale = this.Scale;
            trans.Rotation = this.Rotation;

            return trans;
        }

        protected internal override void OnSerialize(SerializationInfo info)
        {
            info.AddValue(nameof(Position), Position);
            info.AddValue(nameof(Rotation), Rotation);
            info.AddValue(nameof(Scale), Scale);
        }
        
        protected internal override void OnDeserialize(SerializationInfo info)
        {
            Position = info.GetValue(nameof(Position), Vector3.Zero);
            Rotation = info.GetValue(nameof(Rotation), Quaternion.Identity);
            Scale = info.GetValue(nameof(Scale), Vector3.One);
        }
    }
}