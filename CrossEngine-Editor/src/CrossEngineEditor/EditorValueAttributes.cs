using System;
using ImGuiNET;

using System.Numerics;

namespace CrossEngineEditor
{
    public enum NumberInputTypeRepresentation
    {
        Drag,
        Input,
        Slider,
    }

    // maybe add generics later
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class EditorValueAttribute : Attribute
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

        #region Constructors
        public EditorNumberValueAttribute()
        {

        }

        public EditorNumberValueAttribute(string name) : base(name)
        {

        }

        public EditorNumberValueAttribute(NumberInputTypeRepresentation numberInputType)
        {
            NumberInputType = numberInputType;
        }

        public EditorNumberValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min)
        {
            NumberInputType = numberInputType;
            Max = max;
            Min = min;
        }

        public EditorNumberValueAttribute(float max, float min)
        {
            Max = max;
            Min = min;
        }

        public EditorNumberValueAttribute(float max, float min, float step)
        {
            Max = max;
            Min = min;
            Step = step;
        }

        public EditorNumberValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min, float step)
        {
            NumberInputType = numberInputType;
            Max = max;
            Min = min;
            Step = step;
        }
        #endregion
    }

    #region Number
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorInt32ValueAttribute : EditorNumberValueAttribute
    {
        public EditorInt32ValueAttribute() { }
        public EditorInt32ValueAttribute(string name) : base(name) { }
        public EditorInt32ValueAttribute(NumberInputTypeRepresentation numberInputType) : base(numberInputType) { }
        public EditorInt32ValueAttribute(float max, float min) : base(max, min) { }
        public EditorInt32ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min) : base(numberInputType, max, min) { }
        public EditorInt32ValueAttribute(float max, float min, float step) : base(max, min, step) { }
        public EditorInt32ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min, float step) : base(numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorSingleValueAttribute : EditorNumberValueAttribute
    {
        public EditorSingleValueAttribute() { }
        public EditorSingleValueAttribute(string name) : base(name) { }
        public EditorSingleValueAttribute(NumberInputTypeRepresentation numberInputType) : base(numberInputType) { }
        public EditorSingleValueAttribute(float max, float min) : base(max, min) { }
        public EditorSingleValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min) : base(numberInputType, max, min) { }
        public EditorSingleValueAttribute(float max, float min, float step) : base(max, min, step) { }
        public EditorSingleValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min, float step) : base(numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorVector2ValueAttribute : EditorNumberValueAttribute
    {
        public EditorVector2ValueAttribute() { }
        public EditorVector2ValueAttribute(string name) : base(name) { }
        public EditorVector2ValueAttribute(NumberInputTypeRepresentation numberInputType) : base(numberInputType) { }
        public EditorVector2ValueAttribute(float max, float min) : base(max, min) { }
        public EditorVector2ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min) : base(numberInputType, max, min) { }
        public EditorVector2ValueAttribute(float max, float min, float step) : base(max, min, step) { }
        public EditorVector2ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min, float step) : base(numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorVector3ValueAttribute : EditorNumberValueAttribute
    {
        public EditorVector3ValueAttribute() { }
        public EditorVector3ValueAttribute(string name) : base(name) { }
        public EditorVector3ValueAttribute(NumberInputTypeRepresentation numberInputType) : base(numberInputType) { }
        public EditorVector3ValueAttribute(float max, float min) : base(max, min) { }
        public EditorVector3ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min) : base(numberInputType, max, min) { }
        public EditorVector3ValueAttribute(float max, float min, float step) : base(max, min, step) { }
        public EditorVector3ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min, float step) : base(numberInputType, max, min, step) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorVector4ValueAttribute : EditorNumberValueAttribute
    {
        public EditorVector4ValueAttribute() { }
        public EditorVector4ValueAttribute(string name) : base(name) { }
        public EditorVector4ValueAttribute(NumberInputTypeRepresentation numberInputType) : base(numberInputType) { }
        public EditorVector4ValueAttribute(float max, float min) : base(max, min) { }
        public EditorVector4ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min) : base(numberInputType, max, min) { }
        public EditorVector4ValueAttribute(float max, float min, float step) : base(max, min, step) { }
        public EditorVector4ValueAttribute(NumberInputTypeRepresentation numberInputType, float max, float min, float step) : base(numberInputType, max, min, step) { }
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
}
