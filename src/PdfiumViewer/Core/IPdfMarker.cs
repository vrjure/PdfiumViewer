using System.Windows;
using System.Windows.Media;

namespace PdfiumViewer.Core
{
    /// <summary>
    /// Represents a marker on a PDF page.
    /// </summary>
    internal interface IPdfMarker
    {
        /// <summary>
        /// The page where the marker is drawn on.
        /// </summary>
        int Page { get; }
        Rect[] Bounds { get; }
    }
}
