﻿using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Systems
{
    internal class ScriptSystem : UnicastSystem<ScriptComponent>, IUpdatedSystem, IFixedUpdatedSystem
    {
        List<ScriptComponent> _components = new();
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
            base.OnStart();

            for (int i = 0; i < _components.Count; i++)
            {
                StopComponent(_components[i]);
            }
        }

        private void OnComponentEnabledChanged(Component sender)
        {
            var component = (ScriptComponent)sender;
            if (component.Enabled) Try(component.OnEnable);
            else Try(component.OnDisable);
        }

        private void StartComponent(ScriptComponent component)
        {
            component.EnabledChanged += OnComponentEnabledChanged;

            Try(component.OnAttach);
            if (component.Enabled)
                Try(component.OnEnable);
        }

        private void StopComponent(ScriptComponent component)
        {
            if (component.Enabled)
                Try(component.OnDisable);
            Try(component.OnDetach);

            component.EnabledChanged -= OnComponentEnabledChanged;
        }

        public void OnUpdate()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var c = _components[i];
                if (c.Enabled) Try(c.OnUpdate);
            }
        }

        public void OnFixedUpdate()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var c = _components[i];
                if (c.Enabled) Try(c.OnFixedUpdate);
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
    }
}
