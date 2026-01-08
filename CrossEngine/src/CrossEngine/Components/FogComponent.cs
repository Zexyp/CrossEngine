using CrossEngine.Ecs;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using System;
using System.Numerics;

namespace CrossEngine.Components
{
    public class FogComponent : Component
    {
        [SerializeInclude]
        [EditorDrag]
        public float Start;
        [SerializeInclude]
        [EditorDrag]
        public float Density;
        [SerializeInclude]
        [EditorColor]
        public Vector4 Color;
    }
}
