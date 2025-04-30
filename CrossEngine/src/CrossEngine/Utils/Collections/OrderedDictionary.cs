using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils.Collections
{
    public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IOrderedDictionary { }

    internal class OrderedDictionary<TKey, TValue> : OrderedDictionary, IOrderedDictionary<TKey, TValue>
    {
        public TValue this[TKey key] { get => (TValue)base[key]; set => base[key] = value; }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => base.Keys.Cast<TKey>().ToArray();

        ICollection<TValue> IDictionary<TKey, TValue>.Values => base.Values.Cast<TValue>().ToArray();

        public void Add(TKey key, TValue value)
        {
            base.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key) => base.Contains(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            // do not care
            int i = 0;
            foreach (var item in (IDictionary<TKey, TValue>)this)
            {
                array[arrayIndex + i] = item; 
                i++;
            }
        }

        public bool Remove(TKey key)
        {
            base.Remove(key);
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (base.Contains(key))
            {
                value = (TValue)base[key];
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(base.Keys.GetEnumerator(), base.Values.GetEnumerator());
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator
        {
            private IEnumerator _keys;
            private IEnumerator _values;

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>((TKey)_keys.Current, (TValue)_values.Current);

            object IEnumerator.Current => Current;

            public Enumerator(IEnumerator keys, IEnumerator values)
            {
                _keys = keys;
                _values = values;
            }

            public void Dispose()
            {
                _keys = null;
                _values = null;
            }

            public bool MoveNext()
            {
                var keysRes = _keys.MoveNext();
                var valuesRes = _values.MoveNext();
                Debug.Assert(keysRes == valuesRes);
                return keysRes && valuesRes;
            }

            public void Reset()
            {
                _keys.Reset();
                _values.Reset();
            }
        }
    }
}
