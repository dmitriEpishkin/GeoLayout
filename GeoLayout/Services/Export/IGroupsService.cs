using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Domain.Data;
using Nordwest.Collections;

namespace GeoLayout.Services.Export {
    public interface IGroupsService {

        ObservableRangeCollection<IGroupingNode> Groups { get; }
        ObservableCollectionReadOnlyWrapping<IShapeProvider> Shapes { get; }

        Group DefaultGroup { get; }
    }
}
