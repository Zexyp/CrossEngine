using System.Reflection;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CrossEngine.Assemblies;
using CrossEngine.Assets;
using CrossEngine.Utils.Editor;
using Microsoft.CodeAnalysis.CSharp;

namespace CrossEngine.Assets;

public class ScriptAsset : FileAsset
{
    public override bool Loaded => _exists;

    internal AssemblyLoadContext context;
    internal Assembly assembly;
    
    private bool _exists;
    internal string FullPath;
    
    public override Task Load(IAssetLoadContext context)
    {
        FullPath = context.GetFullPath(RelativePath);
        _exists = File.Exists(FullPath);
        return Task.CompletedTask;
    }

    public override Task Unload(IAssetLoadContext context)
    {
        _exists = false;
        return Task.CompletedTask;
    }
}