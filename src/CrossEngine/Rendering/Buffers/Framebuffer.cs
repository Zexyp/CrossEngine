using System;
using static OpenGL.GL;

using CrossEngine.Rendering.Texturing;

namespace CrossEngine.Rendering.Buffers
{
    public class Framebuffer
    {
        uint fbo = 0;
        uint rbo = 0;

        int width = 0;
        int height = 0;

        public Texture texture;

        public unsafe Framebuffer(int width, int height)
        {
            texture = new Texture(width, height, Texture.ColorChannel.TripleRGB);
            texture.Bind();

            fixed(uint* fbop = &fbo)
                glGenFramebuffers(1, fbop);

            glBindFramebuffer(GL_FRAMEBUFFER, fbo);
            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texture.id, 0);

            fixed (uint* rbop = &rbo)
                glGenRenderbuffers(1, rbop);
            glBindRenderbuffer(rbo);
            glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, width, height); //GL_DEPTH_COMPONENT32, GL_DEPTH_COMPONENT24, GL_DEPTH_COMPONENT16
            glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, rbo);

            if(glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                Log.Error("framebuffer is not complete!");
            }

            // just to display the frame properly
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            this.width = width;
            this.height = height;
        }

        public void Bind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, fbo);
        }

        public static void Unbind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, 0);
        }
    }
}
