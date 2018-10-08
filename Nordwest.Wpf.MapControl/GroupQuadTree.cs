using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Nordwest.Wpf.Controls
{
    public class GroupQuadTree<TData> : MixedQuadTree<TData> where TData : class
    {
        private static int _counter = 0;

        private readonly Action<TData, TData> _coverChild;
        private readonly Action<TData> _uncoverChild;

        public GroupQuadTree(Bounds bounds, Func<TData, Point> getPosition, Action<TData, TData> coverChild, Action<TData> uncoverChild)
            : base(bounds, getPosition)
        {
            _coverChild = coverChild;
            _uncoverChild = uncoverChild;
        }

        public void Group(double groupingRange)
        {
            var groupers = new List<TData>();
            GroupI(groupingRange,groupers, Bounds);
        }

        public void GroupI(double groupingRange, List<TData> groupers, Bounds fullBounds)
        {
            foreach (var child in Children.Cast<GroupQuadTree<TData>>())
            {
                _counter++;
                if (child.DataPoint == null)
                {
                    child.GroupI(groupingRange, groupers, fullBounds);
                }
                else
                {
                    var grouper = groupers.Find(g => GetDistance(GetPoint(g), child.PointPosition) < groupingRange);
                    if (grouper != null) _coverChild(child.DataPoint, grouper);
                    else
                    {
                        groupers.Add(child.DataPoint);
                        _uncoverChild(child.DataPoint);
                    }
                }
            }

            groupers.RemoveAll(grouper => Bounds.Contains(GetPointBounds(grouper, groupingRange)));
        }

        public void Cover(GroupQuadTree<TData> coverer)
        {
            foreach (var child in Children.Cast<GroupQuadTree<TData>>())
            {
                child.Cover(coverer);
                if (child == coverer) continue;
                _coverChild(child.DataPoint, coverer.DataPoint);
                child.CoveredBy = coverer;
            }
        }

        public GroupQuadTree<TData> CoveredBy { get; set; }

        protected override MixedQuadTree<TData> CreateChild(Bounds bounds)
        {
            return new GroupQuadTree<TData>(bounds, GetPosition, _coverChild, _uncoverChild);
        }
    }
}