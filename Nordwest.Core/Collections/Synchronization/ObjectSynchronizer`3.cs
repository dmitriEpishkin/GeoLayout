using System;
using System.Collections.Generic;

namespace Nordwest.Collections.Synchronization {
    public abstract class ObjectSynchronizer<TSource, TDestination, TBinder>
        where TSource : class
        where TDestination : class
        where TBinder : ObjectBinder<TSource, TDestination>, new() {

        private readonly List<ObjectBinder<TSource, TDestination>> _mapping = new List<ObjectBinder<TSource, TDestination>>();

        public void Clear() {
            foreach (var pair in _mapping)
                pair.Unbind();
            _mapping.Clear();
        }

        protected abstract TSource CreateSource(TDestination item);
        protected abstract TDestination CreateDestination(TSource item);

        protected internal TDestination RegisterSource(TSource item) {
            if (_mapping.Exists(b => b.Source == item))
                throw new InvalidOperationException();

            var item2 = CreateDestination(item);
            var binder = new TBinder();
            binder.Bind(item, item2);
            _mapping.Add(binder);

            return item2;
        }
        protected internal TSource RegisterDestination(TDestination item) {
            if (_mapping.Exists(b => b.Destination == item))
                throw new InvalidOperationException();

            var item1 = CreateSource(item);
            var binder = new TBinder();
            binder.Bind(item1, item);
            _mapping.Add(binder);

            return item1;
        }

        protected internal TDestination DeregisterSource(TSource item) {
            var binder = _mapping.Find(b => b.Source == item);
            var item2 = binder.Destination;

            binder.Unbind();
            _mapping.Remove(binder);

            return item2;
        }
        protected internal TSource DeregisterDestination(TDestination item) {
            var binder = _mapping.Find(b => b.Destination == item);
            var item1 = binder.Source;

            binder.Unbind();
            _mapping.Remove(binder);

            return item1;
        }

        public TDestination GetDestination(TSource key) {
            return _mapping.Find(b => b.Source == key).Destination;
        }
        public TSource GetSource(TDestination value) {
            return _mapping.Find(b => b.Destination == value).Source;
        }

    }
}
