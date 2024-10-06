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
        public Rect Bound { get; }
        public bool Current { get; set; }

        public int MatchIndex { get; }

        public PdfMarker(int page, int matchIndex, Rect bound, bool current)
        {
            Page = page;
            Bound = bound;
            this.Current = current;
            MatchIndex = matchIndex;
        }
    }
}
