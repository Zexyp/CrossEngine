﻿using System;

using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public abstract class ColliderComponent : Component
    {
        internal event Action<ColliderComponent, ColliderPropertyFlags> OnPropertyChanged;

        //public Vector3 PositionOffset = Vector3.Zero;
        //public Quaternion RotationOffset = Quaternion.Identity;

        private Matrix4x4 _localOffsets = Matrix4x4.Identity;

        [EditorDrag]
        public Vector3 PositionOffset
        {
            get => _localOffsets.Translation;
            set
            {
                _localOffsets.Translation = value;

                OnPropertyChanged?.Invoke(this, ColliderPropertyFlags.LocalOffsets);
            }
        }

        public Quaternion RotationOffset
        {
            get
            {
                Matrix4x4.Decompose(_localOffsets, out _, out var rotation, out _);
                return rotation;
            }
            set
            {
                var transtlation = _localOffsets.Translation;
                _localOffsets = Matrix4x4.CreateFromQuaternion(value);
                _localOffsets.Translation = transtlation;

                OnPropertyChanged?.Invoke(this, ColliderPropertyFlags.LocalOffsets);
            }
        }

        [EditorDrag]
        public Vector3 EulerRotationOffset
        {
            get
            {
                Matrix4x4Extension.EulerDecompose(out var _, out var rotation, out var _, _localOffsets);
                return rotation;
            }
            set
            {
                var transtlation = _localOffsets.Translation;
                _localOffsets = Matrix4x4.CreateFromQuaternion(QuaternionExtension.RotateXYZ(value));
                _localOffsets.Translation = transtlation;
                OnPropertyChanged?.Invoke(this, ColliderPropertyFlags.LocalOffsets);
            }
        }

        public Matrix4x4 OffsetMatrix
        {
            get => _localOffsets;
            set
            {
                if (_localOffsets == value) return;

                _localOffsets = value;
                OnPropertyChanged?.Invoke(this, ColliderPropertyFlags.LocalOffsets);
            }
        }

        protected void NotifyShapeChangedEvent()
        {
            OnPropertyChanged?.Invoke(this, ColliderPropertyFlags.Shape);
        }

        protected internal override void Attach(World world)
        {
            world.GetSystem<PhysicsSystem>().RegisterCollider(this);
        }

        protected internal override void Detach(World world)
        {
            world.GetSystem<PhysicsSystem>().UnregisterCollider(this);
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            info.AddValue(nameof(OffsetMatrix), OffsetMatrix);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            OffsetMatrix = info.GetValue(nameof(OffsetMatrix), OffsetMatrix);
        }
    }
}
