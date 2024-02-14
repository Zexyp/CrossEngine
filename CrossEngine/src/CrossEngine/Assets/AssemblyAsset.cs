using CrossEngine.Assemblies;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class AssemblyAsset : Asset
    {
        public override bool Loaded => Assembly != null;

        public Assembly Assembly;

        [EditorString]
        public string RelativePath;

        AssemblyLoadContext loadContext;

        public override async void Load(IAssetLoadContext context)
        {
            loadContext = AssemblyManager.Load(await context.OpenRelativeStream(RelativePath), out Assembly);
        }

        public override void Unload(IAssetLoadContext context)
        {
            AssemblyManager.Unload(loadContext);
        }
    }
}
