using System;
using System.Collections.Generic;
using Nordwest;

namespace Nordwest.Collections.Synchronization {
    public static class Synchronize {
        private class DelegateSynchronizer<TSource, TDestination> : ObjectSynchronizer<TSource, TDestination>
            where TSource : class
            where TDestination : class {

            private readonly Func<TSource, TDestination> _createDestinationFunc;
            private readonly Func<TDestination, TSource> _createSourceFunc;

            public DelegateSynchronizer(Func<TSource, TDestination> createDestinationFunc, Func<TDestination, TSource> createSourceFunc) {
                //Requires.NotNull(createDestinationFunc);
                //Requires.NotNull(createSourceFunc);

                _createDestinationFunc = createDestinationFunc;
                _createSourceFunc = createSourceFunc;
            }

            protected override TDestination CreateDestination(TSource item) {
                return _createDestinationFunc(item);
            }
            protected override TSource CreateSource(TDestination item) {
                return _createSourceFunc(item);
            }
        }

        public static CollectionSynchronizer<T> Collections<T>(
            IList<T> source,
            IList<T> destination)
                where T : class {

            Requires.NotNull(source);
            Requires.NotNull(destination);

            return new CollectionSynchronizer<T>(source, destination);
        }
        public static CollectionSynchronizer<TSource, TDestination> Collections<TSource, TDestination>(
            IList<TSource> source,
            IList<TDestination> destination,
            ObjectSynchronizer<TSource, TDestination> synchronizer)
                where TSource : class
                where TDestination : class {

            Requires.NotNull(source);
            Requires.NotNull(destination);
            Requires.NotNull(synchronizer);

            return new CollectionSynchronizer<TSource, TDestination>(source, destination, synchronizer);
        }
        public static CollectionSynchronizer<TSource, TDestination> Collections<TSource, TDestination>(
            IList<TSource> source,
            IList<TDestination> destination,
            Func<TSource, TDestination> createDestinationFunc,
            Func<TDestination, TSource> createSourceFunc)
                where TSource : class
                where TDestination : class {

            Requires.NotNull(source);
            Requires.NotNull(destination);
            //Requires.NotNull(createDestinationFunc);
            //Requires.NotNull(createSourceFunc);

            return Collections(source, destination, CreateObjectSynchonizer(createDestinationFunc, createSourceFunc));
        }
        public static CollectionSynchronizer<TSource, TDestination> Collections<TSource, TDestination>(
            IList<TSource> source,
            IList<TDestination> destination,
            Func<TSource, TDestination> createDestinationFunc)
            where TSource : class
            where TDestination : class {

            Requires.NotNull(source);
            Requires.NotNull(destination);

            return Collections(source, destination, CreateObjectSynchonizer(createDestinationFunc, null));
        }

        private static ObjectSynchronizer<TSource, TDestination> CreateObjectSynchonizer<TSource, TDestination>(
            Func<TSource, TDestination> createDestinationFunc,
            Func<TDestination, TSource> createSourceFunc)
                where TSource : class
                where TDestination : class {
        
            return new DelegateSynchronizer<TSource, TDestination>(createDestinationFunc, createSourceFunc);
        }
    }
}
