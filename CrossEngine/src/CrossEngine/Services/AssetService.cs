using CrossEngine.Assets;
using CrossEngine.Assets.Loaders;
using CrossEngine.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Services
{
    public class AssetService : Service
    {
        private RenderService rser;
        internal static Logger Log = new Logger("assets");
        readonly private List<Loader> _loaders = new List<Loader>();
        readonly public ReadOnlyCollection<Loader> Loaders;

        public AssetService()
        {
            Loaders = _loaders.AsReadOnly();
        }

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

        public async Task LoadAsync(AssetPool assets)
        {
            await assets.LoadAll();
        }

        public async Task UnloadAsync(AssetPool assets)
        {
            await assets.UnloadAll();
        }

        public override void OnStart()
        {
            AssetManager.service = this;

            AddLoader(new TextureLoader());
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
