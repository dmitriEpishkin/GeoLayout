
using System.Collections.ObjectModel;
using System.Linq;
using GeoLayout.Domain;
using GeoLayout.Domain.Data;
using GeoLayout.Domain.Helpers;

namespace GeoLayout.GeoLayoutTools {
    public class CropModifier : IGeoLayoutTool {

        private readonly WaypointsService _waypointsService;

        public CropModifier(WaypointsService waypointsService) {
            _waypointsService = waypointsService;
        }

        public void Apply() {

            var vertices = PolygonVertices.Select(v => WgsUtmConverter.LatLonToUTMXY(v.Location, 0)).ToList();
            var needToRemove = _waypointsService.Waypoints.Where(wpt => !WgsUtmConverter.LatLonToUTMXY(wpt.Location, 0).IsInsidePolygon(vertices)).ToList();

            foreach (var wpt in needToRemove) {
                _waypointsService.Waypoints.Remove(wpt);
            }

        }

        public string Name => "Обрезать по многоугольнику";

        public ObservableCollection<Waypoint> PolygonVertices { get; } = new ObservableCollection<Waypoint>();
    }
}
