using System.Collections.ObjectModel;
using System.Globalization;

namespace GeoLayout.Domain.IO {
    public interface IFileTypeInfo {
        ReadOnlyCollection<string> GetExtensions();
        string GetDescription(CultureInfo ci);
        string[] GetGroups();
    }
}
