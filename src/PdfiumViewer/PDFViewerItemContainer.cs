﻿using PdfiumViewer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using static PdfiumViewer.Core.NativeMethods;

namespace PdfiumViewer
{
    [TemplatePart(Name = "PART_OverlayLayer", Type = typeof(Canvas))]
    public class PDFViewerItemContainer : ContentControl
    {
        private Dictionary<IPdfMarker, Rectangle[]> _markers = new Dictionary<IPdfMarker, Rectangle[]>();
        static PDFViewerItemContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PDFViewerItemContainer), new FrameworkPropertyMetadata(typeof(PDFViewerItemContainer)));
        }
        private Canvas _overlayLayer;
        private Canvas OverlayLayer
        {
            get => _overlayLayer;
            set => _overlayLayer = value;
        }

        public override void OnApplyTemplate()
        {
            OverlayLayer = GetTemplateChild("PART_OverlayLayer") as Canvas;
        }

        internal void ClearMarker()
        {
            OverlayLayer?.Children?.Clear();
        }

        internal void AddOrUpdateMarker(IPdfMarker marker, double zoom, Brush fill, Brush border, double borderThickness)
        {
            if (!_markers.TryGetValue(marker, out Rectangle[] rects))
            {
                rects = new Rectangle[marker.Bounds.Length];
                _markers.Add(marker, rects);
            }

            if (marker.Bounds.Length != rects.Length)
            {
                var newRects = new Rectangle[marker.Bounds.Length];
                _markers[marker] = newRects;

                if (marker.Bounds.Length > rects.Length)
                {
                    for (int i = 0; i < rects.Length; i++)
                    {
                        newRects[i] = rects[i];
                    }
                }
                else
                {
                    for (int i = marker.Bounds.Length; i < rects.Length; i++)
                    {
                        OverlayLayer?.Children.Remove(rects[i]);
                    }
                }

                rects = newRects;
            }

            for (int i = 0; i < rects.Length; i++)
            {
                var rect = rects[i];
                if (rect == null)
                {
                    rects[i] = rect = new Rectangle();
                    OverlayLayer?.Children?.Add(rect);
                }

                var bound = marker.Bounds[i];
                rect.Opacity = 0.35;
                rect.Fill = fill;
                rect.Stroke = border;
                rect.StrokeThickness = borderThickness;

                rect.Width = bound.Width * zoom;
                rect.Height = bound.Height * zoom;
                Canvas.SetLeft(rect, bound.Left * zoom);
                Canvas.SetTop(rect, bound.Top * zoom);
            }
        }

        internal void RemoveMarker(IPdfMarker marker)
        {
            if (_markers.TryGetValue(marker, out Rectangle[] rects))
            {
                foreach (var item in rects)
                {
                    OverlayLayer?.Children.Remove(item);
                }

                _markers.Remove(marker);
            }
        }
    }
}
