using CrossEngine.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Serialization.Json
{
    internal interface IInitializedConverter
    {
        void Init();
        void Finish();
    }

    internal interface ITypeResolveConverter
    {
        TypeResolver Resolver { get; set; }
    }

    // my simple thing :), ITypeResolutionService sounds too scary
    public abstract class TypeResolver
    {
        public static readonly TypeResolver Default = new DefaultTypeResolver();

        public abstract Type Resolve(string str);
    }

    sealed class DefaultTypeResolver : TypeResolver
    {
        public override Type Resolve(string str) => Type.GetType(str, true);
    }

    class CrossAssemblyTypeResolver : TypeResolver
    {
        public override Type Resolve(string str) => Type.GetType(str, false) ?? Assembly.GetEntryAssembly().GetType(str, false) ?? AssemblyManager.GetType(str);
    }
}
