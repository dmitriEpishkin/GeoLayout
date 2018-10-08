using System;

namespace Nordwest.Collections.Synchronization {
    public class ObjectBinder<TSource, TDestination>
        where TSource : class
        where TDestination : class {

        private TSource _source;
        private TDestination _destination;

        public void Bind(TSource source, TDestination destination) {
            if (source == null || destination == null)
                throw new ArgumentNullException();
            if (_source != null || _destination != null)
                throw new InvalidOperationException();

            _source = source;
            _destination = destination;

            BindCore();
        }
        public void Unbind() {
            if (_source == null || _destination == null)
                throw new InvalidOperationException();

            UnbindCore();

            _source = null;
            _destination = null;
        }

        protected virtual void BindCore() { }
        protected virtual void UnbindCore() { }

        public TSource Source {
            get { return _source; }
        }
        public TDestination Destination {
            get { return _destination; }
        }
    }
}
