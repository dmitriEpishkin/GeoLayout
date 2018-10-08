using System;
using System.Collections.Generic;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain {
    public static class ProfileFactory {

        public static List<Waypoint> CreateWithFixedStep(List<Waypoint> keyPoints, double stepInMeter, Func<int, string> namingRule) {

            var p = new List<Waypoint>();

            double r = 0.0;

            int counter = 0;

            for (int i = 1; i < keyPoints.Count; i++) {

                var p1 = keyPoints[i - 1];
                var p2 = keyPoints[i];

                var xy1 = WgsUtmConverter.LatLonToUTMXY(p1.Location.Latitude, p1.Location.Longitude, 0);
                var xy2 = WgsUtmConverter.LatLonToUTMXY(p2.Location.Latitude, p2.Location.Longitude, xy1.Item1);

                var dx = xy2.Item2 - xy1.Item2;
                var dy = xy2.Item3 - xy1.Item3;

                var a = Math.Atan(Math.Abs(dy / dx));

                var len = Math.Sqrt(dx * dx + dy * dy);

                var pos = r;

                while (pos < len) {

                    var stepX = dx > 0 ? Math.Cos(a) * pos : -Math.Cos(a) * pos;
                    var stepY = dy > 0 ? Math.Sin(a) * pos : -Math.Sin(a) * pos;

                    var latLon = WgsUtmConverter.UTMXYToLatLon(xy1.Item2 + stepX, xy1.Item3 + stepY, xy1.Item1, p1.Location.Latitude < 0);

                    p.Add(new Waypoint(namingRule(counter), new GeoLocation(latLon.Item1, latLon.Item2, 0.0)));

                    counter++;
                    pos += stepInMeter;
                }

                r = pos - len;

            }

            return p;
        }

        public static List<Waypoint> CreateWithFixedStep(List<Waypoint> keyPoints, double stepInMeter) {
            return CreateWithFixedStep(keyPoints, stepInMeter, i => i.ToString());
        }

        public static List<Waypoint> CreateWithFixedStep(Waypoint start, double length, double stepInMeter, double angleRad) {
            return CreateWithFixedStep(start, length, stepInMeter, angleRad, i => i.ToString());
        }

        public static List<Waypoint> CreateWithFixedStep(Waypoint start, double length, double stepInMeter, double angleRad, Func<int, string> namingRule) {

            angleRad = Math.PI / 2 - angleRad;

            var p = new List<Waypoint>();

            var counter = 0;
            var pos = 0.0;

            var xy = WgsUtmConverter.LatLonToUTMXY(start.Location.Latitude, start.Location.Longitude, 0);

            while (pos < length) {

                var stepX = Math.Cos(angleRad) * pos;
                var stepY = Math.Sin(angleRad) * pos;

                var latLon = WgsUtmConverter.UTMXYToLatLon(xy.Item2 + stepX, xy.Item3 + stepY, xy.Item1, start.Location.Latitude < 0);

                p.Add(new Waypoint(namingRule(counter), new GeoLocation(latLon.Item1, latLon.Item2, 0.0)));

                counter++;
                pos += stepInMeter;
            }
            
            return p;
        }

        public static List<Waypoint> CreateWithFixedCount(List<Waypoint> keyPoints, int count) {
            if (count <= 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            var totalDist = 0.0;
            for (int i = 1; i < keyPoints.Count; i++) {
                totalDist += keyPoints[i].Location.DistanceTo(keyPoints[i - 1].Location);
            }

            var step = totalDist / (count - 1);

            var res = CreateWithFixedStep(keyPoints, step);
            
            if (res.Count == count - 1)
                res.Add(keyPoints[keyPoints.Count - 1]);

            if (res.Count > count)
                res.RemoveRange(count, res.Count - count);

            if (res.Count != count)
                throw new ArgumentOutOfRangeException();

            return res;
        }
    }
}
