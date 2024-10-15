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

        public static string GetSelectionText(this IPdfDocument document)
        {
            var selections = document.Selections;
            if (selections.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var item in selections)
            {
                var page = document.Pages[item.PageIndex];
                var text = page.GetText(item.StartIndex, (item.EndIndex - item.StartIndex) + 1);
                sb.Append(text);
            }

            return sb.ToString();
        }
    }
}
