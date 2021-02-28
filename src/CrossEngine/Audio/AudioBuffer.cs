using System;
using static OpenAL.AL;
using static OpenAL.ALC;

namespace CrossEngine.Audio
{
	public class AudioBuffer
	{
		public uint id = 0;
		public AudioBuffer(IntPtr dataArray, int length, AudioFormat format, int rate)
		{
			uint[] buffers = new uint[1];
			alGenBuffers(1, buffers);
			id = buffers[0];
			SetData(dataArray, length, format, rate);
		}

		public void Dispose()
		{
			alDeleteBuffers(1, new uint[] { id });
		}

		public void SetData(IntPtr dataArray, int length, AudioFormat format, int rate)
		{
			alBufferData(id, (int)format, dataArray, length, rate);
		}
	}

	public enum AudioFormat
	{
		Mono8bit = AL_FORMAT_MONO8,
		Mono16bit = AL_FORMAT_MONO16,
		Stereo8bit = AL_FORMAT_STEREO8,
		Stereo16bit = AL_FORMAT_STEREO16
	}
}
