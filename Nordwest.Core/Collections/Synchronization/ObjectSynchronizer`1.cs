namespace Nordwest.Collections.Synchronization {
    public class ObjectSynchronizer<T> : ObjectSynchronizer<T, T>
        where T : class {

        protected override T CreateSource(T item) {
            return item;
        }
        protected override T CreateDestination(T item) {
            return item;
        }
    }
}
