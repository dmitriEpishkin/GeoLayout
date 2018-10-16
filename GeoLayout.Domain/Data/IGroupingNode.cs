
using System.ComponentModel;
using Nordwest.Collections;

namespace GeoLayout.Domain.Data {

    public interface IGroupingNode : INotifyPropertyChanged {
        
        string Name { get; set; }

        bool IsVisible { get; set; }
        bool IsSelected { get; set; }

        ObservableRangeCollection<IGroupingNode> Children { get; }
    }

}
