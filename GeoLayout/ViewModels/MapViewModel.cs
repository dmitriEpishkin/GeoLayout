using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Services;

namespace GeoLayout.ViewModels
{
    public class MapViewModel {

        public MapViewModel(WaypointsService waypointsService, GeoLayoutBuildingService geoLayoutBuildingService) {
            WaypointsService = waypointsService;
            GeoLayoutBuildingService = geoLayoutBuildingService;
        }

        public WaypointsService WaypointsService { get; }
        public GeoLayoutBuildingService GeoLayoutBuildingService { get; }

    }
}
