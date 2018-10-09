using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Waf.Applications;
using System.Waf.Foundation;
using GeoLayout.Domain.Data;

namespace GeoLayout {
    public class WaypointsService : Model {

        private readonly List<Waypoint> _points = new List<Waypoint>();

        public WaypointsService() {
            
            RemoveCommand = new DelegateCommand(obj => Waypoints.Remove((Waypoint)obj));

            Waypoints.CollectionChanged += Waypoints_CollectionChanged;

        }

        private void Waypoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var p in e.NewItems.OfType<Waypoint>()) 
                        AddWaypoint(p);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in e.OldItems.OfType<Waypoint>()) 
                        RemoveWaypoint(p);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var p in e.NewItems.OfType<Waypoint>())
                        AddWaypoint(p);
                    foreach (var p in e.OldItems.OfType<Waypoint>())
                        RemoveWaypoint(p);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    VisibleWaypoints.Clear();
                    _points.ForEach(p => p.PropertyChanged -= P_PropertyChanged);
                    _points.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddWaypoint(Waypoint p) {
            if (p.IsVisible)
                VisibleWaypoints.Add(p);
            p.PropertyChanged += P_PropertyChanged;
            _points.Add(p);
        }

        private void RemoveWaypoint(Waypoint p) {
            if (p.IsVisible)
                VisibleWaypoints.Remove(p);
            p.PropertyChanged -= P_PropertyChanged;
            _points.Remove(p);
        }

        private void P_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == @"IsVisible") {
                var p = (Waypoint) sender;
                if (p.IsVisible)
                    VisibleWaypoints.Add(p);
                else
                    VisibleWaypoints.Remove(p);
            }
            else if (e.PropertyName == @"IsSelected") {
                var p = (Waypoint)sender;
                if (p.IsSelected)
                    SelectedWaypoints.Add(p);
                else {
                    SelectedWaypoints.Remove(p);
                }
            }
        }

        public ObservableCollection<Waypoint> Waypoints { get; } = new ObservableCollection<Waypoint>();
        public ObservableCollection<Waypoint> VisibleWaypoints { get; } = new ObservableCollection<Waypoint>();
        public ObservableCollection<Waypoint> SelectedWaypoints { get; } = new ObservableCollection<Waypoint>();

        public DelegateCommand RemoveCommand { get; }

    }
}
