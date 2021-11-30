using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Entities.Components;
using CrossEngine.Utils;

namespace CrossEngine.Physics
{
    using BulletVector3 = BulletSharp.Math.Vector3;

    public struct RaycastHitInfo
    {
        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly RigidBodyComponent rigidbody;
        public readonly ColliderComponent collider;

        internal RaycastHitInfo(Vector3 point, Vector3 normal, RigidBodyComponent rigidbody, ColliderComponent collider = null)
        {
            this.point = point;
            this.normal = normal;
            this.rigidbody = rigidbody;
            this.collider = collider;
        }
    }

    public class Physics
    {
        private static DynamicsWorld _contextWorld;

        internal static void SetContext(object context)
        {
            _contextWorld = (DynamicsWorld)context;
        }

        public static bool Raycast(Vector3 source, Vector3 destination, out RaycastHitInfo info)
        {
            BulletVector3 Bdestination = destination.ToBullet();
            BulletVector3 Bsource = source.ToBullet();

            using (var cb = new ClosestRayResultCallback(ref Bsource, ref Bdestination))
            {
                _contextWorld.RayTestRef(ref Bsource, ref Bdestination, cb);
                if (cb.HasHit)
                {
                    info = new RaycastHitInfo(cb.HitPointWorld.ToNumerics(), Vector3.Normalize(cb.HitNormalWorld.ToNumerics()), (RigidBodyComponent)cb.CollisionObject.UserObject);
                    return true;
                }
                else
                {
                    info = new RaycastHitInfo(destination, new Vector3(1.0f, 0.0f, 0.0f), null);
                    return false;
                }
            }
        }

        public static bool Raycast(Vector3 source, Vector3 direction, float maxDistance, out RaycastHitInfo info)
        {
            BulletVector3 Bdestination = (source + (direction * maxDistance)).ToBullet();

            BulletVector3 Bsource = source.ToBullet();

            using (var cb = new ClosestRayResultCallback(ref Bsource, ref Bdestination))
            {
                _contextWorld.RayTestRef(ref Bsource, ref Bdestination, cb);
                if (cb.HasHit)
                {
                    info = new RaycastHitInfo(cb.HitPointWorld.ToNumerics(), Vector3.Normalize(cb.HitNormalWorld.ToNumerics()), (RigidBodyComponent)cb.CollisionObject.UserObject);
                    return true;
                }
                else
                {
                    info = new RaycastHitInfo(Bdestination.ToNumerics(), new Vector3(1.0f, 0.0f, 0.0f), null);
                    return false;
                }
            }
        }
    }
}
