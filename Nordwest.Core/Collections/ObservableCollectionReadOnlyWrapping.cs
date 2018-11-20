using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Nordwest.Notifications;

namespace Nordwest.Collections {
    public class ObservableCollectionReadOnlyWrapping<T> : ICollection<T>, INotifyCollectionChanged {

        private readonly ObservableCollection<T> _collection;

        public ObservableCollectionReadOnlyWrapping(ObservableCollection<T> collection) {
            _collection = collection;
            _collection.CollectionChanged += (sender, args) => OnCollectionChanged(args);
        }


        public IEnumerator<T> GetEnumerator() {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            throw new NotSupportedException();
        }

        public void Clear() {
            throw new NotSupportedException();
        }

        public bool Contains(T item) {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotSupportedException();
        }

        public bool Remove(T item) {
            throw new NotSupportedException();
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => true; 

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args) {
            CollectionChanged?.Invoke(this, args);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

    }
}
