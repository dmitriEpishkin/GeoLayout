using System.Collections.Generic;
using System.Collections.Specialized;

namespace Nordwest.Notifications {
    public interface ICollectionChangeEventArgs<out T> {
        IReadOnlyList<T> NewItems { get; }
        IReadOnlyList<T> OldItems { get; }
        NotifyCollectionChangedAction Action { get; }
        int NewStartingIndex { get; }
        int OldStartingIndex { get; }
    }
}