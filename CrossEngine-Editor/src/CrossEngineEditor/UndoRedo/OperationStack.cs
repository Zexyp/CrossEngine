using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.UndoRedo
{
    public interface IOperationHistory
    {
        void Push(IOperationShard shard);
    }

    public class OperationStack : IOperationHistory
    {
        private int HistoryLength = 32;
        private LinkedList<IOperationShard> _operations = new LinkedList<IOperationShard>();
        private LinkedListNode<IOperationShard> _lastShard;
        private LinkedListNode<IOperationShard> _nextShard;
        private int _depth = 0;

        public void Push(IOperationShard shard)
        {
            // remove pending edits
            for (int i = 0; i < _depth; i++)
            {
                _operations.RemoveLast();
            }

            _depth = 0;
            _operations.AddLast(shard);
            _lastShard = _operations.Last;
            _nextShard = null;

            // keep the history size
            while (_operations.Count > HistoryLength)
                _operations.RemoveFirst();
        }

        public void Undo()
        {
            if (_lastShard == null)
                return;

            _lastShard.Value.Undo();
            _nextShard = _lastShard;
            _lastShard = _lastShard.Previous;

            _depth++;
        }

        public void Redo()
        {
            if (_nextShard == null)
                return;

            _nextShard.Value.Redo();
            _lastShard = _nextShard;
            _nextShard = _nextShard.Next;

            _depth--;
        }
    }

    public interface IOperationShard
    {
        void Undo();
        void Redo();
    }
}
