using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Nordwest.Notifications;

namespace Nordwest.Collections {
    public class PreviewObservableCollection<T> : ObservableCollection<T>, INotifyCollectionChanged<T>, INotifyCollectionChanging<T> {

        public PreviewObservableCollection() {
        }
        public PreviewObservableCollection(IEnumerable<T> collection)
            : base(collection) {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            var generics = CollectionChangeEventArgs<T>.Wrap(e);

            // BASE
            base.OnCollectionChanged(generics);
            // THIS
            OnCollectionChanged(generics);
        }

        public new event EventHandler<CollectionChangeEventArgs<T>> CollectionChanged; // NEW : GENERIC
        private void OnCollectionChanged(CollectionChangeEventArgs<T> e) {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<CollectionChangingEventArgs<T>> CollectionChanging;
        protected virtual void OnCollectionChanging(CollectionChangingEventArgs<T> e) {
            var handler = CollectionChanging;
            if (handler != null) {
                var subscriptions = handler.GetInvocationList();
                int index = 0;
                try {
                    for (; index < subscriptions.Length; index++) {
                        ((EventHandler<CollectionChangingEventArgs<T>>)subscriptions[index])(this, e);
                        if (e.Cancel)
                            break;
                    }
                } catch (Exception) {
                    e.Cancel = true;
                }
            }

            if (e.Cancel)
                OnCollectionChangingCancelled(EventArgs.Empty);
        }

        public event EventHandler CollectionChangingCancelled;
        protected virtual void OnCollectionChangingCancelled(EventArgs e) {
            var handler = CollectionChangingCancelled;
            if (handler != null)
                handler(this, e);
        }

        private bool TryPassChangingChecks(CollectionChangingEventArgs<T> e) {
            OnCollectionChanging(e);
            return !e.Cancel;
        }

        protected override void ClearItems() {
            if (TryPassChangingChecks(new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Reset)))
                base.ClearItems();
        }
        protected override void InsertItem(int index, T item) {
            if (TryPassChangingChecks(new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Add, item, index)))
                base.InsertItem(index, item);
        }
        protected override void MoveItem(int oldIndex, int newIndex) {
            if (TryPassChangingChecks(new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Move, this[oldIndex], oldIndex, newIndex)))
                base.MoveItem(oldIndex, newIndex);
        }
        protected override void SetItem(int index, T item) {
            if (TryPassChangingChecks(new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Replace, item, this[index], index)))
                base.SetItem(index, item);
        }
        protected override void RemoveItem(int index) {
            if (TryPassChangingChecks(new CollectionChangingEventArgs<T>(NotifyCollectionChangedAction.Remove, this[index], index)))
                base.RemoveItem(index);
        }
    }
}