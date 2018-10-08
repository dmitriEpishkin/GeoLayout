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
using GMap.NET.WindowsPresentation;
using Nordwest.Wpf.Controls.Layers;

namespace Nordwest.Wpf.Controls.Tools {
    public class ShiftPointLayer : BaseToolLayer {

        private MapMarker _selectedMarker;
        private bool _drag;

        private PointLatLng _currentPosition;

        private Line _line;
        private Polygon _arrow;
        private MapToolPanel _panel;

        private Brush _lineBrush = Brushes.Red;

        private Action<MapMarker, PointLatLng> _shiftAction;

        public ShiftPointLayer() {
            _line = new Line { Stroke = _lineBrush, StrokeThickness = 3, StrokeDashArray = new DoubleCollection(new[] { 3.0, 3.0 }), StrokeEndLineCap = PenLineCap.Triangle };
            _arrow = new Polygon {Fill = _lineBrush};
            _panel = new MapToolPanel(false);
        }

        protected override void SubscribeToMap() {
            _mapControl.MouseDown += _mapControl_MouseDown;
            _mapControl.MouseMove += _mapControl_MouseMove;
            _mapControl.MouseUp += _mapControl_MouseUp;
        }
        
        protected override void UnsubscribeFromMap() {
            _mapControl.MouseDown -= _mapControl_MouseDown;
            _mapControl.MouseMove -= _mapControl_MouseMove;
            _mapControl.MouseUp -= _mapControl_MouseUp;
        }
        
        private void _mapControl_MouseUp(object sender, MouseButtonEventArgs e) {

            if (e.ChangedButton != MouseButton.Right)
                return;

            if (_drag) {
                var pos = e.GetPosition(_gMap);
                var latLng = _gMap.FromLocalToLatLng((int)pos.X, (int)pos.Y);
                _shiftAction(_selectedMarker, latLng);
                SelectedMarker = null;
                _drag = false;
                _mapControl.UpperLayer.Children.Remove(_line);
                _mapControl.UpperLayer.Children.Remove(_arrow);
                _mapControl.UpperLayer.Children.Remove(_panel);
            }
        }

        private void _mapControl_MouseMove(object sender, MouseEventArgs e) {
            if (e.RightButton != MouseButtonState.Pressed) {
                SelectedMarker = null;
                _drag = false;
                _mapControl.UpperLayer.Children.Remove(_line);
                _mapControl.UpperLayer.Children.Remove(_arrow);
                _mapControl.UpperLayer.Children.Remove(_panel);
            }
            else {
                var pos = e.GetPosition(_mapControl.UpperLayer);
                _line.X2 = pos.X;
                _line.Y2 = pos.Y;
                CurrentPosition = _gMap.FromLocalToLatLng((int) pos.X, (int) pos.Y);
                UIHelpers.CalculateArrow(_line, _arrow);
                Canvas.SetLeft(_panel, pos.X);
                Canvas.SetTop(_panel, pos.Y - _panel.ActualHeight);
            }
        }

        private void _mapControl_MouseDown(object sender, MouseButtonEventArgs e) {

            var marker = _gMap.Markers.FirstOrDefault(m => ((MapMarkerContainer)m.Shape).IsMouseOver);
            var pos = e.GetPosition(_mapControl.UpperLayer);

            if (marker is MapMarker ma) {
                SelectedMarker = ma;
                CurrentPosition = _gMap.FromLocalToLatLng((int)pos.X, (int)pos.Y);
            }

            if (e.ChangedButton != MouseButton.Right)
                return;
            
            if (marker is MapMarker mark) {
                _drag = true;

                _mapControl.UpperLayer.Children.Add(_line);
                _mapControl.UpperLayer.Children.Add(_arrow);
                _mapControl.UpperLayer.Children.Add(_panel);

                var start = _gMap.FromLatLngToLocal(mark.Position);

                _line.X1 = start.X;
                _line.Y1 = start.Y;
                _line.X2 = pos.X;
                _line.Y2 = pos.Y;

                Canvas.SetLeft(_panel, pos.X);
                Canvas.SetTop(_panel, pos.Y - _panel.ActualHeight);
            }
            else {
                SelectedMarker = null;
                _drag = false;
            }

        }

        protected override void RefreshElements() {
            
        }

        protected override void Reset() {
            
        }

        public UIElement Content {
            get => _panel.Content;
            set {
                _panel.Content = value;
            }
        }

        public Action<MapMarker, PointLatLng> ShiftAction {
            get => _shiftAction;
            set {
                if (_shiftAction != value) {
                    _shiftAction = value;
                    OnPropertyChanged(nameof(ShiftAction));
                }
            }
        }

        public MapMarker SelectedMarker {
            get => _selectedMarker;
            private set {
                if (_selectedMarker != value) {
                    _selectedMarker = value;
                    OnPropertyChanged(nameof(SelectedMarker));
                }
            }
        }

        public PointLatLng CurrentPosition {
            get => _currentPosition;
            set {
                if (_currentPosition != value) {
                    _currentPosition = value;
                    OnPropertyChanged(nameof(CurrentPosition));
                }
            }
        }

    }
}
