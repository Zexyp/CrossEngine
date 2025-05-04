using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Utils;
using CrossEngine.Utils.IO;
using CrossEngineEditor.Platform;

namespace CrossEngineEditor;

public class EditorProject
{
    public string Filepath;

    public void Save(IEditorContext context, string filepath, Action<IniFile> shim = null)
    {
        IniFile ini = new IniFile();
        Filepath = filepath;

        var alistfile = context.Assets?.RuntimeFilepath;
        ini["context"].Write("Assets", alistfile ?? "null");
        var sceneId = EditorApplication.Service.GetCurrentSceneAsset()?.Id;
        ini["context"].Write("Scene", sceneId != null ? sceneId.ToString() : "null");
        var entityId = context.ActiveEntity?.Id;
        ini["context"].Write("ActiveEntity", entityId != null ? entityId.ToString() : "null");
        
        shim?.Invoke(ini);
        
        using (Stream stream = EditorPlatformHelper.FileCreate(filepath))
            IniFile.Dump(ini, stream);
    }

    public Task Load(IEditorContext context, string filepath, Action<IniFile> shim = null)
    {
        IniFile ini;
        using (Stream stream = File.OpenRead(filepath))
            ini = IniFile.Load(stream);
        Filepath = filepath;

        // parse
        var alistfile = ini["context"].ReadString("Assets");
        var sceneId = ini["context"].ReadString("Scene");
        var entityId = ini["context"].ReadString("ActiveEntity");
        
        var task = Task.CompletedTask;
        AssetList alist;
        task = task.ContinueWith(t => AssetManager.ReadFile(alistfile)).Unwrap()
            .ContinueWith(t => context.SetAssets(t.Result)).Unwrap();
        task = task.ContinueWith(t =>
        {
            if (context.Assets != null && Guid.TryParse(sceneId, out var scnguid))
                return context.SetScene(context.Assets.Get<SceneAsset>(scnguid)?.Scene);
            return Task.CompletedTask;
        }).Unwrap();

        task = task.ContinueWith(t =>
        {
            if (context.Scene != null && int.TryParse(entityId, out var entguid))
                context.ActiveEntity = context.Scene.GetEntity(entguid);
        });
        
        shim?.Invoke(ini);

        return task;
    }
}