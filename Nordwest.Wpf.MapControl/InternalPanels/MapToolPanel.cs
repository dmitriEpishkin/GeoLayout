
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Nordwest.Wpf.Controls {
    public class MapToolPanel : StackPanel {

        private readonly Border _border;
        private readonly Path _shape;

        private readonly StackPanel _borderContent;

        private readonly StackPanel _buttons;

        private readonly Button _ok;
        private readonly Button _close;

        private UIElement _content;

        public MapToolPanel() : this(true) { }

        public MapToolPanel(bool showButtons) {

            if (showButtons) {
                _ok = new Button {Width = 18, Height = 18};
                _close = new Button {Width = 18, Height = 18};
            }

            _border = new Border { Background = Brushes.Beige, BorderThickness = new Thickness(1), BorderBrush = Brushes.DarkGray };
            _shape = new Path { Margin = new Thickness(-1.5), Fill = Brushes.Beige, Stroke = Brushes.DarkGray, StrokeThickness = 1, Data = Geometry.Parse("M 10, 0 20, 10 30, 0") };
            
            _borderContent = new StackPanel { Orientation = Orientation.Horizontal };
            
            if (showButtons) {
                _buttons = new StackPanel();
                _buttons.Children.Add(_ok);
                _buttons.Children.Add(_close);
                _borderContent.Children.Add(_buttons);
            }

            _border.Child = _borderContent;

            Children.Add(_border);
            Children.Add(_shape);

            Margin = new Thickness(-19, 0, 0, 0);
        }

        public UIElement Content {
            get => _content;
            set {
                if (_content != null)
                    _borderContent.Children.Remove(_content);
                _content = value;
                if (_content != null)
                    _borderContent.Children.Insert(0, _content);
            }
        }

        public Button Ok => _ok;
        public Button Close => _close;

    }
}
