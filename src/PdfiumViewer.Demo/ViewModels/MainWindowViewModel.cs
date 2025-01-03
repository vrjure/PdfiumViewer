﻿using CommunityToolkit.Mvvm.ComponentModel;
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
        public ICommand PageModeCommand => _pageModeCommand ??= new RelayCommand<string>(mode=> PageMode = Enum.Parse<PdfPageMode>(mode));

        private ICommand _fitWidthCommand;
        public ICommand FitWidthCommand => _fitWidthCommand ??= new RelayCommand(() => FitWidth = !FitWidth);

        private ICommand _rotateLeftCommand;
        public ICommand RotateLeftCommand => _rotateLeftCommand ??= new RelayCommand(Counterclockwise);

        private ICommand _rotateRightCommand;
        public ICommand RotateRightCommand => _rotateRightCommand ??= new RelayCommand(ClockwiseRotate);

        private ICommand _prevFoundCommand;
        public ICommand PrevFoundCommand => _prevFoundCommand ??= new RelayCommand(() => MatchIndex = Math.Max(0, MatchIndex - 1));

        private ICommand _nextFoundCommand;
        public ICommand NextFoundCommand => _nextFoundCommand ??= new RelayCommand(() => MatchIndex = Math.Min(MatchCount - 1, MatchIndex + 1));

        private ICommand _copyCommand;
        public ICommand CopyCommand => _copyCommand ??= new RelayCommand(CopyText);

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
            set
            {
                if(SetProperty(ref _isSearchOpen, value))
                {
                    if (!_isSearchOpen)
                    {
                        Matches = null;
                    }
                    else
                    {
                        Search();
                    }
                }
            }
        }

        private bool _enableHandTools;
        public bool EnableHandTools
        {
            get => _enableHandTools;
            set => SetProperty(ref _enableHandTools, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if(SetProperty(ref _searchText, value))
                {
                    Search();
                }
            }
        }

        private int _matchIndex;
        public int MatchIndex
        {
            get => _matchIndex;
            set => SetProperty(ref _matchIndex, value);
        }

        private int _matchCount;
        public int MatchCount
        {
            get => _matchCount;
            set => SetProperty(ref _matchCount, value);
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

        private bool _highlightAllMatches = false;
        public bool HighlightAllMatches
        {
            get => _highlightAllMatches;
            set => SetProperty(ref _highlightAllMatches, value);
        }

        private PdfMatches _matches;
        public PdfMatches Matches
        {
            get => _matches;
            set => SetProperty(ref _matches, value);
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

        private double _zoomMax = 4;
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

        private PdfPageMode _pageMode = PdfPageMode.Continuous;
        public PdfPageMode PageMode
        {
            get => _pageMode;
            set => SetProperty(ref _pageMode, value);
        }


        private bool _fitWidth;
        public bool FitWidth
        {
            get => _fitWidth;
            set => SetProperty(ref _fitWidth, value);
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
                    var size = Document.Pages[i].Size;
                    var image = Document.Pages[i].Render((int)size.Width, (int)size.Height, 96, 96, false);
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

        public void Search()
        {
            if (Document == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchText))
            {
                Matches = null;
            }

            Matches = Document.Search(SearchText, MatchCase, WholeWordOnly);
            MatchIndex = 0;
            if (Matches == null || Matches.Items == null || Matches.Items.Count == 0)
            {
                MatchCount = 0;
                return;
            }
            else
            {
                MatchCount = Matches.Items.Count;
            }
        }


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

        private void CopyText()
        {
            var str = Document?.GetSelectionText();
            if (string.IsNullOrEmpty(str)) return;
            Clipboard.SetText(str);
        }
    }
}
