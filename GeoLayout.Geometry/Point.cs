using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLayout.Domain.Data {
    public class Point {
        
        public Point(double x, double y) {
            X = x;
            Y = y;
        }

        public static Point operator + (Point a, Point b) {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
        
        public double X { get; }
        public double Y { get; }
    }
}
