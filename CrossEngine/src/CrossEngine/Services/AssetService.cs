using CrossEngine.Assets;
using CrossEngine.Assets.Loaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Services
{
    public class AssetService : Service
    {
        private RenderService rser;
        private List<Loader> _loaders = new List<Loader>();

        public void AddLoader(Loader loader)
        {
            Debug.Assert(!_loaders.Contains(loader));

            _loaders.Add(loader);
            if (loader is GpuLoader gl)
                gl.Emit += OnLoaderEmit;
        }

        public void RemoveLoader(Loader loader)
        {
            Debug.Assert(_loaders.Contains(loader));

            if (loader is GpuLoader gl)
                gl.Emit -= OnLoaderEmit;
            _loaders.Remove(loader);
        }

        public void Load(AssetPool assets)
        {
            assets.Load(_loaders.ToArray());
        }

        public void Unload(AssetPool assets)
        {
            assets.Unload(_loaders.ToArray());
        }

        public override void OnStart()
        {
            AssetManager.service = this;
        }

        public override void OnDestroy()
        {
            AssetManager.service = null;
        }

        public override void OnDetach()
        {
            for (int i = 0; i < _loaders.Count; i++)
            {
                _loaders[i].Shutdown();
            }

            rser = null;
        }

        public override void OnAttach()
        {
            rser = Manager.GetService<RenderService>();

            for (int i = 0; i < _loaders.Count; i++)
            {
                _loaders[i].Init();
            }
        }

        private void OnLoaderEmit(GpuLoader sender, Action action) => rser.Execute(action);
    }
}
