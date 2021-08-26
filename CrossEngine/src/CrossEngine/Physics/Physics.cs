using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Entities.Components;

namespace CrossEngine.Physics
{
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
        public static DynamicsWorld context;

        public static bool Raycast(Vector3 source, Vector3 destination, out RaycastHitInfo info)
        {
            using (var cb = new ClosestRayResultCallback(ref source, ref destination))
            {
                context.RayTestRef(ref source, ref destination, cb);
                if (cb.HasHit)
                {
                    info = new RaycastHitInfo(cb.HitPointWorld, Vector3.Normalize(cb.HitNormalWorld), (RigidBodyComponent)cb.CollisionObject.UserObject);
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
            Vector3 destination = source + (direction * maxDistance);

            using (var cb = new ClosestRayResultCallback(ref source, ref destination))
            {
                context.RayTestRef(ref source, ref destination, cb);
                if (cb.HasHit)
                {
                    info = new RaycastHitInfo(cb.HitPointWorld, Vector3.Normalize(cb.HitNormalWorld), (RigidBodyComponent)cb.CollisionObject.UserObject);
                    return true;
                }
                else
                {
                    info = new RaycastHitInfo(destination, new Vector3(1.0f, 0.0f, 0.0f), null);
                    return false;
                }
            }
        }
    }
}
