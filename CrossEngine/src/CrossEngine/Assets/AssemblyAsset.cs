using CrossEngine.Assemblies;
using CrossEngine.Serialization;
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

        public override async Task Load(IAssetLoadContext context)
        {
            (loadContext, Assembly) = await AssemblyManager.Load(context.GetFullPath(RelativePath));
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Assembly = null;
            AssemblyManager.Unload(loadContext);
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(RelativePath), RelativePath);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            RelativePath = info.GetValue(nameof(RelativePath), RelativePath);
        }
    }
}
