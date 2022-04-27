using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering;

namespace CrossEngine.FX.Particles
{
    public abstract class ParticleEmitter
    {
        public static Vector4 RepresentationColor = new Vector4(1, .9f, .1f, 1);

        public abstract void Emit(ref Particle particle);

        public virtual void DebugDraw(Matrix4x4 transform) { }
    }
    
    //public class PointParticleEmitter : ParticleEmitter
    //{
    //    public override void Emit(ref Particle particle)
    //    {
    //        particle.position = Vector3.Zero;
    //    }
    //}

    public class BoxParticleEmitter : ParticleEmitter
    {
        [EditorDrag(Min = 0)]
        public Vector3 Size = Vector3.One;

        public override void Emit(ref Particle particle)
        {
            particle.position = Vector3Extension.RandomCubeVolume() * Size;
        }

        public override void DebugDraw(Matrix4x4 transform)
        {
            LineRenderer.DrawBox(Matrix4x4.CreateScale(Size * 2) * transform, ParticleEmitter.RepresentationColor);
        }
    }

    public class SphereParticleEmitter : ParticleEmitter
    {
        [EditorDrag(Min = 0)]
        public Vector3 Size = Vector3.One;

        public override void Emit(ref Particle particle)
        {
            particle.position = Vector3Extension.RandomSphereVolume() * Size;
        }

        public override void DebugDraw(Matrix4x4 transform)
        {
            LineRenderer.DrawSphere(Matrix4x4.CreateScale(Size) * transform, ParticleEmitter.RepresentationColor);
        }
    }
}
