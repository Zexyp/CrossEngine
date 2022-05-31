using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace CrossEngine.Serialization.Json.Converters
{
    #region Numerics
    #region Vectors
    class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return new Vector2(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle());
        }

        public override void WriteJson(Utf8JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
        }

        public override void ReadJson(JsonElement reader, Vector2 value, JsonSerializer serializer)
        {
        }
    }

    class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return new Vector3(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle(), reader.GetProperty("Z").GetSingle());
        }

        public override void WriteJson(Utf8JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
        }

        public override void ReadJson(JsonElement reader, Vector3 value, JsonSerializer serializer)
        {
        }
    }

    class Vector4JsonConverter : JsonConverter<Vector4>
    {
        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return new Vector4(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle(), reader.GetProperty("Z").GetSingle(), reader.GetProperty("W").GetSingle());
        }

        public override void WriteJson(Utf8JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteNumber("W", value.W);
        }

        public override void ReadJson(JsonElement reader, Vector4 value, JsonSerializer serializer)
        {
        }
    }
    #endregion

    class QuaternionJsonConverter : JsonConverter<Quaternion>
    {
        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return new Quaternion(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle(), reader.GetProperty("Z").GetSingle(), reader.GetProperty("W").GetSingle());
        }

        public override void WriteJson(Utf8JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteNumber("W", value.W);
        }

        public override void ReadJson(JsonElement reader, Quaternion value, JsonSerializer serializer)
        {
        }
    }

    class Matrix4x4JsonConverter : JsonConverter<Matrix4x4>
    {
        public override bool Bracketable => false;

        public unsafe override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            Matrix4x4 matrix = new Matrix4x4();

            var p = &matrix.M11;
            var values = reader.EnumerateArray().SelectMany(i => i.EnumerateArray()).Select(i => i.GetSingle()).ToArray();
            for (int i = 0; i < 16; i++)
            {
                p[i] = values[i];
            }

            return matrix;
        }

        public override void WriteJson(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            writer.WriteStartArray();
            writer.WriteNumberValue(value.M11);
            writer.WriteNumberValue(value.M12);
            writer.WriteNumberValue(value.M13);
            writer.WriteNumberValue(value.M14);
            writer.WriteEndArray();
            writer.WriteStartArray();
            writer.WriteNumberValue(value.M21);
            writer.WriteNumberValue(value.M22);
            writer.WriteNumberValue(value.M23);
            writer.WriteNumberValue(value.M24);
            writer.WriteEndArray();
            writer.WriteStartArray();
            writer.WriteNumberValue(value.M31);
            writer.WriteNumberValue(value.M32);
            writer.WriteNumberValue(value.M33);
            writer.WriteNumberValue(value.M34);
            writer.WriteEndArray();
            writer.WriteStartArray();
            writer.WriteNumberValue(value.M41);
            writer.WriteNumberValue(value.M42);
            writer.WriteNumberValue(value.M43);
            writer.WriteNumberValue(value.M44);
            writer.WriteEndArray();

            writer.WriteEndArray();
        }

        public override void ReadJson(JsonElement reader, Matrix4x4 value, JsonSerializer serializer)
        {
        }
    }
    #endregion

    class EnumJsonConverter : JsonConverter<Enum>
    {
        public override bool Bracketable => false;

        public override bool CanConvert(Type type)
        {
            return type.IsAssignableTo(typeof(Enum));
        }

        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return Enum.ToObject(type, reader.GetInt64());
        }

        public override void ReadJson(JsonElement reader, Enum value, JsonSerializer serializer)
        {
        }

        public override void WriteJson(Utf8JsonWriter writer, Enum value, JsonSerializer serializer)
        {
            writer.WriteNumberValue(Convert.ToInt64(value));
        }
    }

    class TypeJsonConverter : JsonConverter<Type>
    {
        public override bool CanConvert(Type type)
        {
            return type.IsAssignableTo(typeof(Type));
        }

        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return serializer.Settings.TypeResolver.ResolveType(reader.GetProperty("$name").GetString());
        }

        public override void ReadJson(JsonElement reader, Type value, JsonSerializer serializer)
        {
        }

        public override void WriteJson(Utf8JsonWriter writer, Type value, JsonSerializer serializer)
        {
            writer.WriteString("$name", value.FullName);
        }
    }
}
