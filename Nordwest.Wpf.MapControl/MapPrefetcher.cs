using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Nordwest.Wpf.Controls.Properties;

namespace Nordwest.Wpf.Controls
{
    //based on GMap.NET.WindowsPresentation.TilePrefetcher
    public class MapPrefetcher
    {
        readonly BackgroundWorker _worker = new BackgroundWorker();
        List<GPoint> _list = new List<GPoint>();
        GMapProvider _provider;
        int _sleep;
        RectLatLng _area;
        private int _totalCount = 0;
        private int _doneCount = 0;
        private int _minZoom = 0;
        private int _maxZoom = 0;
        private GMapControl _mapControl;
        public MapPrefetcher(GMapControl control)
        {
            _mapControl = control;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.ProgressChanged += worker_ProgressChanged;
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        public void Start(int minZoom, int maxZoom, int sleep)
        {
            Start(_mapControl.ViewArea, minZoom, maxZoom, sleep);
        }

        public void Start(RectLatLng area, int minZoom, int maxZoom, int sleep)
        {
            Start(area, minZoom, maxZoom, _mapControl.MapProvider, sleep);
        }

        public void Start(RectLatLng area, int minZoom, int maxZoom, GMapProvider provider, int sleep)
        {
            if (_worker.IsBusy)
                return;
            if (minZoom > maxZoom) throw new ArgumentException(Resources.MapPrefetcher_MinZoomGreaterThenMaxZoom_Exception);
            _minZoom = minZoom;
            _maxZoom = maxZoom;
            _totalCount = 0;
            for (int i = minZoom; i < maxZoom; i++)
                // _totalCount += provider.Projection.GetAreaTileList(area, i, 0).Count;
                _totalCount += GetAreaTileCount(provider.Projection, area, i, 0);

            _area = area;
            _provider = provider;
            _sleep = sleep;
            Singleton<GMaps>.Instance.UseMemoryCache = false;
            Singleton<GMaps>.Instance.CacheOnIdleRead = false;
            _worker.RunWorkerAsync();
        }
        private bool _isCanceled = false;// костыль
        public void Stop()
        {
            if (!_worker.IsBusy)
                return;
            _worker.CancelAsync();
            _isCanceled = true;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine(e.ProgressPercentage/100.0 + @"%");
            OnProgressChanged(e.ProgressPercentage);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _list.Clear();

            Singleton<GMaps>.Instance.UseMemoryCache = true;
            Singleton<GMaps>.Instance.CacheOnIdleRead = true;
            OnPrefetchCompleted(_isCanceled);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var countOk = 0;
            for (int i = _minZoom; i < _maxZoom && !_worker.CancellationPending; i++)
                countOk += CacheLevel(i);
            e.Result = countOk;
        }

        private int CacheLevel(int zoom)
        {
            if (_list != null)
            {
                _list.Clear();
                _list = null;
            }
            _list = _provider.Projection.GetAreaTileList(_area, zoom, 0);
            var maxOfTiles = _provider.Projection.GetTileMatrixMaxXY(zoom);
            var all = _list.Count;
            int countOk = 0;
            int retry = 0;
            Shuffle(_list);
            for (int index = 0; index < all && !_worker.CancellationPending; ++index)
            {
                if (CacheTiles(zoom, _list[index], maxOfTiles))
                {
                    ++countOk;
                    retry = 0;
                }
                else if (++retry <= 1)
                {
                    --index;
                    Thread.Sleep(1111);
                    continue;
                }
                else
                    retry = 0;

                _doneCount++;
                _worker.ReportProgress((_doneCount + 1) * 10000 / _totalCount, _doneCount + 1);
                Thread.Sleep(_sleep);
            }
            return countOk;
        }

        private bool CacheTiles(int zoom, GPoint p, GSize maxOfTiles)
        {
            foreach (GMapProvider provider in this._provider.Overlays)
            {
                Exception result;
                PureImage pureImage;
                if (!(provider is TurkeyMapProvider))
                    pureImage = Singleton<GMaps>.Instance.GetImageFrom(provider, p, zoom, out result);
                else
                    pureImage = Singleton<GMaps>.Instance.GetImageFrom(provider,
                                                                       new GPoint(p.X, maxOfTiles.Height - p.Y),
                                                                       zoom, out result);
                if (pureImage == null)
                    return false;
                pureImage.Dispose();
            }
            return true;
        }

        /// <summary>
        /// gets tiles count in rect at specific zoom
        /// </summary>
        public int GetAreaTileCount(PureProjection projection, RectLatLng rect, int zoom, int padding)
        {
            GPoint topLeft = projection.FromPixelToTileXY(projection.FromLatLngToPixel(rect.LocationTopLeft, zoom));
            GPoint rightBottom = projection.FromPixelToTileXY(projection.FromLatLngToPixel(rect.LocationRightBottom, zoom));

            return (int)(((rightBottom.X + padding) - (topLeft.X - padding) + 1) *
                          ((rightBottom.Y + padding) - (topLeft.Y - padding) + 1));
        }

        //копипаста из gmap. хм... гугл не любит когда его карты целенаправленно выкачивают?
        public static readonly Random random = new Random();
        public static void Shuffle<T>(List<T> deck)
        {
            int N = deck.Count;

            for (int i = 0; i < N; ++i)
            {
                int r = i + (int)(random.Next(N - i));
                T t = deck[r];
                deck[r] = deck[i];
                deck[i] = t;
            }
        }



        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        private void OnProgressChanged(ProgressChangedEventArgs e)
        {
            var handler = ProgressChanged;
            if (handler != null) handler(this, e);
        }
        private void OnProgressChanged(int progress, object userState = null)
        {
            OnProgressChanged(new ProgressChangedEventArgs(progress, userState));
        }

        public event EventHandler<MapPrefetcherEventArgs> PrefetchCompleted;
        private void OnPrefetchCompleted(bool isCanceled = false)
        {
            var handler = PrefetchCompleted;
            if (handler != null) handler(this, new MapPrefetcherEventArgs(isCanceled));
        }
    }

    public class MapPrefetcherEventArgs : EventArgs
    {
        public bool IsCanceled { get; private set; }
        public MapPrefetcherEventArgs(bool isCanceled)
        {
            IsCanceled = isCanceled;
        }
    }
}