
using System.Windows;
using GeoLayout.Services;
using GeoLayout.ViewModels;

namespace GeoLayout {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {

        public App() {

            var waypointsService = new WaypointsService();
            var groupsService = new GroupsService(waypointsService);

            GeoLayoutBuildingService = new GeoLayoutBuildingService(waypointsService, groupsService);
            SettingsService = new SettingsService();
            
            MainViewModel = new MainViewModel(waypointsService, groupsService, GeoLayoutBuildingService);
            MapViewModel = new MapViewModel(waypointsService, GeoLayoutBuildingService);
            GroupsViewModel = new GroupsViewModel(groupsService, waypointsService);
        }

        public GeoLayoutBuildingService GeoLayoutBuildingService { get; }
        public SettingsService SettingsService { get; }

        public MainViewModel MainViewModel { get; }
        public GroupsViewModel GroupsViewModel { get; }
        public MapViewModel MapViewModel { get; }
    }
}
