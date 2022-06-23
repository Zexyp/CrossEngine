using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Events;
using CrossEngine.ComponentSystems;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Components;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class TextRendererComponent : UIComponent, ITextRenderData
    {
        [EditorString]
        public string Text { get; set; } = "";
        [EditorColor]
        public Vector4 Color { get; set; } = Vector4.One;
        Matrix4x4 IObjectRenderData.Transform => Entity.Transform?.WorldTransformMatrix ?? Matrix4x4.Identity;

        protected internal override void Attach(World world)
        {
            world.GetSystem<TextRendererSystem>().Register(this);
        }

        protected internal override void Detach(World world)
        {
            world.GetSystem<TextRendererSystem>().Unregister(this);
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            info.AddValue(nameof(Text), Text);
            info.AddValue(nameof(Color), Color);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            Text = info.GetValue(nameof(Text), Text);
            Color = info.GetValue(nameof(Color), Color);
        }
    }
}

namespace CrossEngine.Rendering.Renderables
{
    interface ITextRenderData : IObjectRenderData
    {
        string Text { get; }
        public Vector4 Color { get; }
    }

    class TextRenderable : Renderable<ITextRenderData>
    {
        public override void Submit(ITextRenderData data)
        {
            TextRendererUtil.DrawText(data.Transform, data.Text, data.Color, ((TextRendererComponent)data).Entity.Id);
        }
    }
}
