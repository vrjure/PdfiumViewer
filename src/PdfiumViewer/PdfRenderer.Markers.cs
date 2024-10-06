using PdfiumViewer.Core;
using PdfiumViewer.Drawing;
using PdfiumViewer.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PdfiumViewer
{
    partial class PdfRenderer
    {
        public PdfMarkerCollection Markers { get; }

        private List<IPdfMarker>[] _markers;

        private void UnLoad()
        {
            Document?.Dispose();
            Document = null;
            Frames = null;
            _markers = null;
            Panel.Children.Clear();
            GC.Collect();
        }
        public void ClockwiseRotate()
        {
            // _____
            //      |
            //      |
            //      v
            // Clockwise

            switch (Rotate)
            {
                case PdfRotation.Rotate0:
                    RotatePage(Page, PdfRotation.Rotate90);
                    break;
                case PdfRotation.Rotate90:
                    RotatePage(Page, PdfRotation.Rotate180);
                    break;
                case PdfRotation.Rotate180:
                    RotatePage(Page, PdfRotation.Rotate270);
                    break;
                case PdfRotation.Rotate270:
                    RotatePage(Page, PdfRotation.Rotate0);
                    break;
            }
        }
        public void Counterclockwise()
        {
            //      ^
            //      |
            //      |
            // _____|
            // Counterclockwise

            switch (Rotate)
            {
                case PdfRotation.Rotate0:
                    RotatePage(Page, PdfRotation.Rotate270);
                    break;
                case PdfRotation.Rotate90:
                    RotatePage(Page, PdfRotation.Rotate0);
                    break;
                case PdfRotation.Rotate180:
                    RotatePage(Page, PdfRotation.Rotate90);
                    break;
                case PdfRotation.Rotate270:
                    RotatePage(Page, PdfRotation.Rotate180);
                    break;
            }
        }

        /// <summary>
        /// Scroll the PDF bounds into view.
        /// </summary>
        /// <param name="bounds">The PDF bounds to scroll into view.</param>
        public void ScrollIntoView(PdfRectangle bounds)
        {
            ScrollIntoView(BoundsFromPdf(bounds));
        }

        /// <summary>
        /// Scroll the client rectangle into view.
        /// </summary>
        /// <param name="rectangle">The client rectangle to scroll into view.</param>
        public void ScrollIntoView(Rect rectangle)
        {
            var clientArea = GetScrollClientArea();

            // if (rectangle.Top < 0 || rectangle.Bottom > clientArea.Height)
            // {
            //     var displayRectangle = DisplayRectangle;
            //     int center = rectangle.Top + rectangle.Height / 2;
            //     int documentCenter = center - displayRectangle.Y;
            //     int displayCenter = clientArea.Height / 2;
            //     int offset = documentCenter - displayCenter;
            //
            //     SetDisplayRectLocation(new Point(
            //         displayRectangle.X,
            //         -offset
            //     ));
            // }
        }

        /// <summary>
        /// Converts PDF bounds to client bounds.
        /// </summary>
        /// <param name="bounds">The PDF bounds to convert.</param>
        /// <returns>The bounds of the PDF bounds in client coordinates.</returns>
        public Rect BoundsFromPdf(PdfRectangle bounds)
        {
            return BoundsFromPdf(bounds, true);
        }

        private Rect BoundsFromPdf(PdfRectangle bounds, bool translateOffset)
        {
            var offset = translateOffset ? GetScrollOffset() : Size.Empty;
            // var pageBounds = _pageCache[bounds.Page].Bounds;
            // var pageSize = Document.PageSizes[bounds.Page];
            //
            // var translated = Document.RectangleFromPdf(
            //     bounds.Page,
            //     bounds.Bounds
            // );
            //
            // var topLeft = TranslatePointFromPdf(pageBounds.Size, pageSize, new PointF(translated.Left, translated.Top));
            // var bottomRight = TranslatePointFromPdf(pageBounds.Size, pageSize, new PointF(translated.Right, translated.Bottom));
            //
            // return new Rectangle(
            //     pageBounds.Left + offset.Width + Math.Min(topLeft.X, bottomRight.X),
            //     pageBounds.Top + offset.Height + Math.Min(topLeft.Y, bottomRight.Y),
            //     Math.Abs(bottomRight.X - topLeft.X),
            //     Math.Abs(bottomRight.Y - topLeft.Y)
            // );

            return new Rect(offset.Width, offset.Height, offset.Width, offset.Height);
        }

        private Size GetScrollOffset()
        {
            var bounds = GetScrollClientArea();
            // int maxWidth = (int)(_maxWidth * _scaleFactor) + ShadeBorder.Size.Horizontal + PageMargin.Horizontal;
            // int leftOffset = (HScroll ? DisplayRectangle.X : (bounds.Width - maxWidth) / 2) + maxWidth / 2;
            // int topOffset = VScroll ? DisplayRectangle.Y : 0;
            //
            // return new Size(leftOffset, topOffset);
            return new Size((int)bounds.Width, (int)bounds.Height);
        }

        private Rect GetScrollClientArea()
        {
            return new Rect(0, 0, (int)ViewportWidth, (int)ViewportHeight);
        }

        private void EnsureMarkers()
        {
            if (_markers != null)
                return;

            _markers = new List<IPdfMarker>[1];

            foreach (var marker in Markers)
            {
                if (marker.Page < 0 || marker.Page >= _markers.Length)
                    continue;

                _markers[marker.Page] ??= new List<IPdfMarker>();
                _markers[marker.Page].Add(marker);
            }
        }

        private void DrawMarkers(DrawingContext graphics, int page)
        {
            //if (_markers?.Length > 0 && _markers.Length > page)
            //{
            //    var markers = _markers[page];
            //    if (markers == null)
            //        return;

            //    foreach (var marker in markers)
            //    {
            //        marker.Draw(this, graphics);
            //    }
            //}
        }



        private void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RedrawMarkers();
        }
        private void RedrawMarkers()
        {
            _markers = null;

            GotoPage(Page);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Document == null)
                return;

            EnsureMarkers();

            DrawMarkers(drawingContext, Page);
        }

        ~PdfRenderer() => Dispose(true);
    }
}
