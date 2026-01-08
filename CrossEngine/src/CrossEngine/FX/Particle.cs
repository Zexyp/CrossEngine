using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Utils.Collections;

namespace CrossEngine.FX.Particles
{
    // needed data
    //  
    // active
    // life
    // life remaining
    // color -- gradiented
    // color variation (hsv)
    // position
    // velocity
    // gravity effect
    // rotation
    // rotation velocity
    // scale -- gradiented
    // scale variation

    public struct Particle
    {
        public bool active;
        public float life;
        public float lifeRemaining;

        public Vector3 position;
        public Vector3 velocity;
        public float gravityEffect;

        public float rotation;
        public float rotationVelocity;

        public Gradient<float> size;
        public float sizeVariation;

        public Gradient<Vector4> color;
        public Vector4 colorVariation; // in HSV
    }
}
