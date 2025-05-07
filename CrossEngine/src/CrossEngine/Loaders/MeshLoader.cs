#define LOG_UNUSED

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
using CrossEngine.Geometry;
using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Extensions;

namespace CrossEngine.Loaders
{
    public static class MeshLoader
    {
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

        public class WavefrontMaterial : IMaterial, ISerializable
        {
            [SerializeInclude] [EditorColor]
            public Vector3 ambient;
            [SerializeInclude] [EditorColor]
            public Vector3 diffuse;
            [SerializeInclude] [EditorColor]
            public Vector3 specular;
            [SerializeInclude] [EditorColor]
            public Vector3 emissive;
            [SerializeInclude] [EditorDrag]
            public float specularExponent;
            [SerializeInclude] [EditorDrag]
            public float disolve;
            [SerializeInclude] [EditorDrag]
            public float refractiveIndex;

            public WeakReference<Texture> mapDiffuse;

            public string texturePathDiffuse;

            public WeakReference<ShaderProgram> Shader { get; set; }

            public void Update(ShaderProgram shader)
            {
                shader.Use();
                shader.SetParameterVec3("uMaterial.Ambient", ambient);
                shader.SetParameterVec3("uMaterial.Diffuse", diffuse);
                shader.SetParameterVec3("uMaterial.Specular", specular);
                shader.SetParameterVec3("uMaterial.Emissive", emissive);
                shader.SetParameterFloat("uMaterial.SpecularExponent", specularExponent);
                shader.SetParameterFloat("uMaterial.Disolve", disolve);
                shader.SetParameterFloat("uMaterial.RefractiveIndex", refractiveIndex);
                
                shader.SetParameterInt("uMaterial.MapDiffuse", 0);
                (mapDiffuse ?? TextureLoader.WhiteTexture).GetValue().Bind(0);
            }

            public void GetObjectData(SerializationInfo info) => Serializer.UseAttributesWrite(this, info);
            public void SetObjectData(SerializationInfo info) => Serializer.UseAttributesRead(this, info);
        }

        static readonly NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = "." };
        
        public static Dictionary<string, WavefrontMaterial> ParseMtl(Stream stream)
        {
            Dictionary<string, WavefrontMaterial> materials = new();

            Vector3 ambient = Vector3.One;
            Vector3 diffuse = Vector3.Zero;
            Vector3 specular = Vector3.Zero;
            Vector3 emissive = Vector3.Zero;
            float specularExponent = 0; // 0 to 1000
            float disolve = 0;
            float refractiveIndex = 0;
            
            string texturePathDiffuse = null;
            
            string currentNameMaterial = null;

            void CompleteMaterial()
            {
                if (currentNameMaterial == null) return;
                
                WavefrontMaterial material = new();
                
                material.ambient = ambient;
                material.diffuse = diffuse;
                material.specular = specular;
                material.emissive = emissive;
                material.specularExponent = specularExponent;
                material.disolve = disolve;
                material.refractiveIndex = refractiveIndex;
                
                material.texturePathDiffuse = texturePathDiffuse;

                materials[currentNameMaterial] = material;
                
                ambient = Vector3.One;
                diffuse = Vector3.Zero;
                specular = Vector3.Zero;
                emissive = Vector3.Zero;
                specularExponent = 0;
                disolve = 0;
                refractiveIndex = 0;
                
                texturePathDiffuse = null;
            }
            
            LineReadingWrapper(stream, (line, parts) =>
            {
                switch (line)
                {
                    case var l when l.StartsWith("#"):
                        break;

                    case var l when l.StartsWith("newmtl "):
                        Debug.Assert(parts.Length == 2);
                        CompleteMaterial();
                        currentNameMaterial = parts[1];
                        break;

                    case var l when l.StartsWith("Ka "):
                        Debug.Assert(parts.Length == 4);
                        ambient = new(FloatParse(parts[1]), FloatParse(parts[2]), FloatParse(parts[3]));
                        break;
                    case var l when l.StartsWith("Kd "):
                        Debug.Assert(parts.Length == 4);
                        diffuse = new(FloatParse(parts[1]), FloatParse(parts[2]), FloatParse(parts[3]));
                        break;
                    case var l when l.StartsWith("Ks "):
                        Debug.Assert(parts.Length == 4);
                        specular = new(FloatParse(parts[1]), FloatParse(parts[2]), FloatParse(parts[3]));
                        break;
                    case var l when l.StartsWith("Ke "):
                        Debug.Assert(parts.Length == 4);
                        emissive = new(FloatParse(parts[1]), FloatParse(parts[2]), FloatParse(parts[3]));
                        break;
                    
                    case var l when l.StartsWith("Ns "):
                        Debug.Assert(parts.Length == 2);
                        specularExponent = FloatParse(parts[1]);
                        break;
                    case var l when l.StartsWith("d "):
                        Debug.Assert(parts.Length == 2);
                        disolve = FloatParse(parts[1]);
                        break;
                    case var l when l.StartsWith("Ni "):
                        Debug.Assert(parts.Length == 2);
                        refractiveIndex = FloatParse(parts[1]);
                        break;
                    
                    case var l when l.StartsWith("map_Kd "):
                        Debug.Assert(parts.Length == 2);
                        texturePathDiffuse = parts[1];
                        break;
                    
                    case var l when l.StartsWith("illum "):
                        Debug.Assert(parts.Length == 2);
#if LOG_UNUSED
                        Logging.Log.Default.Trace($"illum '{parts[1]}'");
#endif
                        break;
                    
                    default:
                        Debug.Assert(false, $"unsupported line '{line}'");
                        break;
                }
            });
            
            CompleteMaterial();

            return materials;
        }

        public static Dictionary<string, Mesh<WavefrontVertex>> ParseObj(Stream stream)
        {
            List<WavefrontVertex> vertices = new();
            List<Vector3> position = new();
            List<Vector3> normal = new();
            List<Vector2> texture = new();
            
            Dictionary<string, Mesh<WavefrontVertex>> meshes = new();
            
            string currentNameMesh = null;

            void CompleteMesh()
            {
                if (currentNameMesh == null) return;
                
                meshes.Add(currentNameMesh, new Mesh<WavefrontVertex>(vertices.ToArray()));
                
                vertices.Clear();
            }
            
            LineReadingWrapper(stream, (line, parts) =>
            {
                switch (line)
                {
                    case var l when l.StartsWith("#"):
                        break;

                    case var l when l.StartsWith("o "):
                        Debug.Assert(parts.Length == 2);
                        CompleteMesh();
                        currentNameMesh = parts[1];
                        break;
                    case var l when l.StartsWith("g "):
                        Debug.Assert(parts.Length == 2);
#if LOG_UNUSED
                        Logging.Log.Default.Trace($"g '{parts[1]}'");
#endif
                        break;
                    case var l when l.StartsWith("mtllib "):
                        Debug.Assert(parts.Length == 2);
#if LOG_UNUSED
                        Logging.Log.Default.Trace($"mtllib '{parts[1]}'");
#endif
                        break;
                    case var l when l.StartsWith("usemtl "):
                        Debug.Assert(parts.Length == 2);
#if LOG_UNUSED
                        Logging.Log.Default.Trace($"usemtl '{parts[1]}'");
#endif
                        break;
                    case var l when l.StartsWith("s "):
                        Debug.Assert(parts.Length == 2);
#if LOG_UNUSED
                        Logging.Log.Default.Trace($"s '{parts[1]}'");
#endif
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
            });
            
            CompleteMesh();

            return meshes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float FloatParse(in string s) => float.Parse(s, nfi);

        private static void LineReadingWrapper(Stream stream, Action<string, string[]> feedLine)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    
                    // fixme: yup, i see the problem
                    var parts = line.Split(' ');
                    
                    feedLine.Invoke(line, parts);
                }
            }
        }
    }
}
