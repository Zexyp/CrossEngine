using System;
using static OpenAL.AL;
using static OpenAL.ALC;

namespace CrossEngine.Audio
{
    public class AudioManager
    {
		static IntPtr device;
		static IntPtr context;

		public static void Init()
		{
			device = alcOpenDevice(null);
			context = alcCreateContext(device, null);
			alcMakeContextCurrent(context);
		}

		public static void Shutdown()
		{
			// dispose
			if (context != IntPtr.Zero)
			{
				alcMakeContextCurrent(IntPtr.Zero);
				alcDestroyContext(context);
			}
			context = IntPtr.Zero;

			if (device != IntPtr.Zero)
			{
				alcCloseDevice(device);
			}
			device = IntPtr.Zero;
		}
	}
}
