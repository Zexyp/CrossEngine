using CrossEngine.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Loaders;

namespace CrossEngine.Assets
{
    public class ShaderAsset : FileAsset
    {
        public WeakReference<ShaderProgram> Shader;

        public override bool Loaded => Shader != null;

        public override async Task Load(IAssetLoadContext context)
        {
            using (var stream = await context.OpenRelativeStream(RelativePath))
                Shader = ShaderPreprocessor.CreateProgramFromStream(stream);
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            ShaderPreprocessor.Free(Shader);
            Shader = null;
        }
    }
}
