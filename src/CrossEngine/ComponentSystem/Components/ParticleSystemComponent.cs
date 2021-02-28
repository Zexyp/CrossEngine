using System;
using System.Collections.Generic;
using System.Text;

using CrossEngine.FX.Particles;

namespace CrossEngine.ComponentSystem.Components
{
    public class ParticleSystemComponent : Component
    {
        public ParticleSystem particleSystem = new ParticleSystem();

        public override void OnUpdate(float timestep)
        {
            if (Playing)
            {
                queue += emitPerSecond * timestep;
                while (queue >= 1)
                {
                    particleSystem.Emit(emitCount);
                    queue--;
                }
            }

            particleSystem.OnUpdate(timestep);
        }

        public override void OnRender()
        {
            particleSystem.OnRender();
        }

        public int emitCount = 1;
        public float emitPerSecond = 1.0f;

        private float queue = 0.0f;
        public bool Playing { get; private set; } = true;

        public void Emit()
        {
            particleSystem.Emit(emitCount);
        }

        public void Play()
        {
            Playing = true;
        }

        public void Pause()
        {
            Playing = false;
        }

        public void Stop()
        {
            queue = 0.0f;
            Playing = false;
        }
    }
}
