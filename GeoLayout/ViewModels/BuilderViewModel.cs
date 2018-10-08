
using System.Waf.Applications;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain.Data;
using GeoLayout.GeoLayoutTools;
using GeoLayout.Services;

namespace GeoLayout.ViewModels {
    public class BuilderViewModel : Model {

        private IGeoLayoutTool _selectedTool;
        private DataTemplate _selectedToolTemplate;

        public BuilderViewModel(WaypointsService waypointsService, GroupsService groupsService, GeoLayoutService geoLayoutService) {

            Tools = new IGeoLayoutTool[] {
                geoLayoutService.WaypointBuilder,
                geoLayoutService.ProfileBuilder,
                geoLayoutService.GridBuilder,
                geoLayoutService.ShiftModifier,
                geoLayoutService.RotateModifier
            };

            RunCommand = new DelegateCommand(() => SelectedGeoLayoutTool.Apply());
        }
        
        public IGeoLayoutTool[] Tools { get; }

        public IGeoLayoutTool SelectedGeoLayoutTool {
            get => _selectedTool;
            set {
                if (_selectedTool != value) {
                    _selectedTool = value;
                    SelectedToolTemplate = _selectedTool.GetTemplate();
                    RaisePropertyChanged(nameof(SelectedGeoLayoutTool));
                }
            }
        }

        public DataTemplate SelectedToolTemplate {
            get => _selectedToolTemplate;
            set {
                if (_selectedToolTemplate != value) {
                    _selectedToolTemplate = value;
                    RaisePropertyChanged(nameof(SelectedToolTemplate));
                }
            }
        }  

        public DelegateCommand RunCommand { get; }

    }
}
