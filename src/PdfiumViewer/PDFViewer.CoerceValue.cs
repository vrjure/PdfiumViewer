using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PdfiumViewer
{
    public partial class PDFViewer
    {
        private static object PageCoerceValueChanged(DependencyObject d, object value)
        {
            var v = d as PDFViewer;
            var page = (int)value;
            if (page < 0)
            {
                return 0;
            }
            else if (page >= v.PageCount)
            {
                return Math.Max(v.PageCount - 1, 0);
            }
            return value;
        }


        private static object ZoomCoerceValueChanged(DependencyObject d, object value)
        {
            var v = d as PDFViewer;
            var zoom = (double)value;
            if(zoom < v.ZoomMin)
            {
                return v.ZoomMin;
            }
            else if (zoom > v.ZoomMax)
            {
                return v.ZoomMax;
            }

            return value;
        }

        private static object ZoomMinCoerceValueChanged(DependencyObject d, object value)
        {
            var v = d as PDFViewer;
            var zoomMin = (double)value;
            if (zoomMin > v.ZoomMax)
            {
                return v.ZoomMax;
            }
            return value;
        }

        private static object ZoomMaxCoerceValueChanged(DependencyObject d, object value)
        {
            var v = d as PDFViewer;
            var zoomMax = (double)value;
            if (zoomMax < v.ZoomMin)
            {
                return v.ZoomMin;
            }

            return value;
        }

        private static object MatchIndexCoerceValueChanged(DependencyObject d, object value)
        {
            var v = d as PDFViewer;
            var matchIndex = (int)value;
            var total = v.Matches?.Items?.Count ?? 0;
            if (matchIndex < 0)
            {
                return 0;
            }
            else if (matchIndex >= total)
            {
                return Math.Max(total - 1, 0);
            }

            return value;
        }
    }
}
