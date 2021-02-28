using System;

using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngine.FX.Particles
{
    public class ParticleProperties
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

        public Gradient colorGradient = new Gradient();
        public Vector3 hsvVariation; // hsv

        public Gradient sizeGradient = new Gradient();
        public float sizeVariation;

        public float lifeTime, lifeTimeVariation;

        public float gravityEffect;

        public float rotation, rotationVariation, rotationVelocity, rotationVelocityVariation;

        public static ParticleProperties Default
        {
            get
            {
                ParticleProperties pp = new ParticleProperties();

                pp.velocity = Vector3.Zero;
                pp.velocityVariation = Vector3.Zero;

                pp.colorGradient = Gradient.Default;
                pp.hsvVariation = Vector3.Zero;

                pp.sizeGradient = Gradient.InvertedDefault;

                pp.lifeTime = 1.0f;

                return pp;
            }
        }
    }
}
