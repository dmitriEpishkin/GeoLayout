using System.Collections.Generic;
using System.Collections.Specialized;

namespace Nordwest.Notifications {
    public class CollectionChangingEventArgs<T> : CollectionChangeEventArgs<T> {
        #region base .ctors
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action) : base(action) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, T changedItem) : base(action, changedItem) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, T changedItem, int index) : base(action, changedItem, index) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, IList<T> changedItems) : base(action, changedItems) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, IList<T> changedItems, int startingIndex) : base(action, changedItems, startingIndex) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, T newItem, T oldItem) : base(action, newItem, oldItem) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, T newItem, T oldItem, int index) : base(action, newItem, oldItem, index) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, IList<T> newItems, IList<T> oldItems) : base(action, newItems, oldItems) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, IList<T> newItems, IList<T> oldItems, int startingIndex) : base(action, newItems, oldItems, startingIndex) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, T changedItem, int index, int oldIndex) : base(action, changedItem, index, oldIndex) { }
        public CollectionChangingEventArgs(NotifyCollectionChangedAction action, IList<T> changedItems, int index, int oldIndex) : base(action, changedItems, index, oldIndex) { }
        #endregion

        public bool Cancel { get; set; }
    }
}
