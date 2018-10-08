using System.Collections.Generic;
using Nordwest;

namespace Nordwest.Collections.Synchronization {
    public class CollectionSynchronizer<T> : CollectionSynchronizer<T, T> where T : class {
        public CollectionSynchronizer(IList<T> source, IList<T> destination)
            : base(source, destination, new ObjectSynchronizer<T>()) {

            Requires.NotNull(destination);
        }
    }
}
