
using System.Waf.Foundation;
using GeoLayout.GeoLayoutTools;

namespace GeoLayout.Services {
    public class GeoLayoutBuildingService : Model {
        
        public GeoLayoutBuildingService(WaypointsService waypointsService, GroupsService groupsService) {

            WaypointBuilder = new SingleWaypointBuilder(waypointsService, groupsService);
            ProfileBuilder = new ProfileBuilder(waypointsService, groupsService);
            GridBuilder = new GridBuilder(waypointsService, groupsService);

            ShiftModifier = new ShiftModifier();
            RotateModifier = new RotateModifier();
        }
        
        public SingleWaypointBuilder WaypointBuilder { get; }
        public ProfileBuilder ProfileBuilder { get; }
        public GridBuilder GridBuilder { get; }

        public ShiftModifier ShiftModifier { get; }
        public RotateModifier RotateModifier { get; }
        
    }
}
