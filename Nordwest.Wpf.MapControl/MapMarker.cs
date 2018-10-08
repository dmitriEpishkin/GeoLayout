using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using GMap.NET.WindowsPresentation;

namespace Nordwest.Wpf.Controls
{
    public class MapMarker : GMapMarker
    {
        private bool _isSelected;
        private bool _isMouseOver;

        public MapMarker(MapMarkerContainer markerContainer)
            : base(MapControl.GetPointLatLong((UIElement)markerContainer.Content))
        {
            Shape = markerContainer;
            Shape.MouseEnter += (sender, args) => IsMouseOver = true;
            Shape.MouseLeave += (sender, args) => IsMouseOver = false;
            Shape.MouseDown += (sender, args) => OnMouseDown(args);

            UpdateVisibility();

            var topDescriptor = DependencyPropertyDescriptor.FromProperty(MapControl.PointLatLongProperty, typeof(FrameworkElement));
            topDescriptor.AddValueChanged(markerContainer.Content, (sender, args) => {
                Position = MapControl.GetPointLatLong(sender as FrameworkElement);
                UpdateVisibility();
            });
        }

        private void UpdateZIndex()
        {
            if (IsMouseOver) ZIndex = 999999;
            else if (IsSelected) ZIndex = 99999;
            else ZIndex = 0;
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                UpdateZIndex();
                UpdateLabel();
                UpdateVisibility();
                OnPropertyChanged(@"IsSelected");
                MarkerContainer.IsSelected = value;
            }
        }

        public bool IsMouseOver
        {
            get { return _isMouseOver; }
            set
            {
                if (_isMouseOver == value) return;
                _isMouseOver = value;
                UpdateZIndex();
                UpdateLabel();
                OnPropertyChanged(@"IsMouseOver");
            }
        }

        public event MouseButtonEventHandler MouseDown;

        protected virtual void OnMouseDown(MouseButtonEventArgs e)
        {
            var handler = MouseDown;
            handler?.Invoke(this, e);
        }

        public object Data
        {
            get
            {
                var markerItemContainer = Shape as MapMarkerContainer;
                return markerItemContainer != null ? markerItemContainer.DataContext : null;
            }
        }

        private bool _isActive = true;

        private bool _isInGroup;

        public MapMarkerContainer MarkerContainer { get { return (Shape as MapMarkerContainer); } }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                UpdateVisibility();
                OnPropertyChanged(@"IsActive");
            }
        }

        public bool IsInGroup
        {
            get { return _isInGroup; }
            set
            {
                if (_isInGroup == value) return;
                _isInGroup = value;
                UpdateVisibility();
                OnPropertyChanged(@"IsInGroup");
            }
        }

        private bool _isVisible = true;
        private LabelPlace _labelPlace;

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged(@"IsVisible");
            }

        }

        private void UpdateVisibility()
        {
            IsVisible = (IsSelected || (IsActive && !IsInGroup)) && (Position.Lat != 0 || Position.Lng != 0);
            MarkerContainer.Visibility = IsVisible ? Visibility.Visible : Visibility.Hidden;
        }

        public Point PointPosition
        {
            get
            {
                //return new Point(50, 50);
                return new Point(Position.Lat, Position.Lng);
            }
        }

        public LabelPlace LabelPlace
        {
            get { return _labelPlace; }
            set
            {
                if (_labelPlace == value) return;
                _labelPlace = value;
                UpdateLabel();
                OnPropertyChanged(@"LabelPlace");
            }
        }

        public void UpdateLabel()
        {
            var element = MarkerContainer.LabelContent as FrameworkElement;
            if (element != null)
                element.Visibility = 
                    IsMouseOver || 
                    //IsSelected || 
                    LabelPlace != LabelPlace.None ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}