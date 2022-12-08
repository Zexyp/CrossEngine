using System;
using ImGuiNET;

using System.Numerics;

namespace CrossEngine.Utils.Editor
{
    // used for ordering
    public enum EditorAttributeType
    {
        None = 0,
        Hint,
        Edit,
        Decor,
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorValueAttribute : Attribute
    {
        public string? Name = null;
        public virtual EditorAttributeType Kind => EditorAttributeType.Edit;

        public EditorValueAttribute() { }

        public EditorValueAttribute(string? name)
        {
            Name = name;
        }
    }

    public class EditorDisplayAttribute : EditorValueAttribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorSectionAttribute : EditorValueAttribute
    {
        public override EditorAttributeType Kind => EditorAttributeType.Decor;

        public EditorSectionAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorHintAttribute : EditorValueAttribute
    {
        public override EditorAttributeType Kind => EditorAttributeType.Hint;

        public EditorHintAttribute(string name)
        {
            Name = name;
        }
    }

    public interface IValueRange
    {
        public float Min { get; set; }
        public float Max { get; set; }
    }

    public interface ISteppedValueRange : IValueRange
    {
        public float Step { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorRangeAttribute : EditorValueAttribute, IValueRange
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public EditorRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorDragAttribute : EditorRangeAttribute, ISteppedValueRange
    {
        public float Step { get; set; } = 0.1f;

        public EditorDragAttribute(float min, float max) : base(min, max) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorSliderAttribute : EditorRangeAttribute, IValueRange
    {
        public EditorSliderAttribute(float min, float max) : base(min, max) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorEnumAttribute : EditorValueAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorColorAttribute : EditorValueAttribute
    {
        public bool HDR = true;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorInnerDrawAttribute : EditorValueAttribute
    {
        public EditorInnerDrawAttribute() { }
        public EditorInnerDrawAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorGradientAttribute : EditorValueAttribute
    {
        public EditorGradientAttribute() { }
        public EditorGradientAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorAssetAttribute : EditorValueAttribute
    {
        public Type AssetType;

        public EditorAssetAttribute(Type type)
        {
            AssetType = type;
        }
    }


    public enum NumberInputType
    {
        Drag,
        Input,
        Slider,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class EditorDrawableAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public abstract class EditorNumberValueAttribute : EditorValueAttribute
    {
        public NumberInputType NumberInputType = NumberInputType.Drag;
        public float Max = float.MaxValue;
        public float Min = float.MinValue;
        public float Step = 0.1f;

        public EditorNumberValueAttribute() { }

        public EditorNumberValueAttribute(
            string name = null,
            NumberInputType numberInputType = NumberInputType.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorStringAttribute : EditorValueAttribute
    {
        public EditorStringAttribute() { }
        public EditorStringAttribute(string name) : base(name) { }

        public EditorStringAttribute(string name, uint maxLength) : base(name)
        {
            MaxLength = maxLength;
        }

        public EditorStringAttribute(uint maxLength)
        {
            MaxLength = maxLength;
        }

        public uint MaxLength = 256;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorBooleanValueAttribute : EditorValueAttribute
    {
        public EditorBooleanValueAttribute() { }
        public EditorBooleanValueAttribute(string name) : base(name) { }
    }

    [Obsolete("No longer supported")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorAssetValueAttribute : EditorValueAttribute
    {
        public Type Type;

        public EditorAssetValueAttribute(Type type)
        {
            Type = type;
        }
    }
}
