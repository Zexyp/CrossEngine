using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Geometry;
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

        [EditorNullable]
        [SerializeInclude]
        [EditorAsset]
        public MeshAsset Mesh
        {
            get => _mesh;
            set
            {
                _mesh = value;
                MeshChanged?.Invoke(this);
            }
        }
        
        internal event Action<MeshRendererComponent> MeshChanged;
            
        private MeshAsset _mesh;

        MeshRenderer IMeshRenderData.Renderer { get; set; }
        IMaterial IMeshRenderData.Material => Material?.Material;
    }
}
