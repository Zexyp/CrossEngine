using System;

using System.Numerics;

using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering;
using CrossEngine.Utils;
using CrossEngine.Rendering.Texturing;

namespace CrossEngine.FX.Particles
{
    public class ParticleSystem
    {
        //public struct Particle
        //{
        //    public Vector3 position;
        //    public Vector3 velocity;
        //
        //    public float lifeTime;
        //    public float lifeRemaining;
        //
        //    public float gravityEffect;
        //
        //    public float rotation, rotationVelocity;
        //    public float sizeBegin, sizeEnd;
        //
        //    public Vector4 colorBegin, colorEnd;
        //
        //    public bool active;
        //}

        public struct Particle
        {
            public Vector3 position;
            public Vector3 velocity;

            public float lifeTime;
            public float lifeRemaining;

            public float gravityEffect;

            public float rotation, rotationVelocity;
            public Gradient sizeGradient;
            public float sizeVariation;

            public Gradient colorGradient;
            public Vector3 hsvVariation;

            public bool active;
        }

        const int maxParticleCount = 1000;

        Particle[] particlePool;
        int poolIndex = maxParticleCount - 1;

        //Shader shader;

        bool drawIn2D = false;

        public ParticleSystem(bool drawIn2D = false)
        {
            this.drawIn2D = drawIn2D;
            particlePool = new Particle[maxParticleCount];
        }

        public void OnUpdate(float timestep)
        {
            for (int i = 0; i < particlePool.Length; i++)
            {
                //Log.Debug("particle " + i.ToString() + ": " + particlePool[i].active.ToString());

                if (!particlePool[i].active)
                    continue;

                if (particlePool[i].lifeRemaining <= 0.0f)
                {
                    particlePool[i].active = false; //Log.Debug("setting particle " + i.ToString() + " to inactive: " + particlePool[i].active.ToString());
                    continue;
                }

                particlePool[i].lifeRemaining -= timestep;

                //particlePool[i].velocity += Physics.gravity * particlePool[i].gravityEffect * timestep; // TODO uncomment this
                particlePool[i].position += particlePool[i].velocity * timestep;
                particlePool[i].rotation += particlePool[i].rotationVelocity * timestep; // can be changed
            }
        }

        public void OnRender()
        {
            BatchRenderer.BeginBatch();
            foreach (Particle particle in particlePool)
            {
                if (!particle.active)
                    continue;

                float life = particle.lifeRemaining / particle.lifeTime;

                Vector4 color = particle.colorGradient.SampleVector4(life);
                if (particle.hsvVariation != Vector3.Zero)
                {
                    color = new Vector4(Vector3Extension.RGBToHSV(color.XYZ()), color.W);
                    color.X += particle.hsvVariation.X;
                    color.Y += particle.hsvVariation.Y;
                    color.Z += particle.hsvVariation.Z;
                    color = new Vector4(Vector3Extension.HSVToRGB(color.XYZ()), color.W);
                }
                

                Vector2 size = particle.sizeGradient.SampleVector2(life);
                size = size - size * particle.sizeVariation;
                if (particle.active)
                {
                    if (!drawIn2D)
                        BatchRenderer.DrawBillboard(particle.position, size, color, particle.rotation);
                    else
                        BatchRenderer.DrawQuad(particle.position.XY(), size, color, particle.rotation);
                }
            }
            BatchRenderer.EndBatch();
            BatchRenderer.Flush();
        }

        public ParticleProperties particleProperties;
        public ParticleEmitter particleEmitter;
        public void Emit(int count = 1)
        {
            if (particleProperties == null || particleEmitter == null)
            {
                Log.Warn("particel system has no particle properties or emitter assigned");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                // use emitter add to pool
                Particle particle = particleEmitter.Generate(particleProperties);
                particle.active = true;
                particlePool[poolIndex] = particle;

                //poolIndex++;
                //if (poolIndex > particlePool.Length - 1)
                //    poolIndex = 0;
                poolIndex--;
                if (poolIndex < 0)
                    poolIndex = particlePool.Length - 1;
            }
        }
    }


    //########################################################################################################################################################################################################
    //########################################################################################################################################################################################################
    //########################################################################################################################################################################################################


    /*
    class ParticleSystem
    {
        Random random = new Random(); // reee

        struct Particle
        {
            public Vector3 position;
            public Vector3 velocity;

            public Vector4 colorBegin, colorEnd;

            public float rotation;
            public float sizeBegin, sizeEnd;

            public float lifeTime;
            public float lifeRemaining;

            public bool active;
        }

        const int maxParticleCount = 1000;

        Particle[] particlePool;
        int poolIndex = maxParticleCount - 1;

        Shader shader;

        //Gradient colorGradient = new Gradient();
        //Gradient sizeGradient = new Gradient();

        public ParticleSystem()
        {
            particlePool = new Particle[maxParticleCount];
        }

        public void OnUpdate(float timeStep)
        {
            for (int i = 0; i < particlePool.Length; i++)
            {
                //Log.Debug("particle " + i.ToString() + ": " + particlePool[i].active.ToString());

                if (!particlePool[i].active)
                    continue;

                if(particlePool[i].lifeRemaining <= 0.0f)
                {
                    particlePool[i].active = false; //Log.Debug("setting particle " + i.ToString() + " to inactive: " + particlePool[i].active.ToString());
                    continue;
                }

                particlePool[i].lifeRemaining -= timeStep;
                particlePool[i].position += particlePool[i].velocity * timeStep;
                particlePool[i].rotation += 0.01f * timeStep; // can be changed
            }
        }

        public void OnRender()
        {
            BatchRenderer.BeginBatch();
            foreach(Particle particle in particlePool)
            {
                if (!particle.active)
                    continue;

                float life = particle.lifeRemaining / particle.lifeTime;
                Vector4 color = Vector4.Lerp(particle.colorEnd, particle.colorBegin, life);
                color.W = color.W * life; //transparency
                //Vector4 color = colorGradient.Sample(life);

                float size = MathExtension.Lerp(particle.sizeEnd, particle.sizeBegin, life);
                //float size = sizeGradient.Sample(life).X;
                if (particle.active)
                {
                    BatchRenderer.DrawQuad(particle.position, new Vector2(size), color, particle.rotation);
                    BatchRenderer.DrawParticle(particle.position, size, color, particle.rotation);
                }
            }
            BatchRenderer.EndBatch();
            BatchRenderer.Flush();
        }

        public void Emit(ParticleProps particleProps)
        {
            Particle particle = new Particle(); //particlePool[poolIndex];

            particle.active = true;

            // position
            particle.position = particleProps.position;

            // rotation
            particle.rotation = (float)random.NextDouble() * 2.0f * (float)Math.PI;

            // size
            particle.sizeBegin = particleProps.sizeBegin;
            particle.sizeEnd = particleProps.sizeEnd;

            // velocity
            particle.velocity = particleProps.velocity;
            particle.velocity.X += particleProps.velocityVariation.X * ((float)random.NextDouble() - 0.5f);
            particle.velocity.Y += particleProps.velocityVariation.Y * ((float)random.NextDouble() - 0.5f);
            particle.velocity.Z += particleProps.velocityVariation.Z * ((float)random.NextDouble() - 0.5f);
            //particle.velocity = Vector3.Normalize(particle.velocity) * particleProps.velocityVariation + particleProps.velocity; // velocity correction

            // color
            particle.colorBegin = particleProps.colorBegin;
            particle.colorEnd = particleProps.colorEnd;

            // life
            particle.lifeTime = particleProps.lifeTime;
            particle.lifeRemaining = particleProps.lifeTime;

            // add to pool
            particlePool[poolIndex] = particle;

            //poolIndex++;
            //if (poolIndex > particlePool.Length - 1)
            //    poolIndex = 0;

            poolIndex--;
            if (poolIndex < 0)
                poolIndex = particlePool.Length - 1;
        }

        //public void SetColorGradient(Gradient gradient)
        //{
        //    colorGradient = gradient;
        //}
        //
        //public void SetSizeGradient(Gradient gradient)
        //{
        //    sizeGradient = gradient;
        //}
    }
    */
}
