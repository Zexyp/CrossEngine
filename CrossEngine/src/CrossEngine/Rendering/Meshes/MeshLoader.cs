using CrossEngine.Rendering.Meshes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Textures;

namespace CrossEngine.Loaders
{
    public static class MeshLoader
    {
        [ThreadStatic]
        internal static Func<Action, Task> ServiceRequest;
        
        public struct WavefrontVertex
        {
            public Vector3 Position;
            public Vector2 TexCoord;
            public Vector3 Normal;

            public WavefrontVertex(Vector3 position, Vector2 texCoord, Vector3 normal)
            {
                Position = position;
                TexCoord = texCoord;
                Normal = normal;
            }
        }

        //class WavefrontMaterial : IMaterial
        //{
        //    
        //}

        static readonly NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        public static Mesh<WavefrontVertex> LoadObj(Stream stream)
        {
            var mesh = ParseObj(stream);
            ServiceRequest.Invoke(mesh.SetupGpuResources);
            return mesh;
        }

        public static void ParseMtl(Stream stream)
        {
            
        }

        public static Mesh<WavefrontVertex> ParseObj(Stream stream)
        {
            List<WavefrontVertex> vertices = new();
            List<Vector3> position = new();
            List<Vector3> normal = new();
            List<Vector2> texture = new();
            
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // fixme: yup, i see the problem
                    var parts = line.Split(' ');
                    switch (line)
                    {
                        case var l when l.StartsWith("#"):
                            break;

                        case var l when l.StartsWith("o "):
                            Debug.Assert(parts.Length == 2);
                            Logging.Log.Default.Trace($"o '{parts[1]}'");
                            break;
                        case var l when l.StartsWith("mtllib "):
                            Debug.Assert(parts.Length == 2);
                            Logging.Log.Default.Trace($"mtllib '{parts[1]}'");
                            break;
                        case var l when l.StartsWith("usemtl "):
                            Debug.Assert(parts.Length == 2);
                            Logging.Log.Default.Trace($"usemtl '{parts[1]}'");
                            break;
                        case var l when l.StartsWith("s "):
                            Debug.Assert(parts.Length == 2);
                            Logging.Log.Default.Trace($"s '{parts[1]}'");
                            break;

                        case var l when l.StartsWith("v "):
                            Debug.Assert(parts.Length == 4);
                            position.Add(new(FloatParse(parts[1]), FloatParse(parts[2]), FloatParse(parts[3])));
                            break;
                        case var l when l.StartsWith("vn "):
                            Debug.Assert(parts.Length == 4);
                            normal.Add(new(FloatParse(parts[1]), FloatParse(parts[2]), FloatParse(parts[3])));
                            break;
                        case var l when l.StartsWith("vt "):
                            Debug.Assert(parts.Length == 3);
                            texture.Add(new(FloatParse(parts[1]), FloatParse(parts[2])));
                            break;
                        case var l when l.StartsWith("f "):
                            Debug.Assert(parts.Length == 4);
                            for (int i = 0; i < 3; i++) // hope for a tri
                            {
                                // eeew, wtf
                                int?[] indexes = parts[i + 1].Split("/").Select<string, int?>(v => string.Empty != v ? int.Parse(v) : null).ToArray();
                                Debug.Assert(indexes.Length >= 1);
                                vertices.Add(new(
                                    position[indexes[0].Value - 1],
                                    (indexes.Length >= 2 && indexes[1].HasValue) ? texture[indexes[1].Value - 1] : default,
                                    (indexes.Length >= 3 && indexes[2].HasValue) ? normal[indexes[2].Value - 1] : default));
                            }
                            break;
                        default:
                            Debug.Assert(false, $"unsupported line '{line}'");
                            break;
                    }
                }
            }

            Mesh<WavefrontVertex> mesh = new(vertices.ToArray());
            return mesh;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float FloatParse(in string s) => float.Parse(s, nfi);

        public static void Free(IMesh mesh)
        {
            ServiceRequest.Invoke(mesh.FreeGpuResources);
        }
    }
}
