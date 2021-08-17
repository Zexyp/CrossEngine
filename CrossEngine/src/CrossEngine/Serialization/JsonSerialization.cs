using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text.Json;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Collections;

using CrossEngine.Scenes;
using CrossEngine.Entities;
using CrossEngine.Logging;
using CrossEngine.Serialization.Json.CustomConverters;

namespace CrossEngine.Serialization.Json
{

    public static class JsonSerialization
    {
        public static CustomJsonConverterCollection CreateBaseConvertersCollection()
        {
            return new CustomJsonConverterCollection()
            {
                new Vector2CustomJsonConverter(),
                new Vector3CustomJsonConverter(),
                new Vector4CustomJsonConverter(),
                new QuaternionCustomJsonConverter(),
                new Matrix4x4CustomJsonConverter(),
                new IEnumerableCustomJsonConverter(),
                new EnumCustomJsonConverter(),
                new SceneCustomJsonConverter(),
                new EntityCustomJsonConverter(),
                new TextureCustomJsonConverter(),
            };
        }
    }

    public class JsonSerializer
    {
        readonly Dictionary<object, string> SerializationReferences = new Dictionary<object, string>();
        uint lastRefID = 0;
        readonly Dictionary<object, string> WaitingReferences = new Dictionary<object, string>();

        readonly Dictionary<Type, ICustomJsonConverter> Converters = new Dictionary<Type, ICustomJsonConverter>();
        readonly static Dictionary<Type, Action<Utf8JsonWriter, object>> SimpleTypes = new Dictionary<Type, Action<Utf8JsonWriter, object>> {
            #region Singles
            { typeof(bool), (writer, value) => {
                writer.WriteBooleanValue((bool)value);
            } },

            { typeof(sbyte), (writer, value) => {
                writer.WriteNumberValue((sbyte)value);
            } },
            { typeof(byte), (writer, value) => {
                writer.WriteNumberValue((byte)value);
            } },
            { typeof(short), (writer, value) => {
                writer.WriteNumberValue((short)value);
            } },
            { typeof(ushort), (writer, value) => {
                writer.WriteNumberValue((ushort)value);
            } },
            { typeof(int), (writer, value) => {
                writer.WriteNumberValue((int)value);
            } },
            { typeof(uint), (writer, value) => {
                writer.WriteNumberValue((uint)value);
            } },
            { typeof(long), (writer, value) => {
                writer.WriteNumberValue((long)value);
            } },
            { typeof(ulong), (writer, value) => {
                writer.WriteNumberValue((ulong)value);
            } },

            { typeof(float), (writer, value) => {
                writer.WriteNumberValue((float)value);
            } },
            { typeof(double), (writer, value) => {
                writer.WriteNumberValue((double)value);
            } },

            { typeof(string), (writer, value) => {
                writer.WriteStringValue((string)value);
            } },
            #endregion
            #region Arrays
            { typeof(bool[]), (writer, value) => {
                bool[] actual = (bool[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteBooleanValue(actual[i]);
                }
                writer.WriteEndArray();
            } },

            { typeof(sbyte[]), (writer, value) => {
                sbyte[] actual = (sbyte[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(byte[]), (writer, value) => {
                byte[] actual = (byte[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(short[]), (writer, value) => {
                short[] actual = (short[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(ushort[]), (writer, value) => {
                ushort[] actual = (ushort[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(int[]), (writer, value) => {
                int[] actual = (int[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(uint[]), (writer, value) => {
                uint[] actual = (uint[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(long[]), (writer, value) => {
                long[] actual = (long[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(ulong[]), (writer, value) => {
                ulong[] actual = (ulong[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },

            { typeof(float[]), (writer, value) => {
                float[] actual = (float[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
            { typeof(double[]), (writer, value) => {
                double[] actual = (double[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteNumberValue(actual[i]);
                }
                writer.WriteEndArray();
            } },

            { typeof(string[]), (writer, value) => {
                string[] actual = (string[])value;
                writer.WriteStartArray();
                for (int i = 0; i < actual.Length; i++)
                {
                    writer.WriteStringValue(actual[i]);
                }
                writer.WriteEndArray();
            } },
	        #endregion
        };

        public JsonSerializer(CustomJsonConverterCollection converters = null)
        {
            if (converters != null)
                Converters = converters.GetConvertersDictionary();
        }

        public string Serialize(object value)
        {
            Log.Core.Info("[Serializer] starting serialization");

            using (MemoryStream stream = new MemoryStream())
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true }))
                {
                    SerializationPass(writer, value);
                    writer.Flush();

                    Log.Core.Trace($"[Serializer] total reference ids {lastRefID}");
                    Log.Core.Info("[Serializer] serialization finished");
                    if (WaitingReferences.Count > 0) Log.Core.Warn($"[Serializer] {WaitingReferences.Count} unhadled references!");

                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        internal void SerializationPass(Utf8JsonWriter writer, object value, bool asRef = false)
        {
            Type typeOfValue = value.GetType();

            if (SimpleTypes.ContainsKey(typeOfValue))
            {
                SimpleTypes[typeOfValue](writer, value);
                return;
            }

            writer.WriteStartObject();

            // save as ref
            if (asRef)
            {
                if (SerializationReferences.ContainsKey(value))
                {
                    writer.WriteRef(SerializationReferences[value]);
                }
                else if (!WaitingReferences.ContainsKey(value))
                {
                    lastRefID++;
                    WaitingReferences.Add(value, lastRefID.ToString());
                    writer.WriteRef(lastRefID.ToString());
                }
                else
                    writer.WriteRef(WaitingReferences[value]);
                goto end;
            }

            if (typeOfValue.IsClass)
            {
                // check reference
                if (!SerializationReferences.ContainsKey(value))
                {
                    // check if is waiting
                    if (WaitingReferences.ContainsKey(value))
                    {
                        writer.WriteID(WaitingReferences[value]);
                        WaitingReferences.Remove(value);
                    }
                    else
                    {
                        lastRefID++;
                        SerializationReferences.Add(value, lastRefID.ToString());
                        writer.WriteID(lastRefID.ToString());
                    }
                }
                else
                {
                    writer.WriteRef(SerializationReferences[value]);
                    goto end;
                }

                // type addition only when converter does not exist
                if (!Converters.ContainsKey(typeOfValue)) writer.WriteType(typeOfValue);
            }

            // ---
            Type inh = typeOfValue;
            while (true)
            {
                inh = inh.BaseType;
                if (inh == null)
                {
                    break;
                }
                if (Converters.ContainsKey(inh))
                {
                    typeOfValue = inh;
                    break;
                }
            }
            // ---

            // check for possible conversion
            if (!Converters.ContainsKey(typeOfValue))
            {
                Type[] interfaces = typeOfValue.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (Converters.ContainsKey(interfaces[i]))
                    {
                        Converters[interfaces[i]].Write(writer, value, this);
                        goto end;
                    }
                }
                // maybe
            }

            if (Converters.ContainsKey(typeOfValue))
            {
                Converters[typeOfValue].Write(writer, value, this);
            }
            // else check if object can serialize itself
            else if (value is ISerializable)
            {
                SerializationInfo info = new SerializationInfo();
                ((ISerializable)value).GetObjectData(info);
                SerializationInfo.SerializationEntry[] entries = info.GetData();
                for (int i = 0; i < entries.Length; i++)
                {
                    writer.WritePropertyName(entries[i].Name);
                    SerializationPass(writer, entries[i].Value, entries[i].AsRef);
                }
            }

            end:
            writer.WriteEndObject();
        }
    }

    public class JsonDeserializer
    {
        readonly Dictionary<string, object> DeserializationReferences = new Dictionary<string, object>();
        readonly Dictionary<string, List<(MemberInfo member, object target)>> WaitingReferenceMembers = new Dictionary<string, List<(MemberInfo, object)>>();

        readonly Dictionary<Type, ICustomJsonConverter> Converters = new Dictionary<Type, ICustomJsonConverter>();

        readonly static Dictionary<Type, Func<JsonElement, object>> SimpleTypes = new Dictionary<Type, Func<JsonElement, object>> {
            #region Singles
            { typeof(bool), (element) => {
                return element.GetBoolean();
            } },

            { typeof(sbyte), (element) => {
                return element.GetByte();
            } },
            { typeof(byte), (element) => {
                return element.GetSByte();
            } },
            { typeof(short), (element) => {
                return element.GetInt16();
            } },
            { typeof(ushort), (element) => {
                return element.GetUInt16();
            } },
            { typeof(int), (element) => {
                return element.GetInt32();
            } },
            { typeof(uint), (element) => {
                return element.GetUInt32();
            } },
            { typeof(long), (element) => {
                return element.GetInt64();
            } },
            { typeof(ulong), (element) => {
                return element.GetUInt64();
            } },

            { typeof(float), (element) => {
                return element.GetSingle();
            } },
            { typeof(double), (element) => {
                return element.GetDouble();
            } },

            { typeof(string), (element) => {
                return element.GetString();
            } },
            #endregion
            #region Arrays
            { typeof(bool[]), (element) => {
                bool[] array = new bool[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetBoolean();
                    i++;
	            }
                return array;
            } },

            { typeof(sbyte[]), (element) => {
                sbyte[] array = new sbyte[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetSByte();
                    i++;
                }
                return array;
            } },
            { typeof(byte[]), (element) => {
                byte[] array = new byte[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetByte();
                    i++;
                }
                return array;
            } },
            { typeof(short[]), (element) => {
                short[] array = new short[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetInt16();
                    i++;
                }
                return array;
            } },
            { typeof(ushort[]), (element) => {
                ushort[] array = new ushort[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetUInt16();
                    i++;
                }
                return array;
            } },
            { typeof(int[]), (element) => {
                int[] array = new int[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetInt32();
                    i++;
                }
                return array;
            } },
            { typeof(uint[]), (element) => {
                uint[] array = new uint[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetUInt32();
                    i++;
                }
                return array;
            } },
            { typeof(long[]), (element) => {
                long[] array = new long[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetInt64();
                    i++;
                }
                return array;
            } },
            { typeof(ulong[]), (element) => {
                ulong[] array = new ulong[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetUInt64();
                    i++;
                }
                return array;
            } },

            { typeof(float[]), (element) => {
                float[] array = new float[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetSingle();
                    i++;
                }
                return array;
            } },
            { typeof(double[]), (element) => {
                double[] array = new double[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetDouble();
                    i++;
                }
                return array;
            } },

            { typeof(string[]), (element) => {
                string[] array = new string[element.GetArrayLength()];
                int i = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    array[i] = item.GetString();
                    i++;
                }
                return array;
            } },
	        #endregion
        };

        public JsonDeserializer(CustomJsonConverterCollection converters = null)
        {
            if (converters != null)
                Converters = converters.GetConvertersDictionary();
        }

        public object Deserialize(string json, Type returnType)
        {
            Log.Core.Info("[Deserializer] starting deserialization");

            object value;
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                value = DeserializationPass(doc.RootElement, returnType);
            }

            if (WaitingReferenceMembers.Count > 0) Log.Core.Trace("[Deserializer] setting leftover references");
            while (WaitingReferenceMembers.Count > 0)
            {
                string refid = WaitingReferenceMembers.ElementAt(0).Key;
                foreach ((MemberInfo member, object target) item in WaitingReferenceMembers[refid])
                {
                    switch (item.member.MemberType)
                    {
                        case MemberTypes.Field:
                            ((FieldInfo)item.member).SetValue(item.target, DeserializationReferences[refid]);
                            break;
                        case MemberTypes.Property:
                            ((PropertyInfo)item.member).SetValue(item.target, DeserializationReferences[refid]);
                            break;
                        default:
                            throw new ArgumentException("MemberInfo must be type FieldInfo or PropertyInfo");
                    }
                }
                WaitingReferenceMembers.Remove(refid);
            }

            Log.Core.Info("[Deserializer] deserialization finished");

            return value;
        }

        internal object DeserializationPass(JsonElement element, Type returnType, (MemberInfo, object)? waitingReferenceMember = null)
        {
            if (SimpleTypes.ContainsKey(returnType))
            {
                return SimpleTypes[returnType](element);
            }

            object returnedObject = null;
            if (element.TryGetRef(out string refid))
            {
                if (DeserializationReferences.ContainsKey(refid))
                {
                    // set waiting members
                    if (WaitingReferenceMembers.ContainsKey(refid))
                    {
                        foreach ((MemberInfo member, object target) item in WaitingReferenceMembers[refid])
                        {
                            switch (item.member.MemberType)
                            {
                                case MemberTypes.Field:
                                    ((FieldInfo)item.member).SetValue(item.target, DeserializationReferences[refid]);
                                    break;
                                case MemberTypes.Property:
                                    ((PropertyInfo)item.member).SetValue(item.target, DeserializationReferences[refid]);
                                    break;
                                default:
                                    throw new ArgumentException("MemberInfo must be type FieldInfo or PropertyInfo");
                            }
                        }
                        WaitingReferenceMembers.Remove(refid);
                    }

                    // return the ref
                    returnedObject = DeserializationReferences[refid];
                    goto end;
                }
                else
                {
                    if (waitingReferenceMember == null)
                    {
                        Log.Core.Warn("custom json serializer does not give needed member info to set reference later");
                        throw new Exception();
                    }

                    if (WaitingReferenceMembers.ContainsKey(refid))
                        WaitingReferenceMembers[refid].Add(waitingReferenceMember.Value);
                    else
                        WaitingReferenceMembers.Add(refid, new List<(MemberInfo, object)> { waitingReferenceMember.Value });
                    returnedObject = default;
                    goto end;
                }
            }

            // swap return type if valid
            if (element.TryGetTypeString(out string typeString))
            {
                Type typeOfElement = Type.GetType(typeString);
                if (typeOfElement == null)
                {
                    Log.Core.Error("unknown type '{0}' while deserializing", typeString);
                    throw new Exception("Unknown type");
                }
                else if (typeOfElement.IsSubclassOf(returnType))
                    returnType = typeOfElement;
            }

            // ---
            Type inh = returnType;
            while (true)
            {
                inh = inh.BaseType;
                if (inh == null)
                {
                    break;
                }
                if (Converters.ContainsKey(inh))
                {
                    returnedObject = Converters[inh].Read(element, this, returnType);
                    goto end;
                    break;
                }
            }
            // ---

            if (returnType != null)
            {
                // check for interface converter
                if (!Converters.ContainsKey(returnType))
                {
                    Type[] interfaces = returnType.GetInterfaces();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (Converters.ContainsKey(interfaces[i]))
                        {
                            returnedObject = Converters[interfaces[i]].Read(element, this, returnType);
                            goto end;
                        }
                    }
                }
                // check for possible conversion
                if (Converters.ContainsKey(returnType))
                {
                    returnedObject = Converters[returnType].Read(element, this, returnType);
                    goto end;
                }
                // else check if type can deserialize itself
                else if (returnType.GetInterfaces().Contains(typeof(ISerializable)))
                {
                    ConstructorInfo constructorInfo = returnType.GetConstructor(new Type[1] { typeof(DeserializationInfo) });
                    if (constructorInfo == null) throw new Exception(String.Format("Type '{0}' does not implement desired deserialization constructor.", returnType.FullName));
                    DeserializationInfo info = new DeserializationInfo(this, element);
                    return constructorInfo.Invoke(new object[1] { info });
                }
            }

            throw new NotImplementedException();

            end:
            if (element.TryGetID(out string id))
            {
                DeserializationReferences.Add(id, returnedObject);
            }
            return returnedObject;
        }
    }

    public interface ICustomJsonConverter
    {
        Type WorkingType { get; }
        void Write(Utf8JsonWriter writer, object value, JsonSerializer context);
        object Read(JsonElement valueEl, JsonDeserializer context, Type returnType);
    }

    public abstract class CustomJsonConverter<T> : ICustomJsonConverter
    {
        Type ICustomJsonConverter.WorkingType => typeof(T);
        void ICustomJsonConverter.Write(Utf8JsonWriter writer, object value, JsonSerializer context) => Write(writer, (T)value, context);
        object ICustomJsonConverter.Read(JsonElement valueEl, JsonDeserializer context, Type returnType) => Read(valueEl, context, returnType);

        public abstract void Write(Utf8JsonWriter writer, T value, JsonSerializer context);
        public abstract T Read(JsonElement valueEl, JsonDeserializer context, Type returnType);
    }

    public class CustomJsonConverterCollection : ICollection<ICustomJsonConverter>
    {
        readonly Dictionary<Type, ICustomJsonConverter> Converters = new Dictionary<Type, ICustomJsonConverter>();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => false;

        public CustomJsonConverterCollection()
        {

        }

        public CustomJsonConverterCollection(IEnumerable<ICustomJsonConverter> converters)
        {
            foreach (ICustomJsonConverter item in converters)
            {
                Add(item);
            }
        }

        public void Add(ICustomJsonConverter item) => Converters.Add(item.WorkingType, item);

        public void Clear() => Converters.Clear();

        public bool Contains(ICustomJsonConverter item) => Converters.ContainsValue(item);

        public void CopyTo(ICustomJsonConverter[] array, int arrayIndex) => Converters.Values.CopyTo(array, arrayIndex);

        public IEnumerator<ICustomJsonConverter> GetEnumerator() => Converters.Values.GetEnumerator();

        public bool Remove(ICustomJsonConverter item) => Converters.Remove(item.WorkingType);

        IEnumerator IEnumerable.GetEnumerator() => Converters.Values.GetEnumerator();

        public Dictionary<Type, ICustomJsonConverter> GetConvertersDictionary() => Converters;
    }

    public interface ISerializable
    {
        void GetObjectData(SerializationInfo info);
    }

    public class SerializationInfo
    {
        Dictionary<string, (object value, bool asref)> Data = new Dictionary<string, (object, bool)>();

        public void AddValue(string key, object value)
        {
            Data.Add(key, (value, false));
        }

        public void AddRefValue(string key, object value)
        {
            Data.Add(key, (value, true));
        }

        public SerializationEntry[] GetData()
        {
            SerializationEntry[] entries = new SerializationEntry[Data.Count];

            int i = 0;
            foreach (KeyValuePair<string, (object value, bool asref)> pair in Data)
            {
                entries[i] = new SerializationEntry(pair.Key, pair.Value.value, pair.Value.asref);
                i++;
            }

            return entries;
        }

        public readonly struct SerializationEntry
        {
            public readonly string Name;
            public readonly object Value;
            public readonly bool AsRef;

            public SerializationEntry(string name, object value, bool asref)
            {
                Name = name;
                Value = value;
                AsRef = asref;
            }
        }
    }

    public class DeserializationInfo
    {
        JsonDeserializer context;
        JsonElement element;

        public DeserializationInfo(JsonDeserializer context, JsonElement element)
        {
            this.context = context;
            this.element = element;
        }

        public object GetValue(string name, Type returnType)
        {
            return context.DeserializationPass(element.GetProperty(name), returnType);
        }

        public object GetRefValue(string name, Type returnType, MemberInfo member, object target)
        {
            return context.DeserializationPass(element.GetProperty(name), returnType, (member, target));
        }

        public bool TryGetValue(string name, Type returnType, out object value)
        {
            if (element.TryGetProperty(name, out JsonElement propEl))
            {
                value = context.DeserializationPass(propEl, returnType);
                return true;
            }
            value = null;
            return false;
        }
    }

    static class Utf8JsonWriterExtension
    {
        static public void WriteType(this Utf8JsonWriter writer, Type type) => writer.WriteString("$type", type.FullName);
        static public void WriteID(this Utf8JsonWriter writer, string id) => writer.WriteString("$id", id);
        static public void WriteRef(this Utf8JsonWriter writer, string id) => writer.WriteString("$ref", id);
    }

    static class JsonElementExtension
    {
        static public bool TryGetRef(this JsonElement element, out string id)
        {
            bool succes = element.TryGetProperty("$ref", out JsonElement idEl);
            if (succes) id = idEl.GetString();
            else id = null;
            return succes;
        }

        static public bool TryGetID(this JsonElement element, out string id)
        {
            bool succes = element.TryGetProperty("$id", out JsonElement idEl);
            if (succes) id = idEl.GetString();
            else id = null;
            return succes;
        }

        static public string GetTypeString(this JsonElement element) => element.GetProperty("$type").GetString();
        
        static public bool TryGetTypeString(this JsonElement element, out string typeString)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                typeString = null;
                return false;
            }

            bool succes = element.TryGetProperty("$type", out JsonElement idEl);
            if (succes) typeString = idEl.GetString();
            else typeString = null;
            return succes;
        }
    }
}
