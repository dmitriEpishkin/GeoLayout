using System;

namespace GeoLayout.Domain.Data {
    public class GeoLocation {

        public static readonly GeoLocation Empty = new GeoLocation(double.NaN, double.NaN, double.NaN);

        public GeoLocation(double latitude, double longitude, double elevation) {
            Latitude = latitude;
            Longitude = longitude;
            Elevation = elevation;
        }

        /// <summary>
        /// Расстояние до указанной точки в метрах
        /// </summary>
        /// <param name="p1"></param>
        /// <returns></returns>
        public double DistanceTo(GeoLocation p1) {
            var p2 = this;
            var R = 6371; // радиус Земли в км
            var dLat = (p2.Latitude - p1.Latitude) * Math.PI / 180;
            var dLon = (p2.Longitude - p1.Longitude) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(p1.Latitude * Math.PI / 180) * Math.Cos(p2.Latitude * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d * 1000;
        }

        public double Latitude { get; }
        public double Longitude { get; }
        public double Elevation { get; }

        public bool EqualsTo(GeoLocation p) {
            return Latitude == p.Latitude &&
                   Longitude == p.Longitude &&
                   Elevation == p.Elevation;
        }
    }
}
