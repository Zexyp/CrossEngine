﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class CameraComponent : Component
    {
        public Camera Camera;

        private bool _primary;
        [EditorValue]
        public bool Primary
        {
            get
            {
                if (_boundTo != null)
                    return this == _boundTo.Primary;
                return _primary;
            }
            set
            {
                if (_boundTo != null)
                    _boundTo.Primary = value ? this : null;
                _primary = value;
            }
        }

        private RendererSystem _boundTo;
        private TransformComponent _transformBinding;
        private TransformComponent TransformBinding
        {
            get => _transformBinding;
            set
            {
                if (_transformBinding == value) return;

                if (_transformBinding != null) _transformBinding.OnTransformChanged -= UpdateView;
                _transformBinding = value;
                if (_transformBinding != null) _transformBinding.OnTransformChanged += UpdateView;
                UpdateView(_transformBinding);
            }
        }

        public Matrix4x4? ViewMatrix
        {
            get
            {
                if (Entity.TryGetComponent(out TransformComponent transformComp))
                    return Matrix4x4.CreateTranslation(-transformComp.WorldPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(transformComp.WorldRotation));
                return null;
            }
        }
        
        public CameraComponent()
        {
            Camera = new Camera();
        }

        protected internal override void Attach(World world)
        {
            world.GetSystem<RendererSystem>().RegisterCamera(this);
            _boundTo = world.GetSystem<RendererSystem>();

            Entity.OnComponentAdded += ComponentChange;
            Entity.OnComponentRemoved += ComponentChange;
        }

        protected internal override void Detach(World world)
        {
            Primary = Primary;
            world.GetSystem<RendererSystem>().UnregisterCamera(this);
            _boundTo = null;

            Entity.OnComponentAdded -= ComponentChange;
            Entity.OnComponentRemoved -= ComponentChange;
            TransformBinding = null;
        }

        private void ComponentChange(Entity sender, Component val)
        {
            TransformBinding = Entity.Transform;
        }

        private void UpdateView(TransformComponent component)
        {
            Camera.ViewMatrix = ViewMatrix ?? Matrix4x4.Identity;
        }

        protected override Component CreateClone()
        {
            throw new NotImplementedException();
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            info.AddValue("Primary", Primary);
            info.AddValue("Camera", Camera);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            Primary = info.GetValue("Primary", Primary);
            Camera = info.GetValue("Camera", Camera);
        }
    }
}
