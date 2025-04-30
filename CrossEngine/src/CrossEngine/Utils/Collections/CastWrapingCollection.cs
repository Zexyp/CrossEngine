using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils.Collections
{
    public class CastWrapCollection<T> : ICollection<T>, IList<T>, IList
    {
        private ICollection _underlyingCollection;
        private IEnumerable _underlyingEnumerable;
        private IList _underlyingList;

        public CastWrapCollection(IEnumerable enumerable)
        {
            _underlyingEnumerable = enumerable;
            if (enumerable is ICollection collection)
                _underlyingCollection = collection;
            if (enumerable is IList list)
                _underlyingList = list;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        int ICollection.Count { get => _underlyingCollection.Count; }

        int ICollection<T>.Count
        {
            get
            {
                return _underlyingCollection.Count;
            }
        }

        bool ICollection<T>.IsReadOnly { get; }

        bool ICollection.IsSynchronized { get; }
        object ICollection.SyncRoot { get; }

        bool IList.IsReadOnly => true;
        object IList.this[int index]
        {
            get
            {
                return (T)_underlyingList[index];
            }
            set
            {
                _underlyingList[index] = value;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return (T)_underlyingList[index];
            }
            set
            {
                _underlyingList[index] = value;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (var item in _underlyingEnumerable)
            {
                yield return (T)item;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() => _underlyingEnumerable.GetEnumerator();

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        int IList.Add(object value)
        {
            return _underlyingList.Add(value);
        }

        void IList.Clear()
        {
            _underlyingList.Clear();
        }

        bool IList.Contains(object value)
        {
            return _underlyingList.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return _underlyingList.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            _underlyingList.Insert(index, value);
        }

        void IList.Remove(object value)
        {
            _underlyingList.Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            _underlyingList.RemoveAt(index);
        }


        bool ICollection<T>.Contains(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        int IList<T>.IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        bool IList.IsFixedSize
        {
            get => _underlyingList.IsFixedSize;
        }
    }
}
