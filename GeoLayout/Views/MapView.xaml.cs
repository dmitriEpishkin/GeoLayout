﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GeoLayout.Domain.Data;
using GeoLayout.ViewModels;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Nordwest.Wpf.Controls;
using Nordwest.Wpf.Controls.Layers;
using Nordwest.Wpf.Controls.Tools;

namespace GeoLayout.Views
{
    /// <summary>
    /// Логика взаимодействия для MapView.xaml
    /// </summary>
    public partial class MapView : UserControl {

        private readonly MapViewModel _model;

        private readonly Dictionary<Waypoint, GMapMarker> _markers = new Dictionary<Waypoint, GMapMarker>();

        public MapView()
        {
            InitializeComponent();

            _model = ((App)Application.Current).MapViewModel;
            DataContext = _model;

            ShowAllCommand = new DelegateCommand(ShowAll, CanShowAll);

            Map.MapProvider = GMapProviders.BingSatelliteMap;

            Map.Layers = new ObservableCollection<IGMapElementsLayer>();
            Map.Layers.Clear();
            Map.Layers.Add(PointsLayer);

            ResourceDictionary res = ResourceUtil.GetRelativeResourceDictionary(@"Themes\GeoLayoutTheme.xaml");

            AddPointLayer.AddPointUI = new AddPointMapView();
            AddPointLayer.AddPointAction = lng => {
                _model.BuilderService.WaypointBuilder.Latitude = lng.Lat;
                _model.BuilderService.WaypointBuilder.Longitude = lng.Lng;
                _model.BuilderService.WaypointBuilder.Apply();
            };
            
            AddPointLayer.IsActive = true;
            AddPointLayer.MapControl = Map;
            
            AddPointLayer.Ok.Style = (Style) res["MapToolButtonStyle"];
            AddPointLayer.Ok.Content = new Image { Source = new BitmapImage(new Uri("../Images/Check.png", UriKind.Relative))};
            AddPointLayer.Close.Style = (Style)res["MapToolButtonStyle"];
            AddPointLayer.Close.Content = new Image { Source = new BitmapImage(new Uri("../Images/Delete.png", UriKind.Relative)) };

            AddPointLayer.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "Point") {
                    var point = AddPointLayer.Point;
                    _model.BuilderService.WaypointBuilder.SetPosition(point.Lat, point.Lng);

                    if (!Map.GMapControl.ViewArea.Contains(AddPointLayer.Point))
                        Map.GMapControl.Position = AddPointLayer.Point;
                }
            };

            ShiftPointLayer.Content = new ShiftPointMapView();
            ShiftPointLayer.ShiftAction = (marker, latLng) => {
                if (marker.Data is Waypoint wpt) {
                    wpt.Location = new GeoLocation(latLng.Lat, latLng.Lng, wpt.Location.Elevation);
                }
            };
            ShiftPointLayer.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "CurrentPosition") {
                    _model.BuilderService.ShiftModifier.Location = new GeoLocation(
                        ShiftPointLayer.CurrentPosition.Lat,
                        ShiftPointLayer.CurrentPosition.Lng,
                        _model.BuilderService.ShiftModifier.SelectedWaypoint?.Location.Elevation ?? 0.0);
                }
            };
            ShiftPointLayer.MapControl = Map;

            _model.BuilderService.WaypointBuilder.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "Latitude" || args.PropertyName == "Longitude") {
                    var builder = _model.BuilderService.WaypointBuilder;

                    AddPointLayer.Point = new PointLatLng(builder.Latitude, builder.Longitude);
                }
            };

            MapRuler.MapControl = Map;

            var multiRulerTextBlock = new TextBlock() { Margin = new Thickness(3) }; 
            MultiRulerToolLayer.MapControl = Map;
            MultiRulerToolLayer.Ok.Visibility = Visibility.Collapsed;
            MultiRulerToolLayer.Content = multiRulerTextBlock;
            MultiRulerToolLayer.Close.Style = (Style)res["MapToolButtonStyle"];
            MultiRulerToolLayer.Close.Content = new Image { Source = new BitmapImage(new Uri("../Images/Delete.png", UriKind.Relative)) };
            MultiRulerToolLayer.RouteChanged += (sender, args) => {

                var c = MultiRulerToolLayer.Coordinates;

                double dist = 0.0;
                for (int i = 1; i < c.Count; i++)
                    dist += DistanceHelper.GetDistance(c[i], c[i - 1]);

                if (dist < 1000)
                    multiRulerTextBlock.Text = dist.ToString("F0") + " " + Nordwest.Wpf.Controls.Properties.Resources.MapControl_ScaleRuler_MPostfix;
                else if (dist < 1000000)
                    multiRulerTextBlock.Text = (dist / 1000).ToString("G3") + " " + Nordwest.Wpf.Controls.Properties.Resources.MapControl_ScaleRuler_KmPostfix;
                else
                    multiRulerTextBlock.Text = (dist / 1000).ToString("G4") + " " + Nordwest.Wpf.Controls.Properties.Resources.MapControl_ScaleRuler_KmPostfix;
            };

            RouteToolLayer.MapControl = Map;
            RouteToolLayer.Content = new CreateProfileMapView();
            RouteToolLayer.RouteToolAction = points => {
                _model.BuilderService.ProfileBuilder.Build(points.ConvertAll(p => new Waypoint("", new GeoLocation(p.Lat, p.Lng, 0.0))));
            };
            RouteToolLayer.Ok.Style = (Style) res["MapToolButtonStyle"];
            RouteToolLayer.Ok.Content = new Image { Source = new BitmapImage(new Uri("../Images/Check.png", UriKind.Relative)) };
            RouteToolLayer.Close.Style = (Style) res["MapToolButtonStyle"];
            RouteToolLayer.Close.Content = new Image { Source = new BitmapImage(new Uri("../Images/Delete.png", UriKind.Relative)) };

            RectangleToolLayer.MapControl = Map;
            RectangleToolLayer.RectToolAction = (corner, p1, p2) => {
                _model.BuilderService.GridBuilder.SetupWithGridFrame(new GridFrame(
                    new GeoLocation(corner.Lat, corner.Lng, 0),
                    new GeoLocation(p1.Lat, p1.Lng, 0),
                    new GeoLocation(p2.Lat, p2.Lng, 0)));
                _model.BuilderService.GridBuilder.Apply();
            }; 
            RectangleToolLayer.Content = new CreateGridMapView();
            RectangleToolLayer.Ok.Style = (Style)res["MapToolButtonStyle"];
            RectangleToolLayer.Ok.Content = new Image { Source = new BitmapImage(new Uri("../Images/Check.png", UriKind.Relative)) };
            RectangleToolLayer.Close.Style = (Style)res["MapToolButtonStyle"];
            RectangleToolLayer.Close.Content = new Image { Source = new BitmapImage(new Uri("../Images/Delete.png", UriKind.Relative)) };

            PolygonToolLayer.MapControl = Map;
            
            Map.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            _model.WaypointsService.SelectedWaypoints.CollectionChanged += SelectedWaypoints_CollectionChanged;

        }


        private void SelectedWaypoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var p in e.NewItems.OfType<Waypoint>())
                        if (!Map.SelectedItems.Contains(p))
                            Map.SelectedItems.Add(p);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in e.OldItems.OfType<Waypoint>())
                        Map.SelectedItems.Remove(p);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Map.SelectedItems.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var p in e.NewItems.OfType<Waypoint>())
                        p.IsSelected = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in e.OldItems.OfType<Waypoint>())
                        p.IsSelected = false;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var wpt in _model.WaypointsService.Waypoints)
                        wpt.IsSelected = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool CanShowAll() {
            var vm = _model;
            return vm != null && vm.WaypointsService.Waypoints.Count > 0;
        }

        private void ShowAll() {
            Map.ShowAll();
        }

        public DelegateCommand ShowAllCommand { get; }
        
    }
}