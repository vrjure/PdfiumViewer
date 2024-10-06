using System;
using System.Windows;
using System.Windows.Media;
using PdfiumViewer.Drawing;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace PdfiumViewer.Core
{
    internal class PdfMarker : IPdfMarker
    {
        public int Page { get; }
        public Rect[] Bounds { get; }
        public bool Current { get; set; }
        public PdfMarker(int page, Rect[] bound, bool current)
        {
            Page = page;
            Bounds = bound;
            this.Current = current;
        }
    }
}
