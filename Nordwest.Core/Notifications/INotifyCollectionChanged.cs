using System;

namespace Nordwest.Notifications {
    public interface INotifyCollectionChanged<T> : System.Collections.Specialized.INotifyCollectionChanged {
        new event EventHandler<CollectionChangeEventArgs<T>> CollectionChanged;
    }
}
