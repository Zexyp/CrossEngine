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

        public override void OnStart() { }
        public override void OnDestroy() { }

        public override void OnUpdate()
        {
            Time.UnscaledDeltaTime = sw.Elapsed.Seconds;
            Time.DeltaTime = Time.TimeScale * Time.UnscaledDeltaTime;
            sw.Reset();
            Time.ElapsedTime += Time.UnscaledDeltaTime;
        }
    }
}
