using System;

using System.Numerics;

using CrossEngine.Audio;

namespace CrossEngine.ComponentSystem.Components
{
    class AudioSourceComponent : Component
    {
        public AudioSource audioSource;

        public AudioSourceComponent(AudioBuffer audioBuffer)
        {
            audioSource = new AudioSource();
            audioSource.BindBuffer(audioBuffer);
            entity.transform.OnValueChanged += OnTransformChanged;
        }

        private void OnTransformChanged(object sender, EventArgs e)
        {
            audioSource.Position = entity.transform.Position;
        }

        Vector3 lastPosition;
        public override void OnUpdate(float timestep)
        {
            audioSource.Position = entity.transform.Position;
            audioSource.Velocity = (entity.transform.Position - lastPosition) / timestep;
            lastPosition = entity.transform.Position;
        }
    }
}
