using System;
using System.IO;
using System.Linq;
using CrossEngine.Assemblies;
using CrossEngine.Ecs;
using CrossEngine.Logging;

namespace CrossEngine.Components;

public class ScriptSystem : Ecs.System, IUpdatedSystem
{
    protected internal override void OnStart()
    {
        Log.Default.Debug("compiling scripts");
        var comps = World.Storage.GetArray(typeof(ScriptCompoent));
        if (comps == null) return;
        
        for (int i = 0; i < comps.Count; i++)
        {
            var comp = (ScriptCompoent)comps[i];
            
            if (comp.Script.assembly == null)
                using (var stream = File.OpenRead(comp.Script.FullPath))
                    (comp.Script.context, comp.Script.assembly) = Scripting.Compile(stream);
            
            comp.Behaviour = (Behaviour)Activator.CreateInstance(comp.Script.assembly.GetTypes().First(t => t.IsSubclassOf(typeof(Behaviour))));
            comp.Behaviour.Component = comp;
        }
        
        for (int i = 0; i < comps.Count; i++)
        {
            var comp = (ScriptCompoent)comps[i];

            try
            {
                comp.Behaviour?.Start();
            }
            catch (Exception e)
            {
                Log.Default.Error($"script error:\n{e}");
                throw;
            }
        }
    }

    protected internal override void OnStop()
    {
        Log.Default.Debug("unloading scripts");
        var comps = World.Storage.GetArray(typeof(ScriptCompoent));
        if (comps == null) return;

        for (int i = 0; i < comps.Count; i++)
        {
            var comp = (ScriptCompoent)comps[i];

            try
            {
                comp.Behaviour?.Stop();
            }
            catch (Exception e)
            {
                Log.Default.Error($"script error:\n{e}");
                throw;
            }
        }

        for (int i = 0; i < comps.Count; i++)
        {
            var comp = (ScriptCompoent)comps[i];
            
            if (comp.Script.assembly != null)
                AssemblyManager.Unload(comp.Script.context);

            comp.Behaviour = null;

            comp.Script.context = null;
            comp.Script.assembly = null;
        }
    }

    public void OnUpdate()
    {
        var comps = World.Storage.GetArray(typeof(ScriptCompoent));
        if (comps == null) return;

        for (int i = 0; i < comps.Count; i++)
        {
            var comp = (ScriptCompoent)comps[i];
            
            try
            {
                comp.Behaviour?.Update();
            }
            catch (Exception e)
            {
                Log.Default.Error($"script error:\n{e}");
                throw;
            }
        }
    }
}

public abstract class Behaviour
{
    public Component Component;
    public virtual void Update() { }
    public virtual void Start() { }
    public virtual void Stop() { }
}
