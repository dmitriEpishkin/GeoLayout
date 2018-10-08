using System;
using System.Windows;

namespace Nordwest.Wpf.Controls
{
    public struct Bounds
    {
        public Bounds(Point center, Size halfSize)
            : this()
        {
            HalfSize = halfSize;
            Center = center;
        }

        public Point Center { get; private set; }
        public Size HalfSize { get; private set; }

        public bool Contains(Point point)
        {
            return Math.Abs(Center.X - point.X) <= HalfSize.Width && Math.Abs(Center.Y - point.Y) <= HalfSize.Height;
        }

        public bool Contains(Bounds bounds)
        {
            return Math.Abs(Center.X - bounds.Center.X) + bounds.HalfSize.Width <= HalfSize.Width && Math.Abs(Center.Y - bounds.Center.Y) + bounds.HalfSize.Height <= HalfSize.Height;
        }

        public bool IsIntersect(Bounds bounds)
        {
            return Math.Abs(Center.X - bounds.Center.X)  <= HalfSize.Width + bounds.HalfSize.Width && Math.Abs(Center.Y - bounds.Center.Y)  <= HalfSize.Height+ bounds.HalfSize.Height;
        }

        //public Bounds Intersect(Bounds bounds)
        //{
        //    if (!IsIntersect(bounds)) return new Bounds();
            

        //}

        //public Rect ToRect()
        //{
        //    return    new Rect(Center.X - );
        //}

        public override string ToString()
        {
            return string.Format(@"({0}, {1}); ({2}, {3})", Center.X, Center.Y, HalfSize.Width, HalfSize.Height);
        }
    }
}