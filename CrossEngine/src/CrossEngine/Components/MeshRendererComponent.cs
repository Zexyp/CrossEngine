using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components
{
    public class MeshRendererComponent : RendererComponent, IMeshRenderData
    {
        [EditorNullable]
        [SerializeInclude]
        [EditorAsset]
        public MaterialAsset Material;

        [EditorNullable] [SerializeInclude] [EditorAsset]
        public MeshAsset Mesh;

        IMesh IMeshRenderData.Mesh => Mesh?.Mesh;
        IMaterial IMeshRenderData.Material => Material?.Material;
    }
}
