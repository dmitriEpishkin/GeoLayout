using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GMap.NET;

namespace Nordwest.Wpf.Controls.Clustering {
    public class Cluster : IEnumerable<MapMarker> {
        private readonly MapMarker _primaryMarker;
        private readonly RectLatLng _bounds;

        private readonly List<MapMarker> _children = new List<MapMarker>();

        public Cluster(MapMarker primaryMarker, RectLatLng bounds) {
            _primaryMarker = primaryMarker;
            primaryMarker.IsInGroup = false;

            _bounds = bounds;
        }

        public void AddMarker(MapMarker marker) {
            _children.Add(marker);
            marker.IsInGroup = true;
        }

        public RectLatLng Bounds {
            get { return _bounds; }
        }
        public MapMarker PrimaryMarker {
            get { return _primaryMarker; }
        }

        public List<MapMarker> Children {
            get { return _children; }
        }

        public IEnumerator<MapMarker> GetEnumerator() {
            yield return _primaryMarker;

            for (int i = 0; i < _children.Count; i++)
                yield return _children[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
