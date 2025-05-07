using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CrossEngine.Geometry;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Utils.Structs;

namespace CrossEngine.Utils;

public static class MeshGenerator
{
    public struct GeneratorVertex
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Normal;
    }
    
    public static IndexedMesh<GeneratorVertex> GenerateCube(Vector3 size, IntVec3? numberOfVertices = null)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<uint> indices = new List<uint>();

        if (numberOfVertices != null)
            throw new NotImplementedException();

        Vector3 half = size / 2.0f;

        // Each face: 4 unique vertices → 2 triangles (quad)
        Vector3[] faceNormals = new Vector3[]
        {
            Vector3.UnitZ,   // Front
            -Vector3.UnitZ,  // Back
            Vector3.UnitY,   // Top
            -Vector3.UnitY,  // Bottom
            Vector3.UnitX,   // Right
            -Vector3.UnitX   // Left
        };

        // Face directions and vertex positions (quad vertices for each face)
        (Vector3 normal, Vector3[] corners)[] faces = new[]
        {
            // Front face (Z+)
            (Vector3.UnitZ, new[]
            {
                new Vector3(-half.X, -half.Y,  half.Z),
                new Vector3( half.X, -half.Y,  half.Z),
                new Vector3( half.X,  half.Y,  half.Z),
                new Vector3(-half.X,  half.Y,  half.Z),
            }),
            // Back face (Z-)
            (-Vector3.UnitZ, new[]
            {
                new Vector3( half.X, -half.Y, -half.Z),
                new Vector3(-half.X, -half.Y, -half.Z),
                new Vector3(-half.X,  half.Y, -half.Z),
                new Vector3( half.X,  half.Y, -half.Z),
            }),
            // Top face (Y+)
            (Vector3.UnitY, new[]
            {
                new Vector3(-half.X,  half.Y,  half.Z),
                new Vector3( half.X,  half.Y,  half.Z),
                new Vector3( half.X,  half.Y, -half.Z),
                new Vector3(-half.X,  half.Y, -half.Z),
            }),
            // Bottom face (Y-)
            (-Vector3.UnitY, new[]
            {
                new Vector3(-half.X, -half.Y, -half.Z),
                new Vector3( half.X, -half.Y, -half.Z),
                new Vector3( half.X, -half.Y,  half.Z),
                new Vector3(-half.X, -half.Y,  half.Z),
            }),
            // Right face (X+)
            (Vector3.UnitX, new[]
            {
                new Vector3( half.X, -half.Y,  half.Z),
                new Vector3( half.X, -half.Y, -half.Z),
                new Vector3( half.X,  half.Y, -half.Z),
                new Vector3( half.X,  half.Y,  half.Z),
            }),
            // Left face (X-)
            (-Vector3.UnitX, new[]
            {
                new Vector3(-half.X, -half.Y, -half.Z),
                new Vector3(-half.X, -half.Y,  half.Z),
                new Vector3(-half.X,  half.Y,  half.Z),
                new Vector3(-half.X,  half.Y, -half.Z),
            }),
        };

        Vector2[] faceUVs = new[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        uint vertexOffset = 0;

        foreach (var (normal, corners) in faces)
        {
            // Add vertices
            for (int i = 0; i < 4; i++)
            {
                positions.Add(corners[i]);
                normals.Add(normal);
                uvs.Add(faceUVs[i]);
            }

            // Two triangles per face
            indices.Add(vertexOffset + 0);
            indices.Add(vertexOffset + 1);
            indices.Add(vertexOffset + 2);

            indices.Add(vertexOffset + 0);
            indices.Add(vertexOffset + 2);
            indices.Add(vertexOffset + 3);

            vertexOffset += 4;
        }

        // Zip into GeneratorVertex
        var vertices = positions.Zip(normals, (pos, norm) => (pos, norm))
                                .Zip(uvs, (pn, uv) => new GeneratorVertex
                                {
                                    Position = pn.pos,
                                    Normal = pn.norm,
                                    TexCoord = uv
                                }).ToArray();

        return new IndexedMesh<GeneratorVertex>(vertices, indices.ToArray());
    }
    
    public static IndexedMesh<GeneratorVertex> GenerateGrid(Vector2 size, IntVec2? withResolution = null)
    {
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<uint>();

        var resolution = withResolution ?? new IntVec2(2, 2);
        int cols = resolution.X;
        int rows = resolution.Y;

        Vector2 step = new Vector2(size.X / (cols - 1), size.Y / (rows - 1));
        Vector2 origin = -size / 2f;

        // Generate vertices
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float px = origin.X + x * step.X;
                float py = origin.Y + y * step.Y;

                positions.Add(new Vector3(px, py, 0));
                normals.Add(Vector3.UnitZ); // Facing forward (out of XY plane)
                uvs.Add(new Vector2((float)x / (cols - 1), (float)y / (rows - 1)));
            }
        }

        // Generate indices (2 triangles per quad)
        for (int y = 0; y < rows - 1; y++)
        {
            for (int x = 0; x < cols - 1; x++)
            {
                uint i0 = (uint)(y * cols + x);
                uint i1 = i0 + 1;
                uint i2 = i0 + (uint)cols;
                uint i3 = i2 + 1;

                // Triangle 1
                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i2);

                // Triangle 2
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i2);
            }
        }

        // Zip into GeneratorVertex
        var vertices = positions.Zip(normals, (pos, norm) => (pos, norm))
            .Zip(uvs, (pn, uv) => new GeneratorVertex
            {
                Position = pn.pos,
                Normal = pn.norm,
                TexCoord = uv
            }).ToArray();

        return new IndexedMesh<GeneratorVertex>(vertices, indices.ToArray());
    }

    // Sphere
    // Cyliner
    // Cone
    // Torus
}