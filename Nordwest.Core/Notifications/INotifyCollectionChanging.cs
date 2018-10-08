using System;

namespace Nordwest.Notifications {
    public interface INotifyCollectionChanging<T> {
        event EventHandler<CollectionChangingEventArgs<T>> CollectionChanging;
    }
}
