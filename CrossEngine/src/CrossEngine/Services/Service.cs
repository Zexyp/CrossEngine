using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Services;

namespace CrossEngine.Services
{
    internal abstract class Service
    {
        public ServiceManager Manager { get; internal set; }

        public abstract void OnStart();
        public abstract void OnDestroy();
    }
}
