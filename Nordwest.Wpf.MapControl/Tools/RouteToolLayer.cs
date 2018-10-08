using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using Nordwest.Wpf.Controls.Layers;

namespace Nordwest.Wpf.Controls.Tools {
    public class RouteToolLayer : BaseToolLayer {

        private List<PointLatLng> _latLng = new List<PointLatLng>();
        private List<Ellipse> _points = new List<Ellipse>();

        private List<Line> _segments = new List<Line>();

        private MapToolPanel _panel;

        private bool _canSetMark;
        private int _selectedEllipseIndex = -1;

        private Action<List<PointLatLng>> _routeToolAction;

        private Brush _lineBrush = Brushes.Red;

        public RouteToolLayer() {

            _panel = new MapToolPanel();

            _panel.Ok.Click += Ok_Click;

            _panel.Close.Click += (sender, args) => {
                Reset();
            };

        }
        
        public Button Ok => _panel.Ok;
        public Button Close => _panel.Close;

        protected override void Reset() {
            RemoveElements();
            _points.Clear();
            _segments.Clear();
            _latLng.Clear();
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            RouteToolAction?.Invoke(_latLng);
            Reset();
        }

        public UIElement Content {
            get => _panel.Content;
            set {
                _panel.Content = value;
            }
        }

        public List<PointLatLng> Coordinates => _latLng;

        public Action<List<PointLatLng>> RouteToolAction {
            get => _routeToolAction;
            set {
                if (_routeToolAction != value) {
                    _routeToolAction = value;
                    OnPropertyChanged(nameof(RouteToolAction));
                }
            }
        }

        protected override void SubscribeToMap() {
            _mapControl.MouseDown += _mapControl_MouseDown;
            _mapControl.MouseMove += _mapControl_MouseMove;
            _mapControl.MouseUp += _mapControl_MouseUp;
            _mapControl.MouseLeave += _mapControl_MouseLeave;
            _gMap.OnPositionChanged += _gMap_OnPositionChanged;
            _gMap.OnMapZoomChanged += _gMap_OnMapZoomChanged;
            _gMap.SizeChanged += _gMap_SizeChanged;
        }

        protected override void UnsubscribeFromMap() {
            _mapControl.MouseDown -= _mapControl_MouseDown;
            _mapControl.MouseMove -= _mapControl_MouseMove;
            _mapControl.MouseUp -= _mapControl_MouseUp;
            _mapControl.MouseLeave -= _mapControl_MouseLeave;
            _gMap.OnPositionChanged -= _gMap_OnPositionChanged;
            _gMap.OnMapZoomChanged -= _gMap_OnMapZoomChanged;
            _gMap.SizeChanged -= _gMap_SizeChanged;
        }

        private void _gMap_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateElementsPosition();
        }

        private void _gMap_OnMapZoomChanged() {
            UpdateElementsPosition();
        }

        private void _gMap_OnPositionChanged(PointLatLng point) {
            UpdateElementsPosition();
        }

        private void _mapControl_MouseLeave(object sender, MouseEventArgs e) {
            _canSetMark = false;
            _selectedEllipseIndex = -1;
        }

        private void _mapControl_MouseUp(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(_mapControl.GMapControl);

            if (_canSetMark) {
                AddEllipse(pos);
                OnRouteChanged();
            }

            _selectedEllipseIndex = -1;
        }

        private void _mapControl_MouseMove(object sender, MouseEventArgs e) {
            _canSetMark = false;
            if (_selectedEllipseIndex != -1) {
                var pos = e.GetPosition(_mapControl.UpperLayer);
                var l = SetPoint(_latLng[_selectedEllipseIndex], _points[_selectedEllipseIndex], pos);
                _latLng[_selectedEllipseIndex] = l;
                OnRouteChanged();
            }
            UpdatePanelPosition();
        }

        private void _mapControl_MouseDown(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(_mapControl.UpperLayer);
            var element = _mapControl.UpperLayer.InputHitTest(pos);
            if (element is Ellipse s) {
                _selectedEllipseIndex = _points.IndexOf(s);
            }
            else {
                _canSetMark = true;
                _selectedEllipseIndex = -1;
            }
        }

        private void UpdateElementsPosition() {
            for (int i = 0; i < _latLng.Count; i++)
                UpdateEllipsePosition(_latLng[i], _points[i]);
            for (int i = 0; i < _segments.Count; i++)
                RefreshSegment(_segments[i], _points[i], _points[i + 1]);
            UpdatePanelPosition();
        }

        private void AddEllipse(Point p) {

            var ellipse = new Ellipse { Width = 12, Height = 12, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1, Margin = new Thickness(-6) };
            
            _points.Add(ellipse);
            RefreshElements();
            
            AddLine();

            _latLng.Add(SetPoint(new PointLatLng(), ellipse, p));

            UpdatePanelPosition();
        }

        private void AddLine() {

            if (_points.Count < 2)
                return;

            if (_segments.Count != _points.Count - 2)
                throw new Exception();

            var line = new Line { Stroke = _lineBrush, StrokeThickness = 3 };
            
            _segments.Add(line);
            RefreshElements();

        }

        protected override void RefreshElements() {

            if (_mapControl == null)
                return;
            
            RemoveElements();

            if (!IsActive) 
                return;

            foreach (var s in _segments) {
                MapControl.UpperLayer.Children.Add(s);
            }
            foreach (var p in _points) {
                MapControl.UpperLayer.Children.Add(p);
            }

            if (_points.Count > 1) { 
                MapControl.UpperLayer.Children.Add(_panel);
            }

        }

        private void RemoveElements() {
            foreach (var p in _points) {
                MapControl.UpperLayer.Children.Remove(p);
            }
            foreach (var s in _segments) {
                MapControl.UpperLayer.Children.Remove(s);
            }
            
            MapControl.UpperLayer.Children.Remove(_panel);

        }
        
        private PointLatLng SetPoint(PointLatLng l, Ellipse el, Point p) {

            var newL = _gMap.FromLocalToLatLng((int)p.X, (int)p.Y);

            l.Lat = newL.Lat;
            l.Lng = newL.Lng;

            UpdateEllipsePosition(l, el);

            return l;
        }

        private void UpdateEllipsePosition(PointLatLng l, Ellipse el) {

            var p = _gMap.FromLatLngToLocal(l);

            Canvas.SetLeft(el, p.X);
            Canvas.SetTop(el, p.Y);

            var ind = _points.IndexOf(el);
            if (ind < _points.Count - 1)
                RefreshSegment(_segments[ind], _points[ind], _points[ind + 1]);
            if (ind > 0)
                RefreshSegment(_segments[ind - 1], _points[ind - 1], _points[ind]);

            if (el == _points[_points.Count - 1])
                UpdatePanelPosition();
        }

        private void UpdatePanelPosition() {
            if (_points.Count == 0)
                return;

            Canvas.SetLeft(_panel, Canvas.GetLeft(_points[_points.Count - 1]));
            Canvas.SetTop(_panel, Canvas.GetTop(_points[_points.Count - 1]) - _panel.ActualHeight - 10);
        }

        private void RefreshSegment(Line s, Ellipse p1, Ellipse p2) {
            s.X1 = Canvas.GetLeft(p1);
            s.Y1 = Canvas.GetTop(p1);
            s.X2 = Canvas.GetLeft(p2);
            s.Y2 = Canvas.GetTop(p2);
        }

        private void OnRouteChanged() {
            RouteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RouteChanged;

    }
}
