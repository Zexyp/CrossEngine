using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    internal interface IAsset
    {
        Guid Id { get; }
        void Load();
        void Unload();
    }
}
