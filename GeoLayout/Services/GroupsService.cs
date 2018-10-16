
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Waf.Foundation;
using GeoLayout.Domain.Data;
using Nordwest.Collections;

namespace GeoLayout.Services {
    public class GroupsService : Model {

        private readonly WaypointsService _waypointsService;

        private readonly Group _defaultGroup; 

        private readonly List<Group> _subscribed = new List<Group>();

        public GroupsService(WaypointsService waypointsService) {

            _waypointsService = waypointsService;

            _defaultGroup = new Group("Не сгруппированные");

            _waypointsService.Waypoints.CollectionChanged += Waypoints_CollectionChanged;
            _defaultGroup.Children.CollectionChanged += DefaultWaypoints_CollectionChanged;
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
            UpdateShapesCollection();
        }

        private void Subscribe(Group group) {

            if (group == _defaultGroup)
                return;
            
            foreach (var defaultGroupWaypoint in _defaultGroup.Children.OfType<Waypoint>().ToList()) {
                CheckWaypoint(defaultGroupWaypoint);
            }

            group.Children.CollectionChanged += GroupChildren_CollectionChanged;
            _subscribed.Add(group);

            foreach (var g in group.Children.OfType<Group>())
                Subscribe(g);
        }

        private void GroupChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                foreach (var p in e.NewItems.OfType<Waypoint>())
                    CheckWaypoint(p);
            if (e.OldItems != null)
                foreach (var p in e.OldItems.OfType<Waypoint>())
                    CheckWaypoint(p);
            UpdateShapesCollection();
        }

        private void Unsubscribe(Group group) {
            if (group == _defaultGroup)
                return;

            group.Children.CollectionChanged -= GroupChildren_CollectionChanged;
            _subscribed.Remove(group);

            foreach (var g in group.Children.OfType<Group>())
                Unsubscribe(g);
        }

        private void DefaultWaypoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            #warning надо исправить!!!
            try {
                if (_defaultGroup.Children.Count == 0)
                    Groups.Remove(_defaultGroup);
                else if (_defaultGroup.Children.Count > 0 && !Groups.Contains(_defaultGroup))
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
            RemoveEmptyGroups();
        }

        private void CheckWaypoint(Waypoint wpt) {

            if (Groups.OfType<Group>().Where(g => g != _defaultGroup).Any(g => g.HasWaypoint(wpt))) {
                    _defaultGroup.Children.Remove(wpt);
            }

            else if (!_defaultGroup.Children.Contains(wpt))
                _defaultGroup.Children.Add(wpt);

        }

        private void RemoveWaypoint(Waypoint p, IEnumerable<IGroupingNode> groups) {
            
            _defaultGroup.Children.Remove(p);

            if (groups == null)
                return;

            foreach (var g in groups.ToArray()) {
                g.Children?.Remove(p);
                RemoveWaypoint(p, g.Children);
            }
        }

        private void RemoveEmptyGroups() {
            var toRemove = Groups.OfType<Group>().Where(child => child.Children.Count == 0).ToList();
            Groups.RemoveRange(toRemove);
            foreach (var group in Groups.OfType<Group>()) {
                RemoveEmptyGroups(group);
            }
        }

        private void RemoveEmptyGroups(Group group) {
            var toRemove = group.Children.OfType<Group>().Where(child => child.Children.Count == 0).ToList();
            if (toRemove.Count > 0) {
                group.Children.RemoveRange(toRemove);
            }

            foreach (var childGroup in group.Children.OfType<Group>()) {
                RemoveEmptyGroups(childGroup);
            }

        }

        private void UpdateShapesCollection() {
            Shapes.Clear();
            FindAllShapes(Groups);
        }

        private void FindAllShapes(IEnumerable<IGroupingNode> groups) {

            if (groups == null)
                return;

            foreach (var group in groups) {
                if (group is IShapeProvider shape)
                    Shapes.Add(shape);

                FindAllShapes(group.Children);
            }

        }

        public ObservableRangeCollection<IGroupingNode> Groups { get; } = new ObservableRangeCollection<IGroupingNode>();

        public ObservableCollection<IShapeProvider> Shapes { get; } = new ObservableCollection<IShapeProvider>();

    }
}