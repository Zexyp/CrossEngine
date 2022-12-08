using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Bullet;
using CrossEngine.Serialization;
using CrossEngine.ECS;
using CrossEngine.ComponentSystems;

namespace CrossEngine.Components
{
    using BulletVector3 = BulletSharp.Math.Vector3;

    //[RequireComponent(typeof(TransformComponent))]
    //[RequireComponent(typeof(ColliderComponent))]
    public class RigidBodyComponent : Component
    {
        #region RB Fields
        private bool _static = false;
        private float _mass = 1.0f;

        private Vector3 _velocity = Vector3.Zero;
        private Vector3 _angularVelocity = Vector3.Zero;

        private Vector3 _linearFactor = Vector3.One;
        private Vector3 _angularFactor = Vector3.One;
        #region Properties
        [EditorValue]
        public bool Static
        {
            get => _static;
            set
            {
                if (_static == value) return;
                _static = value;

                OnPropertyChanged?.Invoke(this, RigidBodyPropertyFlags.Static);
            }
        }

        [EditorDrag(float.Epsilon, float.MaxValue)]
        public float Mass
        {
            get => _mass;
            set
            {
                if (value <= 0.0f) throw new ArgumentOutOfRangeException();

                if (_mass == value) return;
                _mass = value;

                OnPropertyChanged?.Invoke(this, RigidBodyPropertyFlags.Mass);
            }
        }

        [EditorSection("Velocities")]
        [EditorDrag(float.MinValue, float.MaxValue)]
        public Vector3 Velocity
        {
            get => _velocity;
            set
            {
                if (_velocity == value) return;
                _velocity = value;

                OnPropertyChanged?.Invoke(this, RigidBodyPropertyFlags.LinearVelocity);
            }
        }

        [EditorDrag(float.MinValue, float.MaxValue)]
        public Vector3 AngularVelocity
        {
            get => _angularVelocity;
            set
            {
                if (_angularVelocity == value) return;
                _angularVelocity = value;

                OnPropertyChanged?.Invoke(this, RigidBodyPropertyFlags.AngularVelocity);
            }
        }

        [EditorDrag(float.MinValue, float.MaxValue)]
        public Vector3 LinearFactor
        {
            get => _linearFactor;
            set
            {
                if (_linearFactor == value) return;
                _linearFactor = value;

                OnPropertyChanged?.Invoke(this, RigidBodyPropertyFlags.LinearFactor);
            }
        }

        [EditorDrag(float.MinValue, float.MaxValue)]
        public Vector3 AngularFactor
        {
            get => _angularFactor;
            set
            {
                if (_angularFactor == value) return;
                _angularFactor = value;

                OnPropertyChanged?.Invoke(this, RigidBodyPropertyFlags.AngularFactor);
            }
        }
        #endregion
        #endregion

        internal event Action<RigidBodyComponent, RigidBodyPropertyFlags> OnPropertyChanged;

        internal protected override void Attach(World world)
        {
            world.GetSystem<PhysicsSystem>().RegisterRigidBody(this);
        }

        internal protected override void Detach(World world)
        {
            world.GetSystem<PhysicsSystem>().UnregisterRigidBody(this);
        }

        protected override Component CreateClone()
        {
            var rb = new RigidBodyComponent();

            rb.Mass = this.Mass;
            rb.Static = this.Static;
            rb.Velocity = this.Velocity;
            rb.AngularVelocity = this.AngularVelocity;
            rb.LinearFactor = this.LinearFactor;
            rb.AngularFactor = this.AngularFactor;
            
            return rb;
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            info.AddValue(nameof(Mass), Mass);
            info.AddValue(nameof(Static), Static);
            info.AddValue(nameof(Velocity), Velocity);
            info.AddValue(nameof(AngularVelocity), AngularVelocity);
            info.AddValue(nameof(LinearFactor), LinearFactor);
            info.AddValue(nameof(AngularFactor), AngularFactor);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            Mass = info.GetValue(nameof(Mass), 1.0f);
            Static = info.GetValue(nameof(Static), true);
            Velocity = info.GetValue(nameof(Velocity), Vector3.Zero);
            AngularVelocity = info.GetValue(nameof(AngularVelocity), Vector3.Zero);
            LinearFactor = info.GetValue(nameof(LinearFactor), Vector3.One);
            AngularFactor = info.GetValue(nameof(AngularFactor), Vector3.One);
        }
    }
}
