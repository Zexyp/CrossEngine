using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Loaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Structs;
using CrossEngineEditor.Platform;
using CrossEngineEditor.Utils;
using ImGuiNET;
using static CrossEngine.Loaders.MeshLoader;

namespace CrossEngineEditor.Modals.Importer;

public class ObjImport : ImportModal
{
    [EditorAsset]
    public ShaderAsset Shader;
    [EditorDrag]
    public float Scale = 1;

    private string path = "";
    
    protected override void DrawModalContent()
    {
        ImGui.InputText("File", ref path, 512);
        ImGui.SameLine();
        if (ImGui.Button("..."))
            EditorApplication.Service.DialogFileOpen().ContinueWith(t => { var newpath = t.Result; if (newpath != null) path = newpath; });

        InspectDrawer.Inspect(this);
        
        base.DrawModalContent();
    }

    protected override void Process()
    {
        var scene = EditorApplication.Service.Context.Scene;
        var alist = EditorApplication.Service.Context.Assets;
        
        var alistdir = Path.Join(Path.GetDirectoryName(alist.RuntimeFilepath), alist.DirectoryOffset);
        
        var objRelative = Path.GetRelativePath(Path.Join(Path.GetDirectoryName(alist.RuntimeFilepath), alist.DirectoryOffset), path);
        var objFile = Path.Join(alistdir, objRelative);

        Dictionary<string, WavefrontMesh> meshes;
        string mtllib;
        using (var stream = EditorPlatformHelper.FileRead(objFile))
            meshes = MeshLoader.ParseObj(stream, out mtllib);

        Dictionary<string, MaterialAsset> matdict = new();
        if (mtllib != null)
        {
            var mtlRelative = Path.Join(Path.GetDirectoryName(objRelative), mtllib);
            var mtlFile = Path.Join(Path.GetDirectoryName(objFile), mtllib);
            var materials = MeshLoader.ParseMtl(EditorPlatformHelper.FileRead(mtlFile));
            var libAsset = new MtlMaterialLibraryAsset(materials) { RelativePath = mtlRelative, Shader = Shader };
            alist.Add(libAsset);
            foreach (var pair in materials)
            {
                if (pair.Value.texturePathDiffuse != null) alist.Add(new TextureAsset() { RelativePath = Path.Join(Path.GetDirectoryName(mtlRelative), pair.Value.texturePathDiffuse), Name = pair.Value.texturePathDiffuse });
                if (pair.Value.texturePathNormal != null) alist.Add(new TextureAsset() { RelativePath = Path.Join(Path.GetDirectoryName(mtlRelative), pair.Value.texturePathNormal), Name = pair.Value.texturePathNormal });
                if (pair.Value.texturePathSpecular != null) alist.Add(new TextureAsset() { RelativePath = Path.Join(Path.GetDirectoryName(mtlRelative), pair.Value.texturePathSpecular), Name = pair.Value.texturePathSpecular });
                if (pair.Value.texturePathSpecularHighlight != null) alist.Add(new TextureAsset() { RelativePath = Path.Join(Path.GetDirectoryName(mtlRelative), pair.Value.texturePathSpecularHighlight), Name = pair.Value.texturePathSpecularHighlight });

                MtlMaterialReferenceAsset material = new MtlMaterialReferenceAsset() { MaterialName = pair.Key, Parent = libAsset, Name = pair.Key };
                alist.Add(material);
                matdict.Add(pair.Key, material);
            }
        }

        ObjModelAsset objAsset = new ObjModelAsset(meshes) { RelativePath = objRelative };
        alist.Add(objAsset);
        foreach (var pair in objAsset.Meshes)
        {
            ObjMeshReferenceAsset mesh = new ObjMeshReferenceAsset() { MeshName = pair.Key, Parent = objAsset, Name = pair.Key };
            alist.Add(mesh);

            var entity = scene.CreateEntity();
            entity.GetComponent<TransformComponent>().Scale = new(Scale);
            entity.Name = pair.Key;
            
            var renderer = entity.AddComponent<MeshRendererComponent>();
            renderer.Mesh = mesh;
            renderer.Material = meshes[pair.Key].MaterialName != null ? matdict[meshes[pair.Key].MaterialName] : null;
        }
    }
}

public class AtlasImport : ImportModal
{
    private Vector2 imageSize;
    private Vector2 tileSize;
    private Vector4 margin;
    private int numTiles;
    
    protected override void DrawModalContent()
    {
        ImGui.InputFloat2("Image Size", ref imageSize);
        ImGui.InputInt("Count", ref numTiles);
        ImGui.DragFloat2("Tile Size", ref tileSize);
        ImGui.DragFloat4("Margin", ref margin);
        
        base.DrawModalContent();
    }

    protected override void Process()
    {
        var alist = EditorApplication.Service.Context.Assets;

        var atlas = new TextureAtlasAsset() { TextureOffsets = TextureAtlas.CreateOffsets(imageSize, tileSize, numTiles, margin) };
        alist.Add(atlas);
        alist.LoadAsset(atlas);
    }
}