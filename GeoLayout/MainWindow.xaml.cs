using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeoLayout.Domain.Data;
using GeoLayout.Views;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Nordwest.Collections.Synchronization;
using Nordwest.Wpf.Controls;
using Nordwest.Wpf.Controls.Layers;
using Nordwest.Wpf.Controls.Tools;

namespace GeoLayout {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private readonly MainViewModel _model;

        public MainWindow() {
            InitializeComponent();
           
            _model = ((App)Application.Current).MainViewModel;
            DataContext = _model;
        
        }

    }
}
