using System;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Physics;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.ComponentSystems;
using CrossEngine.Serialization;

namespace CrossEngine.FX.Particles
{
    public class ParticleProperties : ISerializable
    {
        [EditorSection("Life")]
        [EditorDrag(Min = 0)]
        public float lifeTime;
        [EditorDrag(Min = 0, Max = 1)]
        public float lifeTimeVariation;

        [EditorSection("Velocity")]
        [EditorDrag]
        public Vector3 velocity;
        [EditorDrag]
        public Vector3 velocityVariation;
        [EditorDrag]
        public float gravityEffect;

        [EditorSection("Color")]
        [EditorGradient]
        public Gradient<Vector4> colorGradient = new Gradient<Vector4>((0, Vector4.One));
        [EditorDrag]
        public Vector3 colorVariation; // hsv

        [EditorSection("Size")]
        [EditorGradient]
        public Gradient<float> sizeGradient = new Gradient<float>(1, 0);
        [EditorDrag]
        public float sizeVariation;

        [EditorSection("Rotation")]
        [EditorDrag]
        public float rotation;
        [EditorDrag]
        public float rotationVariation, rotationVelocity, rotationVelocityVariation;

        public static ParticleProperties CreateSimple()
        {
            ParticleProperties pp = new ParticleProperties();

            pp.velocity = Vector3.Zero;
            pp.velocityVariation = Vector3.One * 5;

            pp.colorVariation = Vector3.Zero;

            pp.lifeTime = 1.0f;

            return pp;
        }

        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue(nameof(lifeTime), lifeTime);
            info.AddValue(nameof(lifeTimeVariation), lifeTimeVariation);
            info.AddValue(nameof(velocity), velocity);
            info.AddValue(nameof(velocityVariation), velocityVariation);
            info.AddValue(nameof(gravityEffect), gravityEffect);
            info.AddValue(nameof(colorGradient), colorGradient);
            info.AddValue(nameof(colorVariation), colorVariation);
            info.AddValue(nameof(sizeGradient), sizeGradient);
            info.AddValue(nameof(sizeVariation), sizeVariation);
            info.AddValue(nameof(rotation), rotation);
            info.AddValue(nameof(rotationVariation), rotationVariation);
            info.AddValue(nameof(rotationVelocity), rotationVelocity);
            info.AddValue(nameof(rotationVelocityVariation), rotationVelocityVariation);
        }

        public void SetObjectData(SerializationInfo info)
        {
            lifeTime = info.GetValue(nameof(lifeTime), lifeTime);
            lifeTimeVariation = info.GetValue(nameof(lifeTimeVariation), lifeTimeVariation);
            velocity = info.GetValue(nameof(velocity), velocity);
            velocityVariation = info.GetValue(nameof(velocityVariation), velocityVariation);
            gravityEffect = info.GetValue(nameof(gravityEffect), gravityEffect);
            colorGradient = info.GetValue(nameof(colorGradient), colorGradient);
            colorVariation = info.GetValue(nameof(colorVariation), colorVariation);
            sizeGradient = info.GetValue(nameof(sizeGradient), sizeGradient);
            sizeVariation = info.GetValue(nameof(sizeVariation), sizeVariation);
            rotation = info.GetValue(nameof(rotation), rotation);
            rotationVariation = info.GetValue(nameof(rotationVariation), rotationVariation);
            rotationVelocity = info.GetValue(nameof(rotationVelocity), rotationVelocity);
            rotationVelocityVariation = info.GetValue(nameof(rotationVelocityVariation), rotationVelocityVariation);
        }
    }    
}
