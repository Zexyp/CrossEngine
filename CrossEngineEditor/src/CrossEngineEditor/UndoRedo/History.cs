using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.Reflection;
using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.UndoRedo
{
    interface IOperationShard
    {
        void Undo();
        void Redo();
    }

    internal class History
    {
        class ValueChangedOperation : IOperationShard
        {
            public MemberInfo Member;
            public object Target;
        
            public object PreviousValue;
            public object NextValue;
        
            public void Redo()
            {
                Member.SetFieldOrPropertyValue(Target, NextValue);
            }
        
            public void Undo()
            {
                Member.SetFieldOrPropertyValue(Target, PreviousValue);
            }
        }

        //void Sus()
        //{
        //    if ((result & EditResult.Started) != 0)
        //    {
        //        Debug.Assert(_inprogress == null);
        //
        //        _inprogress = new ValueChangedOperation();
        //        _inprogress.Member = memberInfo;
        //        _inprogress.Target = target;
        //        _inprogress.PreviousValue = value;
        //    }
        //    if ((result & EditResult.DoneEditing) != 0)
        //    {
        //        Debug.Assert(_inprogress != null);
        //        Debug.Assert(_inprogress.Member == memberInfo);
        //        Debug.Assert(_inprogress.Target == target);
        //
        //        _inprogress.NextValue = value;
        //        history.Push(_inprogress);
        //    }
        //    if ((result & EditResult.Ended) != 0)
        //    {
        //        Debug.Assert(_inprogress != null);
        //        _inprogress = null;
        //    }
        //}
    }
}
