using System;
using System.Collections.Generic;
using System.Numerics;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Ecs;
using CrossEngine.Serialization;

namespace CrossEngine.Components;

// wrapper around Transform class
[AllowSinglePerEntity]
public class TransformComponent : Component, ITransform
{
    [Serialize]
    [EditorDrag]
    public Vector3 Position { get => _transform.Position; set => _transform.Position = value; }
    [Serialize]
    public Quaternion Rotation { get => _transform.Rotation; set => _transform.Rotation = value; }
    [Serialize]
    [EditorDrag]
    public Vector3 Scale { get => _transform.Scale; set => _transform.Scale = value; }
    [EditorDrag]
    public Vector3 Euler { get => _transform.Euler; set => _transform.Euler = value; }

    public Vector3 WorldPosition { get => _transform.WorldPosition; set => _transform.WorldPosition = value; }
    public Quaternion WorldRotation { get => _transform.WorldRotation; set => _transform.WorldRotation = value; }
    public Vector3 WorldScale { get => _transform.WorldScale; set => _transform.WorldScale = value; }

    internal TransformComponent Parent { set => _transform.WorldTransformProvider = value?._transform; }

    private readonly Transform _transform = new Transform();

    public Matrix4x4 GetTransformMatrix() => _transform.GetTransform();
    public Matrix4x4 GetWorldTransformMatrix() => _transform.GetWorldTransform();
    Matrix4x4 ITransform.GetMatrix() => GetWorldTransformMatrix();
}