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
            Time.UnscaledDeltaTime = sw.Elapsed.TotalSeconds;
            Time.DeltaTime = Time.TimeScale * Time.UnscaledDeltaTime;
            sw.Reset();
            sw.Start();
            Time.ElapsedTime += Time.UnscaledDeltaTime;
        }
    }
}
