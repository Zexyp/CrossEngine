using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components;

public class ScriptCompoent : Component
{
    [SerializeInclude]
    [EditorAsset]
    public ScriptAsset Script;
    
    // TODO: variables

    internal Behaviour Behaviour = null;
}