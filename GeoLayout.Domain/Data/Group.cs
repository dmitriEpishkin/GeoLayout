
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Waf.Foundation;

namespace GeoLayout.Domain.Data {
    public class Group : Model {

        private bool _isVisible = true;
        private bool _isSelected = false;

        private string _name;

        public Group(string name, Group parent = null) {
            Name = name;
            Parent = parent;

            Children.CollectionChanged += Children_CollectionChanged;
        }

        public void AddWaypoint(Waypoint wpt) {
            Waypoints.Add(new WaypointGroupWrapper(this, wpt));
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public bool CheckWaypoint(Waypoint wpt) {
            return Waypoints.Any(p => p.Waypoint == wpt) || Children.Any(g => g.CheckWaypoint(wpt));
        }

        public void UpdateSelected() {
            _isSelected = Waypoints.All(w => w.Waypoint.IsSelected);
            RaisePropertyChanged(nameof(IsSelected));
        }

        public void UpdateVisible() {
            _isVisible = Waypoints.All(w => w.Waypoint.IsVisible);
            RaisePropertyChanged(nameof(IsVisible));
        }

        public bool IsSelected {
            get => _isSelected;
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    foreach (var waypoint in Waypoints) {
                        waypoint.Waypoint.IsSelected = value;
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
                    foreach (var waypoint in Waypoints) {
                        waypoint.Waypoint.IsVisible = value;
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

        public ObservableCollection<WaypointGroupWrapper> Waypoints { get; } = new ObservableCollection<WaypointGroupWrapper>();

        public Group Parent { get; }
        public ObservableCollection<Group> Children { get; } = new ObservableCollection<Group>();
    }
}