using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;

using CrossEngine.Logging;
using CrossEngine.Utils;

namespace CrossEngine.Assemblies
{
    public static class AssemblyLoader
    {
        public class AssemblyObject
        {
            internal readonly AssemblyLoadContext Context;
            internal readonly AssemblyDependencyResolver Resolver;
            public readonly Assembly RootAssembly;
            public readonly string Path;

            internal AssemblyObject(AssemblyLoadContext context, AssemblyDependencyResolver resolver, Assembly rootAssembly, string path)
            {
                Context = context;
                Resolver = resolver;
                RootAssembly = rootAssembly;
                Path = path;
            }

            public void Destroy()
            {
                Context.Unload();
            }
        }

        //                         path          object
        private static readonly Dictionary<string, AssemblyObject> _assemblies = new Dictionary<string, AssemblyObject>();

        public static Type GetType(string typeName)
        {
            foreach (var assObj in _assemblies.Values)
            {
                Type foundType = assObj.RootAssembly.GetType(typeName);
                if (foundType != null) return foundType;
            }
            return null;
        }

        public static readonly IReadOnlyCollection<AssemblyObject> LoadedAssemblies;

        static AssemblyLoader()
        {
            LoadedAssemblies = _assemblies.Values;
        }

        public static Assembly Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();

            path = PathUtils.GetRelativePath(Environment.CurrentDirectory, path);

            AssemblyLoadContext alc = new AssemblyLoadContext(null, true);
            AssemblyDependencyResolver dr = new AssemblyDependencyResolver(path);
            Assembly rootAss = alc.LoadFromAssemblyPath(Path.GetFullPath(path));

            Log.Core.Info($"[assembly loader] loaded assembly '{path}'");

            AssemblyObject assemblyObject = new AssemblyObject(alc, dr, rootAss, path);
            _assemblies.Add(path, assemblyObject);

            {
                var assnArr = rootAss.GetReferencedAssemblies();
                for (int i = 0; i < assnArr.Length; i++)
                {
                    LoadDependency(assnArr[i], assemblyObject);
                }
            }

            return rootAss;
        }

        private static void LoadDependency(AssemblyName assn, AssemblyObject assObj)
        {
            if (Assembly.GetExecutingAssembly().GetName().Name == assn.Name ||
                Array.ConvertAll(Assembly.GetExecutingAssembly().GetReferencedAssemblies(), (v) => v.Name).Contains(assn.Name)) return;

            // returned path to assembly should be absolute
            string depPath = assObj.Resolver.ResolveAssemblyToPath(assn);
            if (depPath == null) return;
            Assembly ass = assObj.Context.LoadFromAssemblyPath(depPath);
            Log.Core.Info($"[assembly loader] loaded assembly dependency '{Path.GetRelativePath(Environment.CurrentDirectory, depPath)}'");
            {
                var assnArr = ass.GetReferencedAssemblies();
                for (int i = 0; i < assnArr.Length; i++)
                {
                    LoadDependency(assnArr[i], assObj);
                }
            }
        }

        public static void Reload(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();

            Unload(path);
            Load(path);
        }

        public static void ReloadAll()
        {
            string[] paths = new string[_assemblies.Count];
            _assemblies.Keys.CopyTo(paths, 0);
            for (int i = 0; i < paths.Length; i++)
            {
                Reload(paths[i]);
            }
        }

        public static void Unload(Assembly obj)
        {
            string key = null;
            foreach (var pair in _assemblies)
            {
                if (pair.Value.RootAssembly == obj) key = pair.Key;
            }
            if (key == null) throw new InvalidOperationException();

            _assemblies[key].Destroy();

            Log.Core.Info($"[assembly loader] unloaded assembly '{key}'");

            _assemblies.Remove(key);
        }

        public static void Unload(string path)
        {
            path = PathUtils.GetRelativePath(Environment.CurrentDirectory, path);
            
            if (!_assemblies.ContainsKey(path)) throw new InvalidOperationException();

            _assemblies[path].Destroy();

            Log.Core.Info($"[assembly loader] unloaded assembly '{path}'");

            _assemblies.Remove(path);
        }

        public static void UnloadAll()
        {
            while (_assemblies.Count > 0) Unload(_assemblies.Keys.First());
        }

        public static Type[] GetSubTypesOf(Type type)
        {
            List<Type> foundTypes = new List<Type>();
            foreach (var objs in _assemblies.Values)
                foreach (var ass in objs.Context.Assemblies)
                    foreach (var item in ass.ExportedTypes)
                    {
                        if (item.IsSubclassOf(type)) foundTypes.Add(item);
                    }
            return foundTypes.ToArray();
        }
    }
}
