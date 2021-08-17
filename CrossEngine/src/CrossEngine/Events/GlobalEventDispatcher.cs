using System;
using System.Collections.Generic;

namespace CrossEngine.Events
{
    public static class GlobalEventDispatcher
    {
        static Dictionary<Type, List<Func<Event, object>>> eventHandles = new Dictionary<Type, List<Func<Event, object>>> { };

        static bool cleanUnusedLists = true;

        static private Func<Event, object> WrapType<T>(OnEventFunction<T> oef) where T : Event
        {
            return (e) => {
                if (e != null) { oef((T)e); return null; }
                else return oef;
            };

            // bestest
            // https://stackoverflow.com/questions/32512153/dynamic-actiont-invalid-arguments-when-executed
        }

        static public void Register<T>(OnEventFunction<T> oe) where T : Event
        {
            Type type = typeof(T);
            Func<Event, object> func = WrapType(oe);
            if (eventHandles.ContainsKey(type))
            {
                eventHandles[type].Add(func);
            }
            else
            {
                eventHandles.Add(type, new List<Func<Event, object>> { func });
            }

            //Log.Debug("registered: " + oe.ToString());
        }

        static public void Unregister<T>(OnEventFunction<T> oe) where T : Event
        {
            Type type = typeof(T);
            if (eventHandles.ContainsKey(type))
            {
                List<Func<Event, object>> list = eventHandles[type];
                for (int i = 0; i < list.Count; i++)
                {
                    if (oe == (OnEventFunction<T>)list[i](null))
                    {
                        list.RemoveAt(i);
                        if (cleanUnusedLists && list.Count == 0)
                            eventHandles.Remove(type);

                        //Log.Debug("unregistered: " + oe.ToString());

                        return;
                    }
                }   
            }

            throw new Exception("nothing registered!");
        }

        static public void ClearUnusedTypes()
        {
            foreach (KeyValuePair<Type, List<Func<Event, object>>> pair in eventHandles)
            {
                if (pair.Value.Count == 0) eventHandles.Remove(pair.Key);
            }
        }

        static public void Dispatch(Event e)
        {
            Type type = e.GetType();

            //Log.Debug("desired: " + type.ToString());
            //foreach (var item in eventHandlers)
            //{
            //    Log.Debug("contains: " + item.Key.ToString());
            //}

            if (eventHandles.ContainsKey(type))
            {
                for (int i = 0; i < eventHandles[type].Count; i++)
                {
                    eventHandles[type][i](e);
                    //if (e.handled) break;
                }
            }
        }
    }
}
