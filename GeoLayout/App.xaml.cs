
using System.Collections.Generic;
using System.Windows;
using GeoLayout.Domain.IO;
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
            
            var importService = new ImportService(
                new List<IGeoImporter>(new [] {
                    new GpxImporter() 
                }), 
                new MultiFileDialogService(Current.MainWindow), 
                waypointsService);

            var exportService = new ExportService(
                new List<IGeoExporter>(new [] {
                    new GpxExporter() 
                }), 
                new SaveFileDialogService(Current.MainWindow), 
                waypointsService);

            MainViewModel = new MainViewModel(importService, exportService, waypointsService, groupsService, GeoLayoutBuildingService);
            MapViewModel = new MapViewModel(waypointsService, groupsService, GeoLayoutBuildingService);
            GroupsViewModel = new GroupsViewModel(groupsService, waypointsService);
        }

        public GeoLayoutBuildingService GeoLayoutBuildingService { get; }
        public SettingsService SettingsService { get; }

        public MainViewModel MainViewModel { get; }
        public GroupsViewModel GroupsViewModel { get; }
        public MapViewModel MapViewModel { get; }
    }
}
