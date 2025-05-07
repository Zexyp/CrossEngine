using System.IO;
using System.Numerics;
using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Loaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Utils.Structs;
using CrossEngineEditor.Platform;
using ImGuiNET;

namespace CrossEngineEditor.Modals.Importer;

public class ObjImport : ImportModal
{
    private string path = "";
    
    protected override void DrawModalContent()
    {
        ImGui.InputText("File", ref path, 512);
        ImGui.SameLine();
        if (ImGui.Button("..."))
            EditorApplication.Service.DialogFileOpen().ContinueWith(t => { var newpath = t.Result; if (newpath != null) path = newpath; });
        
        base.DrawModalContent();
    }

    protected override void Process()
    {
        var scene = EditorApplication.Service.Context.Scene;
        var alist = EditorApplication.Service.Context.Assets;
        var realative = Path.GetRelativePath(Path.Join(Path.GetDirectoryName(alist.RuntimeFilepath), alist.DirectoryOffset), path);
        using var stream = EditorPlatformHelper.FileRead(path);
        var meshes = MeshLoader.ParseObj(stream);
        foreach (var pair in meshes)
        {
            ObjMeshAsset mesh = new ObjMeshAsset() {RelativePath = realative, MeshName = pair.Key, Mesh = pair.Value};
            alist.Add(mesh);
            
            var entity = scene.CreateEntity();
            entity.Name = pair.Key;
            entity.AddComponent<MeshRendererComponent>().Mesh = mesh;
        }
    }
}

public class MtlImport : ImportModal
{
    private string path = "";
    
    protected override void DrawModalContent()
    {
        ImGui.InputText("File", ref path, 512);
        ImGui.SameLine();
        if (ImGui.Button("..."))
            EditorApplication.Service.DialogFileOpen().ContinueWith(t => { var newpath = t.Result; if (newpath != null) path = newpath; });
        
        base.DrawModalContent();
    }

    protected override void Process()
    {
        var alist = EditorApplication.Service.Context.Assets;
        var realative = Path.GetRelativePath(Path.Join(Path.GetDirectoryName(alist.RuntimeFilepath), alist.DirectoryOffset), path);
        using var stream = EditorPlatformHelper.FileRead(path);
        var materials = MeshLoader.ParseMtl(stream);
        foreach (var pair in materials)
        {
            MtlMaterialAsset material = new MtlMaterialAsset() {RelativePath = realative, MaterialName = pair.Key, Material = pair.Value};
            alist.Add(material);
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