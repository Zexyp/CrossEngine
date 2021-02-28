using System;

using System.Collections.Generic;
using System.Numerics;

using CrossEngine.Rendering.Shading;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering
{
    public class Material
    {
        public Shader shader = null;

        public Material(Shader shader)
        {
            this.shader = shader;
        }

        public virtual void Bind()
        {
        }
    }
}
