using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;
using GeoLayout.Domain;
using GeoLayout.Domain.Data;
using GeoLayout.GeoLayoutTools;

namespace GeoLayout.Services {
    public class GeoLayoutService : Model {

        private readonly WaypointsService _waypointsService;

        public GeoLayoutService(WaypointsService waypointsService, GroupsService groupsService) {

            _waypointsService = waypointsService;

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
