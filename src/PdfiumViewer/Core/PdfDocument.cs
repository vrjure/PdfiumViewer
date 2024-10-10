using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using PdfiumViewer.Drawing;
using PdfiumViewer.Enums;

namespace PdfiumViewer.Core
{
    /// <summary>
    /// Provides functionality to render a PDF document.
    /// </summary>
    public class PdfDocument : IPdfDocument
    {
        private bool _disposed;
        private PdfFile _file;
        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided path.
        /// </summary>
        /// <param name="path">Path to the PDF document.</param>
        public static PdfDocument Load(string path)
        {
            return Load(path, null);
        }

        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided path.
        /// </summary>
        /// <param name="path">Path to the PDF document.</param>
        /// <param name="password">Password for the PDF document.</param>
        public static PdfDocument Load(string path, string password)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return Load(File.OpenRead(path), password);
        }

        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided path.
        /// </summary>
        /// <param name="owner">Window to show any UI for.</param>
        /// <param name="path">Path to the PDF document.</param>
        public static PdfDocument Load(IWin32Window owner, string path)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return Load(owner, File.OpenRead(path), null);
        }

        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided path.
        /// </summary>
        /// <param name="owner">Window to show any UI for.</param>
        /// <param name="stream">Stream for the PDF document.</param>
        public static PdfDocument Load(IWin32Window owner, Stream stream)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return Load(owner, stream, null);
        }

        public static PdfDocument Load(IWin32Window owner, Stream stream, string password)
        {
            try
            {
                while (true)
                {
                    return new PdfDocument(stream, password);
                }
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided stream.
        /// </summary>
        /// <param name="stream">Stream for the PDF document.</param>
        public static PdfDocument Load(Stream stream)
        {
            return Load(stream, null);
        }

        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided stream.
        /// </summary>
        /// <param name="stream">Stream for the PDF document.</param>
        /// <param name="password">Password for the PDF document.</param>
        public static PdfDocument Load(Stream stream, string password)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new PdfDocument(stream, password);
        }

        private int _pageCount;
        /// <summary>
        /// Number of pages in the PDF document.
        /// </summary>
        public int PageCount => _pageCount;

        public IReadOnlyList<PdfPage> Pages { get; }

        /// <summary>
        /// Bookmarks stored in this PdfFile
        /// </summary>
        public PdfBookmarkCollection Bookmarks => _file.Bookmarks;

        public PDFSelectionCollection Selections { get; }


        private PdfDocument(Stream stream, string password)
        {
            _file = new PdfFile(stream, password);
            _pageCount = _file.GetPageCount();

            var pages = new List<PdfPage>();
            for (var i = 0; i < PageCount; i++)
            {
                pages.Add(new PdfPage(_file._document, _file._form, i));
            }

            Pages = pages.AsReadOnly();

            Selections = new PDFSelectionCollection();
        }

        /// <summary>
        /// Save the PDF document to the specified location.
        /// </summary>
        /// <param name="stream">Stream to save the PDF document to.</param>
        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            _file.Save(stream);
        }

        /// <summary>
        /// Finds all occurences of text.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="matchCase">Whether to match case.</param>
        /// <param name="wholeWord">Whether to match whole words only.</param>
        /// <param name="startPage">The page to start searching.</param>
        /// <param name="endPage">The page to end searching.</param>
        /// <returns>All matches.</returns>
        public PdfMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage)
        {
            var matches = new List<PdfMatch>();

            if (String.IsNullOrEmpty(text))
                return new PdfMatches(startPage, endPage, matches);

            for (var page = startPage; page <= endPage; page++)
            {
                matches.AddRange(Pages[page].Search(text, matchCase, wholeWord));
            }
            return new PdfMatches(startPage, endPage, matches);
        }
       
        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the PDF document.
        /// </summary>
        /// <returns></returns>
        public PrintDocument CreatePrintDocument()
        {
            return CreatePrintDocument(PdfPrintMode.CutMargin);
        }

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the PDF document.
        /// </summary>
        /// <param name="printMode">Specifies the mode for printing. The default
        /// value for this parameter is CutMargin.</param>
        /// <returns></returns>
        public PrintDocument CreatePrintDocument(PdfPrintMode printMode)
        {
            return CreatePrintDocument(new PdfPrintSettings(printMode, null));
        }

        /// <summary>
        /// Creates a <see cref="PrintDocument"/> for the PDF document.
        /// </summary>
        /// <param name="settings">The settings used to configure the print document.</param>
        /// <returns></returns>
        public PrintDocument CreatePrintDocument(PdfPrintSettings settings)
        {
            return new PdfPrintDocument(this, settings);
        }

        /// <summary>
        /// Get metadata information from the PDF document.
        /// </summary>
        /// <returns>The PDF metadata.</returns>
        public PdfInformation GetInformation()
        {
            return _file.GetInformation();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <param name="disposing">Whether this method is called from Dispose.</param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                foreach (var item in Pages)
                {
                    item.Dispose();
                }
                if (_file != null)
                {
                    _file.Dispose();
                    _file = null;
                }

                _disposed = true;
            }
        }

        public string GetSelectionText()
        {
            if (Selections.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var item in Selections)
            {
                var page = Pages[item.PageIndex];
                var text = page.GetText(item.StartIndex, (item.EndIndex - item.StartIndex) + 1);
                sb.Append(text);
            }

            return sb.ToString();
        }
    }
}
