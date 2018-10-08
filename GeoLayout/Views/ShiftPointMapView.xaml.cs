using System;
using System.Collections.Generic;
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
using GeoLayout.GeoLayoutTools;
using GeoLayout.ViewModels;

namespace GeoLayout.Views {
    /// <summary>
    /// Логика взаимодействия для ShiftPointMapView.xaml
    /// </summary>
    public partial class ShiftPointMapView : UserControl {

        private ShiftModifier _tool;
        private GeoLocationViewModel _model;

        public ShiftPointMapView() {
            InitializeComponent();

            _tool = ((App) Application.Current).LayoutBuilderService.ShiftModifier; 
            _model = (GeoLocationViewModel)CoordinatesView.DataContext;

            _tool.PropertyChanged += _tool_PropertyChanged;
        }

        private void _tool_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
           if (e.PropertyName != @"Location")
               return;

            _model.Location = _tool.Location;
        }
    }
}
