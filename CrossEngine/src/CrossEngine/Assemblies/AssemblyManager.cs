﻿using CrossEngine.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assemblies
{
    public static class AssemblyManager
    {
        readonly static Logger Log = new Logger("assemblies") { Color = 0xff658612 };
        readonly static List<Assembly> _loaded = new() { Assembly.GetExecutingAssembly() };
        readonly static Dictionary<AssemblyLoadContext, Assembly> _contexts = new() { { AssemblyLoadContext.Default, Assembly.GetExecutingAssembly() } };

        public readonly static ReadOnlyCollection<Assembly> Loaded;

        static AssemblyManager()
        {
            Loaded = _loaded.AsReadOnly();

            Log.Trace("initial assemblies:\n    " + string.Join("\n    ", _loaded.Select(a => GetPrintName(a))));
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

        public static AssemblyLoadContext Load(Stream stream, out Assembly assembly)
        {
            var context = new AssemblyLoadContext(null, true);

            assembly = context.LoadFromStream(stream);
            _contexts.Add(context, assembly);
            _loaded.Add(assembly);
            
            Log.Info($"assembly loaded '{GetPrintName(assembly)}'");

            var tmpa = assembly;
            context.Unloading += c => Log.Info($"assembly unloading '{GetPrintName(tmpa)}'");
             
            return context;
        }

        public static void Unload(AssemblyLoadContext context)
        {
            _loaded.Remove(_contexts[context]);
            _contexts.Remove(context);
            context.Unload();
        }

        private static string GetPrintName(Assembly a)
        {
            var n = a.GetName();
            return $"{n.Name} ({n.Version})";
        }
    }
}