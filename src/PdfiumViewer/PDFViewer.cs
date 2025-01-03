﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using PdfiumViewer.Core;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Media.Imaging;
using PdfiumViewer.Enums;
using System.Diagnostics;
using PdfiumViewer.Helpers;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Security.Cryptography;
using System.Timers;
using System.Windows.Threading;

namespace PdfiumViewer
{
    [TemplatePart(Name ="PART_Scroll")]
    public partial class PDFViewer : ItemsControl
    {

        public static readonly DependencyProperty PageProperty = DependencyProperty.Register(nameof(Page), typeof(int), typeof(PDFViewer), new FrameworkPropertyMetadata(0, PropertyChanged, PageCoerceValueChanged));
        public int Page
        {
            get => (int)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }

        public static readonly DependencyProperty PageCountProperty = DependencyProperty.Register(nameof(PageCount), typeof(int), typeof(PDFViewer), new FrameworkPropertyMetadata(0, PropertyChanged, (s, e)=> (s as PDFViewer).GetPageCount()));
        public int PageCount
        {
            get => (int)GetValue(PageCountProperty);
            set => SetValue(PageCountProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(PDFViewer), new FrameworkPropertyMetadata(null, PropertyChanged));
        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(IPdfDocument), typeof(PDFViewer), new FrameworkPropertyMetadata(null, PropertyChanged));
        public IPdfDocument Document
        {
            get => (IPdfDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public static readonly DependencyProperty DpiProperty = DependencyProperty.Register(nameof(Dpi), typeof(int), typeof(PDFViewer), new FrameworkPropertyMetadata(96, PropertyChanged));
        public int Dpi
        {
            get => (int)GetValue(DpiProperty);
            set => SetValue(DpiProperty, value);
        }

        public static readonly DependencyProperty FlagsProperty = DependencyProperty.Register(nameof(Flags), typeof(PdfRenderFlags), typeof(PDFViewer), new FrameworkPropertyMetadata(PdfRenderFlags.None, PropertyChanged));
        public PdfRenderFlags Flags
        {
            get => (PdfRenderFlags)GetValue(FlagsProperty);
            set => SetValue(FlagsProperty, value);
        }

        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register(nameof(Rotation), typeof(PdfRotation), typeof(PDFViewer), new FrameworkPropertyMetadata(PdfRotation.Rotate0, PropertyChanged));
        public PdfRotation Rotation
        {
            get => (PdfRotation)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(PDFViewer), new FrameworkPropertyMetadata(1d, PropertyChanged, ZoomCoerceValueChanged));
        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public static readonly DependencyProperty ZoomMinProperty = DependencyProperty.Register(nameof(ZoomMin), typeof(double), typeof(PDFViewer), new FrameworkPropertyMetadata(DefaultZoomMin, PropertyChanged, ZoomMinCoerceValueChanged));
        public double ZoomMin
        {
            get => (double)GetValue(ZoomMinProperty);
            set => SetValue(ZoomMinProperty, value);
        }

        public static readonly DependencyProperty ZoomMaxProperty = DependencyProperty.Register(nameof(ZoomMax), typeof(double), typeof(PDFViewer), new FrameworkPropertyMetadata(DefaultZoomMax, PropertyChanged, ZoomMaxCoerceValueChanged));
        public double ZoomMax
        {
            get => (double)GetValue(ZoomMaxProperty);
            set => SetValue(ZoomMaxProperty, value);
        }

        public static readonly DependencyProperty FitWidthProperty = DependencyProperty.Register(nameof(FitWidth), typeof(bool), typeof(PDFViewer), new FrameworkPropertyMetadata(false, PropertyChanged));
        public bool FitWidth
        {
            get => (bool)GetValue(FitWidthProperty);
            set => SetValue(FitWidthProperty, value);
        }

        public static readonly DependencyProperty PageModeProperty = DependencyProperty.Register(nameof(PageMode), typeof(PdfPageMode), typeof(PDFViewer), new FrameworkPropertyMetadata(PdfPageMode.Continuous, PropertyChanged));
        public PdfPageMode PageMode
        {
            get => (PdfPageMode)GetValue(PageModeProperty);
            set => SetValue(PageModeProperty, value);
        }

        private static readonly DependencyPropertyKey RenderRangePropertyKey = DependencyProperty.RegisterReadOnly(nameof(RenderRange), typeof(RenderRange), typeof(PDFViewer), new PropertyMetadata(RenderRange.Invalid, PropertyChanged));
        public static readonly DependencyProperty RenderRangeProperty = RenderRangePropertyKey.DependencyProperty;
        public RenderRange RenderRange
        {
            get => (RenderRange)GetValue(RenderRangePropertyKey.DependencyProperty);
            private set => SetValue(RenderRangePropertyKey, value);
        }


        private ScrollViewer _scroll;
        private ScrollViewer Scroll
        {
            get => _scroll;
            set
            {
                if (_scroll != null)
                {
                    _scroll.ScrollChanged -= _scroll_ScrollChanged;
                }

                _scroll = value;

                if (_scroll != null)
                {
                    _scroll.ScrollChanged += _scroll_ScrollChanged;
                }
            }
        }


        private const double DefaultZoomMin = 0.1;
        private const double DefaultZoomMax = 4;

        private int _scrollPage;
        private Size _renderPageSize;
        private double _renderZoom = 1;
        private Queue<Action> _renderQueue;
        private DispatcherTimer _renderTimer;

        public PDFViewer()
        {
            _renderQueue = new Queue<Action>();
            _renderTimer = new DispatcherTimer(DispatcherPriority.Background, this.Dispatcher);
            _renderTimer.Interval = TimeSpan.FromMilliseconds(15);
            _renderTimer.Tick += _renderTimer_Tick;
        }

        static PDFViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PDFViewer), new FrameworkPropertyMetadata(typeof(PDFViewer)));
        }


        private void _renderTimer_Tick(object sender, EventArgs e)
        {
            _renderTimer.Stop();
            if (_renderQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Scroll = GetTemplateChild("PART_Scroll") as ScrollViewer;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var container = new PDFViewerItemContainer();
            container.PreviewMouseDown -= Container_PreviewMouseDown;
            container.PreviewMouseDown += Container_PreviewMouseDown;

            container.PreviewMouseMove -= Container_PreviewMouseMove;
            container.PreviewMouseMove += Container_PreviewMouseMove;

            container.PreviewMouseUp -= Container_PreviewMouseUp;
            container.PreviewMouseUp += Container_PreviewMouseUp;

            container.MouseLeave -= Container_MouseLeave;
            container.MouseLeave += Container_MouseLeave;

            return container;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return false;
        }


        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var v = d as PDFViewer;
            if (v == null)
            {
                return;
            }

            if (e.Property == SourceProperty)
            {
                v.SourceChanged();
            }
            else if (e.Property == DocumentProperty)
            {
                v.DocumentChanged(e.OldValue as PdfDocument, e.NewValue as PdfDocument);
            }
            else if (e.Property == PageProperty)
            {
                if (v._scrollPage == v.Page)
                {
                    return;
                }
                v.ToPage();
            }
            else if (e.Property == ZoomProperty || e.Property == FitWidthProperty || e.Property == PageModeProperty || e.Property == RotationProperty)
            {
                v.ReadyToRender(v.PageSizeRefresh);
            }
            else if (e.Property == DpiProperty)
            {
                v.ReadyToRender(()=>v.Render(true));
            }
            else if (e.Property == MatchesProperty || e.Property == HighlightAllMatchesProperty)
            {
                v.OnMatchesChanged();
            }
            else if (e.Property == MatchIndexProperty)
            {
                v.OnMatchIndexChanged((int)e.NewValue, (int)e.OldValue);
            }
            else if ( e.Property == MatchBrushProperty || e.Property == MatchBorderBrushProperty || e.Property == MatchBorderThicknessProperty
                || e.Property == CurrentMatchBrushProperty || e.Property == CurrentMatchBorderBrushProperty || e.Property == CurrentMatchBorderThicknessProperty)
            {
                v.RenderMarkers();
            }
            else if (e.Property == SelectionBorderBrushProperty || e.Property == SelectionBrushProperty || e.Property == SelectionBorderThicknessProperty)
            {
                v.RefreshSelection();
            }
            else if (e.Property == RenderRangeProperty)
            {
                if (v.RenderRange != RenderRange.Invalid)
                {
                    v.ReadyToRender(() =>
                    {
                        v.Render();
                        v.RenderMarkers();
                        v.RefreshSelection();
                    });
                }
            }
        }

        private void ReadyToRender(Action renderAction)
        {
            _renderTimer.Stop();
            _renderQueue.TryDequeue(out _);
            _renderQueue.Enqueue(renderAction);
            _renderTimer.Start();
        }

        private void SourceChanged()
        {
            if (Source != null)
            {          
                SetCurrentValue(DocumentProperty, PdfDocument.Load(Source));
            }
            else
            {
                Document?.Dispose();
                SetCurrentValue(DocumentProperty, null);
            }
        }


        private void DocumentChanged(PdfDocument oldDoc, PdfDocument newDoc)
        {
            ClearSelections();
            SetCurrentValue(MatchesProperty, null);
            RenderRange = RenderRange.Invalid;
            oldDoc?.Dispose();
            Items.Clear();
            SetCurrentValue(PageProperty, 0);

            if (newDoc == null)
            {
                return;
            }
            SetCurrentValue(PageCountProperty, GetPageCount());

            if (PageCount <= 0)
            {
                return;
            }

            _renderPageSize = GetRenderPageSize(0);
            for (var i = 0; i < PageCount; i++)
            {
                var image = new Image()
                {
                    Width = _renderPageSize.Width,
                    Height = _renderPageSize.Height,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    LayoutTransform = new ScaleTransform()
                };
                Items.Add(image);
            }

            PageSizeRefresh();
        }

        private void PageSizeRefresh()
        {
            if (Document == null)
            {
                return;
            }

            Render(true);
            RenderMarkers();
            RefreshSelection(true);
        }

        private Size GetRenderPageSize(int page)
        {
            var size = Document.Pages[page].Size;
            var width = 0d;
            if (PageMode == PdfPageMode.Continuous)
            {
                width = FitWidth ? this.ActualWidth : size.Width * Zoom;

            }
            else
            {
                width = FitWidth ? this.ActualWidth / 2 : size.Width * Zoom;
            }
            _renderZoom = FitWidth ? width / size.Width : Zoom;
            var height = FitWidth ? _renderZoom * size.Height : size.Height * _renderZoom;

            return new Size(width, height);
        }

        private int GetPageCount()
        {
            return Document?.PageCount ?? 0;
        }

        private void ToPage()
        {
            if (Items == null || Page >= Items.Count)
            {
                return;
            }

            Debug.WriteLine($"To page {Page}");

            var offset = 0d;
            for (int i = 0; i < Page; i++)
            {
                offset += (ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement).ActualHeight;
            }

            Scroll.ScrollToVerticalOffset(offset);
        }

        private void _scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Document == null || Items.Count == 0)
            {
                return;
            }

            Debug.WriteLine($"scrollchanged = [{e.VerticalChange}, {e.ExtentHeightChange}, {e.ViewportHeightChange}]");
            var firstContainer = ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
            var pageSize = firstContainer.RenderSize;
            var itemHeight = Rotation switch
            {
                PdfRotation.Rotate90 => pageSize.Width,
                PdfRotation.Rotate270 => pageSize.Width,
                _ => pageSize.Height
            };

            if (e.ExtentHeightChange != 0 && RenderRange != RenderRange.Invalid)
            {
                if (PageMode == PdfPageMode.Continuous)
                {
                    var offset = e.ExtentHeightChange / Items.Count * RenderRange.RenderStartIndex;
                    Scroll.ScrollToVerticalOffset(e.VerticalOffset + offset - e.VerticalChange);
                }
                else
                {
                    var offset = e.ExtentHeightChange / Math.Ceiling(Items.Count / 2d) * (RenderRange.RenderStartIndex / 2);
                    Scroll.ScrollToVerticalOffset(e.VerticalOffset + offset - e.VerticalChange);
                }
            }
            else
            {
                var viewPort_h = e.ViewportHeight;
                var offset_v = e.VerticalOffset;

                if (PageMode == PdfPageMode.Continuous)
                {
                    var renderStartIndex = Math.Max((int)((offset_v - viewPort_h) / itemHeight), 0);
                    var renderEndIndex = Math.Min((int)((offset_v + viewPort_h) / itemHeight), PageCount - 1);

                    RenderRange = new RenderRange(Math.Max(renderStartIndex, 0), Math.Min(renderEndIndex + 1, Items.Count - 1));
                    _scrollPage = (int)Math.Round(offset_v / itemHeight);
                    
                }
                else if (PageMode == PdfPageMode.Double)
                {
                    var renderStartIndex = Math.Max((int)((offset_v - viewPort_h) / itemHeight * 2), 0);
                    var renderEndIndex = Math.Min((int)((offset_v + viewPort_h) / itemHeight * 2), PageCount - 1);

                    RenderRange = new RenderRange(Math.Max(renderStartIndex, 0), Math.Min(renderEndIndex + 1, Items.Count - 1));
                    _scrollPage = (int)Math.Round(offset_v / itemHeight * 2);
                }

                Debug.WriteLine($"Render range = [{RenderRange.RenderStartIndex},{RenderRange.RenderEndIndex}]]");
                if (_scrollPage < Items.Count)
                {
                    SetCurrentValue(PageProperty, _scrollPage);
                }
            }
        }

        private void Render(bool force = false)
        {
            _renderPageSize = GetRenderPageSize(0);
            if (this.RenderRange != RenderRange.Invalid && RenderRange.RenderStartIndex <= RenderRange.RenderEndIndex && RenderRange.RenderStartIndex >= 0)
            {
                Render(RenderRange.RenderStartIndex, Math.Min(RenderRange.RenderEndIndex, Items.Count - 1), force);
            }
        }

        private void Render(int startIndex, int endIndex, bool force = false)
        {
            for (int i = startIndex - 1; i >= 0; i--)
            {
                var frame = Items[i] as Image;
                StopRenderAnimation(frame);
                if (frame.Source != null)
                {
                    frame.Source = null;
                }

                if (frame.Width != _renderPageSize.Width || frame.Height != _renderPageSize.Height)
                {
                    frame.Width = _renderPageSize.Width;
                    frame.Height = _renderPageSize.Height;
                }
            }

            var count = Document.PageCount;
            for (int i = endIndex + 1; i < count; i++)
            {
                var frame = Items[i] as Image;
                StopRenderAnimation(frame);
                if (frame.Source != null)
                {
                    frame.Source = null;
                }

                if (frame.Width != _renderPageSize.Width || frame.Height != _renderPageSize.Height)
                {
                    frame.Width = _renderPageSize.Width;
                    frame.Height = _renderPageSize.Height;
                }
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                var frame = Items[i] as Image;
                if (force || frame.Source == null)
                {
                    if (frame.Source == null)
                    {
                        RenderPage(frame, i, false);
                    }
                    else
                    {
                        RenderPage(frame, i, true);
                    }
                }
            }
        }

        private void RenderPage(Image frame, int page, bool animation)
        {
            if (frame == null || Document == null) return;

            var size = _renderPageSize;
            Debug.WriteLine($"page = {page}; size = {size}; animation={animation}");
            var pdfPage = Document.Pages[page];
            if (animation)
            {
                BeginRenderAnimation(frame, pdfPage, size);
            }
            else
            {
                RenderPage(frame, pdfPage, size);
            }
        }

        private void RenderPage(Image frame, PdfPage page, Size renderSize)
        {
            frame.Width = _renderPageSize.Width;
            frame.Height = _renderPageSize.Height;

            this.Dispatcher.BeginInvoke(() =>
            {
                var image = page.Render((int)renderSize.Width, (int)renderSize.Height, Dpi, Dpi, PdfRotation.Rotate0, Flags);
                using (var memory = new MemoryStream())
                {
                    image.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.DecodePixelWidth = image.Width;
                    bitmapImage.DecodePixelHeight = image.Height;
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // not a mistake - see below
                    bitmapImage.EndInit();

                    frame.Source = bitmapImage;
                }
            }, DispatcherPriority.Background);
            
        }

        private void BeginRenderAnimation(Image frame, PdfPage page, Size renderSize)
        {
            var scaleTransform = frame.LayoutTransform as ScaleTransform;
            if (scaleTransform == null)
            {
                return;
            }

            var scaleToX = renderSize.Width / frame.Width;
            var scaleToY = renderSize.Height / frame.Height;

            var duration = TimeSpan.FromSeconds(0.2);
            var scaleXAnimation = new DoubleAnimation
            {
                To = scaleToX,
                FillBehavior = FillBehavior.Stop,
                Duration = duration
            };

            var scaleYAnimation = new DoubleAnimation
            {
                To = scaleToY,
                FillBehavior = FillBehavior.Stop,
                Duration = duration
            };

            scaleYAnimation.Completed += (s, e) =>
            {
                Debug.WriteLine("scaleYAnimation complated");
                RenderPage(frame, page, renderSize);
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation, HandoffBehavior.SnapshotAndReplace);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation, HandoffBehavior.SnapshotAndReplace);
        }

        private void StopRenderAnimation(Image frame)
        {
            var screTransform = frame.LayoutTransform as ScaleTransform;
            if (screTransform == null)
            {
                return;
            }

            screTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            screTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }
    }
}
