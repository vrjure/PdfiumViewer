using System;
using System.Drawing;
using System.Windows;

namespace PdfiumViewer.Drawing
{
    public struct PdfRectangle : IEquatable<PdfRectangle>
    {
        public static readonly PdfRectangle Empty = new PdfRectangle();

        // _page is offset by 1 so that Empty returns an invalid rectangle.
        private readonly int _page;

        public int Page => _page - 1;

        public Rect Bound { get; }

        public bool IsValid => _page != 0;

        public PdfRectangle(int page, Rect bounds)
        {
            _page = page + 1;
            Bound = bounds;
        }

        public bool Equals(PdfRectangle other)
        {
            return
                Page == other.Page &&
                Bound == other.Bound;
        }

        public override bool Equals(object obj)
        {
            return
                obj is PdfRectangle rectangle &&
                Equals(rectangle);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Page * 397) ^ Bound.GetHashCode();
            }
        }

        public static bool operator ==(PdfRectangle left, PdfRectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PdfRectangle left, PdfRectangle right)
        {
            return !left.Equals(right);
        }
    }
}
