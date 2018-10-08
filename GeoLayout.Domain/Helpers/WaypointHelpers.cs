using System;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain {
    public static class WaypointHelpers {

        public static void Shift(Waypoint p, double distanceMeters, double angleRad) {
            var xy = WgsUtmConverter.LatLonToUTMXY(p.Location.Latitude, p.Location.Longitude, 0);

            var x = xy.Item2 + distanceMeters * Math.Cos(angleRad);
            var y = xy.Item3 + distanceMeters * Math.Sin(angleRad);

            var pos = WgsUtmConverter.UTMXYToLatLon(x, y, xy.Item1, p.Location.Latitude < 0);

            p.Location = new GeoLocation(pos.Item1, pos.Item2, p.Location.Elevation);

        }

    }
}
