﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public Rect[] Bounds { get; set; }


        public PdfMarker(int page, Rect[] bound)
        {
            Page = page;
            Bounds = bound;
        }

        public PdfMarker(int page, IReadOnlyList<Rect> bounds): this(page, bounds.ToArray())
        {

        }
    }

    internal class PdfMatchMarker : PdfMarker
    {
        public PdfMatchMarker(int page, int matchIndex, Rect[] bounds, bool current) : base(page, bounds)
        {
            MatchIndex = matchIndex;
            this.Current = current;
        }

        public int MatchIndex { get; }
        public bool Current { get; set; }

    }
}
