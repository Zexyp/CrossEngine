using System;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Physics;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.ComponentSystems;

namespace CrossEngine.FX.Particles
{
    public class ParticleSystem
    {
        private uint _particlePoolSize;

        public uint ParticlePoolSize
        {
            get => _particlePoolSize;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException();

                _particlePoolSize = value;

                Array.Resize(ref _particlePool, (int)_particlePoolSize);
                _poolIndex = Math.Clamp(_poolIndex, 0, _particlePoolSize - 1);
            }
        }


        private Particle[] _particlePool;
        private uint _poolIndex = 0;

        //public ParticleProperties particleProperties;
        //public ParticleEmitter particleEmitter;

        private TextureAtlas textureAtlas;
        //public bool animated

        private bool drawIn2D = false;

        public ParticleSystem(uint poolSize = 1000, bool drawIn2D = false)
        {
            this.drawIn2D = drawIn2D;
            ParticlePoolSize = poolSize;
            _particlePool = new Particle[ParticlePoolSize];
        }

        public void Update(float timestep)
        {
            var gravity = PhysicsInterface.Gravity;
            for (int i = 0; i < _particlePool.Length; i++)
            {
                if (!_particlePool[i].active)
                    continue;

                if (_particlePool[i].lifeRemaining <= 0.0f)
                {
                    _particlePool[i].active = false;
                    continue;
                }

                _particlePool[i].lifeRemaining -= timestep;

                _particlePool[i].velocity += gravity * _particlePool[i].gravityEffect * timestep;
                _particlePool[i].position += _particlePool[i].velocity * timestep;
                _particlePool[i].rotation += _particlePool[i].rotationVelocity * timestep; // can be changed
            }
        }
        
        public void Render()
        {
            var transform = RendererSystem.Instance.Primary.Entity.Transform;
            var cameraRight = transform.GetWorldRightDirection();
            var cameraUp = transform.GetWorldUpDirection();
            var cameraLook = transform.Position;

            for (int i = 0; i < _particlePool.Length; i++)
            {
                var particle = _particlePool[i];
                if (!particle.active)
                    continue;

                float life = particle.life - particle.lifeRemaining / particle.life;

                Vector4 color = particle.color.Sample(life);
                if (particle.colorVariation != Vector4.Zero)
                {
                    color = new Vector4(Color.RGBToHSV(color.XYZ()), color.W);
                    color.X += particle.colorVariation.X;
                    color.Y += particle.colorVariation.Y;
                    color.Z += particle.colorVariation.Z;
                    color = new Vector4(Color.HSVToRGB(color.XYZ()), color.W);
                    color.W += particle.colorVariation.W;
                }

                float size = particle.size.Sample(life);
                size = size - size * particle.sizeVariation;

                Matrix4x4 matrix = Matrix4x4.CreateScale(size);
                if (!drawIn2D)
                {
                    matrix *= Matrix4x4Extension.CreateBillboard(cameraRight, cameraUp, cameraLook, particle.position) * Matrix4x4.CreateRotationZ(particle.rotation);
                }
                else
                    matrix *= Matrix4x4.CreateTranslation(particle.position) * Matrix4x4.CreateRotationZ(particle.rotation);
                Renderer2D.DrawQuad(matrix, color);
            }
        }

        public void Emit(int count = 1)
        {
            //if (particleProperties == null || particleEmitter == null)
            //{
            //    Log.Warn("particel system has no particle properties or emitter assigned");
            //    return;
            //}

            var particleProperties = ParticleProperties.CreateDefault();

            for (int i = 0; i < count; i++)
            {
                // use emitter add to pool
                Particle particle = ParticleEmmiter.GenerateParticle(particleProperties);
                particle.active = true;
                _particlePool[_poolIndex] = particle;

                // decides the way of 'sorting'
                //poolIndex++;
                //if (poolIndex > particlePool.Length - 1)
                //    poolIndex = 0;

                if (_poolIndex == 0)
                    _poolIndex = (uint)_particlePool.Length;
                _poolIndex--;
            }
        }

        //public uint[] SortMeDaPizza(Vector3 position) // returns count of active paricles
        //{
        //    uint[] particleIndices = new uint[maxParticleBuffer];
        //    for (uint i = 0; i < particleIndices.Length; i++)
        //    {
        //        particleIndices[i] = i;
        //    }
        //    Array.Sort(particleIndices, delegate (uint x, uint y)
        //    {
        //        if (particlePool[x].active || particlePool[y].active) return 0;
        //        float xlen = Vector3.DistanceSquared(position, particlePool[x].position);
        //        float ylen = Vector3.DistanceSquared(position, particlePool[y].position);
        //        if (xlen < ylen) return -1;
        //        else if (xlen > ylen) return 1;
        //        else return 0;
        //    });
        //    return particleIndices;
        //}
    }

    class ParticleProperties
    {
        //public Vector3 velocity, velocityVariation;
        //
        //public Vector4 colorBegin, colorEnd;
        //
        //public float sizeBegin, sizeEnd;
        //
        //public float lifeTime, lifeTimeVariation;
        //
        //public float gravityEffect;
        //
        //public float rotation, rotationVariation, rotationVelocity, rotationVelocityVariation;

        //sizeVariation

        public Vector3 velocity, velocityVariation;

        public Gradient<Vector4> colorGradient = new Gradient<Vector4>();
        public Vector3 colorVariation; // hsv

        public Gradient<float> sizeGradient = new Gradient<float>();
        public float sizeVariation;

        public float lifeTime, lifeTimeVariation;

        public float gravityEffect;

        public float rotation, rotationVariation, rotationVelocity, rotationVelocityVariation;

        public static ParticleProperties CreateDefault()
        {
            ParticleProperties pp = new ParticleProperties();

            pp.velocity = Vector3.Zero + Vector3Extension.RandomCube() * 10;
            pp.velocityVariation = Vector3.Zero;

            pp.colorGradient.AddElement(0, Vector4.One);
            pp.colorVariation = Vector3.Zero;

            pp.sizeGradient.AddElement(0, 1);
            pp.sizeGradient.AddElement(1, 0);

            pp.lifeTime = 1.0f;

            return pp;
        }
    }

    class ParticleEmmiter
    {
        private static Random random = new Random();

        public static Particle GenerateParticle(ParticleProperties particleProps)
        {
            Particle particle = new Particle();

            particle.active = true;

            // gravity
            particle.gravityEffect = particleProps.gravityEffect;
            // size
            particle.size = particleProps.sizeGradient;
            particle.sizeVariation = (float)random.NextDouble() * particleProps.sizeVariation;
            // color
            particle.color = particleProps.colorGradient;
            if (particleProps.colorVariation != Vector3.Zero)
            {
                particle.colorVariation.X = ((float)random.NextDouble() - 0.5f) * particleProps.colorVariation.X;
                particle.colorVariation.Y = ((float)random.NextDouble() * 2.0f - 1.0f) * particleProps.colorVariation.Y;
                particle.colorVariation.Z = ((float)random.NextDouble() * 2.0f - 1.0f) * particleProps.colorVariation.Z;
            }
            // position
            // TODO: use meaningful position
            particle.position = Vector3.Zero;

            // velocity
            particle.velocity = Vector3Extension.RandomNormalized() * particleProps.velocityVariation + particleProps.velocity;
            // rotation
            particle.rotation = ((float)random.NextDouble() * 2.0f - 1.0f) * (float)Math.PI * particleProps.rotationVariation + particleProps.rotation;
            particle.rotationVelocity = ((float)random.NextDouble() * 2.0f - 1.0f) * particleProps.rotationVelocityVariation + particleProps.rotationVelocity;
            // life
            float randomLife = (float)random.NextDouble() * particleProps.lifeTimeVariation * particleProps.lifeTime;
            particle.life = particleProps.lifeTime - randomLife;
            particle.lifeRemaining = particleProps.lifeTime - randomLife;

            return particle;
        }
    }
}
