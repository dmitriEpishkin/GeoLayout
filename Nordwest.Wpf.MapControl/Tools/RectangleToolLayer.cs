
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;

namespace Nordwest.Wpf.Controls.Tools {
    public class RectangleToolLayer : BaseToolLayer {

        private enum RectangleToolState {
            None,
            OnlyCenter,
            Full
        }

        private RectangleToolState _state = RectangleToolState.None;

        private PointLatLng _center;
        private PointLatLng _v1;
        private PointLatLng _v2;
        
        private Point _p1;
        private Point _p2;
        private Point _pCenter;

        private Path _c;
        
        private Line _l1;
        private Line _l2;
        private Line _l1Add;
        private Line _l2Add;

        private Ellipse _a1;
        private Ellipse _a2;

        private bool _canSetMark;
        private UIElement _selectedArrow;
        private bool _centerSelected;
        
        private MapToolPanel _panel;
        private Action<PointLatLng, PointLatLng, PointLatLng> _rectToolAction;

        private Brush _lineBrush = Brushes.Red;

        public RectangleToolLayer() {

            _panel = new MapToolPanel();
            _panel.Ok.Click += Ok_Click;
            _panel.Close.Click += (sender, args) => { Reset(); };

            _c = new Path { Fill = Brushes.Beige, Stroke = Brushes.Blue, StrokeThickness = 1, Margin = new Thickness(-5, -5, 0, 0), Data = Geometry.Parse("M 4,0 6,0 6,4 10,4 10,6 6,6 6,10 4,10 4,6 0,6 0,4 4,4 4,0") };

            _l1 = new Line { Stroke = _lineBrush, StrokeThickness = 3 };
            _l2 = new Line { Stroke = _lineBrush, StrokeThickness = 3 };
            _l1Add = new Line {Stroke = _lineBrush, StrokeThickness = 3, StrokeDashArray = new DoubleCollection(new[] {3.0, 3.0})};
            _l2Add = new Line { Stroke = _lineBrush, StrokeThickness = 3, StrokeDashArray = new DoubleCollection(new[] { 3.0, 3.0 }) };

            _a1 = new Ellipse { Width = 12, Height = 12, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1, Margin = new Thickness(-6) };
            _a2 = new Ellipse { Width = 12, Height = 12, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1, Margin = new Thickness(-6) };
            
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            RectToolAction?.Invoke(_center, _v1, _v2);
            Reset();
        }

        public Button Ok => _panel.Ok;
        public Button Close => _panel.Close;

        public UIElement Content {
            get => _panel.Content;
            set {
                _panel.Content = value;
            }
        }

        public Action<PointLatLng, PointLatLng, PointLatLng> RectToolAction {
            get => _rectToolAction;
            set {
                if (_rectToolAction != value) {
                    _rectToolAction = value;
                    OnPropertyChanged(nameof(RectToolAction));
                }
            }
        }

        private void UpdatePanelPosition() {

            if (!_mapControl.UpperLayer.Children.Contains(_a1))
                return;

            var p4 = GetAdditionalPoint();

            var p1 = _p1.Y < _p2.Y ? _p1 : _p2;
            var p2 = _pCenter.Y < p4.Y ? _pCenter : p4;

            var p = p1.X > p2.X ? p1 : p2;

            Canvas.SetLeft(_panel, p.X);
            Canvas.SetTop(_panel, p.Y - _panel.ActualHeight - 10);
        }

        private Point GetAdditionalPoint() {
            var c = new Point((_p1.X + _p2.X) / 2, (_p1.Y + _p2.Y) / 2);
            var dx = c.X - _pCenter.X;
            var dy = c.Y - _pCenter.Y;
            return new Point(_pCenter.X + 2 * dx, _pCenter.Y + 2 * dy);
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
            UpdatePoints();
        }

        private void _gMap_OnMapZoomChanged() {
            UpdatePoints();
        }

        private void _gMap_OnPositionChanged(PointLatLng point) {
            UpdatePoints();
        }

        private void _mapControl_MouseLeave(object sender, MouseEventArgs e) {
            _canSetMark = false;
            _selectedArrow = null;
            _centerSelected = false;
        }

        private void _mapControl_MouseUp(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(_mapControl.GMapControl);

            if (_canSetMark) {
                SetNextState();
                RefreshElements();

                if (_state == RectangleToolState.OnlyCenter) {
                    var l = _gMap.FromLocalToLatLng((int)pos.X, (int)pos.Y);
                    _center = l;
                }
                else if (_state == RectangleToolState.Full) {
                    var l = _gMap.FromLocalToLatLng((int)pos.X, (int)pos.Y);
                    _v1 = l;

                    var pos2 = RotationHelper.Rotate(_pCenter, pos, Math.PI / 2);
                    var l2 = _gMap.FromLocalToLatLng((int) pos2.X, (int) pos2.Y);

                    _v2 = l2;
                }

                UpdatePoints();
            }

            _selectedArrow = null;
            _centerSelected = false;
        }

        private void _mapControl_MouseMove(object sender, MouseEventArgs e) {
            _canSetMark = false;
            if (_selectedArrow != null) {
                var pos = e.GetPosition(_mapControl.UpperLayer);
                var l = _gMap.FromLocalToLatLng((int)pos.X, (int)pos.Y);
                if (_selectedArrow == _a1)
                    _v1 = l;
                else 
                    _v2 = l;
                UpdatePoints();
            }
            else if (_centerSelected) {
                var pos = e.GetPosition(_mapControl.UpperLayer);
                var l = _gMap.FromLocalToLatLng((int)pos.X, (int)pos.Y);
                _center = l;
                UpdatePoints();
            }
            UpdatePanelPosition();
        }

        private void _mapControl_MouseDown(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(_mapControl.UpperLayer);
            var element = _mapControl.UpperLayer.InputHitTest(pos);
            if (element is Ellipse s) {
                _selectedArrow = s;
            }
            else if (element == _c) {
                _centerSelected = true;
            }
            else {
                _canSetMark = true;
                _selectedArrow = null;
                _centerSelected = false;
            }
        }

        private void SetNextState() {
            if (_state == RectangleToolState.None)
                _state = RectangleToolState.OnlyCenter;
            else if (_state == RectangleToolState.OnlyCenter)
                _state = RectangleToolState.Full;
            else if (_state == RectangleToolState.Full)
                _state = RectangleToolState.None;
        }
        
        private void UpdatePoints() {
            var c = _gMap.FromLatLngToLocal(_center);
            var p1 = _gMap.FromLatLngToLocal(_v1);
            var p2 = _gMap.FromLatLngToLocal(_v2);

            _pCenter = new Point(c.X, c.Y);
            _p1 = new Point(p1.X, p1.Y);
            _p2 = new Point(p2.X, p2.Y);
            
            UpdateElementsPosition();
        }

        private void UpdateElementsPosition() {

            Canvas.SetTop(_c, _pCenter.Y);
            Canvas.SetLeft(_c, _pCenter.X);

            UpdateEllipsePosition(_a1, _p1);
            UpdateEllipsePosition(_a2, _p2);

            var p = GetAdditionalPoint();

            UpdateLinePosition(_l1, _pCenter, _p1);
            UpdateLinePosition(_l2, _pCenter, _p2);
            UpdateLinePosition(_l1Add, _p1, p);
            UpdateLinePosition(_l2Add, _p2, p);

            UpdatePanelPosition();
        }

        private void UpdateEllipsePosition(Ellipse e, Point p) {
            Canvas.SetTop(e, p.Y);
            Canvas.SetLeft(e, p.X);
        }

        private void UpdateLinePosition(Line l, Point p1, Point p2) {
            l.X1 = p1.X;
            l.Y1 = p1.Y;
            l.X2 = p2.X;
            l.Y2 = p2.Y;
        }

        protected override void RefreshElements() {

            if (_mapControl == null)
                return;

            RemoveElements();

            if (!IsActive) {
                _state = RectangleToolState.None;
                return;
            }
            
            if (_state == RectangleToolState.Full) {
                _mapControl.UpperLayer.Children.Add(_l1);
                _mapControl.UpperLayer.Children.Add(_l2);
                _mapControl.UpperLayer.Children.Add(_l1Add);
                _mapControl.UpperLayer.Children.Add(_l2Add);

                _mapControl.UpperLayer.Children.Add(_a1);
                _mapControl.UpperLayer.Children.Add(_a2);

                _mapControl.UpperLayer.Children.Add(_panel);
            }

            if (_state != RectangleToolState.None)
                _mapControl.UpperLayer.Children.Add(_c);
        }

        protected override void Reset() {
            _state = RectangleToolState.None;
            _center = new PointLatLng();
            _v1 = new PointLatLng();
            _v2 = new PointLatLng();

            RemoveElements();
        }

        private void RemoveElements() {

            _mapControl.UpperLayer.Children.Remove(_c);

            _mapControl.UpperLayer.Children.Remove(_a1);
            _mapControl.UpperLayer.Children.Remove(_a2);

            _mapControl.UpperLayer.Children.Remove(_l1);
            _mapControl.UpperLayer.Children.Remove(_l2);
            _mapControl.UpperLayer.Children.Remove(_l1Add);
            _mapControl.UpperLayer.Children.Remove(_l2Add);
            
            _mapControl.UpperLayer.Children.Remove(_panel);
        }

    }
}