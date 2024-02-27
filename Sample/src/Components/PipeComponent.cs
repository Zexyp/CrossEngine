using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Core;
using CrossEngine.Ecs;
using CrossEngine.Scenes;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components
{
    public class PipeComponent : ScriptComponent
    {
        static Random random = new Random();

        protected override void OnAttach()
        {
            SpriteAsset[] sprites = new SpriteAsset[]
            {
                AssetManager.GetNamed<SpriteAsset>("spr-pipe-var1"),
                AssetManager.GetNamed<SpriteAsset>("spr-pipe-var2"),
                AssetManager.GetNamed<SpriteAsset>("spr-pipe-var3"),
            };

            Entity.GetComponent<SpriteRendererComponent>().Sprite = sprites[random.Next(sprites.Length)];
        }

        protected override void OnUpdate()
        {
            if (PipeManagerComponent.stop)
                return;

            Entity.Transform.WorldPosition += new Vector3(-PipeManagerComponent.speed, 0, 0) * Time.DeltaF;
            if (Entity.Transform.WorldPosition.X < -PipeManagerComponent.SpawnPosition)
            {
                SceneManager.Current.RemoveEntity(Entity);
            }
        }
    }
}
