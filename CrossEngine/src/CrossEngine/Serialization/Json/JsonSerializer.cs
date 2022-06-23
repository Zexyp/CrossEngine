using System;

using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace CrossEngine.Serialization.Json
{
    public class JsonSerializer
    {
        private enum Mode
        {
            None,
            Serialize,
            Deserialize,
        }

        Mode _mode = Mode.None;

        public JsonSerializerSettings Settings;

        //         value     id
        Dictionary<object, string> _seriReferences;
        //           id    value
        Dictionary<string, object> _deseriReferences;

        //                         type             writer      value
        static readonly Dictionary<Type, Action<Utf8JsonWriter, object>> _coreSeriConvs = new Dictionary<Type, Action<Utf8JsonWriter, object>>()
        {
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

            { typeof(decimal), (writer, value) => {
                writer.WriteNumberValue((decimal)value);
            } },

            //{ typeof(char), (writer, value) => {
            //    writer.((char)value);
            //} },

            { typeof(string), (writer, value) => {
                writer.WriteStringValue((string)value);
            } },
        };
        static readonly Dictionary<Type, Func<JsonElement, object>> _coreDeseriConvs = new Dictionary<Type, Func<JsonElement, object>>()
        {
            { typeof(bool), (reader) => {
                return reader.GetBoolean();
            } },

            { typeof(sbyte), (reader) => {
                return reader.GetSByte();
            } },
            { typeof(byte), (reader) => {
                return reader.GetByte();
            } },
            { typeof(short), (reader) => {
                return reader.GetInt16();
            } },
            { typeof(ushort), (reader) => {
                return reader.GetUInt16();
            } },
            { typeof(int), (reader) => {
                return reader.GetInt32();
            } },
            { typeof(uint), (reader) => {
                return reader.GetUInt32();
            } },
            { typeof(long), (reader) => {
                return reader.GetInt64();
            } },
            { typeof(ulong), (reader) => {
                return reader.GetUInt64();
            } },

            { typeof(float), (reader) => {
                return reader.GetSingle();
            } },
            { typeof(double), (reader) => {
                return reader.GetDouble();
            } },

            { typeof(decimal), (reader) => {
                return reader.GetDecimal();
            } },

            //{ typeof(char), (reader) => {
            //    return reader.();
            //} },

            { typeof(string), (reader) => {
                return reader.GetString();
            } },
        };

        public JsonSerializer(JsonSerializerSettings options)
        {
            Settings = options;
        }

        int _depth = 0;

        public void Serialize(object value, Stream stream)
        {
            using (Utf8JsonWriter writer = new Utf8JsonWriter(stream, Settings.WriterOptions))
            {
                Serialize(writer, value);
            }
        }     

        public void Serialize(Utf8JsonWriter writer, object value)
        {
            void EndDepth()
            {
                _depth--;

                if (_depth == 0)
                {
                    _seriReferences.Clear();
                    _mode = Mode.None;
                }
            }

            if (_mode == Mode.None)
            {
                _mode = Mode.Serialize;
                _seriReferences = new Dictionary<object, string>();
            }
            if (_mode == Mode.Deserialize) throw new InvalidOperationException();

            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            Type typeOfValue = value.GetType();

            // check basic type
            if (_coreSeriConvs.ContainsKey(typeOfValue))
            {
                _coreSeriConvs[typeOfValue](writer, value);
                return;
            }

            JsonConverter selectedConverter = null;
            for (int i = 0; i < Settings.Converters.Count; i++)
            {
                if (Settings.Converters[i].CanConvert(typeOfValue))
                {
                    selectedConverter = Settings.Converters[i];
                    break;
                }
            }

            if (!(selectedConverter?.Bracketable == false)) writer.WriteStartObject();
            // check if converter was found
            if (selectedConverter != null)
            {
                // check refernces
                if (_seriReferences.ContainsKey(value))
                {
                    // write ref
                    writer.WriteRef(_seriReferences[value]);
                }
                else
                {
                    // write id if type is suitable
                    if (!typeOfValue.IsValueType)
                    {
                        string newid = (_seriReferences.Count + 1).ToString();
                        writer.WriteID(newid);
                        _seriReferences.Add(value, newid);
                    }

                    _depth++;
                    selectedConverter.WriteJson(writer, value, this);
                    EndDepth();
                }
            }
            if (!(selectedConverter?.Bracketable == false)) writer.WriteEndObject();
        }

        public object? Deserialize(Stream stream, Type type)
        {
            using (JsonDocument document = JsonDocument.Parse(stream))
            {
                return Deserialize(document.RootElement, type);
            }
        }

        public object? Deserialize(JsonElement reader, Type inptype)
        {
            void EndDepth()
            {
                _depth--;
                
                if (_depth == 0)
                {
                    _deseriReferences.Clear();
                    _mode = Mode.None;
                }
            }

            if (_mode == Mode.None)
            {
                _mode = Mode.Deserialize;
                _deseriReferences = new Dictionary<string, object>();
            }
            if (_mode == Mode.Serialize) throw new InvalidOperationException();

            if (reader.ValueKind == JsonValueKind.Null) return null;

            Type typeOfValue = inptype;

            if (_coreDeseriConvs.ContainsKey(typeOfValue))
            {
                return _coreDeseriConvs[typeOfValue](reader);
            }

            // TODO: check assignability
            // check for explicit type
            if (reader.TryGetTypeString(out string typeString)) typeOfValue = Settings.TypeResolver.ResolveType(typeString);

            JsonConverter selectedConverter = null;
            // find converter
            if (typeOfValue != null)
            {
                for (int i = 0; i < Settings.Converters.Count; i++)
                {
                    if (Settings.Converters[i].CanConvert(typeOfValue))
                    {
                        selectedConverter = Settings.Converters[i];
                        break;
                    }
                }
            }

            // check if converter was found
            if (selectedConverter != null)
            {
                // check refernces
                if (reader.TryGetRef(out string refid))
                {
                    return _deseriReferences[refid];
                }
                else
                {
                    _depth++;

                    object value = selectedConverter.Create(reader, typeOfValue, this);
                    // check if creation succeeded (there is no point of populating null)
                    if (value == null)
                    {
                        EndDepth();
                        return null;
                    }

                    selectedConverter.ReadJson(reader, value, this);

                    if (reader.TryGetID(out string objid)) _deseriReferences.Add(objid, value);

                    EndDepth();
                    return value;
                }
            }

            // mby throw exception
            return null;
        }
    }

    static class JsonExtensions
    {
        private const string IDExpression = "$id";
        private const string RefExpression = "$ref";
        private const string TypeExpression = "$type";

        public static bool TryGetRef(this JsonElement element, out string id)
        {
            id = null;

            if (element.ValueKind != JsonValueKind.Object) return false;

            bool succes = element.TryGetProperty(RefExpression, out JsonElement idEl);

            if (succes) id = idEl.GetString();

            return succes;
        }

        public static bool TryGetID(this JsonElement element, out string id)
        {
            id = null;

            if (element.ValueKind != JsonValueKind.Object) return false;

            bool succes = element.TryGetProperty(IDExpression, out JsonElement idEl);

            if (succes) id = idEl.GetString();

            return succes;
        }

        public static bool TryGetTypeString(this JsonElement element, out string typeString)
        {
            typeString = null;

            if (element.ValueKind != JsonValueKind.Object) return false;

            bool succes = element.TryGetProperty(TypeExpression, out JsonElement idEl);

            if (succes) typeString = idEl.GetString();

            return succes;
        }

        public static string GetTypeString(this JsonElement element) => element.GetProperty("$type").GetString();

        public static void WriteID(this Utf8JsonWriter writer, string id) => writer.WriteString(IDExpression, id);

        public static void WriteRef(this Utf8JsonWriter writer, string id) => writer.WriteString(RefExpression, id);
    }
}
