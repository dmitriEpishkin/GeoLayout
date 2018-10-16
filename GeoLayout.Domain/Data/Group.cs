
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Waf.Foundation;
using Nordwest.Collections;

namespace GeoLayout.Domain.Data {
    public class Group : Model, IGroupingNode {

        private bool _isVisible = true;
        private bool _isSelected = false;

        private string _name;

        private readonly List<IGroupingNode> _subscribed = new List<IGroupingNode>();

        public Group(string name) {
            Name = name;
            
            Children.CollectionChanged += Children_CollectionChanged;
        }

        ~Group() {
            Children.CollectionChanged -= Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var node in e.NewItems.OfType<IGroupingNode>())
                        SubsribeToNode(node);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var node in e.OldItems.OfType<IGroupingNode>())
                        UnsubscribeFromNode(node);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var node in _subscribed)
                        node.PropertyChanged -= Node_PropertyChanged;
                    _subscribed.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateSelected();
            UpdateVisible();
        }

        private void SubsribeToNode(IGroupingNode node) {
            _subscribed.Add(node);
            node.PropertyChanged += Node_PropertyChanged;
        }
        
        private void UnsubscribeFromNode(IGroupingNode node) {
            _subscribed.Remove(node);
            node.PropertyChanged -= Node_PropertyChanged;
        }
        
        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(IsVisible))
                UpdateVisible();
            if (e.PropertyName == nameof(IsSelected))
                UpdateSelected();
        }

        public bool HasWaypoint(Waypoint wpt) {
            return Children.Any(child => child == wpt || (child is Group group && group.HasWaypoint(wpt)));
        }

        public void UpdateSelected() {
            _isSelected = Children.All(w => w.IsSelected);
            RaisePropertyChanged(nameof(IsSelected));
        }

        public void UpdateVisible() {
            _isVisible = Children.All(w => w.IsVisible);
            RaisePropertyChanged(nameof(IsVisible));
        }

        public bool IsSelected {
            get => _isSelected;
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    foreach (var group in Children) {
                        group.IsSelected = value;
                    }
                    RaisePropertyChanged(nameof(IsSelected));

                    IsVisible |= IsSelected;
                }
            }
        }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    foreach (var group in Children) {
                        group.IsVisible = value;
                    }
                    RaisePropertyChanged(nameof(IsVisible));

                    IsSelected &= IsVisible;
                }
            }
        }

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public ObservableRangeCollection<IGroupingNode> Children { get; } = new ObservableRangeCollection<IGroupingNode>();
    }
}