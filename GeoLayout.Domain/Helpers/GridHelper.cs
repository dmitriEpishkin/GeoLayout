
using System.Collections.Generic;
using System.Windows;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.Helpers {
    public static class GridHelper {

        public static bool IsInside(this Waypoint testingPoint, List<Waypoint> waypoints) {
            return true;
        }

        public static GridFrame GetFrame(this List<Waypoint> waypoints) {
            var minLat = 90.0;
            var minLng = 180.0;
            var maxLat = -90.0;
            var maxLng = -180.0;

            foreach (var wpt in waypoints) {
                var l = wpt.Location;
                if (minLat > l.Latitude)
                    minLat = l.Latitude;
                if (minLng > l.Longitude)
                    minLng = l.Longitude;
                if (maxLat < l.Latitude)
                    maxLat = l.Latitude;
                if (maxLng < l.Longitude)
                    maxLng = l.Longitude;
            }

            return new GridFrame(new GeoLocation(minLat, minLng, 0), new GeoLocation(minLat, maxLng, 0), new GeoLocation(maxLat, minLng, 0));
        }

    }
}