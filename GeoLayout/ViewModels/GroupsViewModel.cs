using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Foundation;
using GeoLayout.Domain.Data;
using GeoLayout.Services;

namespace GeoLayout.ViewModels {
    public class GroupsViewModel : Model {

        public GroupsViewModel(GroupsService groupsService, WaypointsService waypointsService) {
            WaypointsService = waypointsService;
            GroupsService = groupsService;
            RemoveCommand = new DelegateCommand(Remove);
        }

        private void Remove(object obj) {

            if (obj is Waypoint waypoint) {
                WaypointsService.RemoveCommand.Execute(waypoint);
                return;
            }

            var group = (Group) obj;
            ClearGroup(group);
            GroupsService.Groups.Remove(group);
            
        }

        private void ClearGroup(Group group) {

            foreach (var g in group.Children.OfType<Group>().ToList())
                ClearGroup(g);

            foreach (var p in group.Children.OfType<Waypoint>().ToList())
                WaypointsService.RemoveCommand.Execute(p);
        }

        public WaypointsService WaypointsService { get; }
        public GroupsService GroupsService { get; }

        public DelegateCommand RemoveCommand { get; }

    }
}
