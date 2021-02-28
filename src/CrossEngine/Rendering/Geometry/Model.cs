using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Numerics;
using System.IO;

using CrossEngine.Rendering.Texturing;

namespace CrossEngine.Rendering.Geometry
{
	public class Model
	{
		public List<Mesh> meshes = new List<Mesh> { };
		string directory = "";
		public List<Texture> loadedTextures = new List<Texture> { };

		public Model(string path)
		{
			LoadModel(path);
		}

		public void Dispose()
        {
			foreach(Mesh mesh in meshes)
            {
				mesh.Dispose();
            }
        }

		/*
		public void Draw(Shader shader)
		{
			for (int i = 0; i < meshes.Count; i++)
				meshes[i].Draw(shader);
		}
		*/

		private void LoadModel(string path)
		{
			if(!File.Exists(path))
			{
				Log.Error("model not found!");
				return;
			}
			Assimp.AssimpContext importer = new Assimp.AssimpContext();
			Assimp.Scene scene = importer.ImportFile(path, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.GenerateNormals);
			if (!(scene != null) || scene.SceneFlags == Assimp.SceneFlags.Incomplete || !(scene.RootNode != null))
			{
				Log.Error("model loading failed!");
				return;
			}
			else
				Log.Info("loading model");
			directory = path.Substring(0, path.LastIndexOf('/'));

			// go through nodes
			ProcessNode(scene.RootNode, scene);
		}

		private void ProcessNode(Assimp.Node node, Assimp.Scene scene)
		{
			for (int i = 0; i < node.MeshCount; i++)
			{
				Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
				meshes.Add(ProcessMesh(mesh, scene));
			}
			for (int i = 0; i < node.ChildCount; i++)
			{
				ProcessNode(node.Children[i], scene);
			}
		}

		private Mesh ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene)
		{
			List<MeshVertex> vertecies = new List<MeshVertex> { };
			List<uint> indices = new List<uint> { }; ;
			List<Texture> textures = new List<Texture> { };

			for (int i = 0; i < mesh.VertexCount; i++)
			{
				MeshVertex vertex;
				Vector3 vector;

				vector.X = mesh.Vertices[i].X;
				vector.Y = mesh.Vertices[i].Y;
				vector.Z = mesh.Vertices[i].Z;
				vertex.position = vector;

				vector.X = mesh.Normals[i].X;
				vector.Y = mesh.Normals[i].Y;
				vector.Z = mesh.Normals[i].Z;
				vertex.normal = vector;

				if (mesh.TextureCoordinateChannels[0] != null && mesh.TextureCoordinateChannels[0].Count > 0) // also checks if there is something to read from
				{
					Vector2 vec;
					vec.X = mesh.TextureCoordinateChannels[0][i].X;
					vec.Y = mesh.TextureCoordinateChannels[0][i].Y;
					vertex.texCoords = vec;
				}
				else
					vertex.texCoords = new Vector2(0.0f, 0.0f);

				vertecies.Add(vertex);
			}
			for (int i = 0; i < mesh.FaceCount; i++)
			{
				Assimp.Face face = mesh.Faces[i];
				for (int j = 0; j < face.IndexCount; j++)
					indices.Add(Convert.ToUInt32(face.Indices[j]));
			}

			//Assimp.Material material = scene.Materials[mesh.MaterialIndex];
			//// diffuse
			//List<Texture> diffuseMaps = LoadMaterialTextures(material, Assimp.TextureType.Diffuse, Texture.MaterialTextureType.Diffuse, this.directory);
			//textures.AddRange(diffuseMaps);
			//// specular
			//List<Texture> specularMaps = LoadMaterialTextures(material, Assimp.TextureType.Specular, Texture.MaterialTextureType.Specular, this.directory);
			//textures.AddRange(specularMaps);
			//// normal
			//List<Texture> normalMaps = LoadMaterialTextures(material, Assimp.TextureType.Normals, Texture.MaterialTextureType.Normal, this.directory);
			//textures.AddRange(normalMaps);
			//// height
			//List<Texture> heightMaps = LoadMaterialTextures(material, Assimp.TextureType.Height, Texture.MaterialTextureType.Diffuse, this.directory);
			//textures.AddRange(heightMaps);

			return new Mesh(vertecies, indices/*, textures*/);
		}
		/*
		public List<Texture> LoadMaterialTextures(Assimp.Material mat, Assimp.TextureType type, Texture.MaterialTextureType typeName, string directory)
		{
			List<Texture> textures = new List<Texture> { };
			for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
			{
				Assimp.TextureSlot texSlot;
				mat.GetMaterialTexture(type, i, out texSlot);

				bool skip = false; // for checking if the texture is already loaded
				for (int j = 0; j < loadedTextures.Count; j++)
				{
					if (loadedTextures[j].path == texSlot.FilePath)
					{
						textures.Add(loadedTextures[j]);
						skip = true;
						break;
					}
				}
				if (!skip)
				{
					Texture texture = TextureManager.TextureLoader.TextureFromFile(directory + "/" + texSlot.FilePath);
					texture.textureType = typeName;
					texture.path = texSlot.FilePath;
					textures.Add(texture);
					loadedTextures.Add(texture);
				}
			}
			return textures;
		}
		*/
	}
}
