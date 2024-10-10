using PdfiumViewer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PdfiumViewer.Core
{
    public static class PdfPageExtensions
    {
        public static System.Drawing.Image Render(this PdfPage page, int width, int height, float dpiX, float dpiY, bool forPrinting)
        {
            return Render(page, width, height, dpiX, dpiY, forPrinting ? PdfRenderFlags.ForPrinting : PdfRenderFlags.None);
        }

        public static System.Drawing.Image Render(this PdfPage page, int width, int height, float dpiX, float dpiY, PdfRenderFlags flags)
        {
            return page.Render(width, height, dpiX, dpiY, 0, flags);
        }

        public static Rect PageToDevice(this PdfPage page, double left, double top, double right, double bottom)
        {
            var point1 = page.PageToDevice(new Point(left, top));
            var point2 = page.PageToDevice(new Point(right, bottom));

            return new Rect(point1.X, point1.Y, point2.X - point1.X + 1, point2.Y - point1.Y + 1);
        }

        public static Rect DeviceToPage(this PdfPage page, Rect deviceRect)
        {
            var point1 = page.DeviceToPage(new Point(deviceRect.Left, deviceRect.Top));
            var point2 = page.DeviceToPage(new Point(deviceRect.Right, deviceRect.Bottom));

            return new Rect(point1.X, point1.Y, point2.X - point1.X + 1, point2.Y - point1.Y + 1);
        }
    }
}
