namespace Nordwest.Collections.Synchronization {
    public abstract class ObjectSynchronizer<TSource, TDestination> : ObjectSynchronizer<TSource, TDestination, ObjectBinder<TSource, TDestination>>
        where TSource : class
        where TDestination : class { }
}
