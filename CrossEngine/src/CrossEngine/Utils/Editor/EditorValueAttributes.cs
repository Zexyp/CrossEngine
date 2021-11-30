using System;
using ImGuiNET;

using System.Numerics;

namespace CrossEngine.Utils.Editor
{
    public enum NumberInputTypeRepresentation
    {
        Drag,
        Input,
        Slider,
    }

    // maybe add generics later
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorValueAttribute : Attribute
    {
        public string Name = null;

        public EditorValueAttribute()
        {

        }

        public EditorValueAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class EditorNumberValueAttribute : EditorValueAttribute
    {
        public NumberInputTypeRepresentation NumberInputType = NumberInputTypeRepresentation.Drag;
        public float Max = float.MaxValue;
        public float Min = float.MinValue;
        public float Step = 0.1f;

        public EditorNumberValueAttribute() { }

        public EditorNumberValueAttribute(
            string name = null,
            NumberInputTypeRepresentation numberInputType = NumberInputTypeRepresentation.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name) { }
    }

    #region Number
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorInt32ValueAttribute : EditorNumberValueAttribute
    {
        public EditorInt32ValueAttribute() { }
        public EditorInt32ValueAttribute(string name) : base(name) { }
        public EditorInt32ValueAttribute(
            string name = null,
            NumberInputTypeRepresentation numberInputType = NumberInputTypeRepresentation.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name, numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorSingleValueAttribute : EditorNumberValueAttribute
    {
        public EditorSingleValueAttribute() { }
        public EditorSingleValueAttribute(string name) : base(name) { }
        public EditorSingleValueAttribute(
            string name = null,
            NumberInputTypeRepresentation numberInputType = NumberInputTypeRepresentation.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name, numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorVector2ValueAttribute : EditorNumberValueAttribute
    {
        public EditorVector2ValueAttribute() { }
        public EditorVector2ValueAttribute(string name) : base(name) { }
        public EditorVector2ValueAttribute(
            string name = null,
            NumberInputTypeRepresentation numberInputType = NumberInputTypeRepresentation.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name, numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorVector3ValueAttribute : EditorNumberValueAttribute
    {
        public EditorVector3ValueAttribute() { }
        public EditorVector3ValueAttribute(string name) : base(name) { }
        public EditorVector3ValueAttribute(
            string name = null,
            NumberInputTypeRepresentation numberInputType = NumberInputTypeRepresentation.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name, numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorVector4ValueAttribute : EditorNumberValueAttribute
    {
        public EditorVector4ValueAttribute() { }
        public EditorVector4ValueAttribute(string name) : base(name) { }
        public EditorVector4ValueAttribute(
            string name = null,
            NumberInputTypeRepresentation numberInputType = NumberInputTypeRepresentation.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name, numberInputType, max, min, step) { }
    }
    #endregion

    #region Color
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorColor3ValueAttribute : EditorValueAttribute
    {
        public EditorColor3ValueAttribute() { }
        public EditorColor3ValueAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorColor4ValueAttribute : EditorValueAttribute
    {
        public EditorColor4ValueAttribute() { }
        public EditorColor4ValueAttribute(string name) : base(name) { }
    }
    #endregion

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorStringValueAttribute : EditorValueAttribute
    {
        public EditorStringValueAttribute() { }
        public EditorStringValueAttribute(string name) : base(name) { }

        public EditorStringValueAttribute(string name, uint maxLength) : base(name)
        {
            MaxLength = maxLength;
        }

        public EditorStringValueAttribute(uint maxLength)
        {
            MaxLength = maxLength;
        }

        public uint MaxLength = 256;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorBooleanValueAttribute : EditorValueAttribute
    {
        public EditorBooleanValueAttribute() { }
        public EditorBooleanValueAttribute(string name) : base(name) { }
    }

    public class EditorEnumValueAttribute : EditorValueAttribute
    {
        public EditorEnumValueAttribute() { }
        public EditorEnumValueAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorAssetValueAttribute : EditorValueAttribute
    {
        public Type Type;

        public EditorAssetValueAttribute(Type type)
        {
            Type = type;
        }
    }
}
