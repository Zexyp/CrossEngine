using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Serialization;

namespace CrossEngine.Utils
{
    //public interface IGradient
    //{
    //    IList<Vector4> GetPoints();
    //    int EditPoint(int pointIndex, Vector4 value);
    //    Vector4 GetPoint(float t);
    //    void AddPoint(Vector4 value);
    //}
    
    public interface IGradient : ISerializable
    {
        int ElementCount { get; }
        Type Type { get; }
    
        void AddElement(float position, object element);
        void RemoveElement(int index);
        void Clear();
        object Sample(float position);
        void SetElementValue(int index, object value);
        int SetElementPosition(int index, float position);
    }

    /// <summary>
    /// Only allowed generic parameters for <typeparamref name="T"/> are <see cref="float"/>, <see cref="Vector2"/>, <see cref="Vector3"/> and <see cref="Vector4"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Gradient<T> : IGradient where T : struct
    {
        public struct GradientElement : ISerializable
        {
            public float position;
            public T value;

            public GradientElement(float position, T value)
            {
                this.position = position;
                this.value = value;
            }

            public override string ToString()
            {
                return "position: " + position + "; value: " + value;
            }

            public void GetObjectData(SerializationInfo info)
            {
                info.AddValue(nameof(position), position);
                info.AddValue(nameof(value), value);
            }

            public void SetObjectData(SerializationInfo info)
            {
                position = info.GetValue<float>(nameof(position));
                value = info.GetValue<T>(nameof(value));
            }
        }

        private static readonly Dictionary<Type, Func<object, object, float, object>> Lerps = new Dictionary<Type, Func<object, object, float, object>>()
        {
            { typeof(float), (a, b, factor) => MathExtension.Lerp((float)a, (float)b, factor) },
            { typeof(Vector2), (a, b, factor) => Vector2.Lerp((Vector2)a, (Vector2)b, factor) },
            { typeof(Vector3), (a, b, factor) => Vector3.Lerp((Vector3)a, (Vector3)b, factor) },
            { typeof(Vector4), (a, b, factor) => Vector4.Lerp((Vector4)a, (Vector4)b, factor) },
        };

        public Gradient()
        {
            var type = typeof(T);
            if (!Lerps.ContainsKey(type))
            {
                throw new ArgumentException($"Disallowed type ('{type.Name}').");
            }

            Type = type;
            Elements = elements.AsReadOnly();

            Clear();
        }

        public Gradient(params (float Position, T Value)[] stops) : this()
        {
            if (stops == null)
                throw new ArgumentNullException();
            if (stops.Length == 0)
                throw new ArgumentException();

            else
            {
                elements.Clear();
                for (int i = 0; i < stops.Length; i++)
                {
                    AddElement(stops[i].Position, stops[i].Value);
                }
            }
        }

        public Gradient(params T[] values) : this()
        {
            if (values == null)
                throw new ArgumentNullException();
            if (values.Length == 0)
                throw new ArgumentException();

            else
            {
                elements.Clear();
                float offset = (values.Length > 1) ? 1f / (values.Length - 1) : 0;
                float pos = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    AddElement(pos, values[i]);
                    pos += offset;
                }
            }
        }

        private readonly List<GradientElement> elements = new List<GradientElement>();
        public readonly ReadOnlyCollection<GradientElement> Elements;

        public int ElementCount { get { return elements.Count; } }

        public Type Type { get; set; }

        public void Clear()
        {
            elements.Clear();
            AddElement(0, default);
        }

        public int AddElement(float position, T value)
        {
            if (elements.Count == 0)
            {
                elements.Add(new GradientElement(position, value));
                return 0;
            }

            if (elements[0].position >= position)
            {
                elements.Insert(0, new GradientElement(position, value));
                return 0;
            }
            if (elements[elements.Count - 1].position <= position)
            {
                elements.Add(new GradientElement(position, value));
                return elements.Count - 1;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].position < position && elements[i + 1].position >= position)
                {
                    elements.Insert(i + 1, new GradientElement(position, value));
                    return i + 1;
                }
            }

            Debug.Assert(false, "unexpected end of a method!");
            return default;
        }

        public void RemoveElement(int index)
        {
            if (elements.Count == 1)
                throw new InvalidOperationException();

            elements.RemoveAt(index);
        }

        public T Sample(float position)
        {
            if (elements.Count == 0)
                return default;

            if (elements[0].position >= position)
                return elements[0].value;
            if (elements[elements.Count - 1].position <= position)
                return elements[elements.Count - 1].value;

            GradientElement firstElement = elements[0];
            for (int i = 1; i < elements.Count; i++)
            {
                GradientElement secondElement = elements[i];

                if (position == secondElement.position)
                    return secondElement.value;

                if (firstElement.position < position && secondElement.position > position)
                {
                    float sample = (position - firstElement.position) / (secondElement.position - firstElement.position);

                    return (T)Lerps[Type](firstElement.value, secondElement.value, sample);
                }
                firstElement = secondElement;
            }

            Debug.Assert(false, "unexpected end of a method!");
            return default;
        }

        public void SetElementValue(int index, T value)
        {
            GradientElement el = elements[index];
            el.value = value;
            elements[index] = el;
        }

        public void SetElementValue(int index, object value) => SetElementValue(index, (T)value);

        public int SetElementPosition(int index, float position)
        {
            GradientElement el = elements[index];
            elements.RemoveAt(index);

            if (elements.Count == 0)
            {
                elements.Add(new GradientElement(position, el.value));
                return 0;
            }

            if (elements[0].position >= position)
            {
                elements.Insert(0, new GradientElement(position, el.value));
                return 0;
            }
            if (elements[elements.Count - 1].position <= position)
            {
                elements.Add(new GradientElement(position, el.value));
                return elements.Count - 1;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].position < position && elements[i + 1].position >= position)
                {
                    elements.Insert(i + 1, new GradientElement(position, el.value));
                    return i + 1;
                }
            }

            Debug.Assert(false, "unexpected end of a method!");
            return default;
        }

        void IGradient.AddElement(float position, object element) => AddElement(position, (T)element);
        object IGradient.Sample(float position) => Sample(position);

        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue(nameof(Elements), elements.ToArray());
        }

        public void SetObjectData(SerializationInfo info)
        {
            var els = info.GetValue(nameof(Elements), elements.ToArray());

            elements.Clear();
            for (int i = 0; i < els.Length; i++)
            {
                AddElement(els[i].position, els[i].value);
            }
        }
    }
}
