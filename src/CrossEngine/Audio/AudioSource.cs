using System;
using static OpenAL.AL;
using static OpenAL.ALC;

using System.Numerics;

namespace CrossEngine.Audio
{
	public class AudioSource
	{
		public uint id = 0;

        #region Properties
        public bool Looping
		{
			get
			{
				int looping;
				alGetSourcei(id, AL_LOOPING, out looping);
				return looping == AL_TRUE;
			}
			set
			{
				if (value) alSourcei(id, AL_LOOPING, AL_TRUE);
				else alSourcei(id, AL_LOOPING, AL_FALSE);
			}
		}
		public AudioSourceState State
		{
			get
			{
				int state;
				alGetSourcei(id, AL_SOURCE_STATE, out state);
				return (AudioSourceState)state;
			}
		}
		public float Volume
        {
			set
            {
				alSourcef(id, AL_GAIN, value);
			}
        }
		public float Pitch
		{
			set
			{
				alSourcef(id, AL_PITCH, value);
			}
		}
		public Vector3 Position
        {
            set
            {
				alSource3f(id, AL_POSITION, value.X, value.Y, value.Z);
			}
        }
		public Vector3 Velocity
		{
			set
			{
				alSource3f(id, AL_VELOCITY, value.X, value.Y, value.Z);
			}
		}
		#endregion

		public AudioSource()
		{
			uint[] sources = new uint[1];
			alGenSources(1, sources);
			id = sources[0];

			alSourcef(id, AL_GAIN, 1.0f);
			alSourcef(id, AL_PITCH, 1.0f);
			alSource3f(id, AL_POSITION, 0, 0, 0);
		}

		public void BindBuffer(AudioBuffer buffer)
		{
			alSourcei(id, AL_BUFFER, (int)buffer.id);
		}

		public void Play()
		{
			alSourcePlay(id);
		}

		public void Stop()
		{
			alSourceStop(id);
		}

		public void Pause()
		{
			alSourcePause(id);
		}
	}

	public enum AudioSourceState
	{
		Playing = AL_PLAYING,
		Stopped = AL_STOPPED,
		Paused = AL_PAUSED,
		Initial = AL_INITIAL
	}
}
