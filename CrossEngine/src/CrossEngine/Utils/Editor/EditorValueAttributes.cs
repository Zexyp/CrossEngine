using System;

using System.Numerics;
using System.Runtime.CompilerServices;

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

    file class Helper
    {
        public unsafe static T GetMaxValue<T>() where T : struct
        {

            T buffer;
            T* p = &buffer;
            TypeCode typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode)
            {
                case TypeCode.Byte:
                    {
                        var value = byte.MaxValue;
                        *p = Unsafe.As<byte, T>(ref value);
                    }
                    break;
                case TypeCode.SByte:
                    {
                        var value = sbyte.MaxValue;
                        *p = Unsafe.As<sbyte, T>(ref value);
                    }
                    break;
                case TypeCode.Int16:
                    {
                        var value = short.MaxValue;
                        *p = Unsafe.As<short, T>(ref value);
                    }
                    break;
                case TypeCode.UInt16:
                    {
                        var value = ushort.MaxValue;
                        *p = Unsafe.As<ushort, T>(ref value);
                    }
                    break;
                case TypeCode.Int32:
                    {
                        var value = int.MaxValue;
                        *p = Unsafe.As<int, T>(ref value);
                    }
                    break;
                case TypeCode.UInt32:
                    {
                        var value = uint.MaxValue;
                        *p = Unsafe.As<uint, T>(ref value);
                    }
                    break;
                case TypeCode.Int64:
                    {
                        var value = long.MaxValue;
                        *p = Unsafe.As<long, T>(ref value);
                    }
                    break;
                case TypeCode.UInt64:
                    {
                        var value = ulong.MaxValue;
                        *p = Unsafe.As<ulong, T>(ref value);
                    }
                    break;
                case TypeCode.Single:
                    {
                        var value = float.MaxValue;
                        *p = Unsafe.As<float, T>(ref value);
                    }
                    break;
                case TypeCode.Double:
                    {
                        var value = double.MaxValue;
                        *p = Unsafe.As<double, T>(ref value);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return *p;
        }

        public unsafe static T GetMinValue<T>() where T : struct
        {

            T buffer;
            T* p = &buffer;
            TypeCode typeCode = Type.GetTypeCode(typeof(T));
            switch (typeCode)
            {
                case TypeCode.Byte:
                    {
                        var value = byte.MinValue;
                        *p = Unsafe.As<byte, T>(ref value);
                    }
                    break;
                case TypeCode.SByte:
                    {
                        var value = sbyte.MinValue;
                        *p = Unsafe.As<sbyte, T>(ref value);
                    }
                    break;
                case TypeCode.Int16:
                    {
                        var value = short.MinValue;
                        *p = Unsafe.As<short, T>(ref value);
                    }
                    break;
                case TypeCode.UInt16:
                    {
                        var value = ushort.MinValue;
                        *p = Unsafe.As<ushort, T>(ref value);
                    }
                    break;
                case TypeCode.Int32:
                    {
                        var value = int.MinValue;
                        *p = Unsafe.As<int, T>(ref value);
                    }
                    break;
                case TypeCode.UInt32:
                    {
                        var value = uint.MinValue;
                        *p = Unsafe.As<uint, T>(ref value);
                    }
                    break;
                case TypeCode.Int64:
                    {
                        var value = long.MinValue;
                        *p = Unsafe.As<long, T>(ref value);
                    }
                    break;
                case TypeCode.UInt64:
                    {
                        var value = ulong.MinValue;
                        *p = Unsafe.As<ulong, T>(ref value);
                    }
                    break;
                case TypeCode.Single:
                    {
                        var value = float.MinValue;
                        *p = Unsafe.As<float, T>(ref value);
                    }
                    break;
                case TypeCode.Double:
                    {
                        var value = double.MinValue;
                        *p = Unsafe.As<double, T>(ref value);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return *p;
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorValueAttribute : Attribute
    {
        public string? Name = null;
        public virtual EditorAttributeType Kind => EditorAttributeType.Edit;

        public EditorValueAttribute()
        {
            
        }

        public EditorValueAttribute(string? name) : this()
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorSectionAttribute : EditorValueAttribute
    {
        public override EditorAttributeType Kind => EditorAttributeType.Decor;

        public EditorSectionAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorHintAttribute : EditorValueAttribute
    {
        public override EditorAttributeType Kind => EditorAttributeType.Hint;

        public EditorHintAttribute(string name)
        {
            Name = name;
        }
    }

    #region Range
    public interface IRangeValue
    {

    }

    public interface IRangeValue<T> : IRangeValue where T : struct, IComparable<T>, IComparable, IEquatable<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public T SoftMin { get; set; }
        public T SoftMax { get; set; }
    }

    public interface ISteppedRangeValue
    {

    }

    public interface ISteppedRangeValue<TRange> : ISteppedRangeValue, IRangeValue<TRange> where TRange : struct, IComparable<TRange>, IComparable, IEquatable<TRange>
    {
        public float Step { get; set; }
    }

    // idfk what i'm doing pls help

    // used for vectors
    #region Simple
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
    public class EditorDragAttribute : EditorRangeAttribute, ISteppedRangeValue<float>
    {
        public float Step { get; set; } = 0.1f;
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorSliderAttribute : EditorRangeAttribute, IRangeValue<float>
    {
        //public float Step { get; set; } = 0.1f;
    }
    #endregion

    // can be used for any simple type
    #region Generic
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorRangeAttribute<T> : EditorValueAttribute where T : struct, IComparable<T>, IComparable, IEquatable<T>
    {
        public T Min { get; set; } = Helper.GetMinValue<T>();
        public T Max { get; set; } = Helper.GetMaxValue<T>();
        public T SoftMin { get; set; }
        public T SoftMax { get; set; }

        public EditorRangeAttribute()
        {
            SoftMin = Min;
            SoftMax = Max;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorDragAttribute<T> : EditorRangeAttribute<T>, ISteppedRangeValue<T> where T : struct, IComparable<T>, IComparable, IEquatable<T>
    {
        public float Step { get; set; } = 0.1f;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorSliderAttribute<T> : EditorRangeAttribute<T>, IRangeValue<T> where T : struct, IComparable<T>, IComparable, IEquatable<T>
    {
        //public float Step { get; set; } = 0.1f;
    }
    #endregion
    #endregion

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorEnumAttribute : EditorValueAttribute
    {
        // TODO: renaming values mby
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EditorColorAttribute : EditorValueAttribute
    {
        public bool HDR = true;
    }

    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    //public class EditorInnerDrawAttribute : EditorValueAttribute
    //{
    //    public EditorInnerDrawAttribute() { }
    //    public EditorInnerDrawAttribute(string name) : base(name) { }
    //}

    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    //public class EditorGradientAttribute : EditorValueAttribute
    //{
    //    public EditorGradientAttribute() { }
    //    public EditorGradientAttribute(string name) : base(name) { }
    //}

    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    //public class EditorAssetAttribute<TAsset> : EditorValueAttribute where TAsset : Asset
    //{
    //    public Type AssetType;
    //
    //    public EditorAssetAttribute(Type type)
    //    {
    //        AssetType = type;
    //    }
    //}

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
        
    }
}