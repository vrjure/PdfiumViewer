using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfiumViewer.Core
{
    public class PDFSelectionCollection : IReadOnlyCollection<PdfSelection>, IEnumerable<PdfSelection>
    {
        internal Stack<PdfSelection> _selections = new Stack<PdfSelection>();

        internal PDFSelectionCollection()
        {

        }

        public int Count => _selections.Count;

        public IEnumerator<PdfSelection> GetEnumerator()
        {
            return _selections.OrderBy(f => f.PageIndex).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
