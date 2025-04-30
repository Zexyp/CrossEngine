//#define WRITE_VEC_ARRAY

using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossEngine.Serialization.Json
{
    #region Numerics
    #region Vectors
    public class Vector2JsonConverter : ElementJsonConverter<Vector2>
    {
        public override Vector2 Read(JsonElement reader, Type type, JsonSerializerOptions options)
        {
            switch (reader.ValueKind)
            {
                case JsonValueKind.Object:
                    return new Vector2(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle());
                case JsonValueKind.Array:
                    return new Vector2(reader[0].GetSingle(), reader[1].GetSingle());
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
#if WRITE_VEC_ARRAY
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
#else
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteEndObject();
#endif
        }
    }

    public class Vector3JsonConverter : ElementJsonConverter<Vector3>
    {
        public override Vector3 Read(JsonElement reader, Type type, JsonSerializerOptions options)
        {
            switch (reader.ValueKind)
            {
                case JsonValueKind.Object:
                    return new Vector3(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle(), reader.GetProperty("Z").GetSingle());
                case JsonValueKind.Array:
                    return new Vector3(reader[0].GetSingle(), reader[1].GetSingle(), reader[2].GetSingle());
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
#if WRITE_VEC_ARRAY
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteEndArray();
#else
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteEndObject();
#endif
        }
    }

    public class Vector4JsonConverter : ElementJsonConverter<Vector4>
    {
        public override Vector4 Read(JsonElement reader, Type type, JsonSerializerOptions options)
        {
            switch (reader.ValueKind)
            {
                case JsonValueKind.Object:
                    return new Vector4(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle(), reader.GetProperty("Z").GetSingle(), reader.GetProperty("W").GetSingle());
                case JsonValueKind.Array:
                    return new Vector4(reader[0].GetSingle(), reader[1].GetSingle(), reader[2].GetSingle(), reader[3].GetSingle());
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
        {
#if WRITE_VEC_ARRAY
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteNumberValue(value.W);
            writer.WriteEndArray();
#else
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteNumber("W", value.W);
            writer.WriteEndObject();
#endif
        }
    }
    #endregion

    public class QuaternionJsonConverter : ElementJsonConverter<Quaternion>
    {
        public override Quaternion Read(JsonElement reader, Type type, JsonSerializerOptions options)
        {
            switch (reader.ValueKind)
            {
                case JsonValueKind.Object:
                    return new Quaternion(reader.GetProperty("X").GetSingle(), reader.GetProperty("Y").GetSingle(), reader.GetProperty("Z").GetSingle(), reader.GetProperty("W").GetSingle());
                case JsonValueKind.Array:
                    return new Quaternion(reader[0].GetSingle(), reader[1].GetSingle(), reader[2].GetSingle(), reader[3].GetSingle());
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
        {
#if WRITE_VEC_ARRAY
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteNumberValue(value.W);
            writer.WriteEndArray();
#else
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteNumber("W", value.W);
            writer.WriteEndObject();
#endif
        }
    }

    public class Matrix4x4JsonConverter : ElementJsonConverter<Matrix4x4>
    {
        public unsafe override Matrix4x4 Read(JsonElement reader, Type type, JsonSerializerOptions options)
        {
            Matrix4x4 matrix = new Matrix4x4();

            // uh-oh stinky
            var p = &matrix.M11;
            var values = reader.EnumerateArray().SelectMany(i => i.EnumerateArray()).Select(i => i.GetSingle()).ToArray();
            for (int i = 0; i < 16; i++)
            {
                p[i] = values[i];
            }

            return matrix;
        }

        public override void Write(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializerOptions options)
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
    }
    #endregion
}