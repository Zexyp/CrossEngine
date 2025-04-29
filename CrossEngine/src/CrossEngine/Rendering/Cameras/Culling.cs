using System;

using System.Numerics;
using System.Runtime.InteropServices;
using System.Diagnostics;

using CrossEngine.Utils;

namespace CrossEngine.Rendering.Culling
{
    public interface IVolume
    {
        public Halfspace IsInFrustum(in Frustum frustum);
    }

    public enum Halfspace
    {
        Outside = 0,
        Intersect,
        Inside
    }

    public struct AABox : IVolume
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

        public Halfspace IsInFrustum(in Frustum frustum) => frustum.IsAABoxIn(this);
    }

    public struct Sphere : IVolume
    {
        public Vector3 center;
        public float radius;

        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
        
        public Halfspace IsInFrustum(in Frustum frustum) => frustum.IsSphereIn(center, radius);
    }

    // should be called Frustrum becase it's frustrating
    public unsafe struct Frustum
    {
        const int SIZEOF_PLANE = 4 * sizeof(float);//sizeof(Plane)
        private fixed float _planes[6 * (SIZEOF_PLANE / sizeof(float))];

        #region Props
        public Plane Left
        {
            get
            {
                fixed (void* p = &_planes[sizeof(Plane) * 0])
                {
                    Plane pp = *(Plane*)p;
                    return pp;
                }
            }
        }
        public Plane Right
        {
            get
            {
                fixed (void* p = &_planes[sizeof(Plane) * 1])
                {
                    Plane pp = *(Plane*)p;
                    return pp;
                }
            }
        }
        public Plane Bottom
        {
            get
            {
                fixed (void* p = &_planes[sizeof(Plane) * 2])
                {
                    Plane pp = *(Plane*)p;
                    return pp;
                }
            }
        }
        public Plane Top
        {
            get
            {
                fixed (void* p = &_planes[sizeof(Plane) * 3])
                {
                    Plane pp = *(Plane*)p;
                    return pp;
                }
            }
        }
        public Plane Near
        {
            get
            {
                fixed (void* p = &_planes[sizeof(Plane) * 4])
                {
                    Plane pp = *(Plane*)p;
                    return pp;
                }
            }
        }
        public Plane Far
        {
            get
            {
                fixed (void* p = &_planes[sizeof(Plane) * 5])
                {
                    Plane pp = *(Plane*)p;
                    return pp;
                }
            }
        }
        #endregion

        public static Frustum Create(in Matrix4x4 projectionMatrix, in Matrix4x4 viewMatrix)
        {
            Frustum frustum = new Frustum();
            frustum.Update(projectionMatrix, viewMatrix);
            return frustum;
        }

#if DEBUG // for debug perpouses
        Vector3 centrus;
#endif

        public unsafe void Update(in Matrix4x4 projectionMatrix, in Matrix4x4 viewMatrix)
        {
            var result = Matrix4x4.Invert(viewMatrix, out var inv);
            Debug.Assert(result);
            fixed (void* pl = &_planes[0])
                ExtractPlanes((Plane*)pl, projectionMatrix, inv, true);
#if DEBUG
            centrus = Vector3.Transform(Vector3.Zero, inv);
#endif
        }

        public unsafe Halfspace IsPointIn(Vector3 p)
        {
            Plane* pp;
            fixed (void* pl = &_planes[0])
                pp = (Plane*)pl;

            var result = Halfspace.Inside;
            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(p, pp[i]) < 0)
                    return Halfspace.Outside;
            }
            return result;
        }

        public unsafe Halfspace IsSphereIn(Vector3 p, float radius)
        {
            Plane* pp;
            fixed (void* pl = &_planes[0])
                pp = (Plane*)pl;

            var result = Halfspace.Inside;
            float distance;
            for (int i = 0; i < 6; i++)
            {
                    distance = DistanceToPlane(p, pp[i]);
                if (distance < -radius)
                    return Halfspace.Outside;
                else if (distance < radius)
                    result = Halfspace.Intersect;
            }
            return result;
        }

        public unsafe Halfspace IsAABoxIn(AABox b)
        {
            Plane* pp;
            fixed (void* pl = &_planes[0])
                pp = (Plane*)pl;

            var result = Halfspace.Inside;
            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(b.GetVertexP(pp[i].Normal), pp[i]) < 0)
                    return Halfspace.Outside;
                else if (DistanceToPlane(b.GetVertexN(pp[i].Normal), pp[i]) < 0)
                    result = Halfspace.Intersect;
            }
            return result;
        }

#if DEBUG
        public unsafe void Draw()
        {
            Plane* pp;
            fixed (void* pl = &_planes[0])
                pp = (Plane*)pl;

            LineRenderer.DrawLine(centrus, centrus + pp[0].Normal * -pp[0].D, new Vector4(0, 1, 1, 1));
            LineRenderer.DrawLine(centrus, centrus + pp[1].Normal * -pp[1].D, new Vector4(1, 0, 0, 1));
            LineRenderer.DrawLine(centrus, centrus + pp[2].Normal * -pp[2].D, new Vector4(1, 0, 1, 1));
            LineRenderer.DrawLine(centrus, centrus + pp[3].Normal * -pp[3].D, new Vector4(0, 1, 0, 1));
            LineRenderer.DrawLine(centrus, centrus + pp[4].Normal * -pp[4].D, new Vector4(1, 1, 0, 1));
            LineRenderer.DrawLine(centrus, centrus + pp[5].Normal * -pp[5].D, new Vector4(0, 0, 1, 1));
        }
#endif



        private static float DistanceToPlane(Vector3 pt, Plane plane)
        {
            return plane.Normal.X * pt.X + plane.Normal.Y * pt.Y + plane.Normal.Z * pt.Z + plane.D;
        }

        // idk the *exact* source but...
        // please god make this work cuz somehow the D3D version of this works on OGL
        private static unsafe void ExtractPlanes(Plane* planes, Matrix4x4 matrix, Matrix4x4 transform, bool normalize)
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
