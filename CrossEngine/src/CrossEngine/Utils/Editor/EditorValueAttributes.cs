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
        static private int lastOrder = 0;
        public int Order;

        public EditorValueAttribute()
        {
            Order = ++lastOrder;
        }

        public EditorValueAttribute(string? name) : this()
        {
            Name = name;
        }
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

    public interface IRangeValue
    {

    }

    public interface IRangeValue<T> : IRangeValue where T : IComparable<T>, IComparable
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public T SoftMin { get; set; }
        public T SoftMax { get; set; }
    }

    public interface ISteppedRangeValue
    {

    }

    public interface ISteppedRangeValue<TRange> : ISteppedRangeValue, IRangeValue<TRange> where TRange : IComparable<TRange>, IComparable
    {
        public float Step { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorRangeAttribute : EditorValueAttribute, IRangeValue<float>
    {
        public float Min { get; set; } = float.MinValue;
        public float Max { get; set; } = float.MaxValue;
        public float SoftMin { get; set; }
        public float SoftMax { get; set; }

        public EditorRangeAttribute()
        {
            SoftMin = Min;
            SoftMax = Max;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorRangeIntAttribute : EditorValueAttribute, IRangeValue<int>
    {
        public int Min { get; set; } = int.MinValue;
        public int Max { get; set; } = int.MaxValue;
        public int SoftMin { get; set; }
        public int SoftMax { get; set; }

        public EditorRangeIntAttribute()
        {
            SoftMin = Min;
            SoftMax = Max;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorDragAttribute : EditorRangeAttribute, ISteppedRangeValue<float>
    {
        public float Step { get; set; } = 0.1f;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorSliderAttribute : EditorRangeAttribute, IRangeValue<float>
    {
        //public float Step { get; set; } = 0.1f;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorDragIntAttribute : EditorRangeIntAttribute, ISteppedRangeValue<int>
    {
        public float Step { get; set; } = 0.1f;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorSliderIntAttribute : EditorRangeIntAttribute, IRangeValue<int>
    {
        //public int Step { get; set; } = 1;
    }

    public class EditorEnumAttribute : EditorValueAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorColorAttribute : EditorValueAttribute
    {
        public bool HDR = true;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorInnerDrawAttribute : EditorValueAttribute
    {
        public EditorInnerDrawAttribute() { }
        public EditorInnerDrawAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorGradientAttribute : EditorValueAttribute
    {
        public EditorGradientAttribute() { }
        public EditorGradientAttribute(string name) : base(name) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
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

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
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

    #region Number
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorInt32ValueAttribute : EditorNumberValueAttribute
    {
        public EditorInt32ValueAttribute() { }
        public EditorInt32ValueAttribute(string name) : base(name) { }
        public EditorInt32ValueAttribute(
            string name = null,
            NumberInputType numberInputType = NumberInputType.Drag,
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
            NumberInputType numberInputType = NumberInputType.Drag,
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
            NumberInputType numberInputType = NumberInputType.Drag,
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
            NumberInputType numberInputType = NumberInputType.Drag,
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
            NumberInputType numberInputType = NumberInputType.Drag,
            float max = float.MaxValue,
            float min = float.MinValue,
            float step = 0.1f)
            : base(name, numberInputType, max, min, step) { }
    }
    #endregion

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
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

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EditorBooleanValueAttribute : EditorValueAttribute
    {
        public EditorBooleanValueAttribute() { }
        public EditorBooleanValueAttribute(string name) : base(name) { }
    }

    [Obsolete("No longer supported")]
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
