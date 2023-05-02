using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Services
{
    internal abstract class UpdatableService : Service
    {
        protected abstract void OnUpdate();
    }
}
