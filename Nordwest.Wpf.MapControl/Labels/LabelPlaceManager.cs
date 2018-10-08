using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Nordwest.Wpf.Controls.Labels;

namespace Nordwest.Wpf.Controls.Clustering {
    public class LabelPlaceManager {

        private int _currentZoom;
        private readonly Dictionary<int, List<LabelRect>> _clusters = new Dictionary<int, List<LabelRect>>();
        
        private bool _updateCanceling = false;

        public void PlaceLabels(List<MapMarker> visibleMarkers) {

            if (visibleMarkers.Count == 0)
                return;

            _updateCanceling = false;

            var zoom = _currentZoom;

            var lRects = new List<Rect>();

            List<MarkerSpace> v = null;
            List<MarkerSpace> l1 = null;
            List<MarkerSpace> l2 = null;

            var labelPlaces = new List<LabelPlace> { LabelPlace.Left, LabelPlace.Top, LabelPlace.Right, LabelPlace.Bottom };

            bool hasCache = false;
            
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {

                if (_clusters.ContainsKey(zoom)) {
                    SetLabels(_clusters[zoom]);
                    hasCache = true;
                    return;
                }

                v = visibleMarkers.ConvertAll(marker => {

                    var m = (FrameworkElement) marker.MarkerContainer.Content;
                    var markerSize = m.RenderSize;
                    var pointPos = GetClientPoint(marker);
                    var labelActualSize = marker.MarkerContainer.LabelContent is FrameworkElement label ? ((label as Border)?.Child.DesiredSize ?? label.RenderSize) : new Size(60, 20);

                    return new MarkerSpace(marker, pointPos, markerSize, labelPlaces.ConvertAll(ap => GetLabelRect(marker, pointPos, markerSize, labelActualSize, ap)));
                });

            }));

            if (hasCache)
                return;

            var lP = new List<LabelRect>();

            foreach (var point in v) {
                ProcessMarker(v, point, lRects, lP);
                if (_updateCanceling)
                    return;
            }

            _clusters.Add(zoom, lP);

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                SetLabels(lP);
            }));

        }

        public void Reset() {
            _clusters.Clear();
        }

        private void SetLabels(List<LabelRect> lP) {
            foreach (var point in lP) {
                SetLabel(point.MapMarker, point.Rect.Location);
                point.MapMarker.LabelPlace = point.Place;
                point.MapMarker.IsMouseOver = true;
                point.MapMarker.IsMouseOver = false;
            }
        }

        private static void ProcessMarker(List<MarkerSpace> visibleMarkers, MarkerSpace point, List<Rect> lRects, List<LabelRect> lP) {

            foreach (var point2 in visibleMarkers) {
                if (point2 == point)
                    continue;

                var toRemove = point.AvailablePlaces.FindAll(ap => ap.Rect.IntersectsWith(point2.Rect));
                foreach (var r in toRemove) {
                    point.AvailablePlaces.Remove(r);
                    point2.AvailablePlaces.RemoveAll(p => p.Place == GetOpposite(r.Place));
                }

                if (!point.AvailablePlaces.Any())
                    break;
            }

            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                if (point.AvailablePlaces.Any()) {
                    var lPlace = point.AvailablePlaces.First();

                    if (lRects.TrueForAll(r => !lPlace.Rect.IntersectsWith(r))) {
                        lRects.Add(lPlace.Rect);
                        lP.Add(new LabelRect(point.Marker, lPlace.Rect, lPlace.Place));
                        //SetLabel(point.Marker, lPlace.Rect.Location);
                        //point.Marker.LabelPlace = lPlace.Place;
                    }
                    else {
                        lP.Add(new LabelRect(point.Marker, point.DefaultLabelPlace.Rect, LabelPlace.None));
                        //SetLabel(point.Marker, point.DefaultLabelPlace.Rect.Location);
                        //point.Marker.LabelPlace = LabelPlace.None;
                    }
                }
                //else if (point.Marker.IsSelected) {
                //    var lRect = point.DefaultLabelPlace.Rect;
                //    lRects.Add(lRect);
                //    SetLabel(point.Marker, lRect.Location);
                //    point.Marker.LabelPlace = point.DefaultLabelPlace.Place;
                //}
                else {
                    lP.Add(new LabelRect(point.Marker, point.DefaultLabelPlace.Rect, LabelPlace.None));
                //SetLabel(point.Marker, point.DefaultLabelPlace.Rect.Location);
                //point.Marker.LabelPlace = LabelPlace.None;
            }

            //}));
        }

        private static Point GetClientPoint(MapMarker marker) {
            var gmap = marker.Map;
            var proj = marker.Map.MapProvider.Projection;
            var clientPos = proj.FromLatLngToPixel(marker.Position, (int)gmap.Zoom);
            return new Point(clientPos.X, clientPos.Y);
        }

        private static LabelRect GetLabelRect(MapMarker marker, Point markerPosition, Size markerSize, Size labelSize, LabelPlace place) {
            switch (place) {
                case LabelPlace.None:
                case LabelPlace.Left:
                    return new LabelRect(marker, new Rect(markerPosition.X - markerSize.Width / 2 - labelSize.Width, markerPosition.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height), place);
                case LabelPlace.Top:
                    return new LabelRect(marker, new Rect(markerPosition.X - labelSize.Width / 2, markerPosition.Y - labelSize.Height - markerSize.Height / 2, labelSize.Width, labelSize.Height), place);
                case LabelPlace.Right:
                    return new LabelRect(marker, new Rect(markerPosition.X + markerSize.Width / 2, markerPosition.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height), place);
                case LabelPlace.Bottom:
                    return new LabelRect(marker, new Rect(markerPosition.X - labelSize.Width / 2, markerPosition.Y + markerSize.Height / 2, labelSize.Width, labelSize.Height), place);
                default:
                    throw new ArgumentOutOfRangeException("place");
            }
        }

        private static void SetLabel(MapMarker marker, Point labelPosition) {
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

        public int CurrentZoom {
            get => _currentZoom;
            set {
                if (_currentZoom != value) {
                    _currentZoom = value;
                    _updateCanceling = true;
                }
            }
        }

    }
}
