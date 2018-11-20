
using System.Collections.ObjectModel;
using System.Waf.Applications;
using GeoLayout.Domain.Data;
using Nordwest.Collections;

namespace GeoLayout.Services.Export {
    public interface IWaypointsService {

        ObservableRangeCollection<Waypoint> Waypoints { get; } 
        ObservableCollectionReadOnlyWrapping<Waypoint> VisibleWaypoints { get; }
        ObservableCollectionReadOnlyWrapping<Waypoint> SelectedWaypoints { get; }

        DelegateCommand RemoveCommand { get; }
    }
}
