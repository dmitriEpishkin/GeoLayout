using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain.Data;
using GeoLayout.Services;
using GeoLayout.Services.Export;
using Nordwest.Collections;

namespace GeoLayout {
    [Export(typeof(IWaypointsService))]
    public class WaypointsService : Model, IWaypointsService {

        private readonly List<Waypoint> _points = new List<Waypoint>();

        private readonly ObservableRangeCollection<Waypoint> _visibleWaypoints = new ObservableRangeCollection<Waypoint>();
        private readonly ObservableRangeCollection<Waypoint> _selectedWaypoints = new ObservableRangeCollection<Waypoint>();

        [ImportingConstructor]
        public WaypointsService() {

            VisibleWaypoints = new ObservableCollectionReadOnlyWrapping<Waypoint>(_visibleWaypoints);
            SelectedWaypoints = new ObservableCollectionReadOnlyWrapping<Waypoint>(_selectedWaypoints);

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
                    _visibleWaypoints.Clear();
                    _selectedWaypoints.Clear();
                    _points.ForEach(p => p.PropertyChanged -= P_PropertyChanged);
                    _points.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddWaypoints(List<Waypoint> waypoints) {

            var visibleWaypoints = waypoints.FindAll(waypoint => waypoint.IsVisible);
            _visibleWaypoints.AddRange(visibleWaypoints);

            var selectedWaypoints = waypoints.FindAll(waypoint => waypoint.IsSelected);
            _selectedWaypoints.AddRange(selectedWaypoints);

            waypoints.ForEach(waypoint => waypoint.PropertyChanged += P_PropertyChanged);

            _points.AddRange(waypoints);
        }

        private void RemoveWaypoint(Waypoint p) {

            if (p.IsVisible)
                _visibleWaypoints.Remove(p);
            if (p.IsSelected)
                _selectedWaypoints.Remove(p);

            p.PropertyChanged -= P_PropertyChanged;
            _points.Remove(p);
        }
        
        private void P_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == @"IsVisible") {
                var p = (Waypoint) sender;
                if (p.IsVisible) {
                    _visibleWaypoints.Add(p);
                }
                else {
                    _visibleWaypoints.Remove(p);
                }
            }
            else if (e.PropertyName == @"IsSelected") {
                var p = (Waypoint)sender;
                if (p.IsSelected)
                    _selectedWaypoints.Add(p);
                else {
                    _selectedWaypoints.Remove(p);
                }
            }
        }

        public ObservableRangeCollection<Waypoint> Waypoints { get; } = new ObservableRangeCollection<Waypoint>();
        public ObservableCollectionReadOnlyWrapping<Waypoint> VisibleWaypoints { get; } 
        public ObservableCollectionReadOnlyWrapping<Waypoint> SelectedWaypoints { get; } 

        public DelegateCommand RemoveCommand { get; }

    }
}
