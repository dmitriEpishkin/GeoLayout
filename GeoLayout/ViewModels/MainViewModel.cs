
using System.Waf.Applications;
using System.Waf.Foundation;
using GeoLayout.Domain.Data;
using GeoLayout.Services;

namespace GeoLayout {
    public class MainViewModel : Model {

        public MainViewModel(ImportService importService, ExportService exportService, WaypointsService waypointsService, GroupsService groupsService, GeoLayoutBuildingService layoutBuildingService) {

            ImportService = importService;
            ExportService = exportService;
            WaypointsService = waypointsService;
            GroupsService = groupsService;
            GeoLayoutBuildingService = layoutBuildingService;

            OpenCommand = new DelegateCommand(() => ImportService.ImportWaypoints());
            SaveCommand = new DelegateCommand(() => ExportService.ExportWaypoints());

            //var inputFile = @"C:\Users\Епишкин Дмитрий\Desktop\ВЛУ\keyPoints2.gpx";
            //var importer = new GpxImporter();
            //var keyPoints = importer.ImportWaypoints(inputFile);
            
            //foreach (var p in keyPoints) {
            //    waypointsService.Waypoints.Add(p);
            //}

        }
        
        public ImportService ImportService { get; }
        public ExportService ExportService { get; }
        public WaypointsService WaypointsService { get; }
        public GroupsService GroupsService { get; }
        public GeoLayoutBuildingService GeoLayoutBuildingService { get; }

        public DelegateCommand OpenCommand { get; }
        public DelegateCommand SaveCommand { get; }

    }
}
