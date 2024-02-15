using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils
{
    internal class CastWrapCollection<T> : ICollection<T>
    {
        private ICollection _underlyingCollection;

        public CastWrapCollection(ICollection collection)
        {
            _underlyingCollection = collection;
        }

        public int Count => _underlyingCollection.Count;

        public bool IsReadOnly => true;

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

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _underlyingCollection)
            {
                yield return (T)item;
            }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => _underlyingCollection.GetEnumerator();
    }
}
