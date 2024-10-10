using PdfiumViewer.Drawing;
using PdfiumViewer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PdfiumViewer.Core
{
    public class PdfPage : IDisposable
    {
        private static readonly Encoding FPDFEncoding = new UnicodeEncoding(false, false, false);
        private readonly IntPtr _document;
        private readonly IntPtr _form;
        private readonly int _pageIndex;

        private bool _disposed;

        private IntPtr _page = IntPtr.Zero;
        public IntPtr Page
        {
            get
            {
                if (_page == IntPtr.Zero)
                {
                    _page = NativeMethods.FPDF_LoadPage(_document, _pageIndex);
                    NativeMethods.FORM_OnAfterLoadPage(Page, _form);
                    NativeMethods.FORM_DoPageAAction(Page, _form, NativeMethods.FPDFPAGE_AACTION.OPEN);
                }

                return _page;
            }
        }

        private IntPtr _textPage = IntPtr.Zero;
        public IntPtr TextPage
        {
            get
            {
                if (_textPage == IntPtr.Zero)
                {
                    _textPage = NativeMethods.FPDFText_LoadPage(Page);
                }
                return _textPage;
            }
        }

        private double _width = 0;
        public double Width
        {
            get
            {
                if (_width == 0)
                {
                    _width = NativeMethods.FPDF_GetPageWidth(Page);
                }

                return _width;
            }
        }

        private double _height = 0;
        public double Height
        {
            get
            {
                if (_height == 0)
                {
                    _height = NativeMethods.FPDF_GetPageHeight(Page);
                }

                return _height;
            }
        }

        public Size Size => new Size(Width, Height);

        internal PdfPage(IntPtr document, IntPtr form, int pageIndex)
        {
            _document = document;
            _form = form;
            _pageIndex = pageIndex;
        }

        public int GetCharIndexAtPos(double x, double y, double xToLerance, double yToLerance)
        {
            return NativeMethods.FPDF_GetCharIndexAtPos(TextPage, x, y, xToLerance, yToLerance);
        }

        public int GetCountChars()
        {
            return NativeMethods.FPDFText_CountChars(TextPage);
        }

        public IReadOnlyList<Rect> GetTextBounds(int index, int matchLength)
        {
            var resultBounds = new List<Rect>();
            var lastBound = new Rect();
            bool isFirst = true;

            var len = NativeMethods.FPDFText_CountRects(TextPage, index, matchLength);
            for (var i = 0; i < len; i++)
            {
                var bound = GetBound(i);

                if (bound.Width == 0 || bound.Height == 0)
                    continue;
                if (isFirst)
                {
                    isFirst = false;
                    lastBound.X = bound.X;
                    lastBound.Y = bound.Y;
                    lastBound.Width = bound.Width;
                    lastBound.Height = bound.Height;
                }
                else
                {
                    if (Math.Abs(bound.Y - lastBound.Y) >= lastBound.Height)
                    {
                        resultBounds.Add(lastBound);
                        lastBound = new Rect
                        {
                            X = bound.X,
                            Y = bound.Y,
                            Width = bound.Width,
                            Height = bound.Height
                        };
                    }
                    else
                    {
                        lastBound.Y = Math.Min(lastBound.Y, bound.Y);
                        lastBound.Height = Math.Max(lastBound.Height, bound.Height);
                        lastBound.Width += bound.Right - lastBound.Right;
                    }

                }
            }

            resultBounds.Add(lastBound);
            return resultBounds.AsReadOnly();
        }

        public string GetText()
        {
            var chars = NativeMethods.FPDFText_CountChars(TextPage);
            if (chars < 0)
            {
                return string.Empty;
            }
            return GetText(0, chars);
        }

        public string GetText(int start_index, int count)
        {
            var buffer = new byte[count * 2];
            var total = NativeMethods.FPDFText_GetText(_textPage, start_index, count, buffer);
            return FPDFEncoding.GetString(buffer, 0, buffer.Length);
        }

        public IReadOnlyCollection<PdfMatch> Search(string text, bool matchCase, bool wholeWord)
        {
            var matches = new List<PdfMatch>();
            NativeMethods.FPDF_SEARCH_FLAGS flags = 0;
            if (matchCase)
                flags |= NativeMethods.FPDF_SEARCH_FLAGS.FPDF_MATCHCASE;
            if (wholeWord)
                flags |= NativeMethods.FPDF_SEARCH_FLAGS.FPDF_MATCHWHOLEWORD;

            var handle = NativeMethods.FPDFText_FindStart(TextPage, FPDFEncoding.GetBytes(text), flags, 0);

            try
            {
                while (NativeMethods.FPDFText_FindNext(handle))
                {
                    var index = NativeMethods.FPDFText_GetSchResultIndex(handle);

                    var matchLength = NativeMethods.FPDFText_GetSchCount(handle);

                    var result = new byte[(matchLength + 1) * 2];
                    NativeMethods.FPDFText_GetText(TextPage, index, matchLength, result);
                    var match = FPDFEncoding.GetString(result, 0, matchLength * 2);
                    matches.Add(new PdfMatch(match,new PdfTextSpan(_pageIndex, index, matchLength), _pageIndex));
                }
            }
            finally
            {
                NativeMethods.FPDFText_FindClose(handle);
            }

            Close();

            return matches.AsReadOnly();
        }

        public PdfPageLinks GetPageLinks()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var links = new List<PdfPageLink>();

            var link = 0;
            IntPtr annotation;

            while (NativeMethods.FPDFLink_Enumerate(Page, ref link, out annotation))
            {
                var destination = NativeMethods.FPDFLink_GetDest(_document, annotation);
                int? target = null;
                string uri = null;

                if (destination != IntPtr.Zero)
                    target = (int)NativeMethods.FPDFDest_GetDestPageIndex(_document, destination);

                var action = NativeMethods.FPDFLink_GetAction(annotation);
                if (action != IntPtr.Zero)
                {
                    const uint length = 1024;
                    var sb = new StringBuilder(1024);
                    NativeMethods.FPDFAction_GetURIPath(_document, action, sb, length);

                    uri = sb.ToString();
                }

                var rect = new NativeMethods.FS_RECTF();

                if (NativeMethods.FPDFLink_GetAnnotRect(annotation, rect) && (target.HasValue || uri != null))
                {
                    links.Add(new PdfPageLink(
                        new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top),
                        target,
                        uri
                    ));
                }
            }

            return new PdfPageLinks(links);
        }

        public System.Drawing.Image Render(int width, int height, float dpiX, float dpiY, PdfRotation rotate, PdfRenderFlags flags)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if ((flags & PdfRenderFlags.CorrectFromDpi) != 0)
            {
                width = width * (int)dpiX / 72;
                height = height * (int)dpiY / 72;
            }

            var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bitmap.SetResolution(dpiX, dpiY);

            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

            try
            {
                var handle = NativeMethods.FPDFBitmap_CreateEx(width, height, 4, data.Scan0, width * 4);

                try
                {
                    var background = (flags & PdfRenderFlags.Transparent) == 0 ? 0xFFFFFFFF : 0x00FFFFFF;

                    NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, width, height, background);

                    bool renderFormFill = (flags & PdfRenderFlags.Annotations) != 0;
                    NativeMethods.FPDF fpdfFlag = FlagsToFPDFFlags(flags);


                    if (renderFormFill)
                        fpdfFlag &= ~NativeMethods.FPDF.ANNOT;

                    NativeMethods.FPDF_RenderPageBitmap(handle, Page, 0, 0, width, height, (int)rotate, fpdfFlag);

                    if (renderFormFill)
                        NativeMethods.FPDF_FFLDraw(_form, handle, Page, 0, 0, width, height, (int)rotate, fpdfFlag);

                }
                finally
                {
                    NativeMethods.FPDFBitmap_Destroy(handle);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        public void Render(System.Drawing.Graphics graphics, float dpiX, float dpiY, System.Drawing.Rectangle bounds, PdfRenderFlags flags)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var graphicsDpiX = graphics.DpiX;
            var graphicsDpiY = graphics.DpiY;

            var dc = graphics.GetHdc();

            try
            {
                if ((int)graphicsDpiX != (int)dpiX || (int)graphicsDpiY != (int)dpiY)
                {
                    var transform = new NativeMethods.XFORM
                    {
                        eM11 = graphicsDpiX / dpiX,
                        eM22 = graphicsDpiY / dpiY
                    };

                    NativeMethods.SetGraphicsMode(dc, NativeMethods.GmAdvanced);
                    NativeMethods.ModifyWorldTransform(dc, ref transform, NativeMethods.MwtLeftMultiply);
                }

                var point = new NativeMethods.POINT();
                NativeMethods.SetViewportOrgEx(dc, bounds.X, bounds.Y, out point);

                NativeMethods.FPDF_RenderPage(dc, Page, 0, 0, bounds.Width, bounds.Height, 0, FlagsToFPDFFlags(flags));

                NativeMethods.SetViewportOrgEx(dc, point.X, point.Y, out point);
            }
            finally
            {
                graphics.ReleaseHdc(dc);
            }
        }


        public Point PageToDevice(Point point)
        {
            NativeMethods.FPDF_PageToDevice(Page, 0, 0, (int)Width, (int)Height, 0, point.X, point.Y, out var deviceX, out var deviceY);
            return new Point(deviceX, deviceY);
        }

        public Point DeviceToPage(Point point)
        {
            NativeMethods.FPDF_DeviceToPage(Page, 0, 0, (int)Width, (int)Height, 0, (int)point.X, (int)point.Y, out var deviceX, out var deviceY);

            return new Point(deviceX, deviceY);
        }


        public void Close()
        {
            if (_page != IntPtr.Zero)
            {
                NativeMethods.FORM_DoPageAAction(Page, _form, NativeMethods.FPDFPAGE_AACTION.CLOSE);
                NativeMethods.FORM_OnBeforeClosePage(Page, _form);

                if (_textPage != IntPtr.Zero)
                {
                    NativeMethods.FPDFText_ClosePage(TextPage);
                }

                NativeMethods.FPDF_ClosePage(Page);

                _page = IntPtr.Zero;
                _textPage = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Close();

                _disposed = true;
            }
        }

        private Rect GetBound(int index)
        {
            NativeMethods.FPDFText_GetRect(TextPage, index, out var left, out var top, out var right, out var bottom);

            return this.PageToDevice(left, top, right, bottom);
        }

        private NativeMethods.FPDF FlagsToFPDFFlags(PdfRenderFlags flags)
        {
            return (NativeMethods.FPDF)(flags & ~(PdfRenderFlags.Transparent | PdfRenderFlags.CorrectFromDpi));
        }

        private void CheckDispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PdfPage));
            }
        }
    }
}
