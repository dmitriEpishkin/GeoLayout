
using System.Windows;

namespace Nordwest.Wpf.Controls.Labels {
    public class LabelRect {

        public LabelRect(MapMarker marker, Rect rect, LabelPlace place) {
            MapMarker = marker;
            Rect = rect;
            Place = place;
        }

        public MapMarker MapMarker { get; }
        public Rect Rect { get; }
        public LabelPlace Place { get; }
    }
}
