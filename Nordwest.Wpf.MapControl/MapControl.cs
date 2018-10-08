using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Windows.Threading;
using Nordwest.Wpf.Controls.Clustering;
using Nordwest.Wpf.Controls.Layers;
using Nordwest.Wpf.Controls.Tools;

namespace Nordwest.Wpf.Controls
{
    [TemplatePart(Name = @"GMapControl", Type = typeof(GMapControl))]
    [TemplatePart(Name = @"UpperLayer", Type = typeof(Canvas))]
    public class MapControl : Control
    {
        public static readonly string MapCacheLocation = Directory.GetCurrentDirectory() + @"\MapCache\";

        private GMapControl _gMap;
        private Canvas _upperLayer;
        private ZoomControl _zoomControl;
        private ScaleRulerControl _scaleRuler;
        private UIElement _showAllControl;
        private MapPrefetcher _prefetcher;

        private bool _clustersUpdating = false;
        private bool _needClustersUpdating = false;
         
        private readonly OneTimeMointor _addMarkerMonitor = new OneTimeMointor();

        private readonly List<BaseToolLayer> _registeredTools = new List<BaseToolLayer>();

        public MapControl()
        {
            Tools.CollectionChanged += Tools_CollectionChanged;
            SelectedItems.CollectionChanged += SelectedItemsCollectionChanged;
        }

        private void Tools_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var t in e.NewItems.OfType<BaseToolLayer>()) {
                        RegisterTool(t);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var t in e.OldItems.OfType<BaseToolLayer>()) {
                        RemoveTool(t);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var t in _registeredTools.ToArray())
                        RemoveTool(t);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RegisterTool(BaseToolLayer t) {
            t.MapControl = this;
            _registeredTools.Add(t);
            t.PropertyChanged += TOnPropertyChanged;
        }

        private void RemoveTool(BaseToolLayer t) {
            if (t.MapControl == this)
                t.MapControl = null;
            _registeredTools.Remove(t);
            t.PropertyChanged -= TOnPropertyChanged;
        }

        private void TOnPropertyChanged(object sender, PropertyChangedEventArgs args) {

            if (args.PropertyName == "IsActive") {
                var tool = (BaseToolLayer) sender;
                if (tool.IsActive) {
                    foreach (var t in _registeredTools) {
                        if (!Equals(t, tool))
                            t.IsActive = false;
                    }
                }
            }

            else if (args.PropertyName == "MapControl") {
                var tool = (BaseToolLayer)sender;
                if (tool.MapControl != this)
                    RemoveTool(tool);
            }

        }

        static MapControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapControl), new FrameworkPropertyMetadata(typeof(MapControl)));

            #region YANDEX FIX
            var up1 = typeof(YandexSatelliteMapProvider).GetField("UrlFormat", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var urlMask1 = (string)up1.GetValue(YahooSatelliteMapProvider.Instance);
            up1.SetValue(YahooSatelliteMapProvider.Instance, urlMask1.Replace(@".ru", @".net"));

            var up2 = typeof(YandexHybridMapProvider).GetField("UrlFormat", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var urlMask2 = (string)up2.GetValue(YandexHybridMapProvider.Instance);
            up2.SetValue(YandexHybridMapProvider.Instance, urlMask2.Replace(@".ru", @".net"));

            var up3 = typeof(YandexMapProvider).GetField("UrlFormat", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var urlMask3 = (string)up3.GetValue(YandexMapProvider.Instance);
            up3.SetValue(YandexMapProvider.Instance, urlMask3.Replace(@".ru", @".net"));
            #endregion
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            GMapControl = GetTemplateChild(@"GMapControl") as GMapControl;
            _upperLayer = GetTemplateChild(@"UpperLayer") as Canvas;

            if (_gMap != null)
            {
                _gMap.ShowCenter = false;
                _gMap.CacheLocation = MapCacheLocation;
                _gMap.LevelsKeepInMemmory = 12;
                _gMap.Manager.Mode = AccessMode.ServerAndCache;
                _gMap.Position = new PointLatLng(62.197471, 99.123851);

                BindingOperations.SetBinding(_gMap, GMapControl.MapPointProperty, new Binding() { Source = this, Path = new PropertyPath(@"MapPosition") });

                _gMap.EmptyTileBorders = new Pen(Brushes.Transparent, 0);
                _gMap.EmptyTileText =
                    new FormattedText(Properties.Resources.MapControl_EmptyTileText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                      new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), 12, new SolidColorBrush(Colors.DimGray));
                _gMap.EmptytileBrush = new SolidColorBrush(Colors.LightGray);

                _gMap.IgnoreMarkerOnMouseWheel = true;
                _gMap.DragButton = MouseButton.Left;
                
                _gMap.OnMapZoomChanged += _gMap_OnMapZoomChanged;
                _gMap.OnPositionChanged += p =>
                {
                    OnMapResolutionChanged();
                    OnPositionChanged();
                };
                _addMarkerMonitor.EventFired += _gMap_LayoutUpdated;
                _gMap.LayoutUpdated += _addMarkerMonitor.Fire;
                _prefetcher = new MapPrefetcher(_gMap);
            }

            if (_upperLayer != null) {
                _upperLayer.ClipToBounds = true;
            }

            foreach (var l in Layers) {
                l.MapControl = this;
                l.ResetLayer();
            }

            _scaleRuler = (ScaleRulerControl)GetTemplateChild(@"ScaleRuler");
            _zoomControl = (ZoomControl)GetTemplateChild(@"Zoom");
            _showAllControl = (UIElement)GetTemplateChild(@"ShowAll");

            if (_zoomControl != null && _gMap != null)
            {
                _zoomControl.Maximum = _gMap.MaxZoom;
                _zoomControl.Minimum = _gMap.MinZoom;
            }
        }

        void _gMap_OnMapZoomChanged()
        {
            if (Layers == null)
                return;

            if (_clustersUpdating) {
                _needClustersUpdating = true;
                return;
            }

            _clustersUpdating = true;

            var ls = Layers;
            var showLabel = ShowLabel;

            new Task(() => {

                foreach (var l in ls)
                    l.UpdateClusters();

                if (showLabel) {
                    LabelPlaceHelper.PlaceLabels(_gMap.Markers.Where(m => m is MapMarker && ((MapMarker)m).IsVisible).Select(m => (MapMarker)m).ToList());
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                    OnMapResolutionChanged();
                    ZoomLevel = (int) _gMap.Zoom;
                    OnZoomChanged();
                    _clustersUpdating = false;

                    if (_needClustersUpdating) {
                        _needClustersUpdating = false;
                        _gMap_OnMapZoomChanged();
                    }
                }));
            }).Start();

        }
        
        void _gMap_LayoutUpdated(object sender, EventArgs e) {
            _gMap_OnMapZoomChanged();
        }
        
        public void ActivateAddMarkerMonitor() {
            _addMarkerMonitor.Activate();
        }

        public void SelectMarker(MapMarker mapMarker, bool keepPrev)
        {
            if (!keepPrev)
            {
                SelectedItems.Clear();
            }
            if (!SelectedItems.Contains(mapMarker.Data))
                SelectedItems.Add(mapMarker.Data);
        }

        private class OneTimeMointor
        {
            private bool _isActive;
            public void Activate() { _isActive = true; }

            public void Fire(object sender, EventArgs args)
            {
                if (!_isActive) return;
                _isActive = false;
                var handler = EventFired;
                if (handler != null) handler(sender, args);
            }

            public event EventHandler EventFired;
        }

        public void UpdateZoomLayout() {
            _gMap_OnMapZoomChanged();
        }

        void OnMapResolutionChanged()
        {
            if (_scaleRuler == null) return;

            var prov = _gMap.MapProvider;
            var rez = prov.Projection.GetGroundResolution((Int32)Math.Round(_gMap.Zoom), _gMap.Position.Lat);

            var distanceInPixels = Math.Min(Math.Round(_gMap.ActualWidth / 4), 100);
            var desizedWidthInPixels = distanceInPixels;
            var distanceInM = rez * distanceInPixels;

            var pow = Math.Pow(10, Math.Floor(Math.Log10(distanceInM)));
            var temp = (Int32)Math.Ceiling(distanceInM / pow);


            if (temp > 5)
                temp = 10;
            if (temp > 2)
                temp = 5;

            distanceInM = Math.Round(temp * pow);
            distanceInPixels = distanceInM / rez;

            string postfix = Properties.Resources.MapControl_ScaleRuler_MPostfix;

            if (distanceInM >= 1000)
            {
                distanceInM = Math.Round(distanceInM / 1000.0);
                postfix = Properties.Resources.MapControl_ScaleRuler_KmPostfix;
            }

            if (double.IsNaN(distanceInPixels))
            {
                _scaleRuler.RulerWidth = desizedWidthInPixels;
                _scaleRuler.RulerText = Properties.Resources.MapControl_ScaleRuler_ErrorText;
            }
            else
            {
                _scaleRuler.RulerWidth = distanceInPixels;
                _scaleRuler.RulerText = distanceInM + @" " + postfix;
            }
        }

        public void ShowAll()
        {
            if (_gMap == null)
                return;

            var rect = GetRectOfAllMarkers();
            //var rect = _gMap.GetRectOfAllMarkers(null);
            if (rect.HasValue &&
                !double.IsNaN(rect.Value.Lat) && !double.IsInfinity(rect.Value.Lat) &&
                !double.IsNaN(rect.Value.Lng) && !double.IsInfinity(rect.Value.Lng) &&
                !double.IsNaN(rect.Value.WidthLng) && !double.IsInfinity(rect.Value.WidthLng) &&
                !double.IsNaN(rect.Value.HeightLat) && !double.IsInfinity(rect.Value.HeightLat) &&
                RectLatLng.FromLTRB(-180, 90, 180, -90).Contains(rect.Value))
                _gMap.SetZoomToFitRect(rect.Value);
        }

        public RectLatLng? GetRectOfAllMarkers()
        {
            //стремная, слегка измененная(чтобы учитывать невилимые/сгруппированные точки) копипаста из MapControl
            RectLatLng? rect = null;
            var leftLng = double.MaxValue;
            var topLat = double.MinValue;
            var rightLng = double.MinValue;
            var bottomLat = double.MaxValue;
            foreach (var marker in _gMap.Markers.OfType<MapMarker>().Where(m => m.IsVisible))
            {
                if (marker.Position.Lng < leftLng)
                    leftLng = marker.Position.Lng;
                if (marker.Position.Lat > topLat)
                    topLat = marker.Position.Lat;
                if (marker.Position.Lng > rightLng)
                    rightLng = marker.Position.Lng;
                if (marker.Position.Lat < bottomLat)
                    bottomLat = marker.Position.Lat;
            }
            if (leftLng != double.MaxValue && rightLng != double.MinValue && topLat != double.MinValue &&
                bottomLat != double.MaxValue)
                rect = RectLatLng.FromLTRB(leftLng, topLat, rightLng, bottomLat);
            return rect;
        }

        public void ShowExportDialog()
        {
            _gMap.ShowExportDialog();
        }
        public void ShowImportDialog()
        {
            _gMap.ShowImportDialog();
        }

        public bool SetZoomToFitRect(RectLatLng rect)
        {
            return _gMap.SetZoomToFitRect(rect);
        }

        public PointLatLng FromLocalToLatLng(int x, int y)
        {
            return _gMap.FromLocalToLatLng(x, y);
        }

        public GPoint FromLatLngToLocal(PointLatLng p)
        {
            return _gMap.FromLatLngToLocal(p);
        }

        public int MaxZoom { get { return _gMap.MaxZoom; } }
        public int MinZoom { get { return _gMap.MinZoom; } }
        
        public static readonly DependencyProperty PointLatLongProperty =
            DependencyProperty.RegisterAttached("PointLatLong", typeof(PointLatLng), typeof(MapControl), new FrameworkPropertyMetadata(default(PointLatLng), PointLatLongChangedCallback));

        private static void PointLatLongChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            // var c = new ContentControl();
        }

        public static void SetPointLatLong(UIElement element, PointLatLng value)
        {
            element.SetValue(PointLatLongProperty, value);
        }

        public static PointLatLng GetPointLatLong(UIElement element)
        {
            return (PointLatLng)element.GetValue(PointLatLongProperty);
        }

        public ObservableCollection<BaseToolLayer> Tools { get; } = new ObservableCollection<BaseToolLayer>();

        public static readonly DependencyProperty LayersProperty =
            DependencyProperty.Register("Layers", typeof(ObservableCollection<IGMapElementsLayer>), typeof(MapControl), new PropertyMetadata(default(ObservableCollection<IGMapElementsLayer>)));
                
        public void UpdateLayersIndexes() {
            var ind = 0;
            foreach (var l in Layers) {
                ind += l.ElementsCount;
                l.LastElementIndex = ind;
            }
        }

        public ObservableCollection<IGMapElementsLayer> Layers {
            get { return (ObservableCollection<IGMapElementsLayer>)GetValue(LayersProperty); }
            set { SetValue(LayersProperty, value); }
        }

        private static readonly DependencyPropertyKey SelectedItemsPropertyKey =
            DependencyProperty.RegisterReadOnly("SelectedItems", typeof(ObservableCollection<object>), typeof(MapControl), new FrameworkPropertyMetadata(new ObservableCollection<object>()));
        public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;
        
        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Layers == null)
                return;
            foreach (var l in Layers.OfType<MapMarkersLayer>()) {
                if (l.MarkersTree == null)
                    return;

                switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems) {
                            var mapMarker = l.MarkersTree.Clusters.SelectMany(c => c).FirstOrDefault(m => m.Data == item);
                            if (mapMarker != null)
                                mapMarker.IsSelected = true;
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems) {
                            var mapMarker = l.MarkersTree.Clusters.SelectMany(c => c).FirstOrDefault(m => m.Data == item);
                            if (mapMarker != null)
                                mapMarker.IsSelected = false;
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (var mapMarker in l.MarkersTree.Clusters.SelectMany(c => c))
                            mapMarker.IsSelected = false;
                        SelectedItemsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SelectedItems));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            LabelPlaceHelper.PlaceLabels(_gMap.Markers.Where(m => m is MapMarker && ((MapMarker) m).IsVisible)
                .Select(m => (MapMarker) m).ToList());
        }

        public ObservableCollection<object> SelectedItems {
            get { return (ObservableCollection<object>)GetValue(SelectedItemsProperty); }
        }
        
        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register("ShowLabel", typeof(bool), typeof(MapControl), new FrameworkPropertyMetadata(true, ShowLabelChanged));

        private static void ShowLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var control = (MapControl)d;
            if (control.Layers == null)
                return;
            foreach (var l in control.Layers.OfType<MapMarkersLayer>())
                l.ResetLayer();
        }

        public bool ShowLabel {
            get { return (bool)GetValue(ShowLabelProperty); }
            set { SetValue(ShowLabelProperty, value); }
        }

        public static readonly DependencyProperty MapProviderProperty =
            DependencyProperty.Register("MapProvider", typeof(GMapProvider), typeof(MapControl), new FrameworkPropertyMetadata(GMapProviders.EmptyProvider, MapProviderChanged));

        private static void MapProviderChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (MapControl)dependencyObject;
            if (control._zoomControl != null && control._gMap != null)
            {
                control._zoomControl.Maximum = control._gMap.MaxZoom;
                control._zoomControl.Minimum = control._gMap.MinZoom;
            }
        }

        public GMapProvider MapProvider
        {
            get { return (GMapProvider)GetValue(MapProviderProperty); }
            set { SetValue(MapProviderProperty, value); }
        }

        public static readonly DependencyProperty AccessModeProperty =
            DependencyProperty.Register("AccessMode", typeof(AccessMode), typeof(MapControl), new FrameworkPropertyMetadata(default(AccessMode), OnAccessModeChanged));

        private static void OnAccessModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((MapControl)dependencyObject)._gMap.Manager.Mode = (AccessMode)dependencyPropertyChangedEventArgs.NewValue;
        }

        public AccessMode AccessMode
        {
            get { return (AccessMode)GetValue(CacheModeProperty); }
            set { SetValue(CacheModeProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(int), typeof(MapControl), new FrameworkPropertyMetadata(default(int), OnZoomLevelChanged));

        private static void OnZoomLevelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((MapControl)dependencyObject)._gMap.Zoom = (int)dependencyPropertyChangedEventArgs.NewValue;
        }

        public int ZoomLevel
        {
            get { return (int)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        public static readonly DependencyProperty MapPositionProperty =
            DependencyProperty.Register("MapPosition", typeof(Point), typeof(MapControl), new PropertyMetadata(default(Point)));

        public Point MapPosition
        {
            get { return (Point)GetValue(MapPositionProperty); }
            set { SetValue(MapPositionProperty, value); }
        }

        public static readonly DependencyProperty ShowScaleControlProperty = DependencyProperty.Register(
            "ShowScaleControl", typeof(bool), typeof(MapControl), new FrameworkPropertyMetadata(true, ShowScaleControlPropertyChanged));

        private static void ShowScaleControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs baseValue) {
            var map = (MapControl)d;
            if (map._scaleRuler == null)
                return;
            if (map.ShowScaleControl)
                map._scaleRuler.Visibility = Visibility.Visible;
            else
                map._scaleRuler.Visibility = Visibility.Hidden;            
        }

        public bool ShowScaleControl {
            get { return (bool)GetValue(ShowScaleControlProperty); }
            set { SetValue(ShowScaleControlProperty, value); }
        }

        public static readonly DependencyProperty ShowZoomControlProperty = DependencyProperty.Register(
            "ShowZoomControl", typeof(bool), typeof(MapControl), new FrameworkPropertyMetadata(true, ShowZoomControlPropertyChanged));

        private static void ShowZoomControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs baseValue) {
            var map = (MapControl)d;
            if (map._zoomControl == null || map._showAllControl == null)
                return;
            if (map.ShowZoomControl) {
                map._zoomControl.Visibility = Visibility.Visible;
                map._showAllControl.Visibility = Visibility.Visible;
            }
            else {
                map._zoomControl.Visibility = Visibility.Hidden;
                map._showAllControl.Visibility = Visibility.Hidden;
            }
        }

        public bool ShowZoomControl {
            get { return (bool)GetValue(ShowZoomControlProperty); }
            set { SetValue(ShowZoomControlProperty, value); }
        }

        #region Commands
        public static readonly DependencyProperty ShowAllCommandProperty =
            DependencyProperty.Register("ShowAllCommand", typeof(ICommand), typeof(MapControl), new PropertyMetadata(default(ICommand)));

        public ICommand ShowAllCommand
        {
            get { return (ICommand)GetValue(ShowAllCommandProperty); }
            set { SetValue(ShowAllCommandProperty, value); }
        }

        public static readonly DependencyProperty ZoomInCommandProperty =
            DependencyProperty.Register("ZoomInCommand", typeof(ICommand), typeof(MapControl), new PropertyMetadata(default(ICommand)));

        public ICommand ZoomInCommand
        {
            get { return (ICommand)GetValue(ZoomInCommandProperty); }
            set { SetValue(ZoomInCommandProperty, value); }
        }

        public static readonly DependencyProperty ZoomOutCommandProperty =
            DependencyProperty.Register("ZoomOutCommand", typeof(ICommand), typeof(MapControl), new PropertyMetadata(default(ICommand)));


        public ICommand ZoomOutCommand
        {
            get { return (ICommand)GetValue(ZoomOutCommandProperty); }
            set { SetValue(ZoomOutCommandProperty, value); }
        }

        public MapPrefetcher Prefetcher
        {
            get { return _prefetcher; }
        }

        #endregion

        public event EventHandler PositionChanged;

        protected virtual void OnPositionChanged()
        {
            var handler = PositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ZoomChanged;

        protected virtual void OnZoomChanged() {
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        //адский ад!!!
        public GMapControl GMapControl {
            get => _gMap;
            set {
                if (!Equals(_gMap, value)) {
                    _gMap = value;
                    GMapControlSet?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler GMapControlSet;

        public Canvas UpperLayer => _upperLayer;

    }
}