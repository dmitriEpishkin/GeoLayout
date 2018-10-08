using GMap.NET.WindowsPresentation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using Nordwest.Wpf.Controls.Layers;

namespace Nordwest.Wpf.Controls {
    public class MapSegmentsLayer : Control, IGMapElementsLayer, INotifyPropertyChanged {
        
        private readonly Dictionary<object, MapSegment> _routes = new Dictionary<object, MapSegment>();

        private MapControl _mapControl;
        private GMapControl _gMap;

        private int _lastInd;
        private int _count;

        public void Add(object obj, double lat1, double lon1, double lat2, double lon2, Brush brush) {
            
            var r = new GMapRoute(new[] { new PointLatLng(lat1, lon1), new PointLatLng(lat2, lon2) });
            var s = new MapSegment(r, brush);
            _routes.Add(obj, s);

            if (_gMap == null)
                return;

            ShowMapSegment(s);

            CreateShape(r, brush);
            
            if (!LayerIsVisible) 
                HideMapElement(s);            

        }

        private void CreateShape(GMapRoute r, Brush brush) {
            r.RegenerateShape(_gMap);
            var p = (Path)r.Shape;

            var dashes = new DoubleCollection { 2, 1 };
            p.StrokeDashArray = dashes;

            p.StrokeThickness = 2;
            p.Stroke = brush;
            p.Opacity = 1;
            p.Effect = null;
        }

        public void Remove(object obj) {
            if (_routes.ContainsKey(obj)) {
                HideMapElement(_routes[obj]);
                _routes.Remove(obj);                
            }
        }

        public void Clear() {
            if (LayerIsVisible) {
                Hide();
            }
            _routes.Clear();            
        }

        private void Hide() {
            foreach (var r in _routes)
                HideMapElement(r.Value);
        }

        private void HideMapElement(MapSegment r) {
            if (_gMap.Markers.Remove(r.Route)) {
                r.IsVisible = false;
                LastElementIndex--;
                _count--;
            }
        }

        private void Show() {
            foreach (var r in _routes) {
                if (!_gMap.Markers.Contains(r.Value.Route)) {
                    ShowMapSegment(r.Value);
                }
                if (r.Value.Route.Shape == null)
                    CreateShape(r.Value.Route, r.Value.Brush);
            }
        }

        private void ShowMapSegment(MapSegment r) {
            _gMap.Markers.Insert(LastElementIndex, r.Route);
            r.IsVisible = true;
            LastElementIndex++;
            _count++;
        }

        public static readonly DependencyProperty LayerIsVisibleProperty = DependencyProperty.Register(
            "LayerIsVisible", typeof(bool), typeof(MapSegmentsLayer), new FrameworkPropertyMetadata(true, LayerIsVisibleChanged));

        private static void LayerIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var l = (MapSegmentsLayer) d;
            if (l.LayerIsVisible)
                l.Show();
            else {
                l.Hide();
            }
        }

        public bool LayerIsVisible {
            get => (bool)GetValue(LayerIsVisibleProperty);
            set => SetValue(LayerIsVisibleProperty, value);
        }

        public MapControl MapControl {
            get { return _mapControl; }
            set {
                _mapControl = value;
                _gMap = _mapControl.GMapControl;
            }
        }

        public void ResetLayer() {
            Hide();
            Show();
        }

        // пока их нет
        public void UpdateClusters() {
            
        }

        public int ElementsCount => _count;

        public int LastElementIndex {
            get { return _lastInd; }
            set {
                if (_lastInd != value) {
                    _lastInd = value;
                    OnPropertyChanged(nameof(LastElementIndex));
                    _mapControl?.UpdateLayersIndexes();
                }
            }
        }

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
