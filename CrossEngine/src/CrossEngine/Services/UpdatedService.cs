using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Services;

namespace CrossEngine.Services
{
    public abstract class UpdatedService : Service
    {
        public abstract void OnUpdate();
    }
}
