using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;

namespace Nordwest.Wpf.Controls.Tools {
    public class AddPointLayer : BaseToolLayer {

        private bool _isSet = false;

        private Point _pos1;

        private PointLatLng _point;

        private readonly Path _marker;

        private readonly MapToolPanel _addPanel;

        private Action<PointLatLng> _addPointAction;

        public AddPointLayer() {

            _marker = new Path { Fill = Brushes.Beige, Stroke = Brushes.DarkRed, StrokeThickness = 1, Margin = new Thickness(-5,-5,0,0), Data = Geometry.Parse("M 4,0 6,0 6,4 10,4 10,6 6,6 6,10 4,10 4,6 0,6 0,4 4,4 4,0") };

            _addPanel = new MapToolPanel();

            _addPanel.Ok.Click += AddPoint;
            _addPanel.Close.Click += (sender, args) => {
                Reset();
            };
            
        }

        public Button Ok => _addPanel.Ok;
        public Button Close => _addPanel.Close;

        protected override void Reset() {
            _isSet = false;
            _point = new PointLatLng();
            _pos1 = new Point();
            UpdatePanelPosition(Point);
            RefreshElements();
        }

        public UIElement AddPointUI {
            get => _addPanel.Content;
            set => _addPanel.Content = value;
        }

        private void AddPoint(object sender, RoutedEventArgs args) {
            _addPointAction?.Invoke(_point);
            Reset();
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

        private void RemoveElements() {
            _mapControl.UpperLayer.Children.Remove(_marker);
            _mapControl.UpperLayer.Children.Remove(_addPanel);
        }

        private void _gMap_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdatePanelPosition(_point);
        }

        private void _gMap_OnMapZoomChanged() {
            UpdatePanelPosition(_point);
        }

        private void _gMap_OnPositionChanged(PointLatLng point) {
            UpdatePanelPosition(_point);
        }

        private void _mapControl_MouseLeave(object sender, MouseEventArgs e) {
            
        }

        private void _mapControl_MouseUp(object sender, MouseButtonEventArgs e) {

            var pos = e.GetPosition(_mapControl.UpperLayer);

            if (pos != _pos1)
                return;
            
            _isSet = true;

            Point = _gMap.FromLocalToLatLng((int)pos.X, (int)pos.Y);

            RefreshElements();

            UpdatePanelPosition(Point);
        }

        private void _mapControl_MouseMove(object sender, MouseEventArgs e) {
            
        }

        private void _mapControl_MouseDown(object sender, MouseButtonEventArgs e) {
            _pos1 = e.GetPosition(_mapControl.UpperLayer);
            _isSet = true;
            RefreshElements();
        }
        
        private void UpdatePanelPosition(PointLatLng l) {

            var p = _gMap.FromLatLngToLocal(l);

            Canvas.SetLeft(_marker, p.X);
            Canvas.SetTop(_marker, p.Y);

            Canvas.SetLeft(_addPanel, p.X);
            Canvas.SetTop(_addPanel, p.Y - _addPanel.ActualHeight - 10);
        }

        protected override void RefreshElements() {

            if (_mapControl == null)
                return;

            if (!IsActive) {
                RemoveElements();
                _isSet = false;
                return;
            }
            
            RemoveElements();
            
            if (_isSet) {
                _mapControl.UpperLayer.Children.Add(_marker);
                _mapControl.UpperLayer.Children.Add(_addPanel);
            }

        }

        public Action<PointLatLng> AddPointAction {
            get => _addPointAction;
            set {
                if (_addPointAction != value) {
                    _addPointAction = value;
                    OnPropertyChanged(nameof(AddPointAction));
                }
            }
        }

        public PointLatLng Point {
            get => _point;
            set {
                if (_point != value) {
                    _point = value;
                    UpdatePanelPosition(_point);
                    OnPropertyChanged(nameof(Point));
                }
            }
        }

    }
}
