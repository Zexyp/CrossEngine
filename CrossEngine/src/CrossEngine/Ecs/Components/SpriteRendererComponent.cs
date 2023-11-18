using CrossEngine.Ecs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Renderables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Components
{
    internal class SpriteRendererComponent : Component, ISpriteRenderData
    {
        public Vector4 Color { get; set; } = Vector4.One;

        Matrix4x4 IObjectRenderData.Transform => Entity.Transform?.WorldTransformMatrix ?? Matrix4x4.Identity;
    }
}
