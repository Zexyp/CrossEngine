using System;
using static OpenGL.GL;

using System.Numerics;
using System.Collections.Generic;

using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Buffers;

namespace CrossEngine.Rendering.Geometry
{
	public struct MeshVertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector2 texCoords;

		public MeshVertex(Vector3 position, Vector3 normal, Vector2 texCoords)
		{
			this.position = position;
			this.normal = normal;
			this.texCoords = texCoords;
		}
	}

	public class Mesh
	{
		public List<MeshVertex> vertices = new List<MeshVertex> { };
		public List<uint> indices = new List<uint> { };

		public List<Texture> textures = new List<Texture> { }; // will be later separeted and moved to material system some day

		public VertexArray va;
		public VertexBuffer vb;
		public IndexBuffer ib;

		public Mesh(List<MeshVertex> vertices, List<uint> indices)
		{
			this.vertices = vertices;
			this.indices = indices;

			SetupMesh();
		}

		// cleanup
		/*
		unsafe ~Mesh()
		{
			// should be implemented in texture class
			//foreach(Texture tex in textures)
			//    fixed(uint* idp = &tex.id)
			//  	    glDeleteTextures(1, idp);
			

            fixed (uint* ebop = &ebo, vbop = &vbo, vaop = &vao)
			{
				glDeleteVertexArrays(1, vaop);
				glDeleteBuffers(1, ebop);
				glDeleteBuffers(1, vbop);
			}
		}
		*/

		public unsafe void Dispose()
        {
			va.Dispose();
			vb.Dispose();
			ib.Dispose();
		}

		/*
		public unsafe void Draw(Shader shader)
		{
			uint diffuseNr = 1;
			uint specularNr = 1;
			uint normalNr = 1;
			uint heightNr = 1;

			for (int i = 0; i < textures.Count; i++)
			{
				glActiveTexture(GL_TEXTURE0 + i);
				string number = "";
				Texture.TextureType type = textures[i].textureType;
				if (type == Texture.TextureType.Diffuse)
					number = Convert.ToString(diffuseNr++);
				else if (type == Texture.TextureType.Specular)
					number = Convert.ToString(specularNr++);
				else if (type == Texture.TextureType.Normal)
					number = Convert.ToString(normalNr++);
				else if (type == Texture.TextureType.Height)
					number = Convert.ToString(heightNr++);

				if (Texture.TypeEnumToString(type) + number != "")
					glUniform1i(glGetUniformLocation(shader.id, ("texture_" + Texture.TypeEnumToString(type) + number)), i);

				glBindTexture(GL_TEXTURE_2D, textures[i].id);
			}

			//draw
			//glBindVertexArray(vao);
			va.Bind();
			ib.Bind();
			glDrawElements(GL_TRIANGLES, indices.Count, GL_UNSIGNED_INT, (void*)0);
			//va.Unbind();

			// just to keep it default
			//glBindVertexArray(0);
			//glActiveTexture(GL_TEXTURE0);
		}
		*/

		private unsafe void SetupMesh()
		{
			va = new VertexArray();
			va.Bind(); // needs to be bound because it was ONLY created

			fixed (MeshVertex* verteciesp = &vertices.ToArray()[0])
				vb = new VertexBuffer(verteciesp, sizeof(MeshVertex) * vertices.Count);
			fixed (uint* indicesp = &indices.ToArray()[0])
				ib = new IndexBuffer(indicesp, indices.Count);

			// this is really nice
			VertexBufferLayout layout = new VertexBufferLayout();
			layout.Add(typeof(MeshVertex));

			va.AddBuffer(vb, layout);

			VertexArray.Unbind();

			//fixed (uint* vaop = &vao, vbop = &vbo, ebop = &ebo)
			//{
			//	glGenVertexArrays(1, vaop);
			//	glGenBuffers(1, vbop);
			//	glGenBuffers(1, ebop);
			//}
			//
			//glBindVertexArray(vao);
			//
			//fixed (Vertex* verteciesp = &vertices.ToArray()[0])
			//{
			//	glBindBuffer(GL_ARRAY_BUFFER, vbo);
			//	glBufferData(GL_ARRAY_BUFFER, sizeof(Vertex) * vertices.Count, verteciesp, GL_STATIC_DRAW);
			//}
			//
			//fixed (uint* indicesp = &indices.ToArray()[0])
			//{
			//	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
			//	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indices.Count, indicesp, GL_STATIC_DRAW);
			//}
			//
			//// attribute positions
			//glEnableVertexAttribArray(0);
			//glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(Vertex), (void*)0);
			//glEnableVertexAttribArray(1);
			//glVertexAttribPointer(1, 3, GL_FLOAT, false, sizeof(Vertex), (void*)(sizeof(float) * 3));
			//glEnableVertexAttribArray(2);
			//glVertexAttribPointer(2, 2, GL_FLOAT, false, sizeof(Vertex), (void*)(sizeof(float) * 3 + sizeof(float) * 3));
			//
			//// just to keep it default
			//glBindVertexArray(0);
		}

		/*
		public unsafe void SetGeomtryData(List<MeshVertex> newVertices = null, List<uint> newIndices = null)
        {
			if(newVertices != null && newIndices != null) // checks for problems
            {
				if (newVertices.Count > 0 && newIndices.Count > 0)
				{
					vertices = newVertices;

					fixed (MeshVertex* verteciesp = &newVertices.ToArray()[0])
						vb.SetData(verteciesp, sizeof(MeshVertex) * newVertices.Count);

					indices = newIndices;

					fixed (uint* indicesp = &newIndices.ToArray()[0])
						ib.SetData(indicesp, newIndices.Count);
				}
				else
				{
					vertices.Clear();
					indices.Clear();

					vb.SetData((void*)0, 0);
					ib.SetData((uint*)(void*)0, 0);
				}
			}
		}
		*/
	}
}
