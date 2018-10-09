
using System;
using System.Collections.Generic;
using System.Windows;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain.Helpers {
    public static class GridHelper {
        
        public static bool IsInsidePolygon(this GeoLocationXY p, List<GeoLocationXY> poly) {

            // отсюда
            // https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
            
            bool inside = false;

            if (poly.Count < 3) {
                throw new ArgumentException("poly.Count < 3", nameof(poly));
            }

            var oldPoint = new Point(
                poly[poly.Count - 1].X, poly[poly.Count - 1].Y);

            for (int i = 0; i < poly.Count; i++) {
                var newPoint = new Point(poly[i].X, poly[i].Y);

                Point p2;
                Point p1;
                if (newPoint.X > oldPoint.X) {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X)) {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
        
    }
}