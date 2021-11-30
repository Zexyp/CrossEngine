﻿using System.Numerics;

using CrossEngine.Scenes;
using CrossEngine.Rendering.Buffers;

namespace CrossEngine.Rendering.Passes
{
    public abstract class RenderPass
    {
        public abstract void Draw(Scene scene, Matrix4x4 viewProjectionMatrix, Framebuffer framebuffer = null);
    }
}
