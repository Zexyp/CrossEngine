using System.Numerics;

namespace CrossEngine.Utils;

public interface ITransform
{
    Matrix4x4 GetMatrix();
}

public class Transform : ITransform
{
    public Vector3 Position
    {
        get => _translation;
        set
        {
            _dirty = true;
            _translation = value;
        }
    }
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _dirty = true;
            _rotation = value;
        }
    }
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _dirty = true;
            _scale = value;
        }
    }

    public Vector3 Euler
    {
        get => QuaternionExtension.ToEuler(_rotation);
        set
        {
            _dirty = true;
            _rotation = QuaternionExtension.RotateXYZ(value);
        }
    }

    private bool _dirty = true;

    private Matrix4x4 _matrix;
    private Vector3 _translation;
    private Quaternion _rotation;
    private Vector3 _scale;

    public Matrix4x4 GetMatrix() {
        if (_dirty)
            _matrix = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
        _dirty = false;
        return _matrix;
    }
}
