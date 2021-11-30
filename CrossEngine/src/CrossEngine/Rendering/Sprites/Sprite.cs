using static OpenGL.GL;
using System;

using System.Numerics;

using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;

namespace CrossEngine.Rendering.Sprites
{
    public class Sprite// : ISerializable
    {
        public Texture Texture;
        public Vector2[] TexCoords;

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
        }

        public Sprite(Texture texture, Vector2[] texCoords)
        {
            if (texCoords.Length != 4)
                throw new Exception("wrong number of texture coordinates");

            this.Texture = texture;
            this.TexCoords = texCoords;
        }

        #region ISerializable
        //public void GetObjectData(SerializationInfo info)
        //{
        //    info.AddValue("Texture", Texture);
        //    info.AddValue("TexCoords", TexCoords);
        //}

        //public Sprite(DeserializationInfo info)
        //{
        //    Texture = (Texture)info.GetRefValue("Texture", typeof(Texture), typeof(Sprite).GetMember(nameof(Sprite.Texture))[0], this);
        //    TexCoords = (Vector2[])info.GetValue("TexCoords", typeof(Vector2[]));
        //}
        #endregion
    }
}
