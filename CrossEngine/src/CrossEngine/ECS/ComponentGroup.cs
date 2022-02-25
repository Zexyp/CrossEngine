using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.ECS
{
    //public class ComponentGroup<T1, T2> : IComponentGroup where T1 : Component where T2 : Component
    //{
    //    public Entity CommonEntity { get; private set; }
    //    public int Length => 2;
    //
    //    private T1 _item1;
    //    private T2 _item2;
    //    public T1 Item1
    //    {
    //        get => _item1;
    //        set
    //        {
    //            if (CommonEntity != null)
    //            {
    //                if (CommonEntity != value.Entity) throw new InvalidOperationException("The component doesn't have common entity.");
    //            }
    //            _item1 = value;
    //            if (((IComponentGroup)this).IsEmpty) CommonEntity = null;
    //            else CommonEntity = _item1.Entity;
    //        }
    //    }
    //    public T2 Item2
    //    {
    //        get => _item2;
    //        set
    //        {
    //            if (CommonEntity != null)
    //            {
    //                if (CommonEntity != value.Entity) throw new InvalidOperationException("The component doesn't have common entity.");
    //            }
    //            _item2 = value;
    //            if (((IComponentGroup)this).IsEmpty) CommonEntity = null;
    //            else CommonEntity = _item2.Entity;
    //        }
    //    }
    //
    //    public ComponentGroup()
    //    {
    //
    //    }
    //
    //    public ComponentGroup(T1 item1, T2 item2)
    //    {
    //        Item1 = item1;
    //        Item2 = item2;
    //    }
    //
    //    public Component this[int index]
    //    {
    //        get
    //        {
    //            switch (index)
    //            {
    //                case 0: return Item1;
    //                case 1: return Item2;
    //                default: throw new IndexOutOfRangeException();
    //            }
    //        }
    //        set
    //        {
    //            switch (index)
    //            {
    //                case 0: Item1 = (T1)value; break;
    //                case 1: Item2 = (T2)value; break;
    //                default: throw new IndexOutOfRangeException();
    //            }
    //        }
    //    }
    //
    //    public static implicit operator ComponentGroup<T1, T2>(ValueTuple<T1, T2> tuple) => new ComponentGroup<T1, T2>(tuple.Item1, tuple.Item2);
    //}
    //
    //interface IComponentGroup
    //{
    //    Entity CommonEntity { get; }
    //
    //    Component this[int index] { get; set; }
    //    int Length { get; }
    //
    //    virtual bool IsEmpty
    //    {
    //        get
    //        {
    //            for (int i = 0; i < Length; i++)
    //            {
    //                if (this[i] != null)
    //                    return false;
    //            }
    //            return true;
    //        }
    //    }
    //}
}
