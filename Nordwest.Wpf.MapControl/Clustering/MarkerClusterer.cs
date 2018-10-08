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
        private readonly List<Cluster> _clusters = new List<Cluster>(); // dict cache

        private readonly GMapControl _gMap;

        private int _gridXPixels;
        private int _gridYPixels;

        private bool _clustersUpdating = false;

        public MarkerClusterer(GMapControl gMap) {
            _gMap = gMap;
        }

        public void AddMarkers(IList<MapMarker> markers) {
            _markers.AddRange(markers);
        }

        public void RemoveMarkers(IList<MapMarker> markers) {
            _markers.RemoveAll(markers.Contains);
        }

        public void UpdateClusters(int gridXPixels, int gridYPixels)
        {
            _gridXPixels = gridXPixels;
            _gridYPixels = gridYPixels;

            var markersCopy = _markers.ToList();

            _clusters.Clear();
            
            foreach (var marker in markersCopy)
            {
                AddToClosestCluster(marker);
            }
        }

        private void AddToClosestCluster(MapMarker marker)
        {
            Cluster clusterToAddTo = _clusters.Find(c => c.Bounds.Contains(marker.Position));

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                if (clusterToAddTo != null) {
                    clusterToAddTo.AddMarker(marker);
                }
                else {
                    var cluster = new Cluster(marker, GetBounds(marker.Position));
                    _clusters.Add(cluster);
                }
            }));

        }

        public ReadOnlyCollection<MapMarker> Markers { get { return _markers.AsReadOnly(); } }
        public IList<Cluster> Clusters { get { return _clusters; } }

        public void Clear() {
            _clusters.Clear();
            _markers.Clear();
        }

        private RectLatLng GetBounds(PointLatLng point)
        {
            var local = _gMap.FromLatLngToLocal(point);

            var t = local.Y - _gridYPixels / 2;
            var l = local.X - _gridXPixels / 2;
            var b = local.Y + _gridYPixels / 2;
            var r = local.X + _gridXPixels / 2;

            var tl = _gMap.FromLocalToLatLng((int)l, (int)t);
            var br = _gMap.FromLocalToLatLng((int)r, (int)b);

            return new RectLatLng(tl.Lat, tl.Lng, br.Lng - tl.Lng, Math.Abs(br.Lat - tl.Lat));
        }
    }
}
