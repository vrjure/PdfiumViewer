using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using PdfiumViewer.Core;
using PdfiumViewer.Drawing;
using PdfiumViewer.Enums;

namespace PdfiumViewer
{
    /// <summary>
    /// Represents a PDF document.
    /// </summary>
    public interface IPdfDocument : IDisposable
    {
        /// <summary>
        /// Number of pages in the PDF document.
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Bookmarks stored in this PdfFile
        /// </summary>
        PdfBookmarkCollection Bookmarks { get; }

        IReadOnlyList<PdfPage> Pages { get; }
        PDFSelectionCollection Selections { get; }
        /// <summary>
        /// Save the PDF document to the specified location.
        /// </summary>
        /// <param name="stream">Stream to save the PDF document to.</param>
        void Save(Stream stream);

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="startPage">The page to start searching.</param>
        /// <param name="endPage">The page to end searching.</param>
        /// <returns>All matches.</returns>
        PdfMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage);

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the PDF document.
        /// </summary>
        /// <returns></returns>
        PrintDocument CreatePrintDocument();

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the PDF document.
        /// </summary>
        /// <param name="printMode">Specifies the mode for printing. The default
        /// value for this parameter is CutMargin.</param>
        /// <returns></returns>
        PrintDocument CreatePrintDocument(PdfPrintMode printMode);

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the PDF document.
        /// </summary>
        /// <param name="settings">The settings used to configure the print document.</param>
        /// <returns></returns>
        PrintDocument CreatePrintDocument(PdfPrintSettings settings);

        /// <summary>
        /// Get metadata information from the PDF document.
        /// </summary>
        /// <returns>The PDF metadata.</returns>
        PdfInformation GetInformation();
    }
}
