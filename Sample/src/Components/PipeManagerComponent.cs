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
using static System.Formats.Asn1.AsnWriter;

namespace Sample.Components
{
    internal class PipeManagerComponent : ScriptComponent
    {
        [EditorString]
        public string topTag;
        [EditorString]
        public string bottomTag;
        [EditorDrag]
        public float timer = 3;

        public const float SpawnPosition = 10;
        public static bool stop = false;
        public static float speed = 3;
        internal static float distance = 0;

        Random random = new Random();
        Entity top;
        Entity bottom;
        float timeAgregate;

        protected override void OnAttach()
        {
            foreach (var ent in SceneManager.Current.Entities)
            {
                if (ent.TryGetComponent<TagComponent>(out var tag))
                {
                    if (tag.Tag == topTag)
                        top = ent;
                    if (tag.Tag == bottomTag)
                        bottom = ent;
                }
            }
        }

        protected override void OnUpdate()
        {
            distance += speed * Time.DeltaF;
        }

        protected override void OnFixedUpdate()
        {
            if (stop)
                return;

            timeAgregate += (float)Time.FixedUnscaledDelta;
            if (timeAgregate >= timer)
            {
                timeAgregate -= timer;

                Spawn();
            }
        }

        private void Spawn()
        {
            void Add(Entity src, float offsetY = 0)
            {
                var clone = (Entity)src.Clone();
                clone.GetComponent<PipeComponent>().Enabled = true;
                clone.Transform.Position = new Vector3(SpawnPosition, clone.Transform.Position.Y + offsetY, clone.Transform.Position.Z);
                SceneManager.Current.AddEntity(clone);
            }
            var r = ((float)random.NextDouble() * 2 - 1) * 3;
            Add(top, r);
            Add(bottom, r);
        }
    }
}
