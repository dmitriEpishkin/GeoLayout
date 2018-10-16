
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

namespace GeoLayout.Domain.Data {
    public abstract class LayoutGroup : Group, IShapeProvider {
        
        private ReadOnlyCollection<GeoLocation> _layout;

        private readonly IDisposable _childrenChanged;

        protected LayoutGroup(string name) : base(name) {

            _childrenChanged = Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => (sender, args) => handler(args),
                    handler => Children.CollectionChanged += handler,
                    handler => Children.CollectionChanged -= handler)
                .Subscribe(e => Children_CollectionChanged(Children, e));

        }

        ~LayoutGroup() {
            _childrenChanged.Dispose();
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {

            if (e.NewItems != null && e.NewItems.OfType<Group>().Any())
                throw new ArgumentException();

            UpdateLayout();
        }

        protected abstract void UpdateLayout();

        public ReadOnlyCollection<GeoLocation> Shape {
            get => _layout;
            protected set {
                if (_layout != value) {
                    _layout = value;
                    RaisePropertyChanged(nameof(Shape));
                }
            }
        }
    }
}
