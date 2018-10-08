using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Nordwest.Wpf.Controls.Labels {
    public class MarkerSpace {

        public MarkerSpace(MapMarker marker, Point position, Size size, List<LabelRect> availablePlaces) {
            Marker = marker;
            Position = position;
            Size = size;
            Rect = new Rect(position.X - size.Width / 2, position.Y - size.Height / 2, size.Width, size.Height);
            AvailablePlaces = availablePlaces;
            DefaultLabelPlace = availablePlaces[0];
        }

        public MapMarker Marker { get; }
        public Point Position { get; }
        public Size Size { get; }
        public Rect Rect { get; }
        public List<LabelRect> AvailablePlaces { get; }
        public LabelRect DefaultLabelPlace { get; }
        public LabelRect ChoosenLabelPlace { get; set; }
    }
}
