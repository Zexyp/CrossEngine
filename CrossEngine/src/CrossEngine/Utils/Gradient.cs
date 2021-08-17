using System;

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CrossEngine.Utils
{
    //interface IGradient
    //{
    //    int ElementCount { get; }
    //
    //    bool AddElement(float element);
    //    bool AddElement(Vector2 element);
    //    bool AddElement(Vector3 element);
    //    bool AddElement(Vector4 element);
    //
    //    void RemoveElement(int index);
    //    void ClearElements();
    //
    //    float SampleFloat(float position);
    //    Vector2 SampleVector2(float position);
    //    Vector3 SampleVector3(float position);
    //    Vector4 SampleVector4(float position);
    //}

    public class Gradient
    {
        // idea: extrapolation

        public struct GradientElement
        {
            public float position;
            public Vector4 value;

            public GradientElement(float position, Vector4 value)
            {
                this.position = position;
                this.value = value;
            }

            public override string ToString()
            {
                return "position: " + position + "; value: " + value;
            }
        }

        List<GradientElement> elements = new List<GradientElement> { };

        public int ElementCount { get { return elements.Count; } }
        public GradientElement[] GetElements()
        {
            return elements.ToArray();
        }

        public void RemoveElement(int index)
        {
            elements.RemoveAt(index);
        }
        public void ClearElements()
        {
            elements.Clear();
        }
        public void Reverse()
        {
            elements.Reverse();

            GradientElement el; // positions need to be reversed
            for (int i = 0; i < elements.Count; i++)
            {
                el = elements[i];
                el.position = -el.position + 1;
                elements[i] = el;
            }
        }

        #region AddElement
        public void AddElement(float position, float value) => AddElement(position, new Vector4(value));
        public void AddElement(float position, Vector2 value) => AddElement(position, new Vector4(value, 0, 0));
        public void AddElement(float position, Vector3 value) => AddElement(position, new Vector4(value, 0));
        public void AddElement(float position, Vector4 value)
        {
            if (elements.Count == 0)
            {
                elements.Add(new GradientElement(position, value));
                return;
            }

            if (elements[0].position > position)
            {
                elements.Insert(0, new GradientElement(position, value));
                return;
            }
            if (elements[elements.Count - 1].position < position)
            {
                elements.Add(new GradientElement(position, value));
                return;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].position < position && elements[i + 1].position > position)
                {
                    elements.Insert(i + 1, new GradientElement(position, value));
                    return;
                }
            }

            throw new Exception("unexpected end of method!");
        }
        #endregion

        #region SetElement
        public bool SetElementValue(int index, float value) => SetElementValue(index, new Vector4(value));
        public bool SetElementValue(int index, Vector2 value) => SetElementValue(index, new Vector4(value, 0, 0));
        public bool SetElementValue(int index, Vector3 value) => SetElementValue(index, new Vector4(value, 0));
        public bool SetElementValue(int index, Vector4 value)
        {
            if (index > elements.Count - 1 || index < 0)
                return false;

            GradientElement el = elements[index];
            el.value = value;
            elements[index] = el;
            return true;
        }
        #endregion

        #region Sample
        public float SampleFloat(float position)
        {
            return Sample(position).X;
        }
        public Vector2 SampleVector2(float position)
        {
            Vector4 vec = Sample(position);
            return new Vector2(vec.X, vec.Y);
        }
        public Vector3 SampleVector3(float position)
        {
            Vector4 vec = Sample(position);
            return new Vector3(vec.X, vec.Y, vec.Z);
        }
        public Vector4 SampleVector4(float position) => Sample(position);

        public Vector4 Sample(float position)
        {
            if (elements.Count == 0)
                return Vector4.Zero;

            if (elements[0].position > position)
                return elements[0].value;
            if (elements[elements.Count - 1].position < position)
                return elements[elements.Count - 1].value;

            GradientElement firstElement = elements[0];
            for (int i = 1; i < elements.Count; i++)
            {
                GradientElement secondElement = elements[i];

                if (position == secondElement.position) // small check
                    return secondElement.value;

                if (firstElement.position < position && secondElement.position > position)
                {
                    float sample = (position - firstElement.position) / (secondElement.position - firstElement.position);

                    return Vector4.Lerp(firstElement.value, secondElement.value, sample);
                }
                firstElement = secondElement;
            }

            throw new Exception("unexpected end of sampling method!");
        }
        #endregion

        static public Gradient Default 
        {
            get
            {
                Gradient gradient = new Gradient();
                gradient.AddElement(0, new Vector4(0, 0, 0, 1));
                gradient.AddElement(1, new Vector4(1, 1, 1, 1));
                return gradient;
            } 
        }
        static public Gradient InvertedDefault
        {
            get
            {
                Gradient gradient = new Gradient();
                gradient.AddElement(0, new Vector4(1, 1, 1, 1));
                gradient.AddElement(1, new Vector4(0, 0, 0, 1));
                return gradient;
            }
        }
    }
}
