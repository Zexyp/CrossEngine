using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Serialization
{
    public abstract class TypeResolver
    {
        public abstract Type ResolveType(string typeName);
    }

    public class DefaultTypeResolver : TypeResolver
    {
        public override Type ResolveType(string typeName)
        {
            return Type.GetType(typeName);
        }
    }
}
