using System;
using System.Collections.Generic;
using System.Numerics;
using CrossEngine.Rendering;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.Gui;
using ImGuiNET;

namespace CrossEngineEditor.Panels;

internal class TestWidgetPanel : EditorPanel
{
    public struct TestStruct
    {
        [EditorValue] public float Position;
        [EditorColor] public Vector4 Color;

        public override string ToString() => ReflectionUtil.DumpFileds(this);
    }
    
    [EditorSection("Hello")]
    [EditorValue] public float Value;
    [EditorDrag] public float ValueDrag { get => Value; set => Value = value; }
    [EditorSlider(Min = 0, Max = 10)] public float ValueSlider { get => Value; set => Value = value; }
    [EditorDisplay] public float ValueDisplay => Value;
    [EditorEnum] public ConsoleKey Key;
    [EditorNullable] [EditorString] public string Name;
    [EditorInnerDraw] public TestStruct structure;
    [EditorList] public List<Vector4> VecListitko = new() {VecColor.Red, VecColor.Gray};
    [EditorList] public TestStruct[] ArrayListitko = new TestStruct[1] { new() { Position = 1, Color = ColorHelper.U32ToVec4(0x7C05A300)} };
    private Gradient<Vector4> gradient = new Gradient<Vector4>();

    protected override void DrawWindowContent()
    {
        InspectDrawer.Inspect(this);
        ImGradient.Manipulate(gradient);
    }
}