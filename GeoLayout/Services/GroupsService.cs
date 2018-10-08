
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Waf.Foundation;
using GeoLayout.Domain.Data;

namespace GeoLayout.Services {
    public class GroupsService : Model {

        private readonly WaypointsService _waypointsService;

        private readonly Group _defaultGroup; 

        private readonly List<Group> _subscribed = new List<Group>();

        public GroupsService(WaypointsService waypointsService) {

            _waypointsService = waypointsService;

            _defaultGroup = new Group("Не сгруппированные");

            _waypointsService.Waypoints.CollectionChanged += Waypoints_CollectionChanged;
            _defaultGroup.Waypoints.CollectionChanged += DefaultWaypoints_CollectionChanged;
            Groups.CollectionChanged += Groups_CollectionChanged;
        }

        private void Groups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var g in e.NewItems.OfType<Group>())
                        Subscribe(g);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var g in e.OldItems.OfType<Group>())
                        Unsubscribe(g);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var g in e.NewItems.OfType<Group>())
                        Subscribe(g);
                    foreach (var g in e.OldItems.OfType<Group>())
                        Unsubscribe(g);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _subscribed.ToList().ForEach(Unsubscribe);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }    
        }

        private void Subscribe(Group group) {

            if (group == _defaultGroup)
                return;
            
            foreach (var defaultGroupWaypoint in _defaultGroup.Waypoints.ToList()) {
                CheckWaypoint(defaultGroupWaypoint.Waypoint);
            }

            group.Waypoints.CollectionChanged += WaypointsLocal_CollectionChanged;
            _subscribed.Add(group);

            foreach (var g in group.Children)
                Subscribe(g);
        }

        private void WaypointsLocal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                foreach (var p in e.NewItems.OfType<WaypointGroupWrapper>())
                    CheckWaypoint(p.Waypoint);
            if (e.OldItems != null)
                foreach (var p in e.OldItems.OfType<WaypointGroupWrapper>())
                    CheckWaypoint(p.Waypoint);
        }

        private void Unsubscribe(Group group) {
            if (group == _defaultGroup)
                return;

            group.Waypoints.CollectionChanged -= WaypointsLocal_CollectionChanged;
            _subscribed.Remove(group);

            foreach (var g in group.Children)
                Unsubscribe(g);
        }

        private void DefaultWaypoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
#warning надо исправить!!!
            try {
                if (_defaultGroup.Waypoints.Count == 0)
                    Groups.Remove(_defaultGroup);
                else if (_defaultGroup.Waypoints.Count > 0 && !Groups.Contains(_defaultGroup))
                    Groups.Insert(0, _defaultGroup);
            }
            catch { }
        }

        private void Waypoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch(e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var wpt in e.NewItems.OfType<Waypoint>())
                        CheckWaypoint(wpt);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in e.OldItems.OfType<Waypoint>())
                        RemoveWaypoint(p, Groups);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var wpt in e.NewItems.OfType<Waypoint>())
                        CheckWaypoint(wpt);
                    foreach (var p in e.OldItems.OfType<Waypoint>())
                        RemoveWaypoint(p, Groups);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Groups.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckWaypoint(Waypoint wpt) {

            if (Groups
                .Where(g => g != _defaultGroup)
                .Any(g => g.CheckWaypoint(wpt))) {

                var p = _defaultGroup.Waypoints.FirstOrDefault(w => w.Waypoint == wpt);
                if (p != null)
                    _defaultGroup.Waypoints.Remove(p);
            }

            else if (_defaultGroup.Waypoints.All(p => p.Waypoint != wpt))
                _defaultGroup.Waypoints.Add(new WaypointGroupWrapper(_defaultGroup, wpt));
        }

        private void RemoveWaypoint(Waypoint p, IEnumerable<Group> groups) {
            foreach (var g in groups.ToArray()) {
                var pp = g.Waypoints.FirstOrDefault(w => w.Waypoint == p);
                if (pp != null)
                    g.Waypoints.Remove(pp);

                RemoveWaypoint(p, g.Children);
            }

            var ppp = _defaultGroup.Waypoints.FirstOrDefault(w => w.Waypoint == p);
            _defaultGroup.Waypoints.Remove(ppp);

        }

        public ObservableCollection<Group> Groups { get; } = new ObservableCollection<Group>();

    }
}