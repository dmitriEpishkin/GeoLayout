using System;

namespace GeoLayout.Domain.Data {
    public class GeoLocationXY {

        public GeoLocationXY(int zone, double x, double y, double elevation) {
            Zone = zone;
            X = x;
            Y = y;
            Elevation = elevation;
        }

        public GeoLocationXY Shift(double dx, double dy) {
            return new GeoLocationXY(Zone, X + dx, Y + dy, Elevation);
        }

        public int DistanceInMetersTo(GeoLocationXY point) {
            if (Zone != point.Zone)
                throw new Exception();

            var dx = X - point.X;
            var dy = Y - point.Y;
            var dz = Elevation - point.Elevation;

            return (int)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public XyInt ToXyInt() {
            return new XyInt((int)X, (int)Y);
        }

        public int Zone { get; }
        public double X { get; }
        public double Y { get; }
        public double Elevation { get; }
    }
}
