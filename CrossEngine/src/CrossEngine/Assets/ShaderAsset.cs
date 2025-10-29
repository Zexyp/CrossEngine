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
        public ShaderProgram Shader;

        public override bool Loaded => Shader != null;

        public override async Task Load(IAssetLoadContext context)
        {
            var stream = RelativePath?.StartsWith("internal:") == true
                ? ShaderPreprocessor.GetInternalShaderSource(RelativePath)
                : await context.OpenRelativeStream(RelativePath);
            context.Graphics.Commands.Submit(() =>
            {
                Shader = ShaderPreprocessor.CreateProgramFromStream(stream);
            });
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Shader.Dispose();
            Shader = null;
        }
    }
}
