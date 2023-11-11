using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine;

namespace CrossEngine.Services
{
    internal class TimeSevice : UpdatedService
    {
        Stopwatch sw = new Stopwatch();

        public override void OnStart()
        {
            sw.Start();
        }

        public override void OnDestroy()
        {
            sw.Stop();
        }

        public override void OnUpdate()
        {
            Time.UnscaledDelta = sw.Elapsed.TotalSeconds;
            Time.UnscaledElapsed += Time.UnscaledDelta;
            Time.Delta = Math.Min(Time.UnscaledDelta * Time.Scale, Time.MaximumDelta);
            Time.Elapsed += Time.Delta;

            sw.Reset();
            sw.Start();
        }
    }
}
