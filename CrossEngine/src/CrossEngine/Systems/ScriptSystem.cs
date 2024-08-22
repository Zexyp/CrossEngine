using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Systems
{
    internal class ScriptSystem : UnicastSystem<ScriptComponent>, IUpdatedSystem, IFixedUpdatedSystem
    {
        List<ScriptComponent> _components = new();
        List<(ScriptComponent Component, MethodInfo Method)> _updated = new();
        List<(ScriptComponent Component, MethodInfo Method)> _fixedUpdated = new();
        static internal Logger Log = new Logger("scripts");

        public ScriptSystem() : base(true)
        {
            
        }

        public override void Register(ScriptComponent component)
        {
            _components.Add(component);

            if (Started) StartComponent(component);
        }

        public override void Unregister(ScriptComponent component)
        {
            if (Started) StopComponent(component);

            _components.Remove(component);
        }

        public override void OnStart()
        {
            base.OnStart();

            for (int i = 0; i < _components.Count; i++)
            {
                StartComponent(_components[i]);
            }
        }

        public override void OnStop()
        {
            base.OnStop();

            for (int i = 0; i < _components.Count; i++)
            {
                StopComponent(_components[i]);
            }
        }

        private void OnComponentEnabledChanged(Component sender)
        {
            var component = (ScriptComponent)sender;
            MethodInfo method = null;
            if (component.Enabled)
            {
                if (TryGetMethod(component, ScriptComponent.FuncNameOnEnable, out method))
                    TryMethod(component, method);
            }
            else
            {
                if (TryGetMethod(component, ScriptComponent.FuncNameOnDisable, out method))
                    TryMethod(component, method);
            }
        }

        private bool TryGetMethod(ScriptComponent component, string name, out MethodInfo info)
        {
            info = component.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes);
            return info != null;
        }

        private void StartComponent(ScriptComponent component)
        {
            component.EnabledChanged += OnComponentEnabledChanged;

            MethodInfo method = null;

            if (TryGetMethod(component, ScriptComponent.FuncNameOnAttach, out method))
                TryMethod(component, method);
            if (component.Enabled)
                if (TryGetMethod(component, ScriptComponent.FuncNameOnEnable, out method))
                    TryMethod(component, method);

            if (TryGetMethod(component, ScriptComponent.FuncNameOnUpdate, out method))
                _updated.Add((component, method));
            if (TryGetMethod(component, ScriptComponent.FuncNameOnFixedUpdate, out method))
                _fixedUpdated.Add((component, method));
        }

        private void StopComponent(ScriptComponent component)
        {
            MethodInfo method = null;

            if (TryGetMethod(component, ScriptComponent.FuncNameOnUpdate, out method))
                _updated.Remove((component, method));
            if (TryGetMethod(component, ScriptComponent.FuncNameOnFixedUpdate, out method))
                _fixedUpdated.Remove((component, method));

            if (component.Enabled)
                if (TryGetMethod(component, ScriptComponent.FuncNameOnDisable, out method))
                    TryMethod(component, method);
            if (TryGetMethod(component, ScriptComponent.FuncNameOnDetach, out method))
                TryMethod(component, method);

            component.EnabledChanged -= OnComponentEnabledChanged;
        }

        public void OnUpdate()
        {
            for (int i = 0; i < _updated.Count; i++)
            {
                (var c, var m) = _updated[i];
                if (c.Enabled) TryMethod(c, m);
            }
        }

        public void OnFixedUpdate()
        {
            for (int i = 0; i < _fixedUpdated.Count; i++)
            {
                (var c, var m) = _fixedUpdated[i];
                if (c.Enabled) TryMethod(c, m);
            }
        }

        private void TryIfExists(Action call)
        {
            try
            {
                call.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error($"working with script mishap: {ex}");
                throw;
            }
        }

        private void Try(Action call)
        {
            try
            {
                call.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error($"working with script mishap: {ex}");
                throw;
            }
        }

        private void TryMethod(Component target, MethodInfo method)
        {
            try
            {
                method.Invoke(target, null);
            }
            catch (Exception ex)
            {
                Log.Error($"working with script mishap: {ex}");
                throw;
            }
        }
    }
}
