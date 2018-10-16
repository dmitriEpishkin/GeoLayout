
using System.Collections.Generic;
using System.Linq;
using System.Waf.Applications;
using GeoLayout.Domain.Data;
using GeoLayout.Domain.LayoutTemplates;
using GeoLayout.Services;

namespace GeoLayout.GeoLayoutTools {
    public class TdmLayoutBuilder : IGeoLayoutTool {

        private readonly WaypointsService _waypointsService;
        private readonly GroupsService _groupsService;

        public TdmLayoutBuilder(WaypointsService waypointService, GroupsService groupsService) {

            _waypointsService = waypointService;
            _groupsService = groupsService;

            TdmLayoutTemplate = new TdmLayoutTemplate();
            ApplyCommand = new DelegateCommand(Apply);
        }

        public void Apply() {

            var points = new List<Waypoint>();
            var group = new Group("TDM");

            var selected = _waypointsService.SelectedWaypoints.ToList();

            foreach (var waypoint in selected) {
                var tdmGroup = new TdmGroup(waypoint.Name);
                var layout = TdmLayoutTemplate.ApplyTo(waypoint);
                points.AddRange(layout);
                tdmGroup.Children.AddRange(layout);
                group.Children.Add(tdmGroup);
            }

            _waypointsService.Waypoints.RemoveRange(selected);
            _waypointsService.Waypoints.AddRange(points);
            
            _groupsService.Groups.Add(group);
        }
        
        public string Name => "Применить ЗСБ шаблон";

        public TdmLayoutTemplate TdmLayoutTemplate { get; }

        public DelegateCommand ApplyCommand { get; }

    }
}
