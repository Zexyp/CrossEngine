using static OpenGL.GL;
using System;

using System.Numerics;

using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization.Json;

namespace CrossEngine.Rendering.Sprites
{
    public class Sprite : ISerializable
    {
        public Texture Texture;
        public Vector2[] TexCoords;
        public Vector2 SizeInPixels;

        public Sprite(Texture texture)
        {
            this.Texture = texture;

            Vector2[] texCoords = {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };
            this.TexCoords = texCoords;

            SizeInPixels = new Vector2(this.Texture.Width, this.Texture.Height);
        }

        public Sprite(Texture texture, Vector2[] texCoords, Vector2 sizePixels)
        {
            if (texCoords.Length != 4)
                throw new Exception("wrong number of texture coordinates");

            this.Texture = texture;
            this.TexCoords = texCoords;
            this.SizeInPixels = sizePixels;
        }

        #region ISerializable
        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue("Texture", Texture);
            info.AddValue("TexCoords", TexCoords);
            info.AddValue("SizeInPixels", SizeInPixels);
        }

        public Sprite(DeserializationInfo info)
        {
            Texture = (Texture)info.GetRefValue("Texture", typeof(Texture), typeof(Sprite).GetMember(nameof(Sprite.Texture))[0], this);
            TexCoords = (Vector2[])info.GetValue("TexCoords", typeof(Vector2[]));
            SizeInPixels = (Vector2)info.GetValue("SizeInPixels", typeof(Vector2));
        }
        #endregion
    }
}
