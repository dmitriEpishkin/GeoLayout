using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;

namespace Nordwest.Collections {
    public class PropertyChangedMultiplexor : INotifyPropertyChanged, IDisposable {
        private bool _disposed;

        private readonly INotifyCollectionChanged _collection;

        private readonly List<INotifyPropertyChanged> _subscriptionList = new List<INotifyPropertyChanged>();

        public PropertyChangedMultiplexor(INotifyCollectionChanged collection) {
            if (collection == null)
                throw new ArgumentNullException();

            _collection = collection;
            _collection.CollectionChanged += CollectionChanged;

            CollectionChanged(_collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        ~PropertyChangedMultiplexor() {
            Dispose(false);  
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                _collection.CollectionChanged -= CollectionChanged;
                Unsubscribe(_subscriptionList);
            }
            _disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (_disposed)
                throw new ObjectDisposedException(@"PropertyChangedMultiplexor");

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    Subscribe(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Unsubscribe(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Subscribe(e.NewItems);
                    Unsubscribe(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Unsubscribe(_subscriptionList);
                    e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)_collection);
                    goto case NotifyCollectionChangedAction.Add;
                default: throw new InvalidEnumArgumentException();
            }
        }

        private void Subscribe(IList children) {
            var copy = children.OfType<INotifyPropertyChanged>().ToList();

            foreach (var pc in copy) {
                pc.PropertyChanged += OnChildPropertyChanged;
                _subscriptionList.Add(pc);
            }
        }
        private void Unsubscribe(IList children) {
            var copy = children.OfType<INotifyPropertyChanged>().ToList();

            foreach (var pc in copy)
                pc.PropertyChanged -= OnChildPropertyChanged;
            
            foreach (var pc in copy) {
                bool removed = _subscriptionList.Remove(pc);
                Debug.Assert(removed);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (_disposed)
                throw new ObjectDisposedException(@"PropertyChangedMultiplexor");

            var handler = PropertyChanged;
            if (handler != null)
                handler(sender, e);
        }
    }
}
