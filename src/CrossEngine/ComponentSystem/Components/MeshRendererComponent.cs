using System;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Shading;

namespace CrossEngine.ComponentSystem.Components
{
    public class MeshRendererComponent : Component
    {
        public Material material; // = new Material(AssetManager.Shaders.GetShader("shaders/vertex/model.shader", "shaders/fragment/texture.shader"));
        public Mesh mesh;

        public MeshRendererComponent(Mesh mesh, Material material)
        {
            this.mesh = mesh;
            this.material = material;
        }

        public override void OnRender()
        {
            MeshRenderer.DrawMesh(mesh, ActiveCamera.camera, entity.transform, material);
        }
    }
}
