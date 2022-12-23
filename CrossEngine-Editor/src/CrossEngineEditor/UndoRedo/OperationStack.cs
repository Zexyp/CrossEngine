using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CrossEngineEditor.UndoRedo
{
    public interface IOperationHistory
    {
        void Push(IOperationShard shard);
    }

    public class OperationStack : IOperationHistory
    {
        public IReadOnlyCollection<IOperationShard> History => _operations;

        public IOperationShard RecentLast => _lastShard?.Value;
        public IOperationShard RecentNext => _nextShard?.Value;

        private int HistoryLength = 64;
        private LinkedList<IOperationShard> _operations = new LinkedList<IOperationShard>();
        private LinkedListNode<IOperationShard> _lastShard;
        private LinkedListNode<IOperationShard> _nextShard;
        private int _depth = 0;

        public void Push(IOperationShard shard)
        {
            if (shard == null)
                throw new ArgumentNullException("The fok u think u are doin");
            if (_operations.Contains(shard))
                throw new ArgumentException();

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

        public void JumpAfter(IOperationShard shard)
        {
            if (shard == null)
                throw new ArgumentNullException("The fok u think u are doin");
            if (!_operations.Contains(shard))
                throw new InvalidOperationException("Shard is not a part of the history.");

            var before = _lastShard;
            var after = _nextShard;

            while (shard != before?.Value && shard != after?.Value)
            {
                before = before?.Previous;
                after = after?.Next;
            }

            if (before?.Value == shard)
                while (_lastShard?.Value != shard)
                    Undo();
            if (after?.Value == shard)
                while (_lastShard?.Value != shard)
                    Redo();
        }

        public void JumpBefore(IOperationShard shard)
        {
            if (shard == null)
                throw new ArgumentNullException("The fok u think u are doin");
            if (!_operations.Contains(shard))
                throw new InvalidOperationException("Shard is not a part of the history.");

            var before = _lastShard;
            var after = _nextShard;

            while (shard != before?.Value && shard != after?.Value)
            {
                before = before?.Previous;
                after = after?.Next;
            }

            if (before?.Value == shard)
                while (_nextShard?.Value != shard)
                    Undo();
            if (after?.Value == shard)
                while (_nextShard?.Value != shard)
                    Redo();
        }
    }

    public interface IOperationShard
    {
        void Undo();
        void Redo();
    }
}
