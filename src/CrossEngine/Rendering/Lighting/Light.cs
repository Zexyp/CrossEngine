using System;

using System.Collections.Generic;
using System.IO;
using System.Numerics;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shading;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Lighting
{
    public class Light
    {
        public enum LightType
        {
            Point,
            Directional,
            Spot
        }

        const int MaxPointLights = 4;
        const int MaxDirectionalLights = 4;
        const int MaxSpotLights = 4;

        const int PointLightSize = 16 * 4;
        const int DirectionalLightSize = 12 * 4;
        const int SpotLightSize = 24 * 4;

        //type
        public LightType type;

        //transform
        public Vector3 position;
        public Vector3 direction;
        //colors
        public Vector3 ambient;
        public Vector3 color;
        //fallof
        public float constant = 1.0f;
        public float linear = 0.09f;
        public float quadratic = 0.032f;

        public float cutOff;
        public float outerCutOff;

        public Light()
        {

        }

        public Light(LightType type)
        {
            this.type = type;
        }

        byte[] GetLightData()
        {
            byte[] buffer = null;

            int i = 0;

            switch (type)
            {
                case LightType.Point:
                    {
                        buffer = new byte[16 * 4];

                        BitConverter.GetBytes(position.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(position.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(position.Z).CopyTo(buffer, i); i += 4;
                        i += 4;

                        BitConverter.GetBytes(color.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(color.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(color.Z).CopyTo(buffer, i); i += 4;
                        i += 4;
                        BitConverter.GetBytes(ambient.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(ambient.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(ambient.Z).CopyTo(buffer, i); i += 4;
                        i += 4;

                        BitConverter.GetBytes(constant).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(linear).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(quadratic).CopyTo(buffer, i); i += 4;
                        i += 4;
                    }
                    break;
                case LightType.Directional:
                    {
                        buffer = new byte[12 * 4];

                        BitConverter.GetBytes(direction.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(direction.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(direction.Z).CopyTo(buffer, i); i += 4;
                        i += 4;

                        BitConverter.GetBytes(color.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(color.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(color.Z).CopyTo(buffer, i); i += 4;
                        i += 4;
                        BitConverter.GetBytes(ambient.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(ambient.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(ambient.Z).CopyTo(buffer, i); i += 4;
                        i += 4;
                    }
                    break;
                case LightType.Spot:
                    {
                        buffer = new byte[20 * 4];

                        BitConverter.GetBytes(position.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(position.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(position.Z).CopyTo(buffer, i); i += 4;
                        i += 4;
                        BitConverter.GetBytes(direction.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(direction.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(direction.Z).CopyTo(buffer, i); i += 4;
                        i += 4;

                        BitConverter.GetBytes(color.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(color.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(color.Z).CopyTo(buffer, i); i += 4;
                        i += 4;
                        BitConverter.GetBytes(ambient.X).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(ambient.Y).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(ambient.Z).CopyTo(buffer, i); i += 4;

                        BitConverter.GetBytes(constant).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(linear).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(quadratic).CopyTo(buffer, i); i += 4;

                        BitConverter.GetBytes(MathF.Cos(MathExtension.ToRadians(cutOff))).CopyTo(buffer, i); i += 4;
                        BitConverter.GetBytes(MathF.Cos(MathExtension.ToRadians(outerCutOff))).CopyTo(buffer, i); i += 4;
                    }
                    break;
            }

            return buffer;
        }

        static UniformBuffer pointBuffer;
        static UniformBuffer directionalBuffer;
        static UniformBuffer spotBuffer;

        public static unsafe void Init()
        {
            pointBuffer = new UniformBuffer(null, PointLightSize * MaxPointLights, true);
            directionalBuffer = new UniformBuffer(null, DirectionalLightSize * MaxDirectionalLights, true);
            spotBuffer = new UniformBuffer(null, SpotLightSize * MaxSpotLights, true);

            pointBuffer.BindTo(0);
            directionalBuffer.BindTo(1);
            spotBuffer.BindTo(2);
        }

        public static unsafe void BindLights(List<Light> lights, Shader shader, bool useDistance = false, Vector3? objectPosition = null)
        {
            int pointLightNr = 0;
            int directionalLightNr = 0;
            int spotLightNr = 0;

            if (useDistance)
                SortLightsByDistance(lights, (Vector3)objectPosition);

            using (MemoryStream pointMS = new MemoryStream())
            using (MemoryStream directionalMS = new MemoryStream())
            using (MemoryStream spotMS = new MemoryStream())
            {
                int lightIndex = 0;
                //for (auto light = lights.begin(); light != lights.end();)
                while ((pointLightNr < MaxPointLights || directionalLightNr < MaxDirectionalLights || spotLightNr < MaxSpotLights) && lightIndex < lights.Count)
                {
                    if (lights[lightIndex].type == LightType.Point && pointLightNr < MaxPointLights)
                    {
                        pointMS.Write(lights[lightIndex].GetLightData());
                        pointLightNr++;
                    }
                    if (lights[lightIndex].type == LightType.Directional && directionalLightNr < MaxDirectionalLights)
                    {
                        directionalMS.Write(lights[lightIndex].GetLightData());
                        directionalLightNr++;
                    }
                    if (lights[lightIndex].type == LightType.Spot && spotLightNr < MaxSpotLights)
                    {
                        spotMS.Write(lights[lightIndex].GetLightData());
                        spotLightNr++;
                    }
                    lightIndex++;
                }

                shader.SetInt("uNrPointLights", pointLightNr);
                shader.SetInt("uNrDirectionalLights", directionalLightNr);
                shader.SetInt("uNrSpotLights", spotLightNr);

                if (pointMS.Length > 0)
                {
                    byte[] pointBufferArray = pointMS.ToArray();
                    fixed (byte* p = &pointBufferArray[0])
                        pointBuffer.SetData(p, pointBufferArray.Length);
                }
                if (directionalMS.Length > 0)
                {
                    byte[] directionalBufferArray = directionalMS.ToArray();
                    fixed (byte* p = &directionalBufferArray[0])
                        directionalBuffer.SetData(p, directionalBufferArray.Length);
                }
                if (spotMS.Length > 0)
                {
                    byte[] spotBufferArray = spotMS.ToArray();
                    fixed (byte* p = &spotBufferArray[0])
                        spotBuffer.SetData(p, spotBufferArray.Length);
                }
            }
        }

        static void SortLightsByDistance(List<Light> lights, Vector3 objectPosition)
        {
            lights.Sort(delegate (Light light1, Light light2)
            {
                float dist1 = Vector3.DistanceSquared(light1.position, objectPosition);
                float dist2 = Vector3.DistanceSquared(light1.position, objectPosition);
                if (dist1 < dist2) return -1;
                else return 1;
            });
        }
    }
}
