using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Services;

namespace CrossEngine.Core
{
    public class ConsoleInputService : Service, IUpdatedService
    {
        public bool EnableLineInput
        {
            get => _enableLineInput;
            set
            {
                _enableLineInput = value;
                // no sync here
                _inputBuffer.Clear();
            }
        }
        public event Action<ConsoleInputService, string> LineInput;

        Thread _thread;
        ConcurrentQueue<ConsoleKeyInfo> _queue = new ConcurrentQueue<ConsoleKeyInfo>();
        Logger _log = new Logger("cin");
        StringBuilder _inputBuffer = new StringBuilder();
        bool _enableLineInput = false;

        public override void OnStart()
        {
            Debug.Assert(_thread == null);

            Console.CancelKeyPress += OnCancelKeyPress;

            //_queue.Clear(); // trust issues

            _thread = new Thread(Loop);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public override void OnDestroy()
        {
            _thread = null;
        }

        public override void OnDetach() { }
        public override void OnAttach() { }

        public void OnUpdate()
        {
            while (_queue.TryDequeue(out var ch))
            {
                Manager.SendEvent(new KeyCharEvent(ch.KeyChar));
            }
        }

        private void Loop()
        {
            while (true)
            {
                var key = Console.ReadKey();

                _queue.Enqueue(key);

                if (!EnableLineInput)
                    continue;

                if (key.Key == ConsoleKey.Enter)
                {
                    string input = _inputBuffer.ToString();
                    _inputBuffer.Clear();

                    _log.Trace($"input line: '{input}'");

                    LineInput?.Invoke(this, input);
                    
                    continue;
                }
                else if (key.Key == ConsoleKey.Backspace && _inputBuffer.Length > 0)
                {
                    _inputBuffer.Remove(_inputBuffer.Length - 1, 1);

                    Console.Write(" \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    _inputBuffer.Append(key.KeyChar);
                }
            }
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Manager.SendEvent(new WindowCloseEvent());

            e.Cancel = true;
        }
    }
}
