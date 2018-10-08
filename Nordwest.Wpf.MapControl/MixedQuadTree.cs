using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Nordwest.Wpf.Controls
{
    public class MixedQuadTree<TData> : INotifyPropertyChanged, INotifyCollectionChanged, IEnumerable<MixedQuadTree<TData>> where TData : class
    {
        private readonly Func<TData, Point> _getPosition;
        public MixedQuadTree(Bounds bounds, Func<TData, Point> getPosition)
        {
            _getPosition = getPosition;
            _bounds = bounds;
        }

        public Bounds Bounds
        {
            get { return _bounds; }
            set
            {
                if (value.Equals(_bounds)) return;
                _bounds = value;
                OnPropertyChanged(@"Bounds");
            }
        }
        public Rect Rect { get { return new Rect(Bounds.Center.X - Bounds.HalfSize.Width, Bounds.Center.Y - Bounds.HalfSize.Height, Bounds.HalfSize.Width * 2, Bounds.HalfSize.Height * 2); } }

        public TData DataPoint
        {
            get { return _dataPoint; }
            set
            {
                if (Equals(value, _dataPoint)) return;
                _dataPoint = value;
                OnPropertyChanged(@"DataPoint");
            }
        }

        protected Func<TData, Point> GetPosition
        {
            get { return _getPosition; }
        }

        public List<MixedQuadTree<TData>> Children
        {
            get { return _children; }
        }

        public Point GetPoint(TData data)
        {
            return GetPosition(data);
        }

        public Point PointPosition { get { return GetPosition(DataPoint); } }

        private readonly List<MixedQuadTree<TData>> _children = new List<MixedQuadTree<TData>>();

        private Bounds _bounds;
        private TData _dataPoint;

        private MixedQuadTree<TData> GetOrCreateChildForPoint(Point point)
        {
            var child = Children.Find(c => c.Bounds.Contains(point));
            if (child == null)
            {
                var newHalfSize = new Size(Bounds.HalfSize.Width / 2, Bounds.HalfSize.Height / 2);
                if (point.X < Bounds.Center.X)
                {
                    if (point.Y < Bounds.Center.Y)
                    {
                        child = CreateChild(new Bounds(new Point(Bounds.Center.X - newHalfSize.Width, Bounds.Center.Y - newHalfSize.Height), newHalfSize));
                    }
                    else
                    {
                        child = CreateChild(new Bounds(new Point(Bounds.Center.X - newHalfSize.Width, Bounds.Center.Y + newHalfSize.Height), newHalfSize));
                    }
                }
                else
                {
                    if (point.Y < Bounds.Center.Y)
                    {
                        child = CreateChild(new Bounds(new Point(Bounds.Center.X + newHalfSize.Width, Bounds.Center.Y - newHalfSize.Height), newHalfSize));
                    }
                    else
                    {
                        child = CreateChild(new Bounds(new Point(Bounds.Center.X + newHalfSize.Width, Bounds.Center.Y + newHalfSize.Height), newHalfSize));
                    }
                }
                Children.Add(child);
            }
            return child;
        }

        public void Add(TData data)
        {
            if (DataPoint == null && Children.Count == 0)
            {
                DataPoint = data;
            }
            else
            {
                if (DataPoint != null)
                {
                    GetOrCreateChildForPoint(PointPosition).Add(DataPoint);
                    DataPoint = null;
                }
                GetOrCreateChildForPoint(GetPoint(data)).Add(data);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { data }));
        }

        public void Clear()
        {
            Children.Clear();
            DataPoint = null;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual MixedQuadTree<TData> CreateChild(Bounds bounds)
        {
            return new MixedQuadTree<TData>(bounds, GetPosition);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<MixedQuadTree<TData>> GetEnumerator()
        {
            var nodes = new Queue<MixedQuadTree<TData>>();
            nodes.Enqueue(this);

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                if (node.DataPoint != null)
                    yield return node;
                else
                    foreach (var child in node.Children)
                        nodes.Enqueue(child);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public double GetDistance(Point a, Point b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }

        public Bounds GetPointBounds(double range)
        {
            return new Bounds(PointPosition, new Size(range, range));
        }

        public Bounds GetPointBounds(TData data, double range)
        {
            return new Bounds(GetPoint(data), new Size(range, range));
        }

        public List<GroupQuadTree<TData>> QueryRange(Bounds bounds, double range)
        {
            // Prepare an array of results
            var pointsInRange = new List<GroupQuadTree<TData>>();

            // Automatically abort if the bounds does not collide with this quad
            if (!Bounds.IsIntersect(bounds))
                return pointsInRange; // empty list

            // Check objects at this quad level
            foreach (var child in Children.Cast<GroupQuadTree<TData>>())
            {
                if (child.DataPoint != null && bounds.IsIntersect(child.GetPointBounds(range)))
                    pointsInRange.Add(child);
                pointsInRange.AddRange(child.QueryRange(bounds, range));
            }

            return pointsInRange;
        }

        public IEnumerable<TData> GetRange(Bounds bounds, double range)
        {
            var nodes = new Queue<MixedQuadTree<TData>>();
            nodes.Enqueue(this);

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                if (node.DataPoint != null)
                {
                    var pointBounds = node.GetPointBounds(range);
                    if (bounds.IsIntersect(pointBounds))
                    {
                        yield return node.DataPoint;
                        continue;
                    }
                }
                foreach (var child in node.Children.Where(child => child.Bounds.IsIntersect(Bounds)))
                    nodes.Enqueue(child);
            
            }
        }
    }
}