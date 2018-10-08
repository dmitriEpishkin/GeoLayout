using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Nordwest;

namespace Nordwest.Collections {
    public class ObservableRangeCollection<T> : TransactiveObservableCollection<T> {
        
        public ObservableRangeCollection()
            : this(Enumerable.Empty<T>()) {
        }
        public ObservableRangeCollection(IEnumerable<T> collection)
            : base(collection) {
        }

        public void AddRange(IEnumerable<T> items) {
            Requires.NotNull(items);

            var list = items.ToList();

            if (list.Count == 0)
                return;

            //var e = new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Add, list);
            //OnCollectionChanging(e);
            //if (e.CancellationToken != null)
            //    return; // RET

            using (Transaction(true))
                for (int i = 0; i < list.Count; i++)
                    InsertItem(Items.Count, list[i]);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list));
        }
        public void InsertRange(int index, IEnumerable<T> items) {
            Nordwest.Requires.NotNull(items);

            var list = items.ToList();

            if (list.Count == 0)
                return;

            //var e = new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Add, list, index);
            //OnCollectionChanging(e);
            //if (e.CancellationToken != null)
            //    return; // RET

            using (Transaction(true))
                for (int i = 0; i < list.Count; i++)
                    InsertItem(index + i, list[i]);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, index));
        }
        public void RemoveRange(IEnumerable<T> items) {
            Requires.NotNull(items);

            var list = items.ToList();

            if (list.Count == 0)
                return;

            //var e = new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Remove, list);
            //OnCollectionChanging(e);
            //if (e.CancellationToken != null)
            //    return; // RET

            var removed = new List<T>(list.Count);

            int count = Items.Count;
            using (Transaction(true))
                for (int i = 0; i < list.Count; i++) {
                    RemoveItem(IndexOf(list[i]));
                    if (Items.Count == count - 1) {
                        removed.Add(list[i]);
                        count = Items.Count;
                    }
                }

            if (removed.Count > 0)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }
        public void ReplaceRange(IEnumerable<T> oldItems, IEnumerable<T> newItems) {
            Requires.NotNull(oldItems);
            Requires.NotNull(newItems);

            var oldList = oldItems.ToList();
            var newList = newItems.ToList();

            if (oldList.Count != newList.Count)
                throw new InvalidOperationException();

            if (oldList.Count == 0)
                return;

            if (!oldList.All(Contains))
                throw new InvalidOperationException();

            //var e = new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Replace, oldList, newList);
            //OnCollectionChanging(e);
            //if (e.CancellationToken != null)
            //    return; // RET

            using (Transaction(true))
                for (int i = 0; i < oldList.Count; i++)
                    SetItem(IndexOf(oldList[i]), newList[i]);
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldList, newList));
        }
    }
}