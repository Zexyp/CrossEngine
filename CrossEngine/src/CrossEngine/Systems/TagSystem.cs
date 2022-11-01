using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;

namespace CrossEngine.Systems
{
    public class TagSystem : ISystem<TagComponent>
    {
        public SystemThreadMode ThreadMode => SystemThreadMode.Sync;

        private readonly Dictionary<string, List<TagComponent>> _tags = new Dictionary<string, List<TagComponent>>();

        public ReadOnlyCollection<TagComponent>? GetByTag(string tag)
        {
            if (!_tags.ContainsKey(tag))
                return null;

            return _tags[tag].AsReadOnly();
        }

        public void Register(TagComponent component)
        {
            if (!_tags.ContainsKey(component.Tag))
                _tags[component.Tag] = new List<TagComponent>();

            _tags[component.Tag].Add(component);

            component.OnTagChanged += Component_OnTagChanged;
        }

        public void Unregister(TagComponent component)
        {
            component.OnTagChanged -= Component_OnTagChanged;

            _tags[component.Tag].Remove(component);

            if (_tags[component.Tag].Count == 0)
                _tags.Remove(component.Tag);
        }

        private void Component_OnTagChanged(TagComponent comp, string o, string n)
        {
            Unregister(comp);
            Register(comp);
        }

        public void Init() { }
        public void Shutdown() { }

        public void Update() { }
    }
}
