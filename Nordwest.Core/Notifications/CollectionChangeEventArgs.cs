using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Nordwest;

namespace Nordwest.Notifications {
    public class CollectionChangeEventArgs<T> : NotifyCollectionChangedEventArgs, ICollectionChangeEventArgs<T> {

        private ImmutableList<T> _newItems;
        private ImmutableList<T> _oldItems;

        #region base .ctors
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action) : base(action) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, T changedItem) : base(action, changedItem) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, T changedItem, int index) : base(action, changedItem, index) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, IList<T> changedItems) : base(action, (IList)changedItems) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, IList<T> changedItems, int startingIndex) : base(action, (IList)changedItems, startingIndex) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, T newItem, T oldItem) : base(action, newItem, oldItem) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, T newItem, T oldItem, int index) : base(action, newItem, oldItem, index) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, IList<T> newItems, IList<T> oldItems) : base(action, (IList)newItems, (IList)oldItems) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, IList<T> newItems, IList<T> oldItems, int startingIndex) : base(action, (IList)newItems, (IList)oldItems, startingIndex) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, T changedItem, int index, int oldIndex) : base(action, changedItem, index, oldIndex) { }
        public CollectionChangeEventArgs(NotifyCollectionChangedAction action, IList<T> changedItems, int index, int oldIndex) : base(action, (IList)changedItems, index, oldIndex) { }
        #endregion

        public new IReadOnlyList<T> NewItems { get { return _newItems ?? (_newItems = base.NewItems == null ? null : ImmutableList.CreateRange(base.NewItems.Cast<T>())); } }
        public new IReadOnlyList<T> OldItems { get { return _oldItems ?? (_oldItems = base.OldItems == null ? null : ImmutableList.CreateRange(base.OldItems.Cast<T>())); } }

        public static CollectionChangeEventArgs<T> Wrap(NotifyCollectionChangedEventArgs e) {
            Requires.NotNull(e);

            var newItems = e.NewItems == null ? null : e.NewItems.Cast<T>().ToList();
            var oldItems = e.OldItems == null ? null : e.OldItems.Cast<T>().ToList();

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    if (newItems != null && oldItems != null)
                        throw new NotSupportedException(); // завязка на реализации NPCEA
                    if (newItems != null)
                        return new CollectionChangeEventArgs<T>(e.Action, newItems, e.NewStartingIndex);
                    if (oldItems != null)
                        return new CollectionChangeEventArgs<T>(e.Action, oldItems, e.OldStartingIndex);
                    return new CollectionChangeEventArgs<T>(e.Action); // RESET

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewStartingIndex != e.OldStartingIndex || newItems == null || oldItems == null)
                        throw new NotSupportedException(); // завязка на реализации NPCEA
                    return new CollectionChangeEventArgs<T>(e.Action, newItems, oldItems, e.NewStartingIndex);

                case NotifyCollectionChangedAction.Move:
                    if (newItems == null || oldItems == null || !ReferenceEquals(e.NewItems, e.OldItems))
                        throw new NotSupportedException(); // завязка на реализации NPCEA
                    return new CollectionChangeEventArgs<T>(e.Action, newItems, e.NewStartingIndex, e.OldStartingIndex);

                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
