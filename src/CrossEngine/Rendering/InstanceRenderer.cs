using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Numerics;

using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Errors;
using CrossEngine.Rendering;

using CrossEngine.ComponentSystem.Components;

namespace CrossEngine.Rendering
{
    public class InstanceRenderer
    {
        //int maxNrObjectCapacity;
        //int maxShapeCapacityInBytes;
        //int totalNumInstances = 0;
        //instanceColors

        //static bool textureEnabled = true;
        //static Texture defaultTexture;

        static Matrix4x4[] matrixBuffer;

        static int maxInstancesCount = 1000;
        static int workingPos = 0;

        //static VertexArray instanceVA;
        static VertexBuffer instanceVB;

        static Dictionary<Mesh, List<InstanceRendererComponent>> registered = new Dictionary<Mesh, List<InstanceRendererComponent>> { };

        static public RendererStats rendererStats = new RendererStats();

        static public unsafe void Init()
        {
            matrixBuffer = new Matrix4x4[maxInstancesCount];

            //instanceVA = new VertexArray();
            //instanceVA.Bind();

            instanceVB = new VertexBuffer((void*)null, (int)maxInstancesCount * sizeof(Matrix4x4), true); // may also be referred to as a buffer of matrices with model transforms

            //VertexBufferLayout layout = new VertexBufferLayout();
            //layout.Add(typeof(Vector4));
            //layout.Add(typeof(Vector4));
            //layout.Add(typeof(Vector4));
            //layout.Add(typeof(Vector4));
            //layout.elements[0].divisor = 1;
            //layout.elements[1].divisor = 1;
            //layout.elements[2].divisor = 1;
            //layout.elements[3].divisor = 1;
            //
            //instanceVA.AddBuffer(instanceVB, layout, 3);

            //TransparentInstance
        }

        static public unsafe void Push(Material material)
        {
            //glActiveTexture(GL_TEXTURE0);
            //if (!textureEnabled)
            //    defaultTexture.Bind();
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

            rendererStats.Reset();

            foreach (KeyValuePair<Mesh, List<InstanceRendererComponent>> pair in registered)
            {
                foreach (InstanceRendererComponent comp in pair.Value)
                {
                    if (workingPos >= maxInstancesCount)
                    {
                        Flush(material, pair);
                    }
                    matrixBuffer[workingPos++] = comp.entity.transform.TransformMatrix;

                    rendererStats.itemCount++;
                }
                Flush(material, pair);
            }

            //glPointSize(50);
            //glPolygonMode(GL_FRONT_AND_BACK, GL_POINT);
        }

        private static Matrix4x4 ToReasonableFormat(Matrix4x4 mat)
        {
            Matrix4x4 newmat = new Matrix4x4();
            newmat.M11 = mat.M41;
            newmat.M44 = mat.M42;
            newmat.M43 = mat.M43;
            newmat.M42 = mat.M44;
            newmat.M41 = mat.M31;
            newmat.M34 = mat.M32;
            newmat.M33 = mat.M33;
            newmat.M31 = mat.M34;
            newmat.M32 = mat.M21;
            newmat.M23 = mat.M22;
            newmat.M22 = mat.M23;
            newmat.M21 = mat.M24;
            newmat.M14 = mat.M11;
            newmat.M13 = mat.M12;
            newmat.M12 = mat.M13;
            newmat.M24 = mat.M14;
            return newmat;
        }          

        static unsafe void Flush(Material material, KeyValuePair<Mesh, List<InstanceRendererComponent>> pair)
        {
            if (workingPos == 0)
                return;

            //instanceVA.Bind();
            fixed (void* p = &matrixBuffer[0])
                instanceVB.SetData(p, sizeof(Matrix4x4) * maxInstancesCount);

            material.shader.Use();
            material.Bind();

            material.shader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
            material.shader.SetMat4("view", ActiveCamera.camera.ViewMatrix);

            pair.Key.va.Bind();

            glDrawElementsInstanced(GL_TRIANGLES, pair.Key.ib.GetCount(), GL_UNSIGNED_INT, (void*)0, workingPos);

            workingPos = 0;

            rendererStats.drawCount++;
        }

        public static unsafe void Register(Mesh mesh, InstanceRendererComponent component)
        {
            if (registered.ContainsKey(mesh))
                registered[mesh].Add(component);
            else
            {
                // set attribute pointers for matrix (4 times vec4)
                VertexBufferLayout layout = new VertexBufferLayout();
                layout.Add(typeof(Vector4));
                layout.Add(typeof(Vector4));
                layout.Add(typeof(Vector4));
                layout.Add(typeof(Vector4));
                layout.elements[0].divisor = 1;
                layout.elements[1].divisor = 1;
                layout.elements[2].divisor = 1;
                layout.elements[3].divisor = 1;

                mesh.va.AddBuffer(instanceVB, layout, 3); // extension
                //glEnableVertexAttribArray(3);
                //glVertexAttribPointer(3, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)0);
                //glEnableVertexAttribArray(4);
                //glVertexAttribPointer(4, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)(sizeof(Vector4)));
                //glEnableVertexAttribArray(5);
                //glVertexAttribPointer(5, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)(2 * sizeof(Vector4)));
                //glEnableVertexAttribArray(6);
                //glVertexAttribPointer(6, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)(3 * sizeof(Vector4)));
                //
                //glVertexAttribDivisor(3, 1);
                //glVertexAttribDivisor(4, 1);
                //glVertexAttribDivisor(5, 1);
                //glVertexAttribDivisor(6, 1);

                registered.Add(mesh, new List<InstanceRendererComponent> { component });
            }
        }
    }
}
