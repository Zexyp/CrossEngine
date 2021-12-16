using System;
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
        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            float[][] values = (float[][])serializer.Deserialize(reader.GetProperty("$values"), typeof(float[][]));
            Matrix4x4 matrix;
            matrix.M11 = values[0][0];
            matrix.M12 = values[0][1];
            matrix.M13 = values[0][2];
            matrix.M14 = values[0][3];
            matrix.M21 = values[1][0];
            matrix.M22 = values[1][1];
            matrix.M23 = values[1][2];
            matrix.M24 = values[1][3];
            matrix.M31 = values[2][0];
            matrix.M32 = values[2][1];
            matrix.M33 = values[2][2];
            matrix.M34 = values[2][3];
            matrix.M41 = values[3][0];
            matrix.M42 = values[3][1];
            matrix.M43 = values[3][2];
            matrix.M44 = values[3][3];
            return matrix;
        }

        public override void WriteJson(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WritePropertyName("$values");
            serializer.Serialize(writer, new float[4][]
            {
                new float[4]{ value.M11, value.M12, value.M13, value.M14 },
                new float[4]{ value.M21, value.M22, value.M23, value.M24 },
                new float[4]{ value.M31, value.M32, value.M33, value.M34 },
                new float[4]{ value.M41, value.M42, value.M43, value.M44 },
            });
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
