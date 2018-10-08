using System;
using System.Threading;

namespace Nordwest {
    public class SimpleMonitor {
        private readonly Action _enterAction;
        private readonly Action _leaveAction;

        private int _enterCount;
        private bool _mute;

        public SimpleMonitor() : this(null, null) { }
        public SimpleMonitor(Action enterAction, Action leaveAction) {
            _enterAction = enterAction;
            _leaveAction = leaveAction;
        }

        public IDisposable Context() { return Context(false); }
        public IDisposable Context(bool mute) {
            return new Token(this, mute);
        }

        public void Enter(bool mute) {
            if (Interlocked.Increment(ref _enterCount) != 1)
                return;

            _mute = mute;

            if (!mute)
                _enterAction?.Invoke();
        }
        public void Exit() {
            var mute = _mute;
            var busyCount = Interlocked.Decrement(ref _enterCount);

            if (busyCount < 0)
                throw new InvalidOperationException();

            if (busyCount == 0 && !mute)
                _leaveAction?.Invoke();
        }

        public bool IsActive => _enterCount > 0;

        private class Token : IDisposable {
            private readonly SimpleMonitor _owner;
            private bool _disposed;

            public Token(SimpleMonitor monitor, bool mute) {
                _owner = monitor;
                _owner.Enter(mute);
            }

            public void Dispose() {
                if (_disposed)
                    return;
                _disposed = true;

                _owner.Exit();
            }
        }
    }
}
