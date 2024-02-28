using CrossEngine.Logging;
using CrossEngine.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assemblies
{
    public static class AssemblyManager
    {
        readonly static Logger Log = new Logger("assemblies") { Color = 0xff65df12 };
        readonly static List<Assembly> _loaded = new() { Assembly.GetExecutingAssembly() };
        readonly static Dictionary<AssemblyLoadContext, Assembly> _contexts = new() { { AssemblyLoadContext.Default, Assembly.GetExecutingAssembly() } };

        public readonly static ReadOnlyCollection<Assembly> Loaded;

        static AssemblyManager()
        {
            Loaded = _loaded.AsReadOnly();

            Log.Trace("default load context:\n" + string.Join("\n", AssemblyLoadContext.Default.Assemblies.Select(a => GetPrintName(a))));
            Log.Debug("initial assemblies:\n" + string.Join("\n", _loaded.Select(a => GetPrintName(a))));
        }

        public static IEnumerable<Type> GetSubclasses(Type type)
        {
            foreach (var item in _contexts.Values)
            {
                var types = item.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    var t = types[i];
                    
                    if (t.IsPublic && !t.IsAbstract && t.IsSubclassOf(type))
                    {
                        yield return t;
                    }
                }
            }
        }

        public static Type GetType(string typeName)
        {
            for (int i = 0; i < _loaded.Count; i++)
            {
                Type found = _loaded[i].GetType(typeName, false);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static async Task<(AssemblyLoadContext Context, Assembly Assembly)> Load(string path)
        {
            Debug.Assert(path != null);

            var context = new AssemblyLoadContext(null, true);

            Assembly assembly = context.LoadFromStream(await PlatformHelper.FileRead(path));
            
            // this shoudld prevent duplicit static structures
            foreach (var defaultAssembly in AssemblyLoadContext.Default.Assemblies)
            {
                if (assembly.FullName != defaultAssembly.FullName)
                    continue;

                context.Unload();

                return (AssemblyLoadContext.Default, defaultAssembly);
            }
            
            _contexts.Add(context, assembly);
            _loaded.Add(assembly);

            var tmpa = assembly;
            context.Unloading += c => Log.Info($"assembly unloading '{GetPrintName(tmpa)}'");

            await LoadDependencies(path, assembly, context);

            Log.Info($"assembly loaded '{GetPrintName(assembly)}'");
             
            return (context, assembly);
        }

        public static void Unload(AssemblyLoadContext context)
        {
            if (context == AssemblyLoadContext.Default)
                return;

            Debug.Assert(context != null);

            _loaded.Remove(_contexts[context]);
            _contexts.Remove(context);
            context.Unload();
        }

        private static string GetPrintName(Assembly a)
        {
            return GetPrintName(a.GetName());
        }

        private static string GetPrintName(AssemblyName n)
        {
            return $"{n.Name} ({n.Version})";
        }

        private static async Task LoadDependencies(string path, Assembly assembly, AssemblyLoadContext context)
        {
            var referenced = assembly.GetReferencedAssemblies();
            for (int i = 0; i < referenced.Length; i++)
            {
                var name = referenced[i];
                if (AssemblyLoadContext.Default.Assemblies.Select(a => a.GetName()).Any(n => n.FullName == name.FullName))
                    continue;

                if (name.Name.StartsWith("System."))
                {
                    Log.Trace($"skip loading system like '{GetPrintName(name)}'");
                    continue;
                }

                var stream = await PlatformHelper.FileRead(Path.Join(Path.GetDirectoryName(path), name.Name + ".dll"));
                var a = context.LoadFromStream(stream);

                Log.Info($"loaded dependency '{GetPrintName(name)}'");

                await LoadDependencies(path, a, context);
            }
        }
    }
}
