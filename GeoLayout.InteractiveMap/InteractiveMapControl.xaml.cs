using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeoLayout.InteractiveMap {
    /// <summary>
    /// Логика взаимодействия для InteractiveMapControl.xaml
    /// </summary>
    public partial class InteractiveMapControl : UserControl {

        public InteractiveMapControl() {
            InitializeComponent();
        }

        public static readonly DependencyProperty InteractivePointsProperty = DependencyProperty.Register("InteractivePoints", typeof(ObservableCollection<InteractivePoint>), typeof(InteractiveMapControl), new PropertyMetadata(InteractivePointsChangedCallback));

        private static void InteractivePointsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var map = (InteractiveMapControl) d;
            if (e.OldValue != null) {
                map.InteractivePoints.CollectionChanged -= InteractivePoints_CollectionChanged;
            }
            if (e.NewValue != null) {
                map.InteractivePoints.CollectionChanged += InteractivePoints_CollectionChanged;
            }
        }

        private static void InteractivePoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            throw new NotImplementedException();
        }

        public ObservableCollection<InteractivePoint> InteractivePoints {
            get => (ObservableCollection<InteractivePoint>)GetValue(InteractivePointsProperty);
            set => SetValue(InteractivePointsProperty, value);
        }

    }
}
