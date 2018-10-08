using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Services;

namespace GeoLayout.ViewModels
{
    public class MapViewModel {

        public MapViewModel(WaypointsService waypointsService, GeoLayoutService builderService) {
            WaypointsService = waypointsService;
            BuilderService = builderService;
        }

        public WaypointsService WaypointsService { get; }
        public GeoLayoutService BuilderService { get; }

    }
}
