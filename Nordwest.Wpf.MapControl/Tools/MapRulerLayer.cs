using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Nordwest.Wpf.Controls.Tools;

namespace Nordwest.Wpf.Controls.Layers {
    public class MapRulerLayer : BaseToolLayer {

        private enum RulerState { None = 0, SingleMarker, Full }

        private PointLatLng _l1;
        private PointLatLng _l2;

        private readonly Ellipse _p1;
        private readonly Ellipse _p2;
        private readonly Line _line;
        private readonly StackPanel _label;
        private readonly TextBlock _text;
        
        private bool _canSetMark;
        private Ellipse _selectedEllipse;

        private RulerState _state = RulerState.None;

        private Brush _lineBrush = Brushes.Red;

        public MapRulerLayer() {
            _p1 = new Ellipse { Width = 12, Height = 12, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1, Margin = new Thickness(-6) };
            _p2 = new Ellipse { Width = 12, Height = 12, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1, Margin = new Thickness(-6) };
            _line = new Line { Stroke = _lineBrush, StrokeThickness = 3 };

            _text = new TextBlock { Margin = new Thickness(3, 1, 3, 1), Text = "test", MinWidth = 40, HorizontalAlignment = HorizontalAlignment.Center };
            _label = new StackPanel { Margin = new Thickness(-20, -36, 0, 0) };

            _label.Children.Add(new Border { Child = _text, Background = Brushes.Beige, BorderThickness = new Thickness(1), BorderBrush = Brushes.DarkGray });
            _label.Children.Add(new Path { Margin = new Thickness(-1), Fill = Brushes.Beige, Stroke = Brushes.DarkGray, StrokeThickness = 1, Data = Geometry.Parse("M 10, 0 20, 10 30, 0") });
            
            Panel.SetZIndex(_label, 40);
            Panel.SetZIndex(_p1, 30);
            Panel.SetZIndex(_p2, 20);
            Panel.SetZIndex(_line, 10);
        }

        protected override void Reset() {
            _state = RulerState.None;
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

        private void _gMap_OnMapZoomChanged() {
            UpdateEllipsePosition(_l1, _p1);
            UpdateEllipsePosition(_l2, _p2);
        }

        private void _gMap_OnPositionChanged(PointLatLng point) {
            UpdateEllipsePosition(_l1, _p1);
            UpdateEllipsePosition(_l2, _p2);
        }

        private void _gMap_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateEllipsePosition(_l1, _p1);
            UpdateEllipsePosition(_l2, _p2);
        }

        private void _mapControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            _canSetMark = false;
            _selectedEllipse = null;
        }
        
        private void _mapControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var pos = e.GetPosition(_mapControl.GMapControl);

            if (_canSetMark) {
                SetNextState();
                RefreshElements();
                SetActivePoint(pos);
            }

            _selectedEllipse = null;
        }

        private void _mapControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            _canSetMark = false;
            if (_selectedEllipse != null) {
                var pos = e.GetPosition(_mapControl.UpperLayer);
                var l = SetPoint(Equals(_selectedEllipse, _p1) ? _l1 : _l2, _selectedEllipse, pos);
                if (Equals(_selectedEllipse, _p1)) {
                    _l1 = l;
                }
                else {
                    _l2 = l;
                }
            }
        }

        private void _mapControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var pos = e.GetPosition(_mapControl.UpperLayer);
            var element = _mapControl.UpperLayer.InputHitTest(pos);
            if (element is Ellipse s) {
                _selectedEllipse = s;
            }
            else {
                _canSetMark = true;
                _selectedEllipse = null;
            }
        }

        private void SetNextState() {
            if (_state == RulerState.None)
                _state = RulerState.SingleMarker;
            else if (_state == RulerState.SingleMarker)
                _state = RulerState.Full;
            else if (_state == RulerState.Full)
                _state = RulerState.SingleMarker;
        }
         

        protected override void RefreshElements() {
            
            if (_mapControl == null)
                return;

            if (!IsActive) {
                RemoveElements();
                _state = RulerState.None;
                return;
            }
            
            switch (_state) {
                case RulerState.None:
                    RemoveElements();
                    break;
                case RulerState.SingleMarker:
                    if (!_mapControl.UpperLayer.Children.Contains(_p1))
                        _mapControl.UpperLayer.Children.Add(_p1);
                    _mapControl.UpperLayer.Children.Remove(_p2);
                    _mapControl.UpperLayer.Children.Remove(_line);
                    _mapControl.UpperLayer.Children.Remove(_label);
                    break;
                case RulerState.Full:
                    if (!_mapControl.UpperLayer.Children.Contains(_label))
                        _mapControl.UpperLayer.Children.Add(_label);
                    if (!_mapControl.UpperLayer.Children.Contains(_line))
                        _mapControl.UpperLayer.Children.Add(_line);
                    if (!_mapControl.UpperLayer.Children.Contains(_p1))
                        _mapControl.UpperLayer.Children.Add(_p1);
                    if (!_mapControl.UpperLayer.Children.Contains(_p2))
                        _mapControl.UpperLayer.Children.Add(_p2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RemoveElements() {
            _mapControl.UpperLayer.Children.Remove(_p1);
            _mapControl.UpperLayer.Children.Remove(_p2);
            _mapControl.UpperLayer.Children.Remove(_line);
            _mapControl.UpperLayer.Children.Remove(_label);
        }

        private void SetActivePoint(Point p) {
            switch (_state) {
                case RulerState.None:
                    break;
                case RulerState.SingleMarker:
                    _l1 = SetPoint(_l1, _p1, p);
                    break;
                case RulerState.Full:
                    _l2 = SetPoint(_l2, _p2, p);
                    RefreshLabel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

            RefreshLine();
            RefreshLabel();
        }

        private void RefreshLine() {
            if (_state != RulerState.Full)
                return;

            _line.X1 = Canvas.GetLeft(_p1);
            _line.Y1 = Canvas.GetTop(_p1);
            _line.X2 = Canvas.GetLeft(_p2);
            _line.Y2 = Canvas.GetTop(_p2);
        }

        private void RefreshLabel() {
            if (_state != RulerState.Full)
                return;

            SetText();

            Canvas.SetLeft(_label, Canvas.GetLeft(_p2));
            Canvas.SetTop(_label, Canvas.GetTop(_p2));
        }

        private void SetText() {
            var dist = DistanceHelper.GetDistance(_l1, _l2);
            if (dist < 1000)
                _text.Text = dist.ToString("F0") + " " + Properties.Resources.MapControl_ScaleRuler_MPostfix;
            else if (dist < 1000000)
                _text.Text = (dist / 1000).ToString("G3") + " " + Properties.Resources.MapControl_ScaleRuler_KmPostfix;
            else
                _text.Text = (dist / 1000).ToString("G4") + " " + Properties.Resources.MapControl_ScaleRuler_KmPostfix; 
        }

    }
}
