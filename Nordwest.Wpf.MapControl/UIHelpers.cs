using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace Nordwest.Wpf.Controls {
    public static class UIHelpers {

        public static void CalculateArrow(Line line, Polygon arrow) {

            var x1 = line.X1;
            var y1 = line.Y1;
            var x2 = line.X2;
            var y2 = line.Y2;

            var dx = x2 - x1;
            var dy = y2 - y1;

            double angle;
            if (dx == 0) {
                if (dy > 0)
                    angle = Math.PI;
                else
                    angle = 0;
            }
            else {
                var a = dy / dx;
                angle = (dx > 0 ? Math.PI : 0) + Math.Atan(a);
            }

            var angle1 = angle + Math.PI / 8;
            var angle2 = angle - Math.PI / 8;

            var size = 20;

            var ax = x2 + size * Math.Cos(angle1);
            var ay = y2 + size * Math.Sin(angle1);

            var bx = x2 + size * Math.Cos(angle2);
            var by = y2 + size * Math.Sin(angle2);

            arrow.Points.Clear();
            
            arrow.Points.Add(new Point(ax, ay));
            arrow.Points.Add(new Point(x2, y2));
            arrow.Points.Add(new Point(bx, by));

        }

    }
}
