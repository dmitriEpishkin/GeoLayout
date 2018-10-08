
using System;
using GMap.NET;

namespace Nordwest.Wpf.Controls {
    public static class DistanceHelper {

        /* Параметры эллипсоида (WGS84) */
        const double a = 6378137.0;
        const double b = 6356752.314;
        const double UTMScaleFactor = 0.9996;

        /// <summary>
        /// Расстояние между точками в метрах
        /// </summary>
        public static double GetDistance(PointLatLng p1, PointLatLng p2) {
            var R = 6371; // радиус Земли в км
            var dLat = (p2.Lat - p1.Lat) * Math.PI / 180;
            var dLon = (p2.Lng - p1.Lng) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(p1.Lat * Math.PI / 180) * Math.Cos(p2.Lat * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d * 1000;
        }

        public static double GetDistanceWgs(PointLatLng p1, PointLatLng p2) {
            return GetPoint(p1).DistanceTo(GetPoint(p2));
        }

        private static Point3D GetPoint(PointLatLng p) {
            var h = 0;
            var phi = p.Lat * Math.PI / 180;
            var lambda = p.Lng * Math.PI / 180;

            var sinPhi = Math.Sin(phi);
            var cosPhi = Math.Cos(phi);
            var sinLambda = Math.Sin(lambda);
            var cosLambda = Math.Cos(lambda);

            var ab = (a * a - b * b) / a / a;

            var aSqrt = a / Math.Sqrt(1 - ab * sinPhi * sinPhi);

            var x = (aSqrt + h) * cosPhi * cosLambda;
            var y = (aSqrt + h) * cosPhi * sinLambda;
            var z = (aSqrt * (1 - ab) + h) * sinPhi;

            return new Point3D(x, y, z);
        }
        
        private class Point3D {

            public Point3D(double x, double y, double z) {
                X = x;
                Y = y;
                Z = z;
            }

            public double DistanceTo(Point3D p) {
                var dx = X - p.X;
                var dy = Y - p.Y;
                var dz = Z - p.Z;
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }

            public double X { get; }
            public double Y { get; }
            public double Z { get; }
        }

    }
}
