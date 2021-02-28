using System;

using CrossEngine.Utils;
using CrossEngine.Rendering.Display;
using CrossEngine.MainLoop;

using System.Numerics;

namespace CrossEngine.Rendering.Cameras
{
    public class Camera
    {
        // vectors
        public Vector3 Front { get; private set; } = new Vector3(0.0f, 0.0f, -1.0f);
        public Vector3 Up { get; private set; } = new Vector3(0.0f, 1.0f, 0.0f);
        public Vector3 Right { get; private set; } = new Vector3(-1.0f, 0.0f, 0.0f);

        // matrices
        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        public Matrix4x4 ViewMatrix {
            get
            {
                if (dirtyView)
                {
                    UpdateView();
                    dirtyView = false;
                    return _viewMatrix;
                }
                else
                {
                    return _viewMatrix;
                }
            }
            private set { _viewMatrix = value; }
        }
        public Matrix4x4 ProjectionMatrix {
            get
            {
                if (dirtyProjection)
                {
                    UpdateProjection();
                    dirtyProjection = false;
                    return _projectionMatrix;
                }
                else
                {
                    return _projectionMatrix;
                }
            }
            private set { _projectionMatrix = value; }
        }

        // view matrix stuff
        private bool dirtyView = true;
        private Vector3 _worldUp = new Vector3(0.0f, 1.0f, 0.0f);
        private Transform _transform = new Transform();

        public Vector3 WorldUp { 
            get { return _worldUp; }
            set { _worldUp = value; dirtyView = true; } }
        public Transform Transform { 
            get { return _transform; } 
            set { _transform.OnValueChanged -= OnTransformChanged; _transform = value; _transform.OnValueChanged += OnTransformChanged; dirtyView = true; } } // resubscription

        // projection matrix stuff
        private bool dirtyProjection = true;
        private bool _orthographic = false;
        
        private float _fov = 0.0f;
        private float _nearPlane = 0.1f;
        private float _farPlane = 1000.0f;

        private float _width = 1.0f;
        private float _height = 1.0f;

        private float _zoom = 1.0f;

        public bool Orthographic;
        public float Fov { 
            get { return _fov; }
            set { _fov = value; dirtyProjection = true; } }
        public float NearPlane {
            get { return _nearPlane; }
            set { _nearPlane = value; dirtyProjection = true; } }
        public float FarPlane {
            get { return _farPlane; }
            set { _farPlane = value; dirtyProjection = true; } }
        public float Width {
            get { return _width; }
            set { _width = value; dirtyProjection = true; } }
        public float Height {
            get { return _height; }
            set { _height = value; dirtyProjection = true; } }
        public float Zoom {
            get { return _zoom; }
            set { _zoom = value; dirtyProjection = true; } }

        public Camera()
        {
            this._transform.OnValueChanged += OnTransformChanged;
            UpdateView();
            UpdateProjection();
            UpdateCameraVectors();
        }



        private void OnTransformChanged(object sender, EventArgs e)
        {
            UpdateView();
            UpdateCameraVectors();
        }

        private void UpdateView()
        {
            ViewMatrix = GetViewMatrix();
        }
        private void UpdateProjection()
        {
            ProjectionMatrix = GetProjectionMatrix();
        }

        private Matrix4x4 GetViewMatrix()
        {
            if (!_orthographic)
            {
                return Matrix4x4.CreateLookAt(_transform.Position, _transform.Position + Front, Up);
            }
            else
            {
                //Vector3 cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
                //Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f);
                return Matrix4x4.CreateLookAt(_transform.Position, _transform.Position + Front, Up);
            }
        }
        private Matrix4x4 GetProjectionMatrix()
        {
            if (!_orthographic)
            {
                if (_fov > 0 && _fov < 180)
                    return Matrix4x4.CreatePerspectiveFieldOfView(MathExtension.ToRadians(_fov), _width / _height, _nearPlane, _farPlane);
                else
                    return Matrix4x4.Identity;
            }
            else
                return Matrix4x4.CreateOrthographic(_width * _zoom, _height * _zoom, _nearPlane, _farPlane);
        }

        private void UpdateCameraVectors()
        {
            Front = Vector3.Transform(new Vector3(0.0f, 0.0f, -1.0f), _transform.Rotation);
            Up = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), _transform.Rotation);
            Right = Vector3.Transform(new Vector3(-1.0f, 0.0f, 0.0f), _transform.Rotation);

            //Vector3 ffront;
            //ffront.X = (float)(Math.Cos(MathExtension.ToRadians(yaw)) * Math.Cos(MathExtension.ToRadians(pitch)));
            //ffront.Y = (float)Math.Sin(MathExtension.ToRadians(pitch));
            //ffront.Z = (float)(Math.Sin(MathExtension.ToRadians(yaw)) * Math.Cos(MathExtension.ToRadians(pitch)));
            //front = Vector3.Normalize(ffront);
            //
            //right = Vector3.Normalize(Vector3.Cross(front, worldUp));
            //up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        // temporary
        public Camera(Vector3 position, float fov = 90.0f)
        {
            this._transform.Position = position;
            this._fov = fov;
            this._orthographic = false;

            this._transform.OnValueChanged += OnTransformChanged;
            UpdateView();
            UpdateProjection();
            UpdateCameraVectors();
        }

        public Camera(float width, float height)
        {
            this._orthographic = true;

            this._nearPlane = 0.0f;

            this._width = width;
            this._height = height;

            this._transform.OnValueChanged += OnTransformChanged;
            UpdateView();
            UpdateProjection();
            UpdateCameraVectors();
        }

        /*
        public void Move(Vector3 movement)
        {
            transform.Position += movement;
        }

        public void Rotate(Quaternion rotation)
        {
            rotation = Quaternion.Normalize(rotation);

            transform.Rotation *= rotation;
            
            UpdateCameraVectors();
        }

        //public void Rotation(Quaternion rotation)
        //{
        //    rotation = Quaternion.Normalize(rotation);
        //
        //    transform.Rotation = rotation;
        //
        //    
        //}
        */
    }
}
