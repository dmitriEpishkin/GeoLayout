
using System.Windows;
using System.Windows.Controls;
using GeoLayout.Domain.Data;
using GeoLayout.GeoLayoutTools;
using GeoLayout.ViewModels;

namespace GeoLayout.Views
{
    /// <summary>
    /// Логика взаимодействия для AddPointMapView.xaml
    /// </summary>
    public partial class AddPointMapView : UserControl {

        private readonly SingleWaypointBuilder _tool;
        private readonly GeoLocationViewModel _locationModel;

        public AddPointMapView()
        {
            InitializeComponent();

            _tool = ((App) Application.Current).GeoLayoutBuildingService.WaypointBuilder;
            DataContext = _tool;

            _locationModel = (GeoLocationViewModel)CoordinatesView.DataContext;

            _tool.PropertyChanged += _tool_PropertyChanged;
            _locationModel.PropertyChanged += _locationModel_PropertyChanged;
        }

        private void _locationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName != "Location")
                return;
            _tool.SetPosition(_locationModel.Location.Latitude, _locationModel.Location.Longitude);
        }

        private void _tool_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName != "Latitude" && e.PropertyName != "Longitude")
                return;

            _locationModel.Location = new GeoLocation(_tool.Latitude, _tool.Longitude, 0.0);

        }

    }
}
