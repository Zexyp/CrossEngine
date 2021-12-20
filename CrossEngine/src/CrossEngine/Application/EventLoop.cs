using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine
{
    public class EventLoop
    {
        static List<ValueTuple<DateTime, Action>> _actions = new List<(DateTime, Action)>();

        public static void Update()
        {
            DateTime now = DateTime.Now;
            for (int i = 0; i < _actions.Count; i++)
            {
                if (now >= _actions[i].Item1)
                {
                    _actions[i].Item2.Invoke();
                    _actions.RemoveAt(i);
                }
            }
        }

        public static void CallLater(int milisecondDelay, Action action)
        {
            _actions.Add((DateTime.Now.AddMilliseconds(milisecondDelay), action));
        }

        public static void CallSoon(Action action)
        {
            _actions.Add((DateTime.MinValue, action));
        }
    }
}
