using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Nordwest.Wpf.Controls
{
    public class MapMarkerContainer : ContentControl
    {
        private FrameworkElement _labelContentContainer;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _labelContentContainer = (FrameworkElement)GetTemplateChild(@"LabelContent");
            if (_labelContentContainer != null)
                UpdateLabelPosition(LabelOffset);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MapMarkerContainer), new PropertyMetadata(default(bool)));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty LabelOffsetProperty =
            DependencyProperty.Register("LabelOffset", typeof(Point), typeof(MapMarkerContainer), new FrameworkPropertyMetadata(default(Point), LabelOffsetChangedCallback));

        private static void LabelOffsetChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (MapMarkerContainer) dependencyObject;
            var newValue = (Point) dependencyPropertyChangedEventArgs.NewValue;

            control.UpdateLabelPosition(newValue);
            //control.StartLabelAnimation(newValue);
            control.RaiseEvent(new RoutedEventArgs(LabelOffsetChangedEvent));
        }

        private void UpdateLabelPosition(Point newValue)
        {
            var label = _labelContentContainer;
            if (label == null) return;
            label.SetValue(Canvas.LeftProperty, newValue.X);
            label.SetValue(Canvas.TopProperty, newValue.Y);
        }

        private void StartLabelAnimation(Point newValue)
        {
            if (_labelContentContainer == null) return;

            var animation1 = new DoubleAnimation(newValue.X, new Duration(TimeSpan.FromSeconds(0.5)));
            var animation2 = new DoubleAnimation(newValue.Y, new Duration(TimeSpan.FromSeconds(0.5)));

            Storyboard.SetTarget(animation1, _labelContentContainer);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(Canvas.LeftProperty));

            Storyboard.SetTarget(animation2, _labelContentContainer);
            Storyboard.SetTargetProperty(animation2, new PropertyPath(Canvas.TopProperty));
            Storyboard sb = new Storyboard();
            sb.Children.Add(animation1);
            sb.Children.Add(animation2);
            sb.Begin();
        }

        public static readonly RoutedEvent LabelOffsetChangedEvent = EventManager.RegisterRoutedEvent(
            "LabelOffsetChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapMarkerContainer));

        // Provide CLR accessors for the event
        public event RoutedEventHandler LabelOffsetChanged
        {
            add { AddHandler(LabelOffsetChangedEvent, value); }
            remove { RemoveHandler(LabelOffsetChangedEvent, value); }
        }

        public Point LabelOffset
        {
            get { return (Point)GetValue(LabelOffsetProperty); }
            set { SetValue(LabelOffsetProperty, value); }
        }

        public FrameworkElement DataElement { get { return (FrameworkElement)Content; } }

        public static readonly DependencyProperty LabelContentProperty =
            DependencyProperty.Register("LabelContent", typeof(object), typeof(MapMarkerContainer), new PropertyMetadata(default(object)));
        
        public object LabelContent
        {
            get { return (object)GetValue(LabelContentProperty); }
            set { SetValue(LabelContentProperty, value); }
        }
    }
}