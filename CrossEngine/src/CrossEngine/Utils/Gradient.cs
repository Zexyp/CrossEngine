using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Diagnostics;

namespace CrossEngine.Utils
{
    public interface IGradient
    {
        int ElementCount { get; }
        Type Type { get; }
    
        void AddElement(float position, object element);
        void RemoveElement(int index);
        void ClearElements();
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
        public struct GradientElement
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
        }

        private readonly List<GradientElement> elements = new List<GradientElement>();
        public readonly ReadOnlyCollection<GradientElement> Elements;

        public int ElementCount { get { return elements.Count; } }

        public Type Type { get; set; }

        public void RemoveElement(int index)
        {
            elements.RemoveAt(index);
        }
        public void ClearElements()
        {
            elements.Clear();
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

        public void SetElementValue(int index, T value)
        {
            GradientElement el = elements[index];
            el.value = value;
            elements[index] = el;
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

        void IGradient.AddElement(float position, object element) => AddElement(position, (T) element);

        object IGradient.Sample(float position) => Sample(position);

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
    }
}
