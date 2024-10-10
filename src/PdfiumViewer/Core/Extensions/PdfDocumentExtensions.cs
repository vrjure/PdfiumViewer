using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfiumViewer.Core
{
    public static class PdfDocumentExtensions
    {
        public static PdfMatches Search(this IPdfDocument document, string text, bool matchCase, bool wholeWord, int page)
        {
            return document.Search(text, matchCase, wholeWord, page, page);
        }

        public static PdfMatches Search(this IPdfDocument document, string text, bool matchCase, bool wholeWord)
        {
            return document.Search(text, matchCase, wholeWord, 0, document.PageCount - 1);
        }
    }
}
