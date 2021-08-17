using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Text.Json;
using System.Numerics;

using CrossEngine.Scenes;
using CrossEngine.Entities;
using CrossEngine.Entities.Components;
using CrossEngine.Rendering.Textures;

namespace CrossEngine.Serialization.Json.CustomConverters
{
    #region Primitives
    class Vector2CustomJsonConverter : CustomJsonConverter<Vector2>
    {
        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializer context)
        {
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
        }

        public override Vector2 Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            return new Vector2(
                valueEl.GetProperty("X").GetSingle(),
                valueEl.GetProperty("Y").GetSingle()
                );
        }
    }

    class Vector3CustomJsonConverter : CustomJsonConverter<Vector3>
    {
        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializer context)
        {
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
        }

        public override Vector3 Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            return new Vector3(
                valueEl.GetProperty("X").GetSingle(),
                valueEl.GetProperty("Y").GetSingle(),
                valueEl.GetProperty("Z").GetSingle()
                );
        }
    }

    class Vector4CustomJsonConverter : CustomJsonConverter<Vector4>
    {
        public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializer context)
        {
            writer.WriteNumber("W", value.W);
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
        }

        public override Vector4 Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            return new Vector4(
                valueEl.GetProperty("X").GetSingle(),
                valueEl.GetProperty("Y").GetSingle(),
                valueEl.GetProperty("Z").GetSingle(),
                valueEl.GetProperty("W").GetSingle()
                );
        }
    }

    class QuaternionCustomJsonConverter : CustomJsonConverter<Quaternion>
    {
        public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializer context)
        {
            writer.WriteNumber("W", value.W);
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
        }

        public override Quaternion Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            return new Quaternion(
                valueEl.GetProperty("X").GetSingle(),
                valueEl.GetProperty("Y").GetSingle(),
                valueEl.GetProperty("Z").GetSingle(),
                valueEl.GetProperty("W").GetSingle()
                );
        }
    }

    class Matrix4x4CustomJsonConverter : CustomJsonConverter<Matrix4x4>
    {
        public override void Write(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializer context)
        {
            writer.WritePropertyName("$value");
            context.SerializationPass(writer, new float[4][]
            {
                new float[4]{ value.M11, value.M12, value.M13, value.M14 },
                new float[4]{ value.M21, value.M22, value.M23, value.M24 },
                new float[4]{ value.M31, value.M32, value.M33, value.M34 },
                new float[4]{ value.M41, value.M42, value.M43, value.M44 },
            });
        }
        public override Matrix4x4 Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            float[][] values = (float[][])context.DeserializationPass(valueEl.GetProperty("$value"), typeof(float[][]));
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

    class IEnumerableCustomJsonConverter : CustomJsonConverter<IEnumerable>
    {
        public override void Write(Utf8JsonWriter writer, IEnumerable value, JsonSerializer serialization)
        {
            writer.WriteStartArray("$values");
            foreach (var item in value)
            {
                serialization.SerializationPass(writer, item);
            }
            writer.WriteEndArray();
        }

        public override IEnumerable Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            Type baseType = Type.GetType(valueEl.GetTypeString());
            Type typeOfArray = null;

            if (baseType.IsArray)
            {
                typeOfArray = baseType.GetElementType();
            }
            else if (baseType.IsGenericType)
            {
                typeOfArray = baseType.GenericTypeArguments[0];
            }

            JsonElement array = valueEl.GetProperty("$values");
            object[] values = new object[array.GetArrayLength()];
            int i = 0;
            foreach (JsonElement item in array.EnumerateArray())
            {
                Type typeOfItem = typeOfArray;
                if (item.TryGetTypeString(out string itemTypeString))
                {
                    Type found = Type.GetType(itemTypeString);
                    if (found != null && typeOfItem.IsAssignableFrom(found)) typeOfItem = found;
                    else throw new Exception();
                }

                values[i] = context.DeserializationPass(item, typeOfItem);
                i++;
            }

            if (baseType.IsArray)
            {
                var inst = Array.CreateInstance(typeOfArray, values.Length);
                Array.Copy(values, inst, inst.Length);
                return inst;
            }

            return values;
        }
    }

    class EnumCustomJsonConverter : CustomJsonConverter<Enum>
    {
        public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializer context)
        {
            writer.WriteString("$value", value.ToString());
        }

        public override Enum Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            return (Enum)Enum.Parse(returnType, valueEl.GetProperty("$value").GetString());
        }
    }

    // specific ones

    class SceneCustomJsonConverter : CustomJsonConverter<Scene>
    {
        public override void Write(Utf8JsonWriter writer, Scene value, JsonSerializer context)
        {
            writer.WritePropertyName("Entities");
            context.SerializationPass(writer, value.Entities);
        }

        public override Scene Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            Scene scene = new Scene();

            foreach (JsonElement entityEl in valueEl.GetProperty("Entities").GetProperty("$values").EnumerateArray())
            {
                Entity entity = scene.CreateEntity();
                if (entityEl.TryGetProperty("debugName", out JsonElement debugNameEl))
                    entity.debugName = debugNameEl.GetString();
                entity.Enabled = entityEl.GetProperty("Enabled").GetBoolean();
                entity.ClearComponents();
                foreach (Component item in (IEnumerable)context.DeserializationPass(entityEl.GetProperty("Components"), typeof(IEnumerable)))
                {
                    entity.AddComponent(item);
                }
                if (entityEl.TryGetProperty("Parent", out JsonElement parentEl))
                    entity.Parent = (Entity)context.DeserializationPass(parentEl, typeof(Entity), (returnType.GetMember(nameof(Entity.Parent))[0], entity));
            }

            return scene;
        }
    }

    class EntityCustomJsonConverter : CustomJsonConverter<Entity>
    {
        public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializer context)
        {
            if (!string.IsNullOrEmpty(value.debugName))
                writer.WriteString("debugName", value.debugName);
            writer.WriteBoolean("Enabled", value.Enabled);
            if (value.Parent != null)
            {
                writer.WritePropertyName("Parent");
                context.SerializationPass(writer, value.Parent, true);
            }
            writer.WritePropertyName("Components");
            context.SerializationPass(writer, value.Components);
        }

        public override Entity Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            throw new NotImplementedException();
        }
    }

    class TextureCustomJsonConverter : CustomJsonConverter<Texture>
    {
        public override void Write(Utf8JsonWriter writer, Texture value, JsonSerializer context)
        {
            writer.WriteString("path", AssetManager.Textures.GetTexturePath(value));
        }

        public override Texture Read(JsonElement valueEl, JsonDeserializer context, Type returnType)
        {
            return AssetManager.Textures.GetTexture(valueEl.GetProperty("path").GetString());
        }
    }
}
