using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Core;
using CrossEngine.Ecs;
using CrossEngine.Inputs;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components
{
    public class BirdComponent : ScriptComponent
    {
        [EditorDisplay]
        public float veloctity = 0;
        [EditorDrag]
        public float gravity = -1f;
        [EditorDrag]
        public float mass = 5;
        [EditorAsset]
        public SpriteAsset spriteFalling;
        [EditorAsset]
        public SpriteAsset spritePush;
        [EditorAsset]
        public SpriteAsset spriteIdle;
        [EditorAsset]
        public float hitboxSize = .5f;

        protected override void OnUpdate()
        {
            veloctity += gravity * Time.DeltaF;

            if (Input.GetKeyDown(Key.Space) && !PipeManagerComponent.stop)
                veloctity = .5f;

            Entity.Transform.Position += new Vector3(0, veloctity, 0) / mass;
            Entity.Transform.Position = Vector3.Clamp(Entity.Transform.Position, new Vector3(0, -5, 0), new Vector3(0, 5, 0));

            if (PipeManagerComponent.stop)
                return;

            Entity.Transform.EulerRotation = new Vector3(0, 0, veloctity) * MathExtension.ToDegConstF;

            if (!Entity.TryGetComponent<SpriteRendererComponent>(out var spriteRender))
                return;

            if (veloctity < 0)
                spriteRender.Sprite = spriteFalling;
            else if (Input.GetKey(Key.Space))
                spriteRender.Sprite = spritePush;
            else
                spriteRender.Sprite = spriteIdle;

            var pos = Entity.Transform.Position;
            foreach (var ent in SceneManager.Current.Entities.Where(e => e.HasComponent<PipeComponent>()))
            {
                if (ent.TryGetComponent<SpriteRendererComponent>(out var sprite))
                {
                    var compare = ent.Transform.Position + new Vector3(sprite.DrawOffsets.X, sprite.DrawOffsets.Y, 0);
                    if (pos.X + hitboxSize / 2 > compare.X - sprite.DrawOffsets.Z / 2 &&
                        pos.X - hitboxSize / 2 < compare.X + sprite.DrawOffsets.Z / 2 &&
                        pos.Y + hitboxSize / 2 > compare.Y - sprite.DrawOffsets.W / 2 &&
                        pos.Y - hitboxSize / 2 < compare.Y + sprite.DrawOffsets.W / 2)
                    {
                        veloctity = .5f;
                        PipeManagerComponent.stop = true;
                    }
                }
            }
        }
    }
}
