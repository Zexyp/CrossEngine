using System;

using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices;

using CrossEngine.Utils;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Textures;

namespace CrossEngine.Rendering
{
    public struct RendererStats
    {
        public uint ItemCount;
        public uint DrawCalls;

        public void Reset()
        {
            ItemCount = 0;
            DrawCalls = 0;
        }
    }

    public class Renderer2D
    {
        #region Shader Sources
        const string VertexShaderSource =
            "#version 330 core\n" +
            "\n" +
            "layout(location = 0) in vec3 aPosition;\n" +
            "layout(location = 1) in vec4 aColor;\n" +
            "layout(location = 2) in vec2 aTexCoord;\n" +
            "layout(location = 3) in float aTexIndex;\n" +
            "layout(location = 4) in int aEntityID;\n" +
            "\n" +
            "uniform mat4 uViewProjection = mat4(1);\n" +
            "\n" +
            "out vec4 vColor;\n" +
            "out vec2 vTexCoord;\n" +
            "out float vTexIndex;\n" +
            "flat out int vEntityID;\n" +
            "\n" +
            "void main()\n" +
            "{\n" +
            "	vColor = aColor;\n" +
            "	vTexCoord = aTexCoord;\n" +
            "	vTexIndex = aTexIndex;\n" +
            "   vEntityID = aEntityID;\n" +
            "	gl_Position = uViewProjection * vec4(aPosition, 1.0);\n" +
            "}\n";
        const string FragmentShaderSource =
            "#version 330 core\n" +
            "\n" +
            "layout(location = 0) out vec4 oColor;\n" +
            "layout(location = 1) out int oEntityIDColor;\n" +
            "\n" +
            "in vec4 vColor;\n" +
            "in vec2 vTexCoord;\n" +
            "in float vTexIndex;\n" +
            "flat in int vEntityID;\n" +
            "\n" +
            "uniform sampler2D uTextures[32];\n" +
            "\n" +
            "void main()\n" +
            "{\n" +
            "	vec4 texColor = vColor;\n" +
            "	texColor *= texture(uTextures[int(vTexIndex + 0.5)], vTexCoord);\n" +
            "	oColor = texColor;\n" +
            "   oEntityIDColor = vEntityID;\n" +
            "}\n";
            //"float random(vec2 st)\n" +
            //"{\n" +
            //"    return fract(sin(dot(st.xy,\n" +
            //"                         vec2(12.9898, 78.233))) *\n" +
            //"        43758.5453123);\n" +
            //"}\n";
        const string DiscardingFragmentShaderSource =
            "#version 330 core\n" +
            "\n" +
            "layout(location = 0) out vec4 oColor;\n" +
            "layout(location = 1) out int oEntityIDColor;\n" +
            "\n" +
            "in vec4 vColor;\n" +
            "in vec2 vTexCoord;\n" +
            "in float vTexIndex;\n" +
            "flat in int vEntityID;\n" +
            "\n" +
            "uniform sampler2D uTextures[32];\n" +
            "\n" +
            "void main()\n" +
            "{\n" +
            "	vec4 texColor = texture(uTextures[int(vTexIndex + 0.5)], vTexCoord);\n" +
            "	texColor.xyz *= vColor.xyz;\n" +
            "   if (texColor.w + vColor.w - 1 < 1)\n" +
            "       discard;\n" +
            "	oColor = texColor;\n" +
            "   oEntityIDColor = vEntityID;\n" +
            "}\n";
        #endregion

        public struct PrimitiveVertex
        {
            public Vector3 position;
            public Vector4 color;
            public Vector2 texCoord;
            public float texIndex;

            // editor-only
            public int entityId;
        };

        public struct Renderer2DData
        {
            public const uint MaxQuads = 1024;
            public const uint MaxQuadVertices = MaxQuads * 4;
            public const uint MaxQuadIndices = MaxQuads * 6;
            
            public const uint MaxTris = 1024;
            public const uint MaxTriVertices = MaxTris * 3;

            public const uint MaxPointVertices = 1024;

            public const uint MaxTextureSlots = 32;

            public ShaderProgram currentShader;
            public ShaderProgram discardingShader;
            public ShaderProgram regularShader;
            public Ref<Texture> whiteTexture;

            public PrimitivesData quads;
            public PrimitivesData tris;

            public struct PrimitivesData
            {
                public uint IndexCount;
                public PrimitiveVertex[] VertexBufferBase;
                public unsafe PrimitiveVertex* VertexBufferPtr;
                public Ref<VertexArray> VertexArray;
                public Ref<VertexBuffer> VertexBuffer;
                public Ref<Texture>[] TextureSlots;
                public uint TextureSlotIndex;
                public RendererStats Stats;
            }

            //public struct CameraData
            //{
            //    public Matrix4x4 viewProjection;
            //}
            //public CameraData cameraData;
            //public UniformBuffer cameraUniformBuffer;
        }

        public static Renderer2DData data;

        static readonly uint[] quadIndices = new uint[6] {
                0,
                1,
                2,
                2,
                3,
                0,
            };
        static readonly Vector3[] quadVertexPositions = new Vector3[4] {
                new Vector3(-0.5f, -0.5f,  0.0f),
                new Vector3( 0.5f, -0.5f,  0.0f),
                new Vector3( 0.5f,  0.5f,  0.0f),
                new Vector3(-0.5f,  0.5f,  0.0f),
            };
        static readonly Vector2[] quadTextureCoords = new Vector2[4] {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f),
            };

        static readonly Vector2[] triTextureCoords = new Vector2[3] {
                new Vector2(0.5f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
            };

        public static unsafe void Init()
        {
            Shader vertex = new Shader(VertexShaderSource, ShaderType.Vertex);
            Shader fragment = new Shader(FragmentShaderSource, ShaderType.Fragment);
            Shader fragmentDiscarding = new Shader(DiscardingFragmentShaderSource, ShaderType.Fragment);
            data.regularShader = new ShaderProgram(vertex, fragment); //AssetManager.Shaders.GetShader("shaders/batch/texturedbatch.shader");
            data.discardingShader = new ShaderProgram(vertex, fragmentDiscarding);
            vertex.Dispose();
            fragment.Dispose();
            fragmentDiscarding.Dispose();

            data.whiteTexture = Texture.Create(1, 1, ColorFormat.RGBA);
            uint whiteCol = 0xffffffff;
            ((Texture)data.whiteTexture).SetData(&whiteCol, sizeof(uint));

            int[] samplers = new int[Renderer2DData.MaxTextureSlots];
            for (uint i = 0; i < Renderer2DData.MaxTextureSlots; i++)
                samplers[i] = (int)i;
            data.regularShader.Use();
            data.regularShader.SetIntVec("uTextures", samplers);
            data.discardingShader.Use();
            data.discardingShader.SetIntVec("uTextures", samplers);

            data.currentShader = data.regularShader;

            //data.cameraUniformBuffer = new UniformBuffer(null, sizeof(Renderer2DData.CameraData), BufferUsage.DynamicDraw);
            //data.cameraUniformBuffer.BindTo(0);

            BufferLayout layout = new BufferLayout(
                    new BufferElement(ShaderDataType.Float3, "aPosition"),
                    new BufferElement(ShaderDataType.Float4, "aColor"),
                    new BufferElement(ShaderDataType.Float2, "aTexCoord"),
                    new BufferElement(ShaderDataType.Float, "aTexIndex"),
                    new BufferElement(ShaderDataType.Int, "aEntityID")
                );

            // quads
            {
                data.quads.VertexBufferBase = new PrimitiveVertex[Renderer2DData.MaxQuadVertices];
                data.quads.TextureSlots = new Ref<Texture>[Renderer2DData.MaxTextureSlots];

                uint[] indices = new uint[(int)Renderer2DData.MaxQuadIndices];
                uint offset = 0;
                for (uint i = 0; i < Renderer2DData.MaxQuadIndices; i += 6)
                {
                    indices[i + 0] = offset + quadIndices[(i + 0) % 6];
                    indices[i + 1] = offset + quadIndices[(i + 1) % 6];
                    indices[i + 2] = offset + quadIndices[(i + 2) % 6];

                    indices[i + 3] = offset + quadIndices[(i + 3) % 6];
                    indices[i + 4] = offset + quadIndices[(i + 4) % 6];
                    indices[i + 5] = offset + quadIndices[(i + 5) % 6];

                    offset += 4;
                }

                data.quads.VertexBuffer = VertexBuffer.Create(null, (uint)(Renderer2DData.MaxQuadVertices * sizeof(PrimitiveVertex)), BufferUsageHint.DynamicDraw);
                ((VertexBuffer)data.quads.VertexBuffer).SetLayout(layout);

                Ref<IndexBuffer> quadIB;
                fixed (uint* p = &indices[0])
                    quadIB = IndexBuffer.Create(p, Renderer2DData.MaxQuadIndices, IndexDataType.UInt);
                indices = null; // marked for deletion i hope

                data.quads.VertexArray = VertexArray.Create();
                ((VertexArray)data.quads.VertexArray).AddVertexBuffer(data.quads.VertexBuffer);
                ((VertexArray)data.quads.VertexArray).SetIndexBuffer(quadIB);
            }
            // tris
            {
                data.tris.VertexBufferBase = new PrimitiveVertex[Renderer2DData.MaxTriVertices];
                data.tris.TextureSlots = new Ref<Texture>[Renderer2DData.MaxTextureSlots];

                data.tris.VertexBuffer = VertexBuffer.Create(null, (uint)(Renderer2DData.MaxTriVertices * sizeof(PrimitiveVertex)), BufferUsageHint.DynamicDraw);
                ((VertexBuffer)data.tris.VertexBuffer).SetLayout(layout);

                data.tris.VertexArray = VertexArray.Create();
                ((VertexArray)data.tris.VertexArray).AddVertexBuffer(data.tris.VertexBuffer);
            }

            data.quads.TextureSlots[0] = data.whiteTexture;
            data.tris.TextureSlots[0] = data.whiteTexture;
        }

        public static void Shutdown()
        {

        }

        public static void BeginScene(Matrix4x4 viewProjectionMatrix)
        {
            data.currentShader.Use();
            data.currentShader.SetMat4("uViewProjection", viewProjectionMatrix);

            StartQuadsBatch();
            StartTrisBatch();
        }

        public static void Flush()
        {
            FlushQuads();
            FlushTris();
        }

        public static void EndScene()
        {
            FlushQuads();
            FlushTris();
        }

        #region Batch
        #region Quads
        static unsafe void StartQuadsBatch()
        {
            data.quads.IndexCount = 0;
            fixed (PrimitiveVertex* p = data.quads.VertexBufferBase)
                data.quads.VertexBufferPtr = p;

            data.quads.TextureSlotIndex = 1;
        }
        static unsafe void FlushQuads()
        {
            if (data.quads.IndexCount == 0)
                return;

            fixed (PrimitiveVertex* p = data.quads.VertexBufferBase)
                ((VertexBuffer)data.quads.VertexBuffer).SetData(p, (uint)((byte*)data.quads.VertexBufferPtr - (byte*)p)); // we need the count of bytes not structs that's why we need to cast these

            // bind textures
            for (uint i = 0; i < data.quads.TextureSlotIndex; i++)
            {
                if (!Ref.IsNull(data.quads.TextureSlots[i])) ((Texture)data.quads.TextureSlots[i]).Bind(i);
            }

            data.currentShader.Use();
            Application.Instance.RendererAPI.DrawIndexed(data.quads.VertexArray, data.quads.IndexCount/*, DrawMode.Traingles*/);

            data.quads.Stats.DrawCalls++;
        }
        static void NextQuadsBatch()
        {
            FlushQuads();
            StartQuadsBatch();
        }
        #endregion

        #region Tris
        static unsafe void StartTrisBatch()
        {
            data.tris.IndexCount = 0;
            fixed (PrimitiveVertex* p = data.tris.VertexBufferBase)
                data.tris.VertexBufferPtr = p;

            data.tris.TextureSlotIndex = 1;
        }
        static unsafe void FlushTris()
        {
            if (data.tris.IndexCount == 0)
                return;

            fixed (PrimitiveVertex* p = data.tris.VertexBufferBase)
                ((VertexBuffer)data.tris.VertexBuffer).SetData(p, (uint)((byte*)data.tris.VertexBufferPtr - (byte*)p)); // we need the count of bytes not structs that's why we need to cast these

            // bind textures
            for (uint i = 0; i < data.tris.TextureSlotIndex; i++)
            {
                if (!Ref.IsNull(data.tris.TextureSlots[i])) ((Texture)data.tris.TextureSlots[i]).Bind(i);
            }

            data.currentShader.Use();
            Application.Instance.RendererAPI.DrawArray(data.tris.VertexArray, data.tris.IndexCount/*, DrawMode.Traingles*/);

            data.tris.Stats.DrawCalls++;
        }
        static void NextTrisBatch()
        {
            FlushTris();
            StartTrisBatch();
        }
        #endregion
        #endregion

        public static void EnableDiscardingTransparency(bool enable)
        {
            data.currentShader = enable ? data.discardingShader : data.regularShader;
        }

        public static unsafe void DrawQuad(Matrix4x4 transform, Vector4 color, int entityID = 0)
        {
            const float textureIndex = 0.0f; // white texture index

            if (data.quads.IndexCount >= Renderer2DData.MaxQuadIndices)
                NextQuadsBatch();

            for (uint i = 0; i < quadVertexPositions.Length; i++)
            {
                data.quads.VertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
                data.quads.VertexBufferPtr->color = color;
                data.quads.VertexBufferPtr->texCoord = quadTextureCoords[i];
                data.quads.VertexBufferPtr->texIndex = textureIndex;
                data.quads.VertexBufferPtr->entityId = entityID;
                data.quads.VertexBufferPtr++;
            }

            data.quads.IndexCount += 6;

            data.quads.Stats.ItemCount++;
        }

        public static unsafe void DrawTexturedQuad(Matrix4x4 transform, Ref<Texture> texture, Vector4 tintColor, int entityId = 0)
        {
            if (data.quads.IndexCount >= Renderer2DData.MaxQuadIndices)
                NextQuadsBatch();

            float textureIndex = 0.0f;
            for (uint i = 0; i < data.quads.TextureSlotIndex; i++)
            {
                if (((Texture)data.quads.TextureSlots[i]) == ((Texture)texture))
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                if (data.quads.TextureSlotIndex >= Renderer2DData.MaxTextureSlots)
                    NextQuadsBatch();

                textureIndex = data.quads.TextureSlotIndex;
                data.quads.TextureSlots[data.quads.TextureSlotIndex] = texture;
                data.quads.TextureSlotIndex++;
            }

            for (uint i = 0; i < quadVertexPositions.Length; i++)
            {
                data.quads.VertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
                data.quads.VertexBufferPtr->color = tintColor;
                data.quads.VertexBufferPtr->texCoord = quadTextureCoords[i];
                data.quads.VertexBufferPtr->texIndex = textureIndex;
                data.quads.VertexBufferPtr->entityId = entityId;
                data.quads.VertexBufferPtr++;
            }

            data.quads.IndexCount += 6;

            data.quads.Stats.ItemCount++;
        }

        public static unsafe void DrawTexturedQuad(Matrix4x4 transform, Ref<Texture> texture, Vector4 tintColor, Vector4 texOffsets, int entityId = 0)
        {
            if (data.quads.IndexCount >= Renderer2DData.MaxQuadIndices)
                NextQuadsBatch();

            float textureIndex = 0.0f;
            for (uint i = 0; i < data.quads.TextureSlotIndex; i++)
            {
                if (((Texture)data.quads.TextureSlots[i]) == ((Texture)texture))
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                if (data.quads.TextureSlotIndex >= Renderer2DData.MaxTextureSlots)
                    NextQuadsBatch();

                textureIndex = data.quads.TextureSlotIndex;
                data.quads.TextureSlots[data.quads.TextureSlotIndex] = texture;
                data.quads.TextureSlotIndex++;
            }

            Vector2 texOff = new Vector2(texOffsets.X, texOffsets.Y);
            Vector2 texMult = new Vector2(texOffsets.Z, texOffsets.W);
            for (uint i = 0; i < quadVertexPositions.Length; i++)
            {
                data.quads.VertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
                data.quads.VertexBufferPtr->color = tintColor;
                data.quads.VertexBufferPtr->texCoord = quadTextureCoords[i] * texMult + texOff;
                data.quads.VertexBufferPtr->texIndex = textureIndex;
                data.quads.VertexBufferPtr->entityId = entityId;
                data.quads.VertexBufferPtr++;
            }

            data.quads.IndexCount += 6;

            data.quads.Stats.ItemCount++;
        }

        public static unsafe void DrawTri(Vector3 p1, Vector3 p2, Vector3 p3, Vector4 color, int entityID = 0)
        {
            if (data.tris.IndexCount >= Renderer2DData.MaxTriVertices)
                NextTrisBatch();

            (data.tris.VertexBufferPtr + 0)->position = p1;
            (data.tris.VertexBufferPtr + 1)->position = p2;
            (data.tris.VertexBufferPtr + 2)->position = p3;
            for (uint i = 0; i < 3; i++)
            {
                data.tris.VertexBufferPtr->color = color;
                data.tris.VertexBufferPtr->texIndex = 0.0f;
                data.tris.VertexBufferPtr->entityId = entityID;
                data.tris.VertexBufferPtr++;
            }

            data.tris.IndexCount += 3;

            data.tris.Stats.ItemCount++;
        }

        public static unsafe void DrawTexturedTri(Vector3 p1, Vector3 p2, Vector3 p3, Ref<Texture> texture, Vector4 tintColor, int entityId = 0)
        {
            if (data.tris.IndexCount >= Renderer2DData.MaxTriVertices)
                NextTrisBatch();

            float textureIndex = 0.0f;
            for (uint i = 0; i < data.tris.TextureSlotIndex; i++)
            {
                if (((Texture)data.tris.TextureSlots[i]) == ((Texture)texture))
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                if (data.tris.TextureSlotIndex >= Renderer2DData.MaxTextureSlots)
                    NextTrisBatch();

                textureIndex = data.tris.TextureSlotIndex;
                data.tris.TextureSlots[data.tris.TextureSlotIndex] = texture;
                data.tris.TextureSlotIndex++;
            }

            (data.tris.VertexBufferPtr + 0)->position = p1;
            (data.tris.VertexBufferPtr + 1)->position = p2;
            (data.tris.VertexBufferPtr + 2)->position = p3;
            for (uint i = 0; i < 3; i++)
            {
                data.tris.VertexBufferPtr->color = tintColor;
                data.tris.VertexBufferPtr->texCoord = triTextureCoords[i];
                data.tris.VertexBufferPtr->texIndex = textureIndex;
                data.tris.VertexBufferPtr->entityId = entityId;
                data.tris.VertexBufferPtr++;
            }

            data.tris.IndexCount += 3;

            data.tris.Stats.ItemCount++;
        }

        public static unsafe void DrawTexturedTri(Vector3 p1, Vector3 p2, Vector3 p3, Ref<Texture> texture, Vector4 tintColor, Vector2 uv1, Vector2 uv2, Vector2 uv3, int entityId = 0)
        {
            if (data.tris.IndexCount >= Renderer2DData.MaxTriVertices)
                NextTrisBatch();

            float textureIndex = 0.0f;
            for (uint i = 0; i < data.tris.TextureSlotIndex; i++)
            {
                if (((Texture)data.tris.TextureSlots[i]) == ((Texture)texture))
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                if (data.tris.TextureSlotIndex >= Renderer2DData.MaxTextureSlots)
                    NextTrisBatch();

                textureIndex = data.tris.TextureSlotIndex;
                data.tris.TextureSlots[data.tris.TextureSlotIndex] = texture;
                data.tris.TextureSlotIndex++;
            }

            (data.tris.VertexBufferPtr + 0)->position = p1;
            (data.tris.VertexBufferPtr + 1)->position = p2;
            (data.tris.VertexBufferPtr + 2)->position = p3;
            (data.tris.VertexBufferPtr + 0)->texCoord = uv1;
            (data.tris.VertexBufferPtr + 1)->texCoord = uv2;
            (data.tris.VertexBufferPtr + 2)->texCoord = uv3;
            for (uint i = 0; i < 3; i++)
            {
                data.tris.VertexBufferPtr->color = tintColor;
                data.tris.VertexBufferPtr->texIndex = textureIndex;
                data.tris.VertexBufferPtr->entityId = entityId;
                data.tris.VertexBufferPtr++;
            }

            data.tris.IndexCount += 3;

            data.tris.Stats.ItemCount++;
        }

        //// simple
        //public static void DrawQuad(Vector2 position, Vector2 size, Vector4 color) => DrawQuad(new Vector3(position, 0.0f), size, color);
        //public static void DrawQuad(Vector3 position, Vector2 size, Vector4 color)
        //{
        //    Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateTranslation(position);
        //    DrawQuad(transform, color);
        //}
        //
        //// textured
        //public static void DrawQuad(Vector2 position, Vector2 size, Ref<Texture> texture, Vector4 tintColor) => DrawQuad(new Vector3(position, 0.0f), size, texture, tintColor);
        //public static void DrawQuad(Vector3 position, Vector2 size, Ref<Texture> texture, Vector4 tintColor)
        //{
        //    Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateTranslation(position);
        //    DrawQuad(transform, texture, tintColor);
        //}



        //public static unsafe void DrawQuad(Matrix4x4 transform, Ref<Texture> texture, Vector4 tintColor, Vector4 texOffsets, int entityId = 0)
        //{
        //    if (data.quadIndexCount >= Renderer2DData.MaxIndices)
        //        NextBatch();
        //
        //    float textureIndex = 0.0f;
        //    for (uint i = 0; i < data.quadTextureSlotIndex; i++)
        //    {
        //        if (((Texture)data.quadTextureSlots[i]) == ((Texture)texture))
        //        {
        //            textureIndex = i;
        //            break;
        //        }
        //    }
        //
        //    if (textureIndex == 0.0f)
        //    {
        //        if (data.quadTextureSlotIndex >= Renderer2DData.MaxTextureSlots)
        //            NextBatch();
        //
        //        textureIndex = data.quadTextureSlotIndex;
        //        data.quadTextureSlots[data.quadTextureSlotIndex] = texture;
        //        data.quadTextureSlotIndex++;
        //    }
        //
        //    Vector2 texOff = new Vector2(texOffsets.X, texOffsets.Y);
        //    Vector2 texMult = new Vector2(texOffsets.Z, texOffsets.W);
        //    for (uint i = 0; i < quadVertexPositions.Length; i++)
        //    {
        //        data.quadVertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
        //        data.quadVertexBufferPtr->color = tintColor;
        //        data.quadVertexBufferPtr->texCoord = quadTextureCoords[i] * texMult + texOff;
        //        data.quadVertexBufferPtr->texIndex = textureIndex;
        //        //data.quadVertexBufferPtr->tilingFactor = tilingFactor;
        //        data.quadVertexBufferPtr->entityId = entityId;
        //        data.quadVertexBufferPtr++;
        //    }
        //
        //    data.quadIndexCount += 6;
        //
        //    data.stats.itemCount++;
        //}

        //// simple rotated
        //public static void DrawRotatedQuad(Vector2 position, Vector2 size, float rotation, Vector4 color) => DrawRotatedQuad(new Vector3(position, 0.0f), size, rotation, color);
        //public static void DrawRotatedQuad(Vector3 position, Vector2 size, float rotation, Vector4 color)
        //{
        //    Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position);
        //    DrawQuad(transform, color);
        //}
        //
        //public static void DrawRotatedQuad(Vector3 position, Vector2 size, Quaternion rotation, Vector4 color)
        //{
        //    Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        //    DrawQuad(transform, color);
        //}
        //
        //// textured rotated
        //public static void DrawRotatedQuad(Vector2 position, Vector2 size, float rotation, Ref<Texture> texture, Vector4 tintColor) => DrawRotatedQuad(new Vector3(position, 0.0f), size, rotation, texture, tintColor);
        //public static void DrawRotatedQuad(Vector3 position, Vector2 size, float rotation, Ref<Texture> texture, Vector4 tintColor)
        //{
        //    Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position);
        //    DrawQuad(transform, texture, tintColor);
        //}
        //
        //public static void DrawRotatedQuad(Vector3 position, Vector2 size, Quaternion rotation, Ref<Texture> texture, Vector4 tintColor)
        //{
        //    Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        //    DrawQuad(transform, texture, tintColor);
        //}

        // sprites
        //public static unsafe void DrawSprite(Matrix4x4 transform, Sprite sprite, Vector4 tintColor, int entityId = 0)
        //{
        //    float tilingFactor = 1.0f;
        //
        //    if (data.quadIndexCount >= Renderer2DData.MaxIndices)
        //        NextBatch();
        //
        //    float textureIndex = 0.0f;
        //    for (uint i = 0; i < data.textureSlotIndex; i++)
        //    {
        //        if (data.textureSlots[i] == sprite.Texture)
        //        {
        //            textureIndex = i;
        //            break;
        //        }
        //    }
        //
        //    if (textureIndex == 0.0f)
        //    {
        //        if (data.textureSlotIndex >= Renderer2DData.MaxTextureSlots)
        //            NextBatch();
        //
        //        textureIndex = data.textureSlotIndex;
        //        data.textureSlots[data.textureSlotIndex] = sprite.Texture;
        //        data.textureSlotIndex++;
        //    }
        //
        //    for (uint i = 0; i < quadVertexPositions.Length; i++)
        //    {
        //        data.quadVertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
        //        data.quadVertexBufferPtr->color = tintColor;
        //        data.quadVertexBufferPtr->texCoord = sprite.TexCoords[i] * tilingFactor;
        //        data.quadVertexBufferPtr->texIndex = textureIndex;
        //        //data.quadVertexBufferPtr->tilingFactor = tilingFactor;
        //        data.quadVertexBufferPtr->entityId = entityId;
        //        data.quadVertexBufferPtr++;
        //    }
        //
        //    data.quadIndexCount += 6;
        //
        //    data.stats.itemCount++;
        //}
        //
        //public static void DrawRotatedSprite(Vector3 position, Vector2 size, Quaternion rotation, Sprite sprite, Vector4 tintColor, int entityId = 0)
        //{
        //    Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        //    DrawSprite(transform, sprite, tintColor, entityId);
        //}

        public static void ResetStats()
        {
            data.quads.Stats.Reset();
            data.tris.Stats.Reset();
        }
        //public static RendererStats GetStats() => data.stats;
    }
}
