using System;

using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngine.FX.Particles
{
    public class ParticleEmitter
    {
        public Transform transform = new Transform();

        protected Random random = new Random();

        public ParticleEmitter()
        {

        }

        public ParticleEmitter(Transform transform)
        {
            this.transform = transform;
        }

        public virtual ParticleSystem.Particle Generate(ParticleProperties particleProps)
        {
            return new ParticleSystem.Particle();
        }

        protected ParticleSystem.Particle CopyCommon(ParticleProperties particleProps)
        {
            ParticleSystem.Particle particle = new ParticleSystem.Particle();

            particle.active = true;

            // gravity
            particle.gravityEffect = particleProps.gravityEffect;
            // size
            particle.sizeGradient = particleProps.sizeGradient;
            particle.sizeVariation = (float)random.NextDouble() * particleProps.sizeVariation;
            // color
            particle.colorGradient = particleProps.colorGradient;
            if(particleProps.hsvVariation != Vector3.Zero)
            {
                particle.hsvVariation.X = ((float)random.NextDouble() - 0.5f) * particleProps.hsvVariation.X;
                particle.hsvVariation.Y = ((float)random.NextDouble() * 2.0f - 1.0f) * particleProps.hsvVariation.Y;
                particle.hsvVariation.Z = ((float)random.NextDouble() * 2.0f - 1.0f) * particleProps.hsvVariation.Z;
            }
            // position
            particle.position = transform.Position;

            // velocity
            particle.velocity = Vector3Extension.Random() * particleProps.velocityVariation + particleProps.velocity; // velocity correction
            // rotation
            particle.rotation = ((float)random.NextDouble() * 2.0f - 1.0f) * (float)Math.PI * particleProps.rotationVariation + particleProps.rotation;
            particle.rotationVelocity = ((float)random.NextDouble() * 2.0f - 1.0f) * particleProps.rotationVelocityVariation + particleProps.rotationVelocity;
            // life
            float life = (float)random.NextDouble() * particleProps.lifeTimeVariation * particleProps.lifeTime;
            particle.lifeTime = particleProps.lifeTime - life;
            particle.lifeRemaining = particleProps.lifeTime - life;

            return particle;
        }
    }
}

namespace CrossEngine.FX.Particles.Emitters
{
    public class PointParticleEmitter : ParticleEmitter
    {
        public override ParticleSystem.Particle Generate(ParticleProperties particleProps)
        {
            ParticleSystem.Particle particle = CopyCommon(particleProps); //particlePool[poolIndex];

            // velocity
            //particle.velocity = particleProps.velocity;
            //particle.velocity.X += particleProps.velocityVariation.X * ((float)random.NextDouble() - 0.5f);
            //particle.velocity.Y += particleProps.velocityVariation.Y * ((float)random.NextDouble() - 0.5f);
            //particle.velocity.Z += particleProps.velocityVariation.Z * ((float)random.NextDouble() - 0.5f);
            // position
            particle.position = transform.Position;

            return particle;
        }
    }

    public class AreaParticleEmitter : ParticleEmitter
    {
        public Vector3 size = new Vector3();

        public override ParticleSystem.Particle Generate(ParticleProperties particleProps)
        {
            ParticleSystem.Particle particle = CopyCommon(particleProps); ; //particlePool[poolIndex];

            // position
            particle.position = transform.Position + (new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()) - new Vector3(0.5f)) * size;

            return particle;
        }
    }
}
