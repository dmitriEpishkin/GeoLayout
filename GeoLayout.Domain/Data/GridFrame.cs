
namespace GeoLayout.Domain.Data {
    public class GridFrame {

        public GridFrame(GeoLocation corner, GeoLocation p1, GeoLocation p2) {
            Corner = corner;
            P1 = p1;
            P2 = p2;
        }

        public double GetGridWidthMeters() {
            return Corner.DistanceTo(P1);
        }

        public double GetProfileLengthMeters() {
            return Corner.DistanceTo(P2);
        }

        public GeoLocation Corner { get; }
        public GeoLocation P1 { get; }
        public GeoLocation P2 { get; }

    }
}
