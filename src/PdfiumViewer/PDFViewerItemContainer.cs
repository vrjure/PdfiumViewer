using PdfiumViewer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PdfiumViewer
{
    [TemplatePart(Name = "PART_OverlayLayer", Type = typeof(Canvas))]
    public class PDFViewerItemContainer : ContentControl
    {
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

        internal void AddMarker(FrameworkElement element)
        {
            OverlayLayer?.Children?.Add(element);
        }

        internal void RemoveMarker(FrameworkElement element)
        {
            OverlayLayer?.Children.Remove(element);
        }

        internal void RemoveMarkers(int index, int count)
        {
            OverlayLayer?.Children.RemoveRange(index, count);
        }
    }
}
