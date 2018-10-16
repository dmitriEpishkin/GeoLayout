using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain.Data;
using Nordwest.Collections;

namespace GeoLayout {
    public class WaypointsService : Model {

        private readonly List<Waypoint> _points = new List<Waypoint>();

        private readonly List<Waypoint> _removeVisible = new List<Waypoint>();
        private readonly List<Waypoint> _addVisible = new List<Waypoint>();
        
        public WaypointsService() {
            
            RemoveCommand = new DelegateCommand(obj => Waypoints.Remove((Waypoint)obj));

            Waypoints.CollectionChanged += Waypoints_CollectionChanged;

        }

        private void Waypoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                        AddWaypoints(e.NewItems.OfType<Waypoint>().ToList());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in e.OldItems.OfType<Waypoint>()) 
                        RemoveWaypoint(p);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    AddWaypoints(e.NewItems.OfType<Waypoint>().ToList());
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

        private void AddWaypoints(List<Waypoint> waypoints) {

            var visibleWaypoints = waypoints.FindAll(waypoint => waypoint.IsVisible);
            VisibleWaypoints.AddRange(visibleWaypoints);

            waypoints.ForEach(waypoint => waypoint.PropertyChanged += P_PropertyChanged);

            _points.AddRange(waypoints);
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
                if (p.IsVisible) {
                    VisibleWaypoints.Add(p);
                }
                else {
                    VisibleWaypoints.Remove(p);
                }
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

        public ObservableRangeCollection<Waypoint> Waypoints { get; } = new ObservableRangeCollection<Waypoint>();
        public ObservableRangeCollection<Waypoint> VisibleWaypoints { get; } = new ObservableRangeCollection<Waypoint>();
        public ObservableRangeCollection<Waypoint> SelectedWaypoints { get; } = new ObservableRangeCollection<Waypoint>();

        public DelegateCommand RemoveCommand { get; }

    }
}
