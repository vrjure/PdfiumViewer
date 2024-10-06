﻿using PdfiumViewer.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PdfiumViewer
{
    public partial class PDFViewer
    {
        private PdfMarkerCollection _markers;
        private PdfMarkerCollection Markers
        {
            get => _markers;
            set
            {
                if (_markers != null)
                {
                    _markers.CollectionChanged -= _markers_CollectionChanged;
                }

                _markers = value;

                if (_markers != null)
                {
                    OnMarkersChanged(_markers);
                    _markers.CollectionChanged += _markers_CollectionChanged;
                }
            }
        }

        private void _markers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    OnMarkersChanged(e.NewItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    OnMarkersRemoved(e.NewItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    OnMarkersChanged(e.NewItems, e.OldItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    OnMarkersChanged();
                    break;
                default:
                    break;
            }
        }

        private void OnMarkersChanged(IList newMarkers = null, IList oldMarkers = null)
        {
            OnMarkersRemoved(oldMarkers);

            if (newMarkers == null || newMarkers.Count == 0)
            {
                for (int i = RenderStartIndex; i <= RenderEndIndex; i++)
                {
                    var container = GetContainerFormItem(Items[i] as FrameworkElement);
                    container.ClearMarker();
                }
                return;
            }

            foreach (IPdfMarker marker in newMarkers)
            {
                ApplyMarker(marker);
            }
        }

        private void OnMarkersRemoved(IList markers)
        {
            if (markers == null || markers.Count == 0) return;

            foreach (IPdfMarker marker in markers)
            {
                RemoveMarkder(marker);
            }           
        }

        private void ApplyMarker(IPdfMarker marker)
        {
            if (marker == null)
            {
                return;
            }

            var fill = MatchBrush;
            var border = MatchBorderBrush;
            var borderThickness = MatchBorderThickness;

            if (marker.Current)
            {
                fill = CurrentMatchBrush;
                border = CurrentMatchBorderBrush;
                borderThickness = CurrentMatchBorderThickness;
            }

            var container = GetContainerFormItem(Items[marker.Page] as FrameworkElement);

            var bound = marker.Bound;
            var rect = container.FindMarker(f=> (f.Tag as IPdfMarker)?.MatchIndex == marker.MatchIndex) as Rectangle;
            if (rect == null)
            {
                rect = new Rectangle();
                rect.Opacity = 0.35;
                rect.Tag = marker;

                container.AddMarker(rect);
            }

            rect.Fill = fill;
            rect.Stroke = border;
            rect.StrokeThickness = borderThickness;

            rect.Width = bound.Width * Zoom;
            rect.Height = bound.Height * Zoom;
            Canvas.SetLeft(rect, bound.Left * Zoom);
            Canvas.SetTop(rect, bound.Top * Zoom);
        }

        private void RemoveMarkder(IPdfMarker marker)
        {
            if (marker == null)
            {
                return;
            }
            var container = GetContainerFormItem(Items[marker.Page] as FrameworkElement);
            var rect = container.FindMarker(f => (f.Tag as IPdfMarker)?.MatchIndex == marker.MatchIndex);
            container.RemoveMarker(rect);
        }

        private PDFViewerItemContainer GetContainerFormItem(FrameworkElement element)
        {
            return ContainerFromElement(element) as PDFViewerItemContainer;
        }
    }
}