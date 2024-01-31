using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public abstract class Loader
    {
        public abstract void Init();
        public abstract void Shutdown();
    }
}
