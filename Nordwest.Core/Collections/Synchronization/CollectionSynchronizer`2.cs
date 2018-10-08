using System.Collections.Generic;
using Nordwest;

namespace Nordwest.Collections.Synchronization {
    public class CollectionSynchronizer<TSource, TDestination> : CollectionSynchronizer<TSource, TDestination, ObjectBinder<TSource, TDestination>>
        where TSource : class
        where TDestination : class {
        public CollectionSynchronizer(IList<TSource> source, IList<TDestination> destination, ObjectSynchronizer<TSource, TDestination> synchronizer) :
            base(source, destination, synchronizer) {

            Requires.NotNull(destination);
        }
    }
}
