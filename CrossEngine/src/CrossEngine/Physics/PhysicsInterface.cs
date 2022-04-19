using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Components;
using CrossEngine.Utils;
using CrossEngine.Utils.Bullet;
using CrossEngine.Logging;

namespace CrossEngine.Physics
{
    using BulletVector3 = BulletSharp.Math.Vector3;

    public struct RaycastHitInfo
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly RigidBodyComponent Rigidbody;
        public readonly ColliderComponent Collider;

        internal RaycastHitInfo(Vector3 point, Vector3 normal, RigidBodyComponent rigidbody, ColliderComponent collider = null)
        {
            this.Point = point;
            this.Normal = normal;
            this.Rigidbody = rigidbody;
            this.Collider = collider;
        }
    }

    public static class PhysicsInterface
    {
        public static Vector3 Gravity { get => _context.Gravity; set => _context.Gravity = value; }

        private static RigidBodyWorld _context;
        private static DynamicsWorld _dynamicsWorld;

        internal static void SetContext(RigidBodyWorld context)
        {
            _context = context;
            _dynamicsWorld = _context?.World;
        }

        public static bool Raycast(Vector3 source, Vector3 destination, out RaycastHitInfo info)
        {
            if (_dynamicsWorld == null)
            {
                Log.Core.Error("raycast attempted without world context!");
                throw new InvalidOperationException("Raycast attempted without world context.");
            }

            BulletVector3 Bdestination = destination.ToBullet();
            BulletVector3 Bsource = source.ToBullet();

            using (var cb = new ClosestRayResultCallback(ref Bsource, ref Bdestination))
            {
                _dynamicsWorld.RayTestRef(ref Bsource, ref Bdestination, cb);
                if (cb.HasHit)
                {
                    info = new RaycastHitInfo(cb.HitPointWorld.ToNumerics(),
                                              Vector3.Normalize(cb.HitNormalWorld.ToNumerics()),
                                              (RigidBodyComponent)cb.CollisionObject.UserObject,
                                              (ColliderComponent)cb.CollisionObject.CollisionShape.UserObject);
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
            if (_dynamicsWorld == null)
            {
                Log.Core.Error("raycast attempted without world context!");
                throw new InvalidOperationException("Raycast attempted without world context.");
            }

            BulletVector3 Bdestination = (source + (direction * maxDistance)).ToBullet();

            BulletVector3 Bsource = source.ToBullet();

            using (var cb = new ClosestRayResultCallback(ref Bsource, ref Bdestination))
            {
                _dynamicsWorld.RayTestRef(ref Bsource, ref Bdestination, cb);
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
