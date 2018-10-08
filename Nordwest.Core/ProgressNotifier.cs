using System;
using System.Diagnostics;

namespace Nordwest {
    public class ProgressNotifier
    {
        private const double Epsilon = 1e-6;
        private readonly ProgressNotifier _baseNotifier;
        private readonly double _basePart;

        private double _progress;

        private readonly object _syncLock = new object();

        public ProgressNotifier()
            : this(null, 1) {
        }
        public ProgressNotifier(ProgressNotifier baseNotifier, double basePart) {
            Requires.Range(basePart >= 0 && basePart <= 1);

            _baseNotifier = baseNotifier;
            _basePart = basePart;
        }

        public double Progress {
            get { return _progress; }
            set {
                //Requires.Range(value >= 0 && value <= 1);
                Debug.Assert(value >= 0 - Epsilon && value <= 1 + Epsilon);
                if (value < 0 + Epsilon)
                    value = 0;
                if (value > 1 - Epsilon)
                    value = 1;

                //value = Math.Round(value, 3);
                var delta = value - _progress;

                //if (Math.Abs(delta) > 1e-5){

                _progress = value;
                OnProgressChanged(EventArgs.Empty);

                if (_baseNotifier != null) {
                    lock (_baseNotifier._syncLock) {
                        _baseNotifier.Progress += delta * _basePart;
                    }
                }
                //}
            }
        }

        public event EventHandler ProgressChanged;
        protected virtual void OnProgressChanged(EventArgs e) {
            var handler = ProgressChanged;
            if (handler != null)
                handler(this, e);
        }
    }
}
