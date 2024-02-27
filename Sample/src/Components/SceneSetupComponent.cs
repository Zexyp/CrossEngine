using System;
using CrossEngine.Components;
using CrossEngine.Scenes;
using CrossEngine.Utils;

namespace Sample.Components
{
    public class SceneSetupComponent : ScriptComponent
    {
        protected override void OnAttach()
        {
            SceneManager.Current.RenderData.ClearColor = ColorHelper.U32ToVec4(0xffb1d1ff);
        }
    }
}
