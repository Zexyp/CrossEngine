using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Serialization
{
    internal class CallbackSerializationInfo : SerializationInfo
    {
        private ReadCallback _reader;
        private WriteCallback _writer;

        public delegate bool ReadCallback(string key, Type type, out object? value);
        public delegate void WriteCallback(string key, object? value);

        internal CallbackSerializationInfo(WriteCallback write)
        {
            _writer = write;
        }

        internal CallbackSerializationInfo(ReadCallback read)
        {
            _reader = read;
        }

        public override void AddValue(string name, object? value)
        {
            if (_writer == null) throw new InvalidOperationException("attempted to write read only info");
            _writer.Invoke(name, value);
        }

        public override object? GetValue(string name, Type typeOfValue)
        {
            if (_reader == null) throw new InvalidOperationException("attempted to read write only info");
            return _reader.Invoke(name, typeOfValue, out var value) ? value : null;
        }
        public override bool TryGetValue(string name, Type typeOfValue, out object value)
        {
            if (_reader == null) throw new InvalidOperationException("attempted to read write only info");
            return _reader.Invoke(name, typeOfValue, out value);
        }
    }
}
