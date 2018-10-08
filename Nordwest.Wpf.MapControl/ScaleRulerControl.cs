using System.Windows;
using System.Windows.Controls;

namespace Nordwest.Wpf.Controls
{
    public class ScaleRulerControl:Control
    {
        static ScaleRulerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScaleRulerControl), new FrameworkPropertyMetadata(typeof(ScaleRulerControl)));
        }

        public static readonly DependencyProperty RulerTextProperty =
           DependencyProperty.Register("RulerText", typeof(string), typeof(ScaleRulerControl));
        public static readonly DependencyProperty RulerWidthProperty =
            DependencyProperty.Register("RulerWidth", typeof(double), typeof(ScaleRulerControl));

        /// <summary>
        /// Длина линейка
        /// </summary>
        public double RulerWidth
        {
            get { return (double)GetValue(RulerWidthProperty); }
            set { SetValue(RulerWidthProperty, value); }
        }

        /// <summary>
        /// Текст линейки
        /// </summary>
        public string RulerText
        {
            get { return (string)GetValue(RulerTextProperty); }
            set { SetValue(RulerTextProperty, value); }
        } 
    }
}