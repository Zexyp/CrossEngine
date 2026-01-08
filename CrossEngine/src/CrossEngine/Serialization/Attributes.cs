using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    class SerializeIncludeAttribute : Attribute { }
}
