using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Services
{
    internal abstract class Service
    {
        public bool Active;

        protected abstract void OnActivate();
        protected abstract void OnDeactivate();
        protected abstract void OnAttach();
        protected abstract void OnDetach();
        protected abstract void OnStart();
        protected abstract void OnDestroy();
    }
}
