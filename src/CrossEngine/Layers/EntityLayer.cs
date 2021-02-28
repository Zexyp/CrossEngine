using System;

using System.Collections.Generic;

using CrossEngine.Rendering.Cameras;
using CrossEngine.ComponentSystem;

namespace CrossEngine.Layers
{
    public class EntityLayer : Layer
    {
        protected List<Entity> Entities { get; private set; } = new List<Entity> { };

        bool attached = false;

        public override void OnUpdate(float timestep)
        {
            foreach (Entity entity in Entities)
            {
                if (entity.Active)
                    entity.OnUpdate(timestep);
            }
        }

        public override void OnRender()
        {
            foreach (Entity entity in Entities)
            {
                if (entity.Active)
                    entity.OnRender();
            }
        }

        public override void OnAttach()
        {
            foreach (Entity entity in Entities)
            {
                if (entity.Active)
                    entity.OnAwake();
            }

            attached = true;
        }

        public override void OnDetach()
        {
            foreach (Entity entity in Entities)
            {
                if (entity.Active)
                    entity.OnDie();
            }

            attached = false;
        }

        public void AddEntityToLayer(Entity entity)
        {
            if (attached)
                entity.OnAwake();
            Entities.Add(entity);
        }
    }
}
