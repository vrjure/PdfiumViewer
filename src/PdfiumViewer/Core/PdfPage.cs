using PdfiumViewer.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public Point PointToPdf(Point point)
        {
            NativeMethods.FPDF_DeviceToPage(
                _page,
                0,
                0,
                (int)Width,
                (int)Height,
                0,
                (int)point.X,
                (int)point.Y,
                out var deviceX,
                out var deviceY
            );

            return new Point((float)deviceX, (float)deviceY);
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

        private Rect GetBound(int index)
        {
            NativeMethods.FPDFText_GetRect(TextPage, index, out var left, out var top, out var right, out var bottom);

            return RectangleFromPdf(left, top, right, bottom);
        }

        public string GetText(int start_index, int count)
        {
            var buffer = new byte[count * 2];
            var total = NativeMethods.FPDFText_GetText(_textPage, start_index, count, buffer);
            return FPDFEncoding.GetString(buffer, 0, buffer.Length);
        }

        public Rect RectangleFromPdf(double left, double top, double right, double bottom)
        {
            NativeMethods.FPDF_PageToDevice(
                Page,
                0,
                0,
                (int)Width,
                (int)Height,
                0,
                left,
                top,
                out var deviceX1,
                out var deviceY1
            );

            NativeMethods.FPDF_PageToDevice(
                Page,
                0,
                0,
                (int)Width,
                (int)Height,
                0,
                right,
                bottom,
                out var deviceX2,
                out var deviceY2
            );

            return new Rect(
                deviceX1 - 1,
                deviceY1 - 1,
                deviceX2 - deviceX1 + 2,
                deviceY2 - deviceY1 + 2
            );
        }

        public void Close()
        {
            if (Page != IntPtr.Zero)
            {
                NativeMethods.FORM_DoPageAAction(Page, _form, NativeMethods.FPDFPAGE_AACTION.CLOSE);
                NativeMethods.FORM_OnBeforeClosePage(Page, _form);

                if (TextPage != IntPtr.Zero)
                {
                    NativeMethods.FPDFText_ClosePage(TextPage);
                }

                NativeMethods.FPDF_ClosePage(Page);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (Page != IntPtr.Zero)
                {
                    NativeMethods.FORM_DoPageAAction(Page, _form, NativeMethods.FPDFPAGE_AACTION.CLOSE);
                    NativeMethods.FORM_OnBeforeClosePage(Page, _form);

                    if (TextPage != IntPtr.Zero)
                    {
                        NativeMethods.FPDFText_ClosePage(TextPage);
                    }

                    NativeMethods.FPDF_ClosePage(Page);
                }

                _disposed = true;
            }
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
