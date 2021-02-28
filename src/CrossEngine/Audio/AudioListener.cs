using System;
using static OpenAL.AL;
using static OpenAL.ALC;

using System.Numerics;

namespace CrossEngine.Audio
{
    public static class AudioListener
    {
        public static Vector3 Position
        {
            set
            {
                alListener3f(AL_POSITION, value.X, value.Y, value.Z);
            }
        }
        public static Vector3 Velocity
        {
            set
            {
                alListener3f(AL_VELOCITY, value.X, value.Y, value.Z);
            }
        }

        public static void SetOrientation(Vector3 front, Vector3 up)
        {
            alListenerfv(AL_ORIENTATION, new float[6] { front.X, front.Y, front.Z, up.X, up.Y, up.Z });
        }
    }
}
