using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils
{
    internal class CastWrapCollection<T> : ICollection<T>, IList<T>
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
        
        public int Count
        {
            get
            {
                if (_underlyingCollection == null) throw new InvalidOperationException();
                
                return _underlyingCollection.Count;
            }
        }

        public bool IsReadOnly => true;
        
        public T this[int index]
        {
            get
            {
                if (_underlyingList == null) throw new InvalidOperationException();

                return (T)_underlyingList[index];
            }
            set
            {
                if (_underlyingList == null) throw new InvalidOperationException();

                _underlyingList[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _underlyingEnumerable)
            {
                yield return (T)item;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() => _underlyingEnumerable.GetEnumerator();
        
        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }
        
        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
