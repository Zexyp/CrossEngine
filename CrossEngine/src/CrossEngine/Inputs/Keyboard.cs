using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Inputs
{
    public class Keyboard
    {
        readonly HashSet<Key> keys = new();
        readonly HashSet<Key> keysDown = new();
        readonly HashSet<Key> keysUp = new();

        public void Update()
        {
            keysDown.Clear();
            keysUp.Clear();
        }

        public void Add(Key key)
        {
            if (!keys.Contains(key))
                keysDown.Add(key);
            keys.Add(key);
        }

        public void Remove(Key key)
        {
            keys.Remove(key);
            keysDown.Remove(key);
            keysUp.Add(key);
        }

        public bool Get(Key key) => keys.Contains(key);
        public bool GetUp(Key key) => keysUp.Contains(key);
        public bool GetDown(Key key) => keysDown.Contains(key);
    }
}
