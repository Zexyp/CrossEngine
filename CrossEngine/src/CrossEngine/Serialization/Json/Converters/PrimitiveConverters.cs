using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Numerics;
using System.Collections;

namespace CrossEngine.Serialization.Json.Converters
{
    #region Numerics
    #region Vectors
    class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            serializer.Serialize(writer, value.X);
            writer.WritePropertyName("Y");
            serializer.Serialize(writer, value.Y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Vector2 vec;
            vec.X = obj["X"].Value<float>();
            vec.Y = obj["Y"].Value<float>();
            return vec;
        }
    }

    class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            serializer.Serialize(writer, value.X);
            writer.WritePropertyName("Y");
            serializer.Serialize(writer, value.Y);
            writer.WritePropertyName("Z");
            serializer.Serialize(writer, value.Z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Vector3 vec;
            vec.X = obj["X"].Value<float>();
            vec.Y = obj["Y"].Value<float>();
            vec.Z = obj["Z"].Value<float>();
            return vec;
        }
    }

    class Vector4JsonConverter : JsonConverter<Vector4>
    {
        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            serializer.Serialize(writer, value.X);
            writer.WritePropertyName("Y");
            serializer.Serialize(writer, value.Y);
            writer.WritePropertyName("Z");
            serializer.Serialize(writer, value.Z);
            writer.WritePropertyName("W");
            serializer.Serialize(writer, value.W);
            writer.WriteEndObject();
        }

        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Vector4 vec;
            vec.X = obj["X"].Value<float>();
            vec.Y = obj["Y"].Value<float>();
            vec.Z = obj["Z"].Value<float>();
            vec.W = obj["W"].Value<float>();
            return vec;
        }
    }
    #endregion

    class QuaternionJsonConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            serializer.Serialize(writer, value.X);
            writer.WritePropertyName("Y");
            serializer.Serialize(writer, value.Y);
            writer.WritePropertyName("Z");
            serializer.Serialize(writer, value.Z);
            writer.WritePropertyName("W");
            serializer.Serialize(writer, value.W);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Quaternion quat;
            quat.X = obj["X"].Value<float>();
            quat.Y = obj["Y"].Value<float>();
            quat.Z = obj["Z"].Value<float>();
            quat.W = obj["W"].Value<float>();
            return quat;
        }
    }

    class Matrix4x4JsonConverter : JsonConverter<Matrix4x4>
    {
        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new float[4][]
            {
                new float[4]{ value.M11, value.M12, value.M13, value.M14 },
                new float[4]{ value.M21, value.M22, value.M23, value.M24 },
                new float[4]{ value.M31, value.M32, value.M33, value.M34 },
                new float[4]{ value.M41, value.M42, value.M43, value.M44 },
            });
        }

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray obj = JArray.Load(reader);
            float[][] values;
            using (var re = obj.CreateReader())
                values = serializer.Deserialize<float[][]>(re);
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
    }
    #endregion
}
