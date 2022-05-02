using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.FX.Particles;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.Physics;
using CrossEngine.Logging;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class ParticleSystemComponent : Component, IParticleSystemRenderData
    {
        public enum ParticleSpace
        {
            Global,
            Local,
        }
        public enum ParticleEmitterType
        {
            Unknown = default,
            Box,
            Sphere,
        }

        private uint _particlePoolSize = 1000;

        [EditorDragInt(Min = 1)]
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

        private ParticleEmitterType _emitterType;

        [EditorEnum]
        public ParticleSpace Space = ParticleSpace.Global;

        [EditorEnum]
        public ParticleEmitterType EmitterType
        {
            get => _emitterType;
            set
            {
                _emitterType = value;
                switch (_emitterType)
                {
                    case ParticleEmitterType.Box: _emitter = new BoxParticleEmitter();
                        break;
                    case ParticleEmitterType.Sphere: _emitter = new SphereParticleEmitter();
                        break;
                }
            }
        }

        [EditorInnerDraw("Properties")]
        public ParticleProperties Properties = ParticleProperties.CreateSimple();
        private ParticleEmitter _emitter;
        [EditorInnerDraw("Emitter")]
        public ParticleEmitter Emitter
        {
            get => _emitter;
            set
            {
                _emitter = value;
                if (_emitter is BoxParticleEmitter) _emitterType = ParticleEmitterType.Box;
                else if (_emitter is SphereParticleEmitter) _emitterType = ParticleEmitterType.Sphere;
                else _emitterType = ParticleEmitterType.Unknown;
            }
        }

        private Particle[] _particlePool;
        private uint _poolIndex = 0;

        //private TextureAtlas textureAtlas;
        //public bool animated
        //public func BlendMode

        public ParticleSystemComponent()
        {
            ParticlePoolSize = 1000;
            _particlePool = new Particle[ParticlePoolSize];

            Emitter = new BoxParticleEmitter();
        }

        protected internal override void Attach()
        {
            ParticleSystemSystem.Instance.Register(this);
        }

        protected internal override void Detach()
        {
            ParticleSystemSystem.Instance.Unregister(this);
        }

        protected internal override void Update()
        {
            Emit();
            Update(Time.DeltaTimeF);
        }

        public void Emit(int count = 1)
        {
            if (Properties == null || Emitter == null)
            {
                Log.Core.Warn($"particel system has no particle properties or emitter assigned (entity id: {Entity.Id})");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                // use emitter add to pool
                Particle particle = GenerateParticle();

                Emitter.Emit(ref particle);
                particle.active = true;
                if (Space == ParticleSpace.Global)
                    particle.position = Vector3.Transform(particle.position, Entity.Transform.WorldTransformMatrix);
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

        void Update(float timestep)
        {
            var gravity = PhysicsInterface.Gravity;
            if (Space == ParticleSpace.Local)
                gravity = Vector3.Transform(gravity, Quaternion.Inverse(Entity.Transform.Rotation));
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

        void IParticleSystemRenderData.Render(Matrix4x4 viewMatrix)
        {
            viewMatrix.Translation = Vector3.Zero;
            viewMatrix = Matrix4x4Extension.Invert(viewMatrix);
            var cameraRight = Vector3.Transform(Vector3.UnitX, viewMatrix);
            var cameraUp = Vector3.Transform(Vector3.UnitY, viewMatrix);
            var cameraLook = Entity.Transform.Position;

            for (int i = 0; i < _particlePool.Length; i++)
            {
                var particle = _particlePool[i];
                if (!particle.active)
                    continue;

                float life = 1 - particle.lifeRemaining / particle.life;

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
                matrix *= Matrix4x4.CreateRotationZ(particle.rotation) * Matrix4x4Extension.CreateBillboard(cameraRight, cameraUp, cameraLook, particle.position);
                if (Space == ParticleSpace.Local)
                    matrix *= Entity.Transform.WorldTransformMatrix;
                Renderer2D.DrawQuad(matrix, color, Entity.Id);
            }

            Emitter?.DebugDraw(Entity.Transform.WorldTransformMatrix);
        }

        private Random random = new Random();
        private Particle GenerateParticle()
        {
            Particle particle = new Particle();

            particle.active = true;

            // gravity
            particle.gravityEffect = Properties.gravityEffect;
            // size
            particle.size = Properties.sizeGradient;
            particle.sizeVariation = (float)random.NextDouble() * Properties.sizeVariation;
            // color
            particle.color = Properties.colorGradient;
            if (Properties.colorVariation != Vector3.Zero)
            {
                particle.colorVariation.X = ((float)random.NextDouble() - 0.5f) * Properties.colorVariation.X;
                particle.colorVariation.Y = ((float)random.NextDouble() * 2.0f - 1.0f) * Properties.colorVariation.Y;
                particle.colorVariation.Z = ((float)random.NextDouble() * 2.0f - 1.0f) * Properties.colorVariation.Z;
            }
            // position
            // TODO: use meaningful position
            particle.position = Vector3.Zero;

            // velocity
            particle.velocity = Vector3Extension.RandomSphereVolume() * Properties.velocityVariation + Properties.velocity;
            // rotation
            particle.rotation = ((float)random.NextDouble() * 2.0f - 1.0f) * (float)Math.PI * Properties.rotationVariation + Properties.rotation;
            particle.rotationVelocity = ((float)random.NextDouble() * 2.0f - 1.0f) * Properties.rotationVelocityVariation + Properties.rotationVelocity;
            // life
            float randomLife = (float)random.NextDouble() * Properties.lifeTimeVariation * Properties.lifeTime;
            particle.life = Properties.lifeTime - randomLife;
            particle.lifeRemaining = particle.life;

            return particle;
        }

        //public uint[] SortMeDaPizza(Particle[] particles, Vector3 position) // returns count of active paricles
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

        protected internal override void Serialize(SerializationInfo info)
        {
            var sussy = new System.Diagnostics.StackFrame();
            Logging.Log.Core.Debug($"Impl this: in {sussy.GetFileName()} at line {sussy.GetFileLineNumber()}");
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            var sussy = new System.Diagnostics.StackFrame();
            Logging.Log.Core.Debug($"Impl this: in {sussy.GetFileName()} at line {sussy.GetFileLineNumber()}");
        }
    }
}
