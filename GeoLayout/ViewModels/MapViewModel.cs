using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Services;

namespace GeoLayout.ViewModels
{
    public class MapViewModel {

        public MapViewModel(WaypointsService waypointsService, GroupsService groupsService, GeoLayoutBuildingService geoLayoutBuildingService) {
            WaypointsService = waypointsService;
            GroupsService = groupsService;
            GeoLayoutBuildingService = geoLayoutBuildingService;
        }

        public WaypointsService WaypointsService { get; }
        public GroupsService GroupsService { get; }
        public GeoLayoutBuildingService GeoLayoutBuildingService { get; }

    }
}
