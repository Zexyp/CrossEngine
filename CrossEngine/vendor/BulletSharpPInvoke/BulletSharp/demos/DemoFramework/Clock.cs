﻿using System.Diagnostics;

namespace DemoFramework
{
    public class Clock
    {
        private Stopwatch _physicsTimer = new Stopwatch();
        private Stopwatch _renderTimer = new Stopwatch();
        private Stopwatch _frameTimer = new Stopwatch();

        public long FrameCount { get; private set; }
        public long SubStepCount { get; private set; }

        public float PhysicsAverage
        {
            get
            {
                if (FrameCount == 0) return 0;
                return (((float)_physicsTimer.ElapsedTicks / Stopwatch.Frequency) / SubStepCount) * 1000.0f;
            }
        }

        public float RenderAverage
        {
            get
            {
                if (FrameCount == 0) return 0;
                return (((float)_renderTimer.ElapsedTicks / Stopwatch.Frequency) / FrameCount) * 1000.0f;
            }
        }

        public void StartPhysics()
        {
            _physicsTimer.Start();
        }

        public void StopPhysics(int substepsPassed)
        {
            _physicsTimer.Stop();
            SubStepCount += substepsPassed;
        }

        public void StartRender()
        {
            _renderTimer.Start();
        }

        public void StopRender()
        {
            _renderTimer.Stop();
        }

        public float GetFrameDelta()
        {
            FrameCount++;

            float delta = (float)_frameTimer.ElapsedTicks / Stopwatch.Frequency;
            _frameTimer.Restart();
            return delta;
        }

        public void Reset()
        {
            SubStepCount = 0;
            _physicsTimer.Reset();

            FrameCount = 0;
            _renderTimer.Reset();
        }
    }
}
