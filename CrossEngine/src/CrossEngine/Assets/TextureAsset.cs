using CrossEngine.Rendering.Textures;

namespace CrossEngine.Assets
{
    public class TextureAsset : AssetInfo
    {
        public Ref<Texture> Texture;

        public TextureAsset()
        {

        }

        public override void Load(IPathProvider pathProvider = null)
        {
            Texture = TextureLoader.LoadTexture(pathProvider?.GetActualPath(RelativePath) ?? RelativePath);
            Loaded = true;
        }

        public override void Unload()
        {
            Texture.Value.Dispose();
            Texture = null;
            Loaded = false;
        }
    }
}
