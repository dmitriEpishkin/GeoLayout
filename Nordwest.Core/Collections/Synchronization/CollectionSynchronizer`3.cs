using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using Nordwest;

namespace Nordwest.Collections.Synchronization {
    public class CollectionSynchronizer<TSource, TDestination, TBinder> : IDisposable
        where TSource : class
        where TDestination : class
        where TBinder : ObjectBinder<TSource, TDestination>, new() {

        private readonly IList<TSource> _source;
        private readonly IList<TDestination> _destination;

        private readonly ObjectSynchronizer<TSource, TDestination, TBinder> _synchronizer;

        private readonly object _syncLock = new object();

        private bool _disposed;

        //// Reset
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action);

        //// Add, Remove, Reset
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems);
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem);
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index);
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex);

        //// Replace
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems);
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem);
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex);
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index);

        //// Move
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex);
        //public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex);

        public CollectionSynchronizer(IList<TSource> source, IList<TDestination> destination, ObjectSynchronizer<TSource, TDestination, TBinder> synchronizer) {
            Requires.NotNull(source);
            Requires.NotNull(destination);
            Requires.NotNull(synchronizer);

            // допускаем синхронизацию в любую сторону
            //Requires.Argument(source is INotifyCollectionChanged);
            //Requires.Argument(destination is INotifyCollectionChanged);

            // ниже вызывается Reset
            //Requires.Empty(destination);

            _source = source;
            _destination = destination;
            _synchronizer = synchronizer;

            TryAdd(_source, OnSourceCollectionChanged);
            TryAdd(_destination, OnDestinationCollectionChanged);

            if (_source.Count > 0 || _destination.Count > 0)
                // начальная синхронизация
                OnSourceCollectionChanged(_source, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        ~CollectionSynchronizer() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;

            try {
                if (disposing) {
                    lock (_syncLock) {
                        _synchronizer.Clear();

                        TryRemove(_source, OnSourceCollectionChanged);
                        TryRemove(_destination, OnDestinationCollectionChanged);
                    }
                }
            } finally {
                _disposed = true;
            }
        }

        private static void TryAdd<T>(IList<T> collection, NotifyCollectionChangedEventHandler handler) {
            var ncc = collection as INotifyCollectionChanged;
            if (ncc != null)
                ncc.CollectionChanged += handler;
        }
        private static void TryRemove<T>(IList<T> collection, NotifyCollectionChangedEventHandler handler) {
            var ncc = collection as INotifyCollectionChanged;
            if (ncc != null)
                ncc.CollectionChanged -= handler;
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnCollectionChanged(e, _source, _destination,
                source => _synchronizer.RegisterSource(source), source => _synchronizer.DeregisterSource(source),
                source => _synchronizer.GetDestination(source), destination => _synchronizer.GetSource(destination),
                OnDestinationCollectionChanged
            );
        }
        private void OnDestinationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnCollectionChanged(e, _destination, _source,
                destination => _synchronizer.RegisterDestination(destination), destination => _synchronizer.DeregisterDestination(destination),
                destination => _synchronizer.GetSource(destination), source => _synchronizer.GetDestination(source),
                OnSourceCollectionChanged
            );
        }

        private void OnCollectionChanged<TSrc, TDest>(NotifyCollectionChangedEventArgs e,
            IList<TSrc> source, IList<TDest> destination,
            Func<TSrc, TDest> registerSync, Func<TSrc, TDest> deregisterSync,
            Func<TSrc, TDest> getDestItem, Func<TDest, TSrc> getSrcItem, NotifyCollectionChangedEventHandler handler) {

            if (_disposed)
                throw new ObjectDisposedException(string.Empty);

            lock (_syncLock) {
                try {
                    TryRemove(destination, handler);

                    OnCollectionChanged(e, source, destination, registerSync, deregisterSync, getDestItem, getSrcItem, _synchronizer.Clear);
                } finally {
                    TryAdd(destination, handler);

                }
            }
        }

        private static void OnCollectionChanged<TSrc, TDest>(NotifyCollectionChangedEventArgs e,
            IList<TSrc> source, IList<TDest> destination,
            Func<TSrc, TDest> registerSync, Func<TSrc, TDest> deregisterSync,
            Func<TSrc, TDest> getDestItem, Func<TDest, TSrc> getSrcItem,
            Action clearSync) {

            List<TSrc> newListSource;
            List<TDest> newListDestination;

            Action<Action> updateDestination = action => {
                var transactive = destination as TransactiveObservableCollection<TDest>;
                if (transactive != null)
                    using (transactive.Transaction())
                        action();
                else
                    action();
            };

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    newListSource = e.NewItems.Cast<TSrc>().ToList();

                    newListDestination = new List<TDest>();
                    foreach (var item in newListSource)
                        newListDestination.Add(registerSync(item));

                    if (newListDestination.Count > 0) {
                        if (e.NewStartingIndex == -1) {
                            if (newListDestination.Count == 1)
                                destination.Add(newListDestination[0]);

                            else {
                                updateDestination(() => {
                                    foreach (var item in newListDestination)
                                        destination.Add(item);
                                });
                            }
                        } else {
                            var index = e.NewStartingIndex;

                            if (newListDestination.Count == 1)
                                destination.Insert(index, newListDestination[0]);

                            else {
                                updateDestination(() => {
                                    foreach (var item in newListDestination) {
                                        destination.Insert(index, item);
                                        index++;
                                    }
                                });
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    newListSource = e.OldItems.Cast<TSrc>().ToList();

                    newListDestination = new List<TDest>();
                    foreach (var item in newListSource)
                        newListDestination.Add(deregisterSync(item));

                    if (newListDestination.Count > 0)
                        if (newListDestination.Count == 1)
                            destination.Remove(newListDestination[0]);

                        else {
                            updateDestination(() => {
                                foreach (var item in newListDestination)
                                    destination.Remove(item);
                            });
                        }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems.Count != 1)
                        throw new NotImplementedException(@"NotifyCollectionChangedAction.Replace && e.NewItems.Count != 1");

                    var oldSrc = e.OldItems.Cast<TSrc>().Single();
                    var newSrc = e.NewItems.Cast<TSrc>().Single();

                    var oldDest = deregisterSync(oldSrc);
                    var newDest = registerSync(newSrc);

                    var indexSrc = source.IndexOf(newSrc);
                    var indexDest = destination.IndexOf(oldDest);

                    if (indexSrc != indexDest)
                        throw new InvalidOperationException();

                    destination[indexDest] = newDest;

                    break;

                case NotifyCollectionChangedAction.Reset:

                    var sourceComparer = EqualityComparer<TSrc>.Default;

                    // ложное срабатывание
                    // здесь нельзя вызывать getDestItem, потому что в source уже новые элементы,а в _mapping еще старые. 
                    // в итогде mapping.Find выдает null, а за ним - NullReferenceException
                    // getDestItem - можно вызывать для гарантированно неизменных элементов
                    // getSrcItem - напротив, вызывать можно на всех, ибо destination пока содержит старые элементы
                    // if (source.Count == destination.Count && !destination.SkipWhile((d, i) => d.Equals(getDestItem(source[i]))).Any()) {
                    if (source.Count == destination.Count && source.SequenceEqual(destination.Select(getSrcItem), sourceComparer)) {
                        // nop

                    } else if (source.Count == 0) {
                        destination.Clear();
                        clearSync();

                        // addRange
                    } else if (source.Count > destination.Count && !destination.SkipWhile((d, i) => sourceComparer.Equals(source[i], getSrcItem(d))).Any()) {
                        var items = source.Skip(destination.Count).ToList();

                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items), source, destination, registerSync, deregisterSync, getDestItem, getSrcItem, clearSync);

                        // removeRange
                    } else if (source.Count < destination.Count && !source.Except(destination.Select(getSrcItem), sourceComparer).Any()) {
                        var items = destination.Except(source.Select(getDestItem)).Select(getSrcItem).ToList();

                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items), source, destination, registerSync, deregisterSync, getDestItem, getSrcItem, clearSync);

                    } else {
                        // accurate
                        //Synchronize(source, destination);

                        // rough
                        clearSync();

                        updateDestination(() => {
                            destination.Clear();
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)source), source, destination, registerSync, deregisterSync, getDestItem, getSrcItem, clearSync);
                        });
                    }
                    break;

                default:
                    throw new NotImplementedException(e.Action.ToString());
            }
        }

        public IList<TSource> Source { get { return _source; } }
        public IList<TDestination> Destination { get { return _destination; } }

        public ObjectSynchronizer<TSource, TDestination, TBinder> Synchronizer {
            get {
                if (_disposed)
                    throw new ObjectDisposedException(string.Empty);

                return _synchronizer;
            }
        }

        //private void Synchronize(IList<TSource> source, IList<TDestination> destination) {
        //    // завязано на _synchronizer

        //    var a1 = source;
        //    var a2 = destination;

        //    // 1. Удаление из второй коллекции все элементы, которых нет в первой
        //    //a2.RemoveAll(value => !a1.Contains(value));
        //    var rem = new List<TDestination>();
        //    foreach (var d in a2)
        //        if (!a1.Contains(_synchronizer.DeregisterDestination(d)))
        //            rem.Add(d);
        //    foreach (var d in rem)
        //        a2.Remove(d);

        //    //2. Сортировка второй коллекции в соответствии с первой
        //    // -> -> -> a2.Sort((i1, i2) => a1.IndexOf(i1).CompareTo(a1.IndexOf(i2))); <- <- <-

        //    //3. Пробежка по первой коллекции с добавлением пропущенных элементов во вторую
        //    for (int i = 0; i < a1.Count && i < a2.Count; i++) {
        //        var s = _synchronizer.GetSource(a2[i]);
        //        if (!s.Equals(a1[i]))
        //            a2.Insert(i, _synchronizer.RegisterSource(a1[i]));
        //    }

        //    //4. Добавление оставшихся элементов первой коллекции в конец второй
        //    for (int i = a2.Count; i < a1.Count; i++)
        //        a2.Add(_synchronizer.RegisterSource(a1[i]));

        //    //проверка на вшивость
        //    for (int i = 0; i < a1.Count; i++)
        //        if (!a1[i].Equals(_synchronizer.GetSource(a2[i])))
        //            throw new Exception();
        //}
    }
}
