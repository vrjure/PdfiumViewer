using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfiumViewer.Core;
using PdfiumViewer.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PdfiumViewer.Demo.ViewModels
{
    internal class MainWindowViewModel : ObservableObject
    {
        private ICommand _openPdfCommand;
        public ICommand OpenPdfCommand => _openPdfCommand ??= new RelayCommand(OpenPdf);

        private ICommand _saveImageCommand;
        public ICommand SaveImageCommand => _saveImageCommand ??= new RelayCommand(SaveAsImage);

        private ICommand _showBookMarksCommand;
        public ICommand ShowBookMarksCommand => _showBookMarksCommand ??= new RelayCommand(() => ShowBookmarks = !ShowBookmarks);

        private ICommand _searchOpnCloseCommand;
        public ICommand SearchOpenCloseCommand => _searchOpnCloseCommand ??= new RelayCommand(() => IsSearchOpen = !IsSearchOpen);

        private ICommand _toRtlCommand;
        public ICommand ToRtlCommand => _toRtlCommand ??= new RelayCommand<string>(f => IsRtl = bool.Parse(f));

        private ICommand _prevPageCommand;
        public ICommand PrevPageCommand => _prevPageCommand ??= new RelayCommand(() => Page =  Math.Max(Page - 1, 0));

        private ICommand _nextPageCommand;
        public ICommand NextPageCommand => _nextPageCommand ??= new RelayCommand(() => Page = Math.Min(Page + 1, PageCount - 1));

        private ICommand _zoomInCommand;
        public ICommand ZoomInCommand => _zoomInCommand ??= new RelayCommand(() => Zoom = Math.Min(Zoom + 0.2, ZoomMax));

        private ICommand _zoomOutCommand;
        public ICommand ZoomOutCommand => _zoomOutCommand ??= new RelayCommand(() => Zoom = Math.Max(Zoom - 0.2, ZoomMin));

        private ICommand _pageModeCommand;
        public ICommand PageModeCommand => _pageModeCommand ??= new RelayCommand<string>(mode=> PageMode = Enum.Parse<PdfViewerPagesDisplayMode>(mode));

        private ICommand _zoomModeCommand;
        public ICommand ZoomModeCommand => _zoomModeCommand ??= new RelayCommand<string>(mode => ZoomMode = Enum.Parse<PdfViewerZoomMode>(mode));

        private ICommand _rotateLeftCommand;
        public ICommand RotateLeftCommand => _rotateLeftCommand ??= new RelayCommand(Counterclockwise);

        private ICommand _rotateRightCommand;
        public ICommand RotateRightCommand => _rotateRightCommand ??= new RelayCommand(ClockwiseRotate);

        //private ICommand _prevFoundCommand;
        //public ICommand PrevFoundCommand => _prevFoundCommand ??= new RelayCommand(OnPrevFoundClick);

        //private ICommand _nextFoundCommand;
        //public ICommand NextFoundCommad => _nextFoundCommand ??= new RelayCommand(OnNextFoundClick);
        
        private string _pdfPath;
        public string PdfPath
        {
            get => _pdfPath;
            set => SetProperty(ref _pdfPath, value);
        }

        private IPdfDocument _document;
        public IPdfDocument Document
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }

        private bool _renderAll;
        public bool RenderAll
        {
            get => _renderAll;
            set => SetProperty(ref _renderAll, value);
        }

        private bool _showBookMarks;
        public bool ShowBookmarks
        {
            get => _showBookMarks;
            set => SetProperty(ref _showBookMarks, value);
        }

        private PdfBookmark _selectedBookMark;
        public PdfBookmark SelectedBookMark
        {
            get => _selectedBookMark;
            set
            {
                if(SetProperty(ref _selectedBookMark, value))
                {
                    SelectedBookmarkChanged();
                }
            }
        }

        private bool _isSearchOpen;
        public bool IsSearchOpen
        {
            get => _isSearchOpen;
            set => SetProperty(ref _isSearchOpen, value);
        }

        private bool _enableHandTools;
        public bool EnableHandTools
        {
            get => _enableHandTools;
            set => SetProperty(ref _enableHandTools, value);
        }

        //private PdfSearchManager _searchManager;
        //public PdfSearchManager SearchManager
        //{
        //    get => _searchManager;
        //    set => SetProperty(ref _searchManager, value);
        //}


        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private int _searchMatchItemNo;
        public int SearchMatchItemNo
        {
            get => _searchMatchItemNo;
            set => SetProperty(ref _searchMatchItemNo, value);
        }

        private int _searchMathcsCount;
        public int SearchMatchesCount
        {
            get => _searchMathcsCount;
            set => SetProperty(ref _searchMathcsCount, value);
        }


        private bool _matchCase;
        public bool MatchCase
        {
            get => _matchCase;
            set => SetProperty(ref _matchCase, value);
        }

        private bool _wholeWordOnly;
        public bool WholeWordOnly
        {
            get => _wholeWordOnly;
            set => SetProperty(ref _wholeWordOnly, value);
        }

        private bool _highlightAllMatches;
        public bool HighlightAllMatches
        {
            get => _highlightAllMatches;
            set => SetProperty(ref _highlightAllMatches, value);
        }

        private bool _isRtl;
        public bool IsRtl
        {
            get => _isRtl;
            set => SetProperty(ref _isRtl, value);
        }

        private int _page;
        public int Page
        {
            get => _page;
            set => SetProperty(ref _page, value);
        }


        private int _pageCount;
        public int PageCount
        {
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
        }

        private double _zoom = 1;
        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        private double _zoomMax = 5;
        public double ZoomMax
        {
            get => _zoomMax;
            set => SetProperty(ref _zoomMax, value);
        }

        private double _zoomMin = 0.2;
        public double ZoomMin
        {
            get => _zoomMin;
            set => SetProperty(ref _zoomMin, value);
        }


        private PdfViewerPagesDisplayMode _pageMode = PdfViewerPagesDisplayMode.ContinuousMode;
        public PdfViewerPagesDisplayMode PageMode
        {
            get => _pageMode;
            set => SetProperty(ref _pageMode, value);
        }


        private PdfViewerZoomMode _zoomMode = PdfViewerZoomMode.FitHeight;
        public PdfViewerZoomMode ZoomMode
        {
            get => _zoomMode;
            set => SetProperty(ref _zoomMode, value);
        }

        private PdfRotation _rotation = PdfRotation.Rotate0;
        public PdfRotation Rotation
        {
            get => _rotation;
            set => SetProperty(ref _rotation, value);
        }

        public MainWindowViewModel()
        {
            
        }
        private void OpenPdf()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Open PDF File"
            };

            if (dialog.ShowDialog() == true)
            {
                PdfPath = dialog.FileName;
            }
        }

        private void SaveAsImage()
        {
            // Create a "Save As" dialog for selecting a directory (HACK)
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select a Directory",
                Multiselect = false
            };
            // instead of default "Save As"
            // Prevents displaying files
            // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FolderName;
                // Our final value is in path
                SaveAsImages(path);
            }
        }

        private void SaveAsImages(string path)
        {
            if (Document == null)
            {
                return;
            }
            try
            {
                for (var i = 0; i < Document.PageCount; i++)
                {
                    var size = Document.GetPageSize(i);
                    var image = Document.Render(i, (int)size.Width, (int)size.Height, 96, 96, false);
                    image.Save(Path.Combine(path, $"img{i}.png"));
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void SelectedBookmarkChanged()
        {
            if (SelectedBookMark == null)
            {
                return;
            }

            Page = SelectedBookMark.PageIndex;
        }

        //public void Search()
        //{
        //    SearchMatchItemNo = 0;
        //    SearchManager.MatchCase = MatchCase;
        //    SearchManager.MatchWholeWord = WholeWordOnly;
        //    SearchManager.HighlightAllMatches = HighlightAllMatches;

        //    if (!SearchManager.Search(SearchText))
        //    {
        //        MessageBox.Show("No matches found.");
        //    }
        //    else
        //    {
        //        SearchMatchesCount = SearchManager.MatchesCount;
        //        // DisplayTextSpan(SearchMatches.Items[SearchMatchItemNo++].TextSpan);
        //    }

        //    if (!SearchManager.FindNext(true))
        //        MessageBox.Show( "Find reached the starting point of the search.");
        //}
        //private void OnNextFoundClick()
        //{
        //    if (SearchMatchesCount > SearchMatchItemNo)
        //    {
        //        SearchMatchItemNo++;
        //        //DisplayTextSpan(SearchMatches.Items[SearchMatchItemNo - 1].TextSpan);
        //        SearchManager.FindNext(true);
        //    }
        //}

        //private void OnPrevFoundClick()
        //{
        //    if (SearchMatchItemNo > 1)
        //    {
        //        SearchMatchItemNo--;
        //        // DisplayTextSpan(SearchMatches.Items[SearchMatchItemNo - 1].TextSpan);
        //        SearchManager.FindNext(false);
        //    }
        //}


        public void ClockwiseRotate()
        {
            // _____
            //      |
            //      |
            //      v
            // Clockwise

            switch (Rotation)
            {
                case PdfRotation.Rotate0:
                    Rotation = PdfRotation.Rotate90;
                    break;
                case PdfRotation.Rotate90:
                    Rotation = PdfRotation.Rotate180;
                    break;
                case PdfRotation.Rotate180:
                    Rotation = PdfRotation.Rotate270;
                    break;
                case PdfRotation.Rotate270:
                    Rotation = PdfRotation.Rotate0;
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

            switch (Rotation)
            {
                case PdfRotation.Rotate0:
                    Rotation = PdfRotation.Rotate270;
                    break;
                case PdfRotation.Rotate90:
                    Rotation = PdfRotation.Rotate0;
                    break;
                case PdfRotation.Rotate180:
                    Rotation = PdfRotation.Rotate90;
                    break;
                case PdfRotation.Rotate270:
                    Rotation = PdfRotation.Rotate180;
                    break;
            }
        }
    }
}
