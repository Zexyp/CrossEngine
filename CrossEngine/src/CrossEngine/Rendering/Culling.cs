using System;
using System.Numerics;
using System.Runtime.InteropServices;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Culling
{
    public enum Halfspace
    {
        Outside = 0,
        Intersect,
        Inside
    }

    public struct AABox
    {
        public Vector3 corner;
        public float x, y, z;

        public static AABox CreateFromExtents(Vector3 min, Vector3 max)
        {
            AABox b;
            b.corner = min;
            b.x = max.X - min.X;
            b.y = max.Y - min.Y;
            b.z = max.Z - min.Z;
            return b;
        }

        public Vector3 GetVertexP(Vector3 normal)
        {
            Vector3 res = corner;
            if (normal.X > 0)
                res.X += x;
            if (normal.Y > 0)
                res.Y += y;
            if (normal.Z > 0)
                res.Z += z;
            return res;
        }

        public Vector3 GetVertexN(Vector3 normal)
        {
            Vector3 res = corner;
            if (normal.X < 0)
                res.X += x;
            if (normal.Y < 0)
                res.Y += y;
            if (normal.Z < 0)
                res.Z += z;
            return res;
        }
    }

    public struct Sphere
    {
        public Vector3 center;
        public float radius;

        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }

    // should be called Frustrum becase it's frustrating
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Frustum
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly Plane[] Planes;
        public Plane Left => Planes[0];
        public Plane Right => Planes[1];
        public Plane Bottom => Planes[2];
        public Plane Top => Planes[3];
        public Plane Near => Planes[4];
        public Plane Far => Planes[5];

        // for debug perpouses
        Vector3 centrus;

        public Frustum(Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix)
        {
            Planes = new Plane[6];
            var inv = Matrix4x4Extension.Invert(viewMatrix);
            ExtractPlanes(Planes, projectionMatrix, inv, true);
            centrus = Vector3.Transform(Vector3.One, inv);
        }

        public Halfspace IsPointIn(Vector3 p)
        {
            var result = Halfspace.Inside;
            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(p, Planes[i]) < 0)
                    return Halfspace.Outside;
            }
            return result;
        }

        public Halfspace IsSphereIn(Vector3 p, float radius)
        {
            var result = Halfspace.Inside;
            float distance;
            for (int i = 0; i < 6; i++)
            {
                distance = DistanceToPlane(p, Planes[i]);
                if (distance < -radius)
                    return Halfspace.Outside;
                else if (distance < radius)
                    result = Halfspace.Intersect;
            }
            return result;
        }

        public Halfspace IsAABoxIn(AABox b)
        {
            var result = Halfspace.Inside;
            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(b.GetVertexP(Planes[i].Normal), Planes[i]) < 0)
                    return Halfspace.Outside;
                else if (DistanceToPlane(b.GetVertexN(Planes[i].Normal), Planes[i]) < 0)
                    result = Halfspace.Intersect;
            }
            return result;
        }

        public void Draw()
        {
            LineRenderer.DrawLine(centrus, centrus + Planes[0].Normal * -Planes[0].D, new Vector4(0, 1, 1, 1));
            LineRenderer.DrawLine(centrus, centrus + Planes[1].Normal * -Planes[1].D, new Vector4(1, 0, 0, 1));
            LineRenderer.DrawLine(centrus, centrus + Planes[2].Normal * -Planes[2].D, new Vector4(1, 0, 1, 1));
            LineRenderer.DrawLine(centrus, centrus + Planes[3].Normal * -Planes[3].D, new Vector4(0, 1, 0, 1));
            LineRenderer.DrawLine(centrus, centrus + Planes[4].Normal * -Planes[4].D, new Vector4(1, 1, 0, 1));
            LineRenderer.DrawLine(centrus, centrus + Planes[5].Normal * -Planes[5].D, new Vector4(0, 0, 1, 1));
        }



        private static float DistanceToPlane(Vector3 pt, Plane plane)
        {
            return plane.Normal.X * pt.X + plane.Normal.Y * pt.Y + plane.Normal.Z * pt.Z + plane.D;
        }

        // idk the *exact* source but...
        // please god make this work cuz somehow the D3D version of this works on OGL
        private static void ExtractPlanes(Plane[] planes, Matrix4x4 matrix, Matrix4x4 transform, bool normalize)
        {
            // Left clipping plane
            planes[0].Normal.X = matrix.M14 + matrix.M11;
            planes[0].Normal.Y = matrix.M24 + matrix.M21;
            planes[0].Normal.Z = matrix.M34 + matrix.M31;
            planes[0].D = matrix.M44 + matrix.M41;
            // Right clipping plane
            planes[1].Normal.X = matrix.M14 - matrix.M11;
            planes[1].Normal.Y = matrix.M24 - matrix.M21;
            planes[1].Normal.Z = matrix.M34 - matrix.M31;
            planes[1].D = matrix.M44 - matrix.M41;
            // Bottom clipping plane
            planes[2].Normal.X = matrix.M14 + matrix.M12;
            planes[2].Normal.Y = matrix.M24 + matrix.M22;
            planes[2].Normal.Z = matrix.M34 + matrix.M32;
            planes[2].D = matrix.M44 + matrix.M42;
            // Top clipping plane
            planes[3].Normal.X = matrix.M14 - matrix.M12;
            planes[3].Normal.Y = matrix.M24 - matrix.M22;
            planes[3].Normal.Z = matrix.M34 - matrix.M32;
            planes[3].D = matrix.M44 - matrix.M42;
            // Near clipping plane
            planes[4].Normal.X = matrix.M41 + matrix.M31;
            planes[4].Normal.Y = matrix.M42 + matrix.M32;
            planes[4].Normal.Z = matrix.M43 + matrix.M33;
            planes[4].D = matrix.M44 + matrix.M34;
            // Far clipping plane
            planes[5].Normal.X = matrix.M14 - matrix.M13;
            planes[5].Normal.Y = matrix.M24 - matrix.M23;
            planes[5].Normal.Z = matrix.M34 - matrix.M33;
            planes[5].D = matrix.M44 - matrix.M43;

            planes[0] = Plane.Transform(planes[0], transform);
            planes[1] = Plane.Transform(planes[1], transform);
            planes[2] = Plane.Transform(planes[2], transform);
            planes[3] = Plane.Transform(planes[3], transform);
            planes[4] = Plane.Transform(planes[4], transform);
            planes[5] = Plane.Transform(planes[5], transform);

            // Normalize the plane equations, if requested
            if (normalize == true)
            {
                planes[0] = Plane.Normalize(planes[0]);
                planes[1] = Plane.Normalize(planes[1]);
                planes[2] = Plane.Normalize(planes[2]);
                planes[3] = Plane.Normalize(planes[3]);
                planes[4] = Plane.Normalize(planes[4]);
                planes[5] = Plane.Normalize(planes[5]);
            }
        }

        public static Vector3 ThreePlaneIntersection(Plane p1, Plane p2, Plane p3)
        {
            Vector3 m1 = p1.Normal;
            Vector3 m2 = p2.Normal;
            Vector3 m3 = p3.Normal;
            Vector3 d = new Vector3(p1.D, p2.D, p3.D);

            Vector3 u = Vector3.Cross(m2, m3);
            Vector3 v = Vector3.Cross(m1, d);

            float denom = Vector3.Dot(m1, u);

            if (Math.Abs(denom) < float.Epsilon)
            {
                return default;
            }

            return new Vector3(Vector3.Dot(d, u) / denom, Vector3.Dot(m3, v) / denom, -Vector3.Dot(m2, v) / denom);
        }
    }
}
