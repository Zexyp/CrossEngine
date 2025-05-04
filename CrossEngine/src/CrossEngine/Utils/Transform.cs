using System;
using System.Numerics;

namespace CrossEngine.Utils;

public interface ITransform
{
    Matrix4x4 GetMatrix();
}

public interface ITransformCache : ITransform
{
    event Action<ITransformCache> Invalidated;
    
    Vector3 WorldPosition { get; }
    Quaternion WorldRotation { get; }
    Vector3 WorldScale { get; }
}

public class Transform : ITransformCache
{
    public Vector3 Position
    {
        get => _translation;
        set
        {
            _translation = value;
            _dirty = true;
            Invalidated?.Invoke(this);
        }
    }
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _dirty = true;
            Invalidated?.Invoke(this);
        }
    }
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _dirty = true;
            Invalidated?.Invoke(this);
        }
    }

    public Vector3 WorldPosition
    {
        get => _worldTransformProvider == null ? _translation : Vector3.Transform(_translation, _worldTransformProvider.GetMatrix());
        set => throw new NotImplementedException();
    }
    public Quaternion WorldRotation
    {
        get => _worldTransformProvider == null ? _rotation : _rotation * _worldTransformProvider.WorldRotation;
        set => throw new NotImplementedException();
    }
    public Vector3 WorldScale
    {
        get => _worldTransformProvider == null ? _scale : _scale * _worldTransformProvider.WorldScale;
        set => throw new NotImplementedException();
    }

    public ITransformCache WorldTransformProvider
    {
        get => _worldTransformProvider;
        set
        {
            if (_worldTransformProvider != null) _worldTransformProvider.Invalidated -= OnParentInvlaidated;
            _worldTransformProvider = value;
            if (_worldTransformProvider != null) _worldTransformProvider.Invalidated += OnParentInvlaidated;
            _dirty = true;
            Invalidated?.Invoke(this);
        }
    }

    private bool _dirty = true;
    private Matrix4x4 _worldCache = Matrix4x4.Identity;
    private Vector3 _translation = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;
    private ITransformCache _worldTransformProvider = null;

    public event Action<ITransformCache> Invalidated;

    public Matrix4x4 GetWorldTransform()
    {
        if (_dirty)
            _worldCache = (_worldTransformProvider != null) ? (GetTransform() * _worldTransformProvider.GetMatrix()) : GetTransform();
        _dirty = false;
        return _worldCache;
    }
    public Matrix4x4 GetTransform() => Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);

    Matrix4x4 ITransform.GetMatrix() => GetWorldTransform();

    private void OnParentInvlaidated(ITransformCache transform)
    {
        _dirty = true;
        Invalidated?.Invoke(this);
    }
}
