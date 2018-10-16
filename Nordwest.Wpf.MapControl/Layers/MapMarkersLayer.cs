using GMap.NET.WindowsPresentation;
using Nordwest.Wpf.Controls.Clustering;
using Nordwest.Wpf.Controls.Layers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Nordwest.Wpf.Controls {
    public class MapMarkersLayer : Control, INotifyPropertyChanged, IGMapElementsLayer {
        
        protected MapControl _mapControl;
        protected GMapControl _gMap;
        
        private bool _needToUpdateClusteringSize = false;
        private int _clusteringXSize = 0;
        private int _clusteringYSize = 0;
        private MarkerClusterer _clusterer;
        
        private int _lastInd = 0;

        public MapMarkersLayer() {
            WakeUp();
        }

        private bool _isSleeping = true;
        public void Sleep() {
            if (!_isSleeping) {
                SelectedItems.CollectionChanged -= SelectedItemsCollectionChanged;
                _isSleeping = true;
            }
        }

        public void WakeUp() {
            if (_isSleeping) {
                SelectedItems.CollectionChanged += SelectedItemsCollectionChanged;
                SelectedItemsCollectionChanged(SelectedItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                _isSleeping = false;
            }
        }

        private MapMarker CreateMarker(object sourceItem) {
            var template = ItemTemplateSelector != null
                ? ItemTemplateSelector.SelectTemplate(sourceItem, null) ?? ItemTemplate
                : ItemTemplate;

            if (template == null)
                return null;

            var dataElement = template.LoadContent() as FrameworkElement;
            if (dataElement == null)
                return null;

            var labelTemplate = _mapControl.ShowLabel ? (LabelTemplateSelector != null
                ? LabelTemplateSelector.SelectTemplate(sourceItem, null) ?? LabelTemplate
                : LabelTemplate) : null;

            var container = new MapMarkerContainer();
            container.SetValue(StyleProperty, MarkerContainerStyle);
            container.Content = dataElement;
            container.DataContext = sourceItem;

            if (labelTemplate != null)
                container.LabelContent = labelTemplate.LoadContent() as FrameworkElement;

            var marker = new MapMarker(container);
            marker.MouseDown += (sender, args) => SelectMarker((MapMarker)sender,
                             Keyboard.IsKeyDown(Key.LeftShift) || (Keyboard.IsKeyDown(Key.RightShift) ||
                             Keyboard.IsKeyDown(Key.LeftCtrl) || (Keyboard.IsKeyDown(Key.RightCtrl))));
            return marker;
        }

        public void SelectMarker(MapMarker mapMarker, bool keepPrev) {
            if (!keepPrev) {
                SelectedItems.Clear();
            }
            if (!SelectedItems.Contains(mapMarker.Data))
                SelectedItems.Add(mapMarker.Data);
        }

        private void AddMarkers(IEnumerable sourceItems, bool afterReset) {
            var source = sourceItems.Cast<object>().ToList();
            if (!source.Any())
                return;
            _mapControl.ActivateAddMarkerMonitor();
            var c = _gMap.Markers.Count;
            var newMarkers = source.Select(CreateMarker).ToList();
            int count = 0;
            foreach (var item in newMarkers) {
                if (item != null) {
                    var ind = LastElementIndex + count;
                    _gMap.Markers.Insert(ind, item);
                    count++;
                }
            }
            
            UpdateClusteringGridSize();

            try {
                Dispatcher.BeginInvoke(new Action(() => {
                    _clusterer.AddMarkers(newMarkers);
                    //_mapControl.UpdateLayersIndexes();
                    _mapControl.UpdateZoomLayout();
                    if (!afterReset && c == 0 && _gMap.Markers.Count != 0)                        
                        _mapControl.ShowAll();
                    LastElementIndex += count;
                }), DispatcherPriority.DataBind);
            }
            catch (Exception) { }
                        
        }

        private void ClearMarkers() {
            foreach (var m in _clusterer.Markers)
                _gMap.Markers.Remove(m);
            var count = _clusterer.Markers.Count;            
            _clusterer.Clear();
            LastElementIndex -= count;
        }

        private void RemoveMarkers(IEnumerable sourceItems) {

            var source = sourceItems.Cast<object>().ToList();
            if (!source.Any())
                return;

            var oldMarkers = new List<MapMarker>();

            foreach (var item in source) {
                var marker = _gMap.Markers.First(m => {
                    var element = m.Shape as FrameworkElement;
                    return element != null && element.DataContext == item;
                });
                _gMap.Markers.Remove(marker);
                oldMarkers.Add((MapMarker)marker);
            }
            
            try {
                Dispatcher.Invoke(new Action(() => {

                    _clusterer.RemoveMarkers(oldMarkers);
                    _mapControl.UpdateZoomLayout();

                }), DispatcherPriority.DataBind);
            }
            catch (Exception) { }
        }

        public void UpdateClusteringGridSize() {

            var x = 0.0;
            var y = 0.0;
            int count = 0;
            foreach (var m in _gMap.Markers.OfType<MapMarker>()) {
                var el = ((FrameworkElement)m.MarkerContainer.Content);
                el.UpdateLayout();
                var s = el.RenderSize;
                x += s.Width;
                y += s.Height;
                count++;
            }
            if (count == 0)
                count = 1;

            _clusteringXSize = (int)(x / count);
            _clusteringYSize = (int)(y / count);
        }

        private void ItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (_gMap == null)
                return;

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (LayerIsVisible)
                        AddMarkers(e.NewItems, false);
                    break;
                case NotifyCollectionChangedAction.Remove:
                        RemoveMarkers(e.OldItems);
                    LastElementIndex -= e.OldItems.Count;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearMarkers();
                    if (LayerIsVisible)
                        AddMarkers(ItemsSource, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _mapControl.LabelPlaceManager.Reset();
            _mapControl.LabelPlaceManager.PlaceLabels(_gMap.Markers.OfType<MapMarker>().Where(m => m.IsVisible).ToList());
        }

        public void ResetLayer() {
            if (ItemsSource != null)
                ItemCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public static readonly DependencyProperty LayerIsVisibleProperty = DependencyProperty.Register(
            "LayerIsVisible", typeof(bool), typeof(MapMarkersLayer), new FrameworkPropertyMetadata(true, LayerIsVisibleChanged));

        private static void LayerIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var layer = (MapMarkersLayer)d;
            layer.ResetLayer();
            layer._needToUpdateClusteringSize = true;
            layer.UpdateClusters();
        }

        public bool LayerIsVisible {
            get { return (bool)GetValue(LayerIsVisibleProperty); }
            set { SetValue(LayerIsVisibleProperty, value); }
        }
        
        public void UpdateClusters() {
            if (_clusterer.Markers.Count != 0 && (_clusteringXSize == 0 || _clusteringYSize == 0)) {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(UpdateClusteringGridSize));
            }
            _clusterer.UpdateClusters(_clusteringXSize, _clusteringYSize);
        }
        
        public MapControl MapControl {
            get { return _mapControl; } 
            set {
                _mapControl = value;
                _gMap = _mapControl.GMapControl;
                _clusterer = new MarkerClusterer(_gMap);
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
           DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MapMarkersLayer), new FrameworkPropertyMetadata(default(IEnumerable), ItemsSourceChangedCallback));

        public IEnumerable ItemsSource {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var control = (MapMarkersLayer)dependencyObject;
            var newItem = (IEnumerable)dependencyPropertyChangedEventArgs.NewValue;
            var oldItem = (IEnumerable)dependencyPropertyChangedEventArgs.OldValue;
            if (newItem != null) {
                var itemCollection = newItem as INotifyCollectionChanged;
                if (itemCollection != null)
                    itemCollection.CollectionChanged += control.ItemCollectionChanged;

                if (control._gMap != null) {
                    control.ClearMarkers();
                    control.AddMarkers(newItem, false);
                }
            }
            if (oldItem != null) {
                var oldCollection = oldItem as INotifyCollectionChanged;
                if (oldCollection != null)
                    oldCollection.CollectionChanged -= control.ItemCollectionChanged;
            }

            control._mapControl?.LabelPlaceManager.Reset();
        }
        
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(MapMarkersLayer), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
            "ItemTemplateSelector", typeof(DataTemplateSelector), typeof(MapMarkersLayer), new FrameworkPropertyMetadata(default(DataTemplateSelector), ItemTemplateSelectorChanged));

        private static void ItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((MapMarkersLayer)d).ItemCollectionChanged(d, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public DataTemplateSelector ItemTemplateSelector {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set {
                SetValue(ItemTemplateSelectorProperty, value);
                ItemCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        
        public static readonly DependencyProperty MarkerContainerStyleProperty =
            DependencyProperty.Register("MarkerContainerStyle", typeof(Style), typeof(MapMarkersLayer), new PropertyMetadata(default(Style)));

        public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register("LabelTemplate", typeof(DataTemplate), typeof(MapMarkersLayer), new PropertyMetadata(default(DataTemplate)));

        public Style MarkerContainerStyle {
            get { return (Style)GetValue(MarkerContainerStyleProperty); }
            set { SetValue(MarkerContainerStyleProperty, value); }
        }

        public DataTemplate LabelTemplate {
            get { return (DataTemplate)GetValue(LabelTemplateProperty); }
            set { SetValue(LabelTemplateProperty, value); }
        }

        public static readonly DependencyProperty LabelTemplateSelectorProperty = DependencyProperty.Register(
            "LabelTemplateSelector", typeof(DataTemplateSelector), typeof(MapMarkersLayer), new FrameworkPropertyMetadata(default(DataTemplateSelector), LabelTemplateSelectorChanged));

        private static void LabelTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
           
        }

        public DataTemplateSelector LabelTemplateSelector {
            get { return (DataTemplateSelector)GetValue(LabelTemplateSelectorProperty); }
            set { SetValue(LabelTemplateSelectorProperty, value); }
        }

        private static readonly DependencyPropertyKey SelectedItemsPropertyKey =
            DependencyProperty.RegisterReadOnly("SelectedItems", typeof(ObservableCollection<object>), typeof(MapControl), new FrameworkPropertyMetadata(new ObservableCollection<object>()));
        public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;

        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (MarkersTree == null)
                return;

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems) {
                        var mapMarker = MarkersTree.Clusters.SelectMany(c => c).FirstOrDefault(m => m.Data == item);
                        if (mapMarker != null)
                            mapMarker.IsSelected = true;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems) {
                        var mapMarker = MarkersTree.Clusters.SelectMany(c => c).FirstOrDefault(m => m.Data == item);
                        if (mapMarker != null)
                            mapMarker.IsSelected = false;
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var mapMarker in MarkersTree.Clusters.SelectMany(c => c))
                        mapMarker.IsSelected = false;
                    SelectedItemsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SelectedItems));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public ObservableCollection<object> SelectedItems {
            get { return (ObservableCollection<object>)GetValue(SelectedItemsProperty); }
        }

        public int LastElementIndex {
            get { return _lastInd; }
            set {
                if (_lastInd != value) {
                    _lastInd = value;
                    OnPropertyChanged(nameof(LastElementIndex));
                    _mapControl?.UpdateLayersIndexes();
                }
            }
        }
        
        protected void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        
        public int ElementsCount => MarkersTree?.Markers.Count ?? 0;

        internal MarkerClusterer MarkersTree { get { return _clusterer; } }
    }
}
