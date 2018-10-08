using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Nordwest.Notifications;

namespace Nordwest.Collections {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TransactiveObservableCollection<T> : ObservableCollection<T> {
        private readonly SimpleMonitor _monitor;

        public TransactiveObservableCollection()
            : this(new T[0]) {
        }
        public TransactiveObservableCollection(IEnumerable<T> collection)
            : base(collection) {
            
            _monitor = new SimpleMonitor(null, () => {
                OnPropertyChanged(new PropertyChangedEventArgs(@"Count"));
                OnPropertyChanged(new PropertyChangedEventArgs(@"Item[]"));
                OnCollectionChanged(new CollectionChangeEventArgs<T>(NotifyCollectionChangedAction.Reset));
            });
        }

        public IDisposable Transaction() {
            return Transaction(false);
        }
        protected IDisposable Transaction(bool silent) {
            return _monitor.Context(silent);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (!_monitor.IsActive)
                base.OnCollectionChanged(e);
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (!_monitor.IsActive)
                base.OnPropertyChanged(e);
        }
    }
}