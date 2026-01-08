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
        EditorService.Log.Info("saving project");
        
        IniFile ini = new IniFile();
        Filepath = filepath;

        var alistfile = context.Assets != null ? Path.GetRelativePath(Path.GetDirectoryName(filepath), context.Assets.RuntimeFilepath) : null;
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
        EditorService.Log.Info("loading project");
        
        IniFile ini;
        using (Stream stream = File.OpenRead(filepath))
            ini = IniFile.Load(stream);
        Filepath = filepath;

        // parse
        var alistfile = ini["context"].ReadString("Assets");
        var sceneId = ini["context"].ReadString("Scene");
        var entityId = ini["context"].ReadString("ActiveEntity");
        
        var task = Task.CompletedTask;
        if (alistfile != "null")
        {
            task = task.ContinueWith(t =>
                {
                    EditorService.Log.Trace("reading alist");
                    return AssetManager.ReadFile(Path.Join(Path.GetDirectoryName(filepath), alistfile));
                }).Unwrap()
                .ContinueWith(t =>
                {
                    EditorService.Log.Trace("setting alist");
                    return context.SetAssets(t.Result);
                }).Unwrap();
        }
        task = task.ContinueWith(t =>
        {
            EditorService.Log.Trace("checking for scene");
            if (context.Assets != null && Guid.TryParse(sceneId, out var scnguid))
                return context.SetScene(context.Assets.Get<SceneAsset>(scnguid)?.Scene);
            return Task.CompletedTask;
        }).Unwrap();

        task = task.ContinueWith(t =>
        {
            EditorService.Log.Trace("checking for entity");
            if (context.Scene != null && int.TryParse(entityId, out var entguid))
                context.ActiveEntity = context.Scene.GetEntity(entguid);
        });
        
        shim?.Invoke(ini);

        return task;
    }
}