using System;
using System.Collections.Generic;
using System.Waf.Foundation;
using System.Windows;
using GeoLayout.Domain.Data;

namespace GeoLayout.GeoLayoutTools {
    public class RotateModifier : Model, IGeoLayoutTool {


        public DataTemplate GetTemplate() {
            ResourceDictionary templates = ResourceUtil.GetRelativeResourceDictionary(@"Templates\GeoLayoutToolsTemplate.xaml");
            return (DataTemplate)templates["RotateModifierTemplate"];
        }

        public void Apply() {

        }

        public string Name => "Повернуть";
        
    }
}
