
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Nordwest.Wpf.Controls {
    public static class RotationHelper {

        /// <summary>
        /// Поворот по часовой стрелке вокруг центральной точки
        /// </summary>
        public static Point Rotate(Point center, Point point, double angleRad) {

            var dx = point.X - center.X;
            var dy = point.Y - center.Y;

            var cos = Math.Cos(angleRad);
            var sin = Math.Sin(angleRad);

            var newX = dx * cos - dy * sin;
            var newY = dx * sin + dy * cos;

            return new Point(center.X + newX, center.Y + newY);
        }

    }
}