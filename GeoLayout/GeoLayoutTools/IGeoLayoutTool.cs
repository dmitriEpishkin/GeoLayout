using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GeoLayout.Domain.Data {
    public interface IGeoLayoutTool {

        void Apply();
        DataTemplate GetTemplate();

        string Name { get; }
    }
}
