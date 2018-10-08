using System;
using System.Collections.Generic;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Nordwest.Wpf.Controls.Clustering {
    public class MarkerClusterer {
        private readonly List<MapMarker> _markers = new List<MapMarker>();

        private readonly Dictionary<int, List<Cluster>> _clusters = new Dictionary<int, List<Cluster>>();

        private readonly GMapControl _gMap;

        private int _gridXPixels;
        private int _gridYPixels;

        private bool _updateCanceling = false;

        private int _currentZoom;

        public MarkerClusterer(GMapControl gMap) {
            _gMap = gMap;
        }

        public void AddMarkers(IList<MapMarker> markers) {
            _markers.AddRange(markers);
            _clusters.Clear();
        }

        public void RemoveMarkers(IList<MapMarker> markers) {
            _markers.RemoveAll(markers.Contains);
            _clusters.Clear();
        }

        public void UpdateClusters(int gridXPixels, int gridYPixels) {

            if (gridXPixels == 0 || gridYPixels == 0)
                return;

            _updateCanceling = false;

            var zoom = _currentZoom;

            bool applied = false;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                if (_clusters.ContainsKey(zoom)) {
                    _clusters[zoom].ForEach(c => c.ApplyClusterToMarkers());
                    applied = true;
                }
            }));

            if (applied)
                return;

            _gridXPixels = gridXPixels;
            _gridYPixels = gridYPixels;

            var markersCopy = _markers.ToList();

            var cluster = new List<Cluster>();

            foreach (var marker in markersCopy) {
                AddToClosestCluster(marker, cluster);
                if (_updateCanceling)
                    return;
            }

            _clusters.Add(zoom, cluster);

        }

        private void AddToClosestCluster(MapMarker marker, List<Cluster> clusters) {

            Cluster clusterToAddTo = clusters.Find(c => c.Bounds.Contains(marker.Position));

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                if (clusterToAddTo != null) {
                    clusterToAddTo.AddMarker(marker);
                }
                else {
                    var cluster = new Cluster(marker, GetBounds(marker.Position));
                    clusters.Add(cluster);
                }
            }));

        }

        public void Clear() {
            _clusters.Clear();
            _markers.Clear();
        }

        private RectLatLng GetBounds(PointLatLng point) {
            var local = _gMap.FromLatLngToLocal(point);

            var t = local.Y - _gridYPixels / 2;
            var l = local.X - _gridXPixels / 2;
            var b = local.Y + _gridYPixels / 2;
            var r = local.X + _gridXPixels / 2;

            var tl = _gMap.FromLocalToLatLng((int)l, (int)t);
            var br = _gMap.FromLocalToLatLng((int)r, (int)b);

            return new RectLatLng(tl.Lat, tl.Lng, br.Lng - tl.Lng, Math.Abs(br.Lat - tl.Lat));
        }

        public ReadOnlyCollection<MapMarker> Markers => _markers.AsReadOnly();

        public IList<Cluster> Clusters {
            get {
                if (!_clusters.ContainsKey(_currentZoom))
                    return new List<Cluster>();
                return _clusters[_currentZoom];
            }
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
