using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

// вырвано из Microsoft.VisualStudio.Language.Intellisense.dll
// да простят меня Боги
// namespace Microsoft.VisualStudio.Language.Intellisense
namespace Nordwest.Collections {
    public class FilteredObservableCollection<T> : IList, ICollection, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged {
        private readonly IList<T> _underlyingList;

        private bool _isFiltering;
        private Predicate<T> _filterPredicate;
        private readonly List<T> _filteredList = new List<T>();

        public bool IsFixedSize {
            get {
                return false;
            }
        }
        object IList.this[int index] {
            get {
                return ((IList<T>)this)[index];
            }
            set {
                throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
            }
        }
        public bool IsSynchronized {
            get {
                return false;
            }
        }
        public object SyncRoot {
            get {
                if (_isFiltering) {
                    return ((ICollection)_filteredList).SyncRoot;
                }
                return ((IList)_underlyingList).SyncRoot;
            }
        }
        public T this[int index] {
            get {
                if (_isFiltering) {
                    return _filteredList[index];
                }
                return _underlyingList[index];
            }
            set {
                throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
            }
        }
        public int Count {
            get {
                if (_isFiltering) {
                    return _filteredList.Count;
                }
                return _underlyingList.Count;
            }
        }
        public bool IsReadOnly {
            get {
                return true;
            }
        }
        public FilteredObservableCollection(IList<T> underlyingList, Predicate<T> filter)
            : this(underlyingList) {
            Filter(filter);
        }
        public FilteredObservableCollection(IList<T> underlyingList) {
            if (underlyingList == null) {
                throw new ArgumentNullException("underlyingList");
            }
            if (!(underlyingList is INotifyCollectionChanged)) {
                throw new ArgumentException(@"Underlying collection must implement INotifyCollectionChanged", "underlyingList");
            }
            if (!(underlyingList is IList)) {
                throw new ArgumentException(@"Underlying collection must implement IList", "underlyingList");
            }
            _underlyingList = underlyingList;
            ((INotifyCollectionChanged)_underlyingList).CollectionChanged += new NotifyCollectionChangedEventHandler(OnUnderlyingList_CollectionChanged);
        }
        public int Add(object value) {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public bool Contains(object value) {
            return ((ICollection<T>)this).Contains((T)((object)value));
        }
        public int IndexOf(object value) {
            return ((IList<T>)this).IndexOf((T)((object)value));
        }
        public void Insert(int index, object value) {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public void Remove(object value) {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public void CopyTo(Array array, int index) {
            if (_isFiltering) {
                if (array.Length - index < Count)
                    throw new ArgumentException(@"Array not big enough", "array");

                int num = index;
                using (List<T>.Enumerator enumerator = _filteredList.GetEnumerator()) {
                    while (enumerator.MoveNext()) {
                        T current = enumerator.Current;
                        array.SetValue(current, num);
                        num++;
                    }
                    return;
                }
            }

            ((IList)_underlyingList).CopyTo(array, index);
        }
        public int IndexOf(T item) {
            if (_isFiltering)
                return _filteredList.IndexOf(item);

            return _underlyingList.IndexOf(item);
        }
        public void Insert(int index, T item) {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public void RemoveAt(int index) {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        void IList<T>.RemoveAt(int index) {
            // удовлетворение контрактов
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public void Add(T item) {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public void Clear() {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public bool Contains(T item) {
            if (_isFiltering)
                return _filteredList.Contains(item);
            return _underlyingList.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex) {
            if (_isFiltering)
                _filteredList.CopyTo(array, arrayIndex);
            else
                _underlyingList.CopyTo(array, arrayIndex);
        }
        public bool Remove(T item) {
            throw new InvalidOperationException(@"FilteredObservableCollections are read-only");
        }
        public IEnumerator<T> GetEnumerator() {
            if (_isFiltering)
                return _filteredList.GetEnumerator();

            return _underlyingList.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            if (_isFiltering)
                return ((IEnumerable)_filteredList).GetEnumerator();

            return _underlyingList.GetEnumerator();
        }
        public void Filter() {
            if (_filterPredicate != null) {
                UpdateFilter();
                RaiseCollectionChanged();
            }
        }
        public void Filter(Predicate<T> filterPredicate) {
            if (filterPredicate == null)
                throw new ArgumentNullException("filterPredicate");

            _filterPredicate = filterPredicate;
            _isFiltering = true;
            UpdateFilter();
            RaiseCollectionChanged();
        }
        public void StopFiltering() {
            if (_isFiltering) {
                _filterPredicate = null;
                _isFiltering = false;
                UpdateFilter();
                RaiseCollectionChanged();
            }
        }
        private void OnUnderlyingList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateFilter();
            RaiseCollectionChanged();
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void RaiseCollectionChanged() {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        private void UpdateFilter() {
            _filteredList.Clear();
            if (_isFiltering)
                foreach (T current in _underlyingList)
                    if (_filterPredicate(current))
                        _filteredList.Add(current);
        }
    }
}
