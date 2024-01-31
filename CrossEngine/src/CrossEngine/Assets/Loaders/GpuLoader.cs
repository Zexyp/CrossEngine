using CrossEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossEngine.Assets.Loaders
{
    public abstract class GpuLoader : Loader
    {
        public event Action<GpuLoader, Action> Emit;

        protected void Schedule(Action action)
        {
            Emit?.Invoke(this, action);
        }
    }
}
