using System;
using static OpenGL.GL;

using System.Numerics;
using System.Runtime.InteropServices;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Sprites;

namespace CrossEngine.Rendering
{
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
            "uniform mat4 uViewProjection;\n" +
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
            "	vec4 texColor = vColor;\n" +
            "	texColor *= vec4(texture(uTextures[int(vTexIndex + 0.5)], vTexCoord).xyz, 1);\n" +
            "   if (texColor.w < 1 - vColor.w)\n" +
            "       discard;\n" +
            "	oColor = texColor;\n" +
            "   oEntityIDColor = vEntityID;\n" +
            "}\n";
        #endregion

        struct QuadVertex
        {
            public Vector3 position;
            public Vector4 color;
            public Vector2 texCoord;
            public float texIndex;

            // editor-only
            public int entityId;
        };

        struct Renderer2DData
        {
            public const uint MaxQuads = 5000;
            public const uint MaxVertices = MaxQuads * 4;
            public const uint MaxIndices = MaxQuads * 6;
            public const uint MaxTextureSlots = 32;

            public VertexArray quadVertexArray;
            public VertexBuffer quadVertexBuffer;
            public Shader currentShader;
            public Shader discardingShader;
            public Shader regularShader;
            public Texture whiteTexture;

            public uint quadIndexCount;
            public QuadVertex[] quadVertexBufferBase;
            public unsafe QuadVertex* quadVertexBufferPtr;

            public Texture[] textureSlots;
            public uint textureSlotIndex;

            public RendererStats stats;

            //public struct CameraData
            //{
            //    public Matrix4x4 viewProjection;
            //}
            //public CameraData cameraData;
            //public UniformBuffer cameraUniformBuffer;
        }

        static Renderer2DData data;

        static readonly Vector3[] quadVertexPositions = new Vector3[4] {
                new Vector3(-0.5f, -0.5f,  0.0f),
                new Vector3( 0.5f, -0.5f,  0.0f),
                new Vector3( 0.5f,  0.5f,  0.0f),
                new Vector3(-0.5f,  0.5f,  0.0f)
            };
        static readonly Vector2[] quadTextureCoords = new Vector2[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

        public static unsafe void Init()
        {
            data.quadVertexArray = new VertexArray();

            data.quadVertexBuffer = new VertexBuffer(null, (int)(Renderer2DData.MaxVertices * sizeof(QuadVertex)), BufferUsage.DynamicDraw);
            data.quadVertexBuffer.SetLayout(new BufferLayout(
                new BufferElement(ShaderDataType.Float3, "aPosition"),
                new BufferElement(ShaderDataType.Float4, "aColor"),
                new BufferElement(ShaderDataType.Float2, "aTexCoord"),
                new BufferElement(ShaderDataType.Float, "aTexIndex"),
                /*new BufferElement(ShaderDataType.Float, "aTilingFactor"),*/
                new BufferElement(ShaderDataType.Int, "aEntityID")
            ));
            data.quadVertexArray.AddVertexBuffer(data.quadVertexBuffer);

            data.quadVertexBufferBase = new QuadVertex[Renderer2DData.MaxVertices];

            uint[] quadIndices = new uint[(int)Renderer2DData.MaxIndices];

            uint offset = 0;
            for (uint i = 0; i < Renderer2DData.MaxIndices; i += 6)
            {
                quadIndices[i + 0] = offset + 0;
                quadIndices[i + 1] = offset + 1;
                quadIndices[i + 2] = offset + 2;

                quadIndices[i + 3] = offset + 2;
                quadIndices[i + 4] = offset + 3;
                quadIndices[i + 5] = offset + 0;

                offset += 4;
            }

            IndexBuffer quadIB;
            fixed (uint* p = &quadIndices[0])
                quadIB = new IndexBuffer(p, (int)(Renderer2DData.MaxIndices * sizeof(uint)));

            data.quadVertexArray.SetIndexBuffer(quadIB);
            quadIndices = null; // marked for deletion i hope


            data.textureSlots = new Texture[Renderer2DData.MaxTextureSlots];
            data.whiteTexture = new Texture(0xffffffff);

            int[] samplers = new int[Renderer2DData.MaxTextureSlots];
            for (uint i = 0; i < Renderer2DData.MaxTextureSlots; i++)
                samplers[i] = (int)i;

            data.regularShader = new Shader(VertexShaderSource, FragmentShaderSource); //AssetManager.Shaders.GetShader("shaders/batch/texturedbatch.shader");
            data.discardingShader = new Shader(VertexShaderSource, DiscardingFragmentShaderSource);

            data.regularShader.Use();
            data.regularShader.SetIntVec("uTextures", samplers);
            data.discardingShader.Use();
            data.discardingShader.SetIntVec("uTextures", samplers);

            data.currentShader = data.regularShader;

            data.textureSlots[0] = data.whiteTexture;

            //data.cameraUniformBuffer = new UniformBuffer(null, sizeof(Renderer2DData.CameraData), BufferUsage.DynamicDraw);
            //data.cameraUniformBuffer.BindTo(0);
        }

        public static void Shutdown()
        {

        }

        public static void BeginScene(Matrix4x4 viewProjectionMatrix)
        {
            data.currentShader.Use();
            data.currentShader.SetMat4("uViewProjection", viewProjectionMatrix);

            StartBatch();
        }

        public static void EndScene()
        {
            Flush();
        }

        #region Batch
        static unsafe void StartBatch()
        {
            data.quadIndexCount = 0;
            fixed (QuadVertex* p = &data.quadVertexBufferBase[0])
                data.quadVertexBufferPtr = p;

            data.textureSlotIndex = 1;
        }

        static unsafe void Flush()
        {
            if (data.quadIndexCount == 0)
                return;

            uint dataSize;
            fixed (QuadVertex* p = &data.quadVertexBufferBase[0])
            {
                dataSize = (uint)((byte*)data.quadVertexBufferPtr - (byte*)p);
                data.quadVertexBuffer.SetData(p, (int)dataSize);
            }

            // bind textures
            for (uint i = 0; i < data.textureSlotIndex; i++)
                data.textureSlots[i].Bind((int)i);

            data.currentShader.Use();
            Renderer.DrawIndexed(DrawMode.Traingles, data.quadVertexArray, (int)data.quadIndexCount);

            data.stats.drawCalls++;
        }

        static void NextBatch()
        {
            Flush();
            StartBatch();
        }
        #endregion

        public static void EnableDiscardingTransparency(bool enable)
        {
            data.currentShader = enable ? data.discardingShader : data.regularShader;
        }

        // simple
        public static void DrawQuad(Vector2 position, Vector2 size, Vector4 color) => DrawQuad(new Vector3(position, 0.0f), size, color);
        public static void DrawQuad(Vector3 position, Vector2 size, Vector4 color)
        {
            Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateTranslation(position);
            DrawQuad(transform, color);
        }

        // textured
        public static void DrawQuad(Vector2 position, Vector2 size, Texture texture, Vector4 tintColor) => DrawQuad(new Vector3(position, 0.0f), size, texture, tintColor);
        public static void DrawQuad(Vector3 position, Vector2 size, Texture texture, Vector4 tintColor)
        {
            Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateTranslation(position);
            DrawQuad(transform, texture, tintColor);
        }



        public static unsafe void DrawQuad(Matrix4x4 transform, Vector4 color, int entityID = 0)
        {
            const float textureIndex = 0.0f; // white texture index

            if (data.quadIndexCount >= Renderer2DData.MaxIndices)
                NextBatch();

            for (uint i = 0; i < quadVertexPositions.Length; i++)
            {
                data.quadVertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
                data.quadVertexBufferPtr->color = color;
                data.quadVertexBufferPtr->texCoord = quadTextureCoords[i];
                data.quadVertexBufferPtr->texIndex = textureIndex;
                //data.quadVertexBufferPtr->tilingFactor = tilingFactor;
                data.quadVertexBufferPtr->entityId = entityID;
                data.quadVertexBufferPtr++;
            }

            data.quadIndexCount += 6;

            data.stats.itemCount++;
        }

        public static unsafe void DrawQuad(Matrix4x4 transform, Texture texture, Vector4 tintColor, int entityId = 0)
        {
            if (data.quadIndexCount >= Renderer2DData.MaxIndices)
                NextBatch();

            float textureIndex = 0.0f;
            for (uint i = 0; i < data.textureSlotIndex; i++)
            {
                if (data.textureSlots[i] == texture)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                if (data.textureSlotIndex >= Renderer2DData.MaxTextureSlots)
                    NextBatch();

                textureIndex = data.textureSlotIndex;
                data.textureSlots[data.textureSlotIndex] = texture;
                data.textureSlotIndex++;
            }

            for (uint i = 0; i < quadVertexPositions.Length; i++)
            {
                data.quadVertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
                data.quadVertexBufferPtr->color = tintColor;
                data.quadVertexBufferPtr->texCoord = quadTextureCoords[i];
                data.quadVertexBufferPtr->texIndex = textureIndex;
                //data.quadVertexBufferPtr->tilingFactor = tilingFactor;
                data.quadVertexBufferPtr->entityId = entityId;
                data.quadVertexBufferPtr++;
            }

            data.quadIndexCount += 6;

            data.stats.itemCount++;
        }

        public static unsafe void DrawQuad(Matrix4x4 transform, Texture texture, Vector4 tintColor, Vector4 texOffsets, int entityId = 0)
        {
            if (data.quadIndexCount >= Renderer2DData.MaxIndices)
                NextBatch();

            float textureIndex = 0.0f;
            for (uint i = 0; i < data.textureSlotIndex; i++)
            {
                if (data.textureSlots[i] == texture)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                if (data.textureSlotIndex >= Renderer2DData.MaxTextureSlots)
                    NextBatch();

                textureIndex = data.textureSlotIndex;
                data.textureSlots[data.textureSlotIndex] = texture;
                data.textureSlotIndex++;
            }

            Vector2 texOff = new Vector2(texOffsets.X, texOffsets.Y);
            Vector2 texMult = new Vector2(texOffsets.Z, texOffsets.W);
            for (uint i = 0; i < quadVertexPositions.Length; i++)
            {
                data.quadVertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
                data.quadVertexBufferPtr->color = tintColor;
                data.quadVertexBufferPtr->texCoord = quadTextureCoords[i] * texMult + texOff;
                data.quadVertexBufferPtr->texIndex = textureIndex;
                //data.quadVertexBufferPtr->tilingFactor = tilingFactor;
                data.quadVertexBufferPtr->entityId = entityId;
                data.quadVertexBufferPtr++;
            }

            data.quadIndexCount += 6;

            data.stats.itemCount++;
        }

        // simple rotated
        public static void DrawRotatedQuad(Vector2 position, Vector2 size, float rotation, Vector4 color) => DrawRotatedQuad(new Vector3(position, 0.0f), size, rotation, color);
        public static void DrawRotatedQuad(Vector3 position, Vector2 size, float rotation, Vector4 color)
        {
            Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position);
            DrawQuad(transform, color);
        }

        public static void DrawRotatedQuad(Vector3 position, Vector2 size, Quaternion rotation, Vector4 color)
        {
            Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
            DrawQuad(transform, color);
        }

        // textured rotated
        public static void DrawRotatedQuad(Vector2 position, Vector2 size, float rotation, Texture texture, Vector4 tintColor) => DrawRotatedQuad(new Vector3(position, 0.0f), size, rotation, texture, tintColor);
        public static void DrawRotatedQuad(Vector3 position, Vector2 size, float rotation, Texture texture, Vector4 tintColor)
        {
            Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position);
            DrawQuad(transform, texture, tintColor);
        }

        public static void DrawRotatedQuad(Vector3 position, Vector2 size, Quaternion rotation, Texture texture, Vector4 tintColor)
        {
            Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
            DrawQuad(transform, texture, tintColor);
        }

        // sprites
        public static unsafe void DrawSprite(Matrix4x4 transform, Sprite sprite, Vector4 tintColor, int entityId = 0)
        {
            float tilingFactor = 1.0f;

            if (data.quadIndexCount >= Renderer2DData.MaxIndices)
                NextBatch();

            float textureIndex = 0.0f;
            for (uint i = 0; i < data.textureSlotIndex; i++)
            {
                if (data.textureSlots[i] == sprite.Texture)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                if (data.textureSlotIndex >= Renderer2DData.MaxTextureSlots)
                    NextBatch();

                textureIndex = data.textureSlotIndex;
                data.textureSlots[data.textureSlotIndex] = sprite.Texture;
                data.textureSlotIndex++;
            }

            for (uint i = 0; i < quadVertexPositions.Length; i++)
            {
                data.quadVertexBufferPtr->position = Vector3.Transform(quadVertexPositions[i], transform);
                data.quadVertexBufferPtr->color = tintColor;
                data.quadVertexBufferPtr->texCoord = sprite.TexCoords[i] * tilingFactor;
                data.quadVertexBufferPtr->texIndex = textureIndex;
                //data.quadVertexBufferPtr->tilingFactor = tilingFactor;
                data.quadVertexBufferPtr->entityId = entityId;
                data.quadVertexBufferPtr++;
            }

            data.quadIndexCount += 6;

            data.stats.itemCount++;
        }

        public static void DrawRotatedSprite(Vector3 position, Vector2 size, Quaternion rotation, Sprite sprite, Vector4 tintColor, int entityId = 0)
        {
            Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
            DrawSprite(transform, sprite, tintColor, entityId);
        }

        public static void ResetStats() => data.stats.Reset();
        public static RendererStats GetStats() => data.stats;
    }
}
