using CrossEngine.Logging;
using CrossEngine.Core.Services;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering;

namespace CrossEngine.Assets
{
    public class AssetService : Service, IScheduledService, IUpdatedService
    {
        SingleThreadedTaskScheduler _scheduler = new();

        public Task Execute(Action action) => _scheduler.Schedule(action);
        public Task<TResult> Execute<TResult>(Func<TResult> func) => _scheduler.Schedule(func);

        public TaskScheduler GetScheduler() => _scheduler;

        public override void OnAttach()
        {
            AssetManager.LoadRequest = LoadAssets;
            AssetManager.UnloadRequest = UnloadAssets;
        }

        public override void OnDetach()
        {
            AssetManager.LoadRequest = null;
            AssetManager.UnloadRequest = null;
        }

        public override void OnInit()
        {
            //_scheduler.RunOnCurrentThread(); maybe?
        }
        
        public override void OnDestroy()
        {
            _scheduler.RunOnCurrentThread();
        }

        public void OnUpdate()
        {
            _scheduler.RunOnCurrentThread();
        }

        internal Task LoadAssets(AssetList list)
        {
            Debug.Assert(list != null);
            
            list.Context.Graphics = Manager.GetService<RenderService>().MainSurface.Context;
            return Execute(list.LoadAll);
        }
        
        internal Task UnloadAssets(AssetList list)
        {
            Debug.Assert(list != null);
            
            return Execute(list.UnloadAll).ContinueWith(t => list.Context.Graphics = null);
        }
    }
}
