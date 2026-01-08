using System;
using CrossEngine.Components;
using CrossEngine.Inputs;
using CrossEngine.Scenes;
using CrossEngine.Utils;

namespace Sample.Components
{
    public class SceneSetupComponent : ScriptComponent
    {
        void OnAttach()
        {
            SceneManager.Current.RenderData.ClearColor = ColorHelper.U32ToVec4(0xffb1d1ff);
        }

        void OnUpdate()
        {
            if (Input.GetKeyDown(Key.Space) || Input.GetMouseDown(Button.Left))
            {
                PipeManagerComponent.start = true;
            }
        }
    }
}
