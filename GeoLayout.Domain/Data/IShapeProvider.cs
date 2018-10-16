
using System.Collections.ObjectModel;

namespace GeoLayout.Domain.Data {
    public interface IShapeProvider {
        ReadOnlyCollection<GeoLocation> Shape { get; }
    }
}
