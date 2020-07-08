using System.IO;
using PdfiumViewer.Core;
using PdfiumViewer.Enums;

namespace PdfiumViewer
{
    public class PdfRenderer : ScrollPanel
    {
        public void OpenPdf(string path)
        {
            Document = PdfDocument.Load(path);
            GotoPage(PageNo = 0);
        }
        public void OpenPdf(string path, string password, bool isRightToLeft = false)
        {
            IsRightToLeft = isRightToLeft;
            Document = PdfDocument.Load(path, password);
            GotoPage(PageNo = 0);
        }
        public void OpenPdf(Stream stream, bool isRightToLeft = false)
        {
            IsRightToLeft = isRightToLeft;
            Document = PdfDocument.Load(stream);
            GotoPage(PageNo = 0);
        }
        public void OpenPdf(Stream stream, string password, bool isRightToLeft = false)
        {
            IsRightToLeft = isRightToLeft;
            Document = PdfDocument.Load(stream, password);
            GotoPage(PageNo = 0);
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
                    RotatePage(PageNo, PdfRotation.Rotate90);
                    break;
                case PdfRotation.Rotate90:
                    RotatePage(PageNo, PdfRotation.Rotate180);
                    break;
                case PdfRotation.Rotate180:
                    RotatePage(PageNo, PdfRotation.Rotate270);
                    break;
                case PdfRotation.Rotate270:
                    RotatePage(PageNo, PdfRotation.Rotate0);
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
                    RotatePage(PageNo, PdfRotation.Rotate270);
                    break;
                case PdfRotation.Rotate90:
                    RotatePage(PageNo, PdfRotation.Rotate0);
                    break;
                case PdfRotation.Rotate180:
                    RotatePage(PageNo, PdfRotation.Rotate90);
                    break;
                case PdfRotation.Rotate270:
                    RotatePage(PageNo, PdfRotation.Rotate180);
                    break;
            }
        }
    }
}
