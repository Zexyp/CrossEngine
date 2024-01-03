using CrossEngine.Inputs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossEngine.Serialization
{
    public class SerializationInfo
    {
        private ReadCallback _reader;
        private WriteCallback _writer;

        public delegate bool ReadCallback(string key, Type type, out object? value);
        public delegate void WriteCallback(string key, object? value);

        internal SerializationInfo(WriteCallback write)
        {
            _writer = write;
        }

        internal SerializationInfo(ReadCallback read)
        {
            _reader = read;
        }

        public void AddValue(string name, object? value)
        {
            _writer.Invoke(name, value);
        }

        public object? GetValue(string name, Type typeOfValue)
        {
            return _reader.Invoke(name, typeOfValue, out var value) ? value : null;
        }
        public bool TryGetValue(string name, Type typeOfValue, out object value)
        {
            return _reader.Invoke(name, typeOfValue, out value);
        }

        public object GetValue(string name, Type typeOfValue, object customDefault)
        {
            if (TryGetValue(name, typeOfValue, out object value))
                return value;
            else
                return customDefault;
        }
        public T GetValue<T>(string name, T customDefault)
        {
            if (TryGetValue(name, out T value))
                return value;
            else
                return customDefault;
        }
        public T? GetValue<T>(string name)
        {
            return (T?)GetValue(name, typeof(T));
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            bool succ = TryGetValue(name, typeof(T), out object objval);
            value = succ ? (T)objval : default;
            return succ;
        }
    }
}