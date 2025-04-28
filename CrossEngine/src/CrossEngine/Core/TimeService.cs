using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine;
using CrossEngine.Profiling;
using CrossEngine.Services;

namespace CrossEngine.Core
{
    public class TimeService : Service, IUpdatedService
    {
        Stopwatch sw = new Stopwatch();

        public event Action<TimeService> FixedUpdate;

        public override void OnStart()
        {
            sw.Start();
        }

        public override void OnDestroy()
        {
            sw.Stop();
        }

        public void OnUpdate()
        {
            Time.UnscaledDelta = sw.Elapsed.TotalSeconds;
            Time.UnscaledElapsed += Time.UnscaledDelta;
            Time.Delta = Math.Min(Time.UnscaledDelta * Time.Scale, Time.MaximumDelta);
            Time.Elapsed += Time.Delta;

            while (Time.FixedElapsed < Time.Elapsed)
            {
                Time.FixedUnscaledElapsed += Time.FixedUnscaledDelta;
                Time.FixedDelta = Time.FixedUnscaledDelta * Time.Scale;
                Time.FixedElapsed += Time.FixedDelta;

                Profiler.BeginScope("FixedUpdate");
                FixedUpdate?.Invoke(this);
                Profiler.EndScope();
            }

            sw.Reset();
            sw.Start();
        }

        public override void OnAttach()
        {
            
        }

        public override void OnDetach()
        {
            
        }

        public void Trim()
        {
            Time.Elapsed = 0;
            Time.UnscaledElapsed = 0;
            Time.FixedElapsed = 0;
            Time.FixedUnscaledElapsed = 0;
        }
    }
}
