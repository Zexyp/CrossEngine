using CrossEngine.Components;
using CrossEngine.Core;
using CrossEngine.Ecs;
using CrossEngine.Scenes;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Loader;
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
        public float timer = 2;

        public const float SpawnPosition = 16;
        internal static bool stop = false;
        internal static float speed = 4;
        internal static bool start = false;

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

            timeAgregate = timer;
        }

        protected override void OnUpdate()
        {
            if (stop || !start)
                return;

            SampleApp.Distance += speed * Time.DeltaF;
        }

        protected override void OnFixedUpdate()
        {
            if (stop || !start)
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
