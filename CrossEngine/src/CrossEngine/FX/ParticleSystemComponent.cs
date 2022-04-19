using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.FX.Particles;

namespace CrossEngine.Components
{
    public class ParticleSystemComponent : Component, IParticleSystemRenderData
    {
        public ParticleSystem ParticleSystem { get; set; }

        public override void Attach()
        {
            ParticleSystemSystem.Instance.Register(this);
        }

        public override void Detach()
        {
            ParticleSystemSystem.Instance.Unregister(this);
        }

        public override void Update()
        {
            ParticleSystem.Emit();
            ParticleSystem.Update(Time.DeltaTimeF);
        }
    }
}
