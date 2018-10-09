using System;
using System.Windows;
using System.Windows.Controls;
using GMap.NET;

namespace Nordwest.Wpf.Controls.Tools {
    public abstract class BaseCommandToolLayer : BaseToolLayer {

        protected readonly MapToolPanel Panel;

        public BaseCommandToolLayer(MapToolPanel panel) {
            Panel = panel;
        }

        public Button Ok => Panel.Ok;
        public Button Close => Panel.Close;

        public UIElement PanelUI {
            get => Panel.Content;
            set => Panel.Content = value;
        }

    }
}
