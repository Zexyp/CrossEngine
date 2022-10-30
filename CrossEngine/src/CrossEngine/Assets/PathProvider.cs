using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public enum AssetType
    {
        None = 0,
        Texture,
    }

    internal class DefaultPathProvider : IPathProvider
    {
        public string GetActualPath(string relativePath, AssetType type = AssetType.None) => relativePath;
    }

    public interface IPathProvider
    {
        string GetActualPath(string relativePath, AssetType type = AssetType.None);

        private static IPathProvider _default = new DefaultPathProvider();
        public static IPathProvider Default => _default;
    }
}
