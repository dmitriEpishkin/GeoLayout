using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Nordwest.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ZoomControl.xaml
    /// CopyPast from RangeSlider
    /// </summary>
    [TemplatePart(Name = @"PART_Plus", Type = typeof(ButtonBase))]
    [TemplatePart(Name = @"PART_Minus", Type = typeof(ButtonBase))]
    [TemplatePart(Name = @"PART_Slider", Type = typeof(RangeBase))]
    public class ZoomControl : Control
    {
        public static readonly DependencyProperty DiscreteModeProperty =
        DependencyProperty.Register("DiscreteMode", typeof(bool), typeof(ZoomControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnDiscreteModeChanged));

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(double), typeof(ZoomControl),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged, CoerceValue));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(ZoomControl),
                new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, OnMinimumChanged, CoerceMinimum));
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(ZoomControl),
                new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, OnMaximumChanged, CoerceMaximum));

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register("SmallChange", typeof(double), typeof(ZoomControl),
                new PropertyMetadata(0d, OnSmallChangeChanged/*, CoerceSmallChange*/));
        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(double), typeof(ZoomControl),
                new PropertyMetadata(0d, OnLargeChangeChanged/*, CoerceLargeChange*/));

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(ZoomControl));

        public static readonly RoutedEvent MinimumChangedEvent =
            EventManager.RegisterRoutedEvent("MinimumChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(ZoomControl));
        public static readonly RoutedEvent MaximumChangedEvent =
            EventManager.RegisterRoutedEvent("MaximumChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(ZoomControl));

        public static readonly RoutedEvent SmallChangeChangedEvent =
            EventManager.RegisterRoutedEvent("SmallChangeChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(ZoomControl));
        public static readonly RoutedEvent LargeChangeChangedEvent =
            EventManager.RegisterRoutedEvent("LargeChangeChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(ZoomControl));


        public static readonly DependencyProperty TicksProperty =
            DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(ZoomControl), new PropertyMetadata(default(DoubleCollection)));
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(ZoomControl), new PropertyMetadata(default(double)));


        static ZoomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl), new FrameworkPropertyMetadata(typeof(ZoomControl)));
        }

        private ButtonBase _plus;
        private ButtonBase _minus;
        private RangeBase _slider;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _plus = base.GetTemplateChild(@"PART_Plus") as ButtonBase;
            _minus = base.GetTemplateChild(@"PART_Minus") as ButtonBase;
            _slider = base.GetTemplateChild(@"PART_Slider") as RangeBase;

            if (_plus != null)
            {
                _plus.Click += plus_Click;
                _plus.ToolTip = Properties.Resources.ZoomControl_ZoomInTooltip;
            }
            if (_minus != null)
            {
                _minus.Click += minus_Click;
                _minus.ToolTip = Properties.Resources.ZoomControl_ZoomOutTooltip;
            }
            if (_slider != null)
            {
                BindingOperations.SetBinding(_slider, RangeBase.ValueProperty, new Binding { Path = new PropertyPath(ValueProperty.Name), RelativeSource = RelativeSource.TemplatedParent });
                BindingOperations.SetBinding(_slider, RangeBase.MinimumProperty, new Binding { Path = new PropertyPath(MinimumProperty.Name), RelativeSource = RelativeSource.TemplatedParent });
                BindingOperations.SetBinding(_slider, RangeBase.MaximumProperty, new Binding { Path = new PropertyPath(MaximumProperty.Name), RelativeSource = RelativeSource.TemplatedParent });
                BindingOperations.SetBinding(_slider, RangeBase.LargeChangeProperty, new Binding { Path = new PropertyPath(LargeChangeProperty.Name), RelativeSource = RelativeSource.TemplatedParent });
                BindingOperations.SetBinding(_slider, RangeBase.SmallChangeProperty, new Binding { Path = new PropertyPath(SmallChangeProperty.Name), RelativeSource = RelativeSource.TemplatedParent });
                BindingOperations.SetBinding(_slider, RangeBase.MaximumProperty, new Binding { Path = new PropertyPath(MaximumProperty.Name), RelativeSource = RelativeSource.TemplatedParent });

                if (_slider is Slider)
                {
                    BindingOperations.SetBinding(_slider, Slider.TicksProperty,
                                                 new Binding
                                                 {
                                                     Path = new PropertyPath(TicksProperty.Name),
                                                     RelativeSource = RelativeSource.TemplatedParent
                                                 });
                    BindingOperations.SetBinding(_slider, Slider.TickFrequencyProperty,
                                                new Binding
                                                {
                                                    Path = new PropertyPath(TickFrequencyProperty.Name),
                                                    RelativeSource = RelativeSource.TemplatedParent
                                                });
                }
            }
        }

        public void IncreaseValue()
        {
            this.Value += this.SmallChange;
        }
        public bool CanIncreaseValue()
        {
            return this.Value < this.Maximum;
        }
        public void DecreaseValue()
        {
            this.Value -= this.SmallChange;
        }
        public bool CanDecreaseValue()
        {
            return this.Value > this.Minimum;
        }

        void minus_Click(object sender, RoutedEventArgs e)
        {
            //код для команд сразу сделал
            if (CanDecreaseValue())
                DecreaseValue();
        }

        void plus_Click(object sender, RoutedEventArgs e)
        {
            //код для команд сразу сделал
            if (CanIncreaseValue())
                IncreaseValue();
        }

        private static void OnDiscreteModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            sender.InvalidateProperty(ValueProperty);
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement)sender).RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, ValueChangedEvent));
        }


        private static void OnMinimumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var slider = (ZoomControl)sender;

            slider.InvalidateProperty(ValueProperty);
            slider.RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, MinimumChangedEvent));
        }
        private static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var slider = (ZoomControl)sender;

            slider.InvalidateProperty(ValueProperty);
            slider.RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, MaximumChangedEvent));
        }

        private static void OnSmallChangeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement)sender).RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, SmallChangeChangedEvent));
        }
        private static void OnLargeChangeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement)sender).RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, LargeChangeChangedEvent));
        }

        private static object CoerceValue(DependencyObject sender, object value)
        {
            var val = (double)value;
            
            var slider = (ZoomControl)sender;

            if (slider.DiscreteMode)
                val = Math.Round(val);

            if (val < slider.Minimum)
                val = slider.Minimum;
            if (val > slider.Maximum)
                val = slider.Maximum;

            return val;
        }


        private static object CoerceMinimum(DependencyObject sender, object value)
        {
            var val = (double)value;
            var slider = (ZoomControl)sender;

            if (val > slider.Maximum)
                val = slider.Maximum;

            return val;
        }
        private static object CoerceMaximum(DependencyObject sender, object value)
        {
            var val = (double)value;
            var slider = (ZoomControl)sender;

            if (val < slider.Minimum)
                val = slider.Minimum;

            return val;
        }

        //private static object CoerceSmallChange(DependencyObject sender, object value)
        //{
        //    var val = (double)value;
        //    var slider = (ZoomControl)sender;

        //    if (val > slider.LargeChange)
        //        val = slider.LargeChange;
        //    if (val < 0)
        //        val = 0;

        //    return val;
        //}
        //private static object CoerceLargeChange(DependencyObject sender, object value)
        //{
        //    var val = (double)value;
        //    var slider = (ZoomControl)sender;

        //    if (val < 0)
        //        val = 0;
        //    if (val < slider.SmallChange)
        //        val = slider.SmallChange;

        //    return val;
        //}

        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }


        public event RoutedPropertyChangedEventHandler<double> MinimumChanged
        {
            add { AddHandler(MinimumChangedEvent, value); }
            remove { RemoveHandler(MinimumChangedEvent, value); }
        }
        public event RoutedPropertyChangedEventHandler<double> MaximumChanged
        {
            add { AddHandler(MaximumChangedEvent, value); }
            remove { RemoveHandler(MaximumChangedEvent, value); }
        }

        public event RoutedPropertyChangedEventHandler<double> SmallChangeChanged
        {
            add { AddHandler(SmallChangeChangedEvent, value); }
            remove { RemoveHandler(SmallChangeChangedEvent, value); }
        }
        public event RoutedPropertyChangedEventHandler<double> LargeChangeChanged
        {
            add { AddHandler(LargeChangeChangedEvent, value); }
            remove { RemoveHandler(LargeChangeChangedEvent, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool DiscreteMode
        {
            get { return (bool)GetValue(DiscreteModeProperty); }
            set { SetValue(DiscreteModeProperty, DiscreteMode); }
        }


        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }
        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public DoubleCollection Ticks
        {
            get { return (DoubleCollection)GetValue(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public double TicksFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }
    }
}
