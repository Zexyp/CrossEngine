using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Utils;
using CrossEngine.Utils.IO;

namespace CrossEngineEditor;

public class EditorProject
{
    public string Filepath;

    public void Save(IEditorContext context, string filepath, Action<IniFile> shim = null)
    {
        IniFile ini = new IniFile();
        Filepath = filepath;

        var alistfile = context.Assets?.RuntimeFilepath;
        ini["workspace"].Write("Assets", alistfile ?? "null");
        var sceneId = EditorApplication.Service.GetCurrentSceneAsset()?.Id;
        ini["workspace"].Write("Scene", sceneId != null ? sceneId.ToString() : "null");
        var entityId = context.ActiveEntity?.Id;
        ini["workspace"].Write("ActiveEntity", entityId != null ? entityId.ToString() : "null");
        
        shim?.Invoke(ini);
        
        using (Stream stream = File.Create(filepath))
            IniFile.Dump(ini, stream);
    }

    public void Load(IEditorContext context, string filepath, Action<IniFile> shim = null)
    {
        IniFile ini;
        using (Stream stream = File.OpenRead(filepath))
            ini = IniFile.Load(stream);
        Filepath = filepath;

        var alistfile = ini["workspace"].ReadString("Assets");
        if (File.Exists(alistfile))
        {
            AssetManager.ReadFile(alistfile).ContinueWith(t =>
            {
                AssetList alist = t.Result;
                AssetManager.Bind(alist);
                AssetManager.Load(alist).ContinueWith(t2 =>
                {
                    context.Assets = alist;

                    var sceneId = ini["workspace"].ReadString("Scene");
                    if (context.Assets != null && Guid.TryParse(sceneId, out var scnguid))
                        context.Scene = context.Assets.Get<SceneAsset>(scnguid)?.Scene;
                    var entityId = ini["workspace"].ReadString("ActiveEntity");
                    if (context.Scene != null && int.TryParse(entityId, out var entguid))
                        context.ActiveEntity = context.Scene.GetEntity(entguid);
                });
            });
        }
        
        shim?.Invoke(ini);
    }
}