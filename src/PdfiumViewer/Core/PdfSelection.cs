using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfiumViewer.Core
{
    internal class PdfSelection
    {
        private bool _fixedEndIndex = false;
        public PdfSelection(int pageIndex, int initIndex, bool fixedEndIndex = false) : this(pageIndex, initIndex, initIndex, fixedEndIndex)
        {

        }

        public PdfSelection(int pageIndex, int beginIndex, int endIndex, bool fixedEndIndex = false)
        {
            PageIndex = pageIndex;
            BeginSelectionIndex = beginIndex;
            EndSelectionIndex = endIndex;
            _fixedEndIndex = fixedEndIndex;
        }
        public int PageIndex { get; }
        public int BeginSelectionIndex { get; set; }
        public int EndSelectionIndex { get; set; }

        public int StartIndex  => Math.Min(BeginSelectionIndex, EndSelectionIndex);
        public int EndIndex => Math.Max(BeginSelectionIndex, EndSelectionIndex);

        public void SetEndIndex(int endIndex)
        {
            if (_fixedEndIndex)
            {
                BeginSelectionIndex = endIndex;
            }
            else
            {
                EndSelectionIndex = endIndex;
            }
        }

        public override string ToString()
        {
            return $"selection: ({PageIndex})[{StartIndex}, {EndIndex}]";
        }
    }
}
