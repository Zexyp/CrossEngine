using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

//using CrossEngine.Events;
//using CrossEngine.Physics;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Bullet;
//using CrossEngine.Serialization.Json;
//using CrossEngine.Rendering;
//using CrossEngine.Rendering.Lines;
//using CrossEngine.Serialization;
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

        [EditorDrag(Min = float.Epsilon)]
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
        [EditorDrag]
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

        [EditorDrag]
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

        [EditorDrag]
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

        [EditorDrag]
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

        public override void Attach()
        {
            PhysicsSysten.Instance.RegisterRigidBody(this);
        }

        public override void Detach()
        {
            PhysicsSysten.Instance.UnregisterRigidBody(this);
        }

        public override object Clone()
        {
            var rb = new RigidBodyComponent();
            rb.Enabled = this.Enabled;

            rb.Mass = this.Mass;
            rb.Static = this.Static;
            rb.Velocity = this.Velocity;
            rb.AngularVelocity = this.AngularVelocity;
            rb.LinearFactor = this.LinearFactor;
            rb.AngularFactor = this.AngularFactor;
            
            return rb;
        }
    }
}
