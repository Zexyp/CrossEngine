﻿using CrossEngine.Logging;
using CrossEngine.Services;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class AssetService : Service, IScheduledService, IUpdatedService
    {
        SingleThreadedTaskScheduler _scheduler = new();

        public Task Execute(Action action) => _scheduler.Schedule(action);

        public TaskScheduler GetScheduler() => _scheduler;

        public override void OnAttach()
        {
            Debug.Assert(AssetManager.ServiceRequest == null);
            AssetManager.ServiceRequest = Execute;
        }

        public override void OnDetach()
        {
            Debug.Assert(AssetManager.ServiceRequest == Execute);
            AssetManager.ServiceRequest = null;
        }

        public override void OnDestroy()
        {
        }

        public override void OnStart()
        {
        }

        public void OnUpdate()
        {
            _scheduler.RunOnCurrentThread();
        }
    }
}
