
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

            LayoutBuilderService = new GeoLayoutService(waypointsService, groupsService);
            SettingsService = new SettingsService();
            
            MainViewModel = new MainViewModel(waypointsService, groupsService, LayoutBuilderService);
            MapViewModel = new MapViewModel(waypointsService, LayoutBuilderService);
            BuilderViewModel = new BuilderViewModel(waypointsService, groupsService, LayoutBuilderService);
            GroupsViewModel = new GroupsViewModel(groupsService, waypointsService);
        }

        public GeoLayoutService LayoutBuilderService { get; }
        public SettingsService SettingsService { get; }

        public MainViewModel MainViewModel { get; }
        public BuilderViewModel BuilderViewModel { get; }
        public GroupsViewModel GroupsViewModel { get; }
        public MapViewModel MapViewModel { get; }
    }
}
