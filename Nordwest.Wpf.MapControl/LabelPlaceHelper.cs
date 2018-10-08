using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Nordwest.Wpf.Controls
{
    public static class LabelPlaceHelper
    {
        public static void PlaceLabels(List<MapMarker> visibleMarkers)
        {
            var lRects = new List<Rect>();

            List<MarkerSpace> v = null;
            List<MarkerSpace> l1 = null;
            List<MarkerSpace> l2 = null;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                v = visibleMarkers.ConvertAll(m => new MarkerSpace(m, GetClientPoint(m), ((FrameworkElement)m.MarkerContainer.Content).RenderSize));
                l1 = v.FindAll(m => m.Marker.IsSelected);
                l2 = v.FindAll(m => !m.Marker.IsSelected);
            }));

            foreach (var point in l1)
                ProcessMarker(v, point, lRects);

            foreach (var point in l2)
               ProcessMarker(v, point, lRects);
            
        }

        private static void ProcessMarker(List<MarkerSpace> visibleMarkers, MarkerSpace point, List<Rect> lRects) {

            Point pointPos = new Point();
            Size markerSize = Size.Empty;
            Size labelActualSize = Size.Empty;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                var m = (FrameworkElement) point.Marker.MarkerContainer.Content;
                markerSize = m.RenderSize;
                pointPos = GetClientPoint(point.Marker);
                labelActualSize = point.Marker.MarkerContainer.LabelContent is FrameworkElement label ? ((label as Border)?.Child.DesiredSize ?? label.RenderSize) : new Size(60, 20);
            }));

            foreach (var point2 in visibleMarkers) {
                if (point2 == point)
                    continue;

                var marker2Size = point2.Size;
                var pointPos2 = point2.Position; 
                var markerRect2 = new Rect(pointPos2.X - marker2Size.Width / 2, pointPos2.Y - marker2Size.Height / 2, marker2Size.Width, marker2Size.Height);

                var toRemove = point.AvailablePlaces.FindAll(ap => GetLabelRect(pointPos, markerSize, labelActualSize, ap).IntersectsWith(markerRect2));
                foreach (var r in toRemove) {
                    point.AvailablePlaces.Remove(r);
                    point2.AvailablePlaces.Remove(GetOpposite(r));
                }

                if (!point.AvailablePlaces.Any()) 
                    break;
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                if (point.AvailablePlaces.Any()) {
                    var lPlace = point.AvailablePlaces.First();
                    var lRect = GetLabelRect(pointPos, markerSize, labelActualSize, lPlace);

                    if (lRects.TrueForAll(r => !lRect.IntersectsWith(r))) {
                        lRects.Add(lRect);
                        SetLabel(point.Marker, lRect.Location);
                        point.Marker.LabelPlace = lPlace;
                    }
                    else {
                        SetLabel(point.Marker, GetLabelRect(pointPos, markerSize, labelActualSize, LabelPlace.Left).Location);
                        point.Marker.LabelPlace = LabelPlace.None;
                    }
                }
                else if (point.Marker.IsSelected) {
                    var lPlace = LabelPlace.Left;
                    var lRect = GetLabelRect(pointPos, markerSize, labelActualSize, lPlace);

                    lRects.Add(lRect);
                    SetLabel(point.Marker, lRect.Location);
                    point.Marker.LabelPlace = lPlace;
                }
                else {
                    SetLabel(point.Marker, GetLabelRect(pointPos, markerSize, labelActualSize, LabelPlace.Left).Location);
                    point.Marker.LabelPlace = LabelPlace.None;
                }

                point.Marker.IsMouseOver = true;
                point.Marker.IsMouseOver = false;
            }));

        }

        public static Point GetClientPoint(MapMarker marker)
        {
            var gmap = marker.Map;
            var proj = marker.Map.MapProvider.Projection;
            var clientPos = proj.FromLatLngToPixel(marker.Position, (int)gmap.Zoom);
            return new Point(clientPos.X, clientPos.Y);
        }

        public static Rect GetLabelRect(Point markerPosition, Size markerSize, Size labelSize, LabelPlace place)
        {
            switch (place)
            {
                case LabelPlace.None:
                case LabelPlace.Left:
                    return new Rect(markerPosition.X - markerSize.Width / 2 - labelSize.Width, markerPosition.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height);
                case LabelPlace.Top:
                    return new Rect(markerPosition.X - labelSize.Width / 2, markerPosition.Y - labelSize.Height - markerSize.Height / 2, labelSize.Width, labelSize.Height);
                case LabelPlace.Right:
                    return new Rect(markerPosition.X + markerSize.Width / 2, markerPosition.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height);
                case LabelPlace.Bottom:
                    return new Rect(markerPosition.X - labelSize.Width / 2, markerPosition.Y + markerSize.Height / 2, labelSize.Width, labelSize.Height);
                default:
                    throw new ArgumentOutOfRangeException("place");
            }
        }

        public static void SetLabel(MapMarker marker, Point labelPosition) {
            var element = marker.MarkerContainer.LabelContent as FrameworkElement;
            if (element == null) return;

            var markerPos = GetClientPoint(marker);
            marker.MarkerContainer.LabelOffset = new Point(labelPosition.X - markerPos.X, labelPosition.Y - markerPos.Y);
               
        }

        private static LabelPlace GetOpposite(LabelPlace place) {
            switch (place) {
                case LabelPlace.None:
                    return LabelPlace.None;
                case LabelPlace.Left:
                    return LabelPlace.Right;
                case LabelPlace.Top:
                    return LabelPlace.Bottom;
                case LabelPlace.Right:
                    return LabelPlace.Left;
                case LabelPlace.Bottom:
                    return LabelPlace.Top;
            }
            throw new ArgumentOutOfRangeException();
        }

        private class MarkerSpace {

            public MarkerSpace(MapMarker marker, Point position, Size size) {
                Marker = marker;
                Position = position;
                Size = size;
                AvailablePlaces = new List<LabelPlace> { LabelPlace.Left, LabelPlace.Top, LabelPlace.Right, LabelPlace.Bottom };
            }

            public MapMarker Marker { get; }
            public Point Position { get; }
            public Size Size { get; }
            public List<LabelPlace> AvailablePlaces { get; }
        }
    }

    public enum LabelPlace
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }
}