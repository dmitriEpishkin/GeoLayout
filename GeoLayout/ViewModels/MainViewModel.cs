
using System.Waf.Foundation;
using GeoLayout.Domain.Data;
using GeoLayout.Services;

namespace GeoLayout {
    public class MainViewModel : Model {

        public MainViewModel(WaypointsService waypointsService, GroupsService groupsService, GeoLayoutBuildingService layoutBuildingService) {

            WaypointsService = waypointsService;
            GroupsService = groupsService;
            GeoLayoutBuildingService = layoutBuildingService;


            //var inputFile = @"C:\Users\Епишкин Дмитрий\Desktop\ВЛУ\keyPoints2.gpx";
            //var importer = new GpxImporter();
            //var keyPoints = importer.ImportWaypoints(inputFile);

            var keyPoints = new[] {
                new Waypoint("1", new GeoLocation(60, 60, 0)),
                new Waypoint("2", new GeoLocation(60.2, 60.1, 1)),
                new Waypoint("1", new GeoLocation(59.8, 60.2, 1.5)),
                new Waypoint("2", new GeoLocation(60.2, 59.9, 2))
            };

            foreach (var p in keyPoints) {
                waypointsService.Waypoints.Add(p);
            }

        }
        
        public WaypointsService WaypointsService { get; }
        public GroupsService GroupsService { get; }
        public GeoLayoutBuildingService GeoLayoutBuildingService { get; }
    }
}
