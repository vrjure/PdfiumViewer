using PdfiumViewer.Core;
using PdfiumViewer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Size = System.Drawing.Size;

namespace PdfiumViewer
{
    // ScrollPanel.Properties
    partial class PdfRenderer : ScrollViewer
    {
        public static readonly DependencyProperty PageProperty = DependencyProperty.Register(nameof(Page), typeof(int), typeof(PdfRenderer), new FrameworkPropertyMetadata(0, PropertyChanged));
        public int Page
        {
            get => (int)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }

        public static readonly DependencyProperty DpiProperty = DependencyProperty.Register(nameof(Dpi), typeof(int), typeof(PdfRenderer), new FrameworkPropertyMetadata(96, PropertyChanged));
        public int Dpi
        {
            get => (int)GetValue(DpiProperty);
            set => SetValue(DpiProperty, value);
        }

        public static readonly DependencyProperty ZoomModeProperty = DependencyProperty.Register(nameof(ZoomMode), typeof(PdfViewerZoomMode), typeof(PdfRenderer), new FrameworkPropertyMetadata(PdfViewerZoomMode.FitHeight, PropertyChanged));
        public PdfViewerZoomMode ZoomMode
        {
            get => (PdfViewerZoomMode)GetValue(ZoomModeProperty);
            set => SetValue(ZoomModeProperty, value);
        }

        public static readonly DependencyProperty FlagsProperty = DependencyProperty.Register(nameof(Flags), typeof(PdfRenderFlags), typeof(PdfRenderer), new FrameworkPropertyMetadata(PdfRenderFlags.None, PropertyChanged));
        public PdfRenderFlags Flags
        {
            get => (PdfRenderFlags)GetValue(FlagsProperty);
            set => SetValue(FlagsProperty, value);
        }

        public static readonly DependencyProperty RotateProperty = DependencyProperty.Register(nameof(Rotate), typeof(PdfRotation), typeof(PdfRenderer), new FrameworkPropertyMetadata(PdfRotation.Rotate0, PropertyChanged));
        public PdfRotation Rotate
        {
            get => (PdfRotation)GetValue(RotateProperty);
            set => SetValue(RotateProperty, value);
        }

        public static readonly DependencyProperty PagesDisplayModeProperty = DependencyProperty.Register(nameof(PagesDisplayMode), typeof(PdfViewerPagesDisplayMode), typeof(PdfRenderer), new FrameworkPropertyMetadata(PdfViewerPagesDisplayMode.ContinuousMode, PropertyChanged));
        public PdfViewerPagesDisplayMode PagesDisplayMode
        {
            get => (PdfViewerPagesDisplayMode)GetValue(PagesDisplayModeProperty);
            set => SetValue(PagesDisplayModeProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(PdfRenderer), new FrameworkPropertyMetadata(null, PropertyChanged));
        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(PdfDocument), typeof(PdfRenderer), new FrameworkPropertyMetadata(null, PropertyChanged));
        public PdfDocument Document
        {
            get => (PdfDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }


        private static readonly DependencyPropertyKey PageCountPropertyKey = DependencyProperty.RegisterReadOnly(nameof(PageCount), typeof(int), typeof(PdfRenderer), new PropertyMetadata(0));
        public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;
        public int PageCount
        {
            get => (int)GetValue(PageCountPropertyKey.DependencyProperty);
            private set => SetValue(PageCountPropertyKey, value);
        }


        public static readonly DependencyProperty MouseWheelModeProperty = DependencyProperty.Register(nameof(MouseWheelMode), typeof(MouseWheelMode), typeof(PdfRenderer), new FrameworkPropertyMetadata(MouseWheelMode.PanAndZoom, PropertyChanged));
        public MouseWheelMode MouseWheelMode
        {
            get => (MouseWheelMode)GetValue(MouseWheelModeProperty);
            set => SetValue(MouseWheelModeProperty, value);
        }


        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(PdfRenderer), new FrameworkPropertyMetadata(1d, PropertyChanged));
        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public static readonly DependencyProperty ZoomMinProperty = DependencyProperty.Register(nameof(ZoomMin), typeof(double), typeof(PdfRenderer), new FrameworkPropertyMetadata(DefaultZoomMin, PropertyChanged));
        public double ZoomMin
        {
            get => (double)GetValue(ZoomMinProperty);
            set => SetValue(ZoomMinProperty, value);
        }

        public static readonly DependencyProperty ZoomMaxProperty = DependencyProperty.Register(nameof(ZoomMax), typeof(double), typeof(PdfRenderer), new FrameworkPropertyMetadata(DefaultZoomMax, PropertyChanged));
        public double ZoomMax
        {
            get => (double)GetValue(ZoomMaxProperty);
            set => SetValue(ZoomMaxProperty, value);
        }

        public static readonly DependencyProperty ZoomStepProperty = DependencyProperty.Register(nameof(ZoomStep), typeof(double), typeof(PdfRenderer), new FrameworkPropertyMetadata(DefaultZoomStep, PropertyChanged));
        public double ZoomStep
        {
            get => (double)GetValue(ZoomStepProperty);
            set => SetValue(ZoomStepProperty, value);
        }

        public static readonly DependencyProperty IsRightToLeftProperty = DependencyProperty.Register(nameof(IsRightToLeft), typeof(bool), typeof(PdfRenderer), new FrameworkPropertyMetadata(false));
        public bool IsRightToLeft
        {
            get => (bool)GetValue(IsRightToLeftProperty);
            set => SetValue(IsRightToLeftProperty, value);
        }

        public static readonly DependencyProperty RenderAllPagesProperty = DependencyProperty.Register(nameof(RenderAllPages), typeof(bool), typeof(PdfRenderer), new PropertyMetadata(false, PropertyChanged));
        public bool RenderAllPages
        {
            get => (bool)GetValue(RenderAllPagesProperty);
            set => SetValue(RenderAllPagesProperty, value);
        }


        public PdfRenderer()
        {
            IsTabStop = true;

            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Effect = new DropShadowEffect()
            {
                BlurRadius = 10,
                Direction = 270,
                RenderingBias = RenderingBias.Performance,
                ShadowDepth = 0
            };
            Panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            VirtualizingPanel.SetIsVirtualizing(Panel, true);
            VirtualizingPanel.SetVirtualizationMode(Panel, VirtualizationMode.Recycling);
            Content = Panel;

            ScrollWidth = 50;
            FrameSpace = new Thickness(5);

            Markers = new PdfMarkerCollection();

            this.Loaded += (s, e) =>
            {
                SearchManager = new PdfSearchManager(this);
            };
        }

        public event EventHandler<int> PageChanged;
        public event EventHandler MouseClick;
        public const double DefaultZoomMin = 0.1;
        public const double DefaultZoomMax = 5;
        public const double DefaultZoomStep = 1.2;
        protected bool IsDisposed;
        protected const int SmallScrollChange = 1;
        protected const int LargeScrollChange = 10;
        protected Process CurrentProcess { get; } = Process.GetCurrentProcess();
        protected StackPanel Panel { get; set; }
        protected Thickness FrameSpace { get; set; }
        protected Image Frame1 => Frames?.FirstOrDefault();
        protected Image Frame2 => Frames?.Length > 1 ? Frames[1] : null;
        protected Image[] Frames { get; set; }
        protected Size CurrentPageSize { get; set; }
        protected int ScrollWidth { get; set; }
        protected int MouseWheelDelta { get; set; }
        protected long MouseWheelUpdateTime { get; set; }


        public bool IsDocumentLoaded => Document != null && ActualWidth > 0 && ActualHeight > 0;

        public PdfBookmarkCollection Bookmarks => Document?.Bookmarks;

        protected void ScrollToPage(int page)
        {
            if (PagesDisplayMode == PdfViewerPagesDisplayMode.ContinuousMode)
            {
                //
                // scroll to current page
                //
                // var pageSize = CalculatePageSize(page);
                // var verticalOffset = page * (pageSize.Height + FrameSpace.Top + FrameSpace.Bottom);
                // ScrollToVerticalOffset(verticalOffset);
                Frames?[page].BringIntoView();
            }
        }

        private static void PropertyChanged(DependencyObject d,  DependencyPropertyChangedEventArgs e)
        {
            var sp = d as PdfRenderer;
            if (sp == null)
            {
                return;
            }

            if (e.Property == SourceProperty)
            {
                sp.SourceChanged();
            }
            else if (e.Property == DocumentProperty)
            {
                sp.DocumentChanged(e.OldValue as PdfDocument, e.NewValue as PdfDocument);
            }
            else if (e.Property == PageProperty || e.Property == DpiProperty || e.Property == FlagsProperty)
            {
                sp.GotoPage(sp.Page);
            }
            else if (e.Property == ZoomModeProperty || e.Property == ZoomProperty || e.Property == RotateProperty
                || e.Property == PagesDisplayModeProperty)
            {
                sp.OnPagesDisplayChanged();
            }
            else if (e.Property == ZoomMaxProperty)
            {
                if (sp.Zoom > sp.ZoomMax)
                {
                    sp.SetCurrentValue(ZoomProperty, sp.ZoomMax);
                }
            }
            else if (e.Property == ZoomMinProperty)
            {
                if(sp.Zoom < sp.ZoomMin)
                {
                    sp.SetCurrentValue(ZoomProperty, sp.ZoomMin);
                }
            }
            else if (e.Property == IsRightToLeftProperty)
            {
                sp.Panel.FlowDirection =  (bool)e.NewValue ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }
            else if (e.Property == RenderAllPagesProperty)
            {
                sp.RenderAll();
            }
            else if (e.Property == EnableKineticProperty)
            {
                sp.Cursor = sp.EnableKinetic ? Cursors.Hand : Cursors.Arrow;
            }
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
            oldDoc?.Dispose();

            Frames = null;
            _markers = null;
            Panel.Children.Clear();

            GC.Collect();

            PageCount = newDoc?.PageCount ?? 0;

            OnPagesDisplayChanged();

            SetCurrentValue(PageProperty, 0);
        }

        protected void OnPagesDisplayChanged()
        {
            if (IsDocumentLoaded)
            {
                Panel.Children.Clear();
                Frames = null;

                if (PagesDisplayMode == PdfViewerPagesDisplayMode.SinglePageMode)
                {
                    Frames = new Image[1];
                    Panel.Orientation = Orientation.Horizontal;
                }
                else if (PagesDisplayMode == PdfViewerPagesDisplayMode.BookMode)
                {
                    Frames = new Image[2];
                    Panel.Orientation = Orientation.Horizontal;
                }
                else if (PagesDisplayMode == PdfViewerPagesDisplayMode.ContinuousMode)
                {
                    // frames created at scrolling
                    Frames = new Image[PageCount];
                    Panel.Orientation = Orientation.Vertical;
                }

                for (var i = 0; i < Frames.Length; i++)
                {
                    Frames[i] ??= new Image { Margin = FrameSpace };

                    var pageSize = CalculatePageSize(i);
                    Frames[i].Width = pageSize.Width;
                    Frames[i].Height = pageSize.Height;

                    Panel.Children.Add(Frames[i]);
                }

                GC.Collect();
                GotoPage(Page);
            }
        }

        private void RenderAll()
        {
            var pageStep = PagesDisplayMode == PdfViewerPagesDisplayMode.BookMode ? 2 : 1;
            GotoPage(0);
            while (Page < PageCount - pageStep)
            {
                NextPage();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            MouseClick?.Invoke(this, EventArgs.Empty);
        }

        protected BitmapImage RenderPage(Image frame, int page, int width, int height)
        {
            if (frame == null) return null;
            var image = Document.Render(page, width, height, Dpi, Dpi, Rotate, Flags);
            BitmapImage bitmapImage;
            using (var memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // not a mistake - see below
                bitmapImage.EndInit();
                image.Dispose();
            }
            // Why BitmapCacheOption.OnLoad?
            // It seems counter intuitive, but this flag has two effects:
            // It enables caching if caching is possible, and it causes the load to happen at EndInit().
            // In our case caching is impossible, so all it does it cause the load to happen immediately.

            CurrentProcess?.Refresh();
            Dispatcher.Invoke(() =>
            {
                frame.Width = width;
                frame.Height = height;
                frame.Source = bitmapImage;
            });
            GC.Collect();
            return bitmapImage;
        }
        protected Size CalculatePageSize(int? page = null)
        {
            page ??= Page;

            var isReverse = (Rotate == PdfRotation.Rotate90 || Rotate == PdfRotation.Rotate270);
            var containerWidth = ActualWidth - Padding.Left - Padding.Right - FrameSpace.Left - FrameSpace.Right; // ViewportWidth
            var containerHeight = ActualHeight - Padding.Top - Padding.Bottom - FrameSpace.Top - FrameSpace.Bottom; // ViewportHeight

            if (IsDocumentLoaded && containerWidth > 0 && containerHeight > 0)
            {
                var currentPageSize = Document.GetPageSize(page.Value);

                var zoom = Zoom;
                if (ZoomMode == PdfViewerZoomMode.FitHeight)
                {
                    zoom = containerHeight / currentPageSize.Height;
                }
                if (ZoomMode == PdfViewerZoomMode.FitWidth)
                {
                    zoom = (containerWidth - ScrollWidth) / currentPageSize.Width;
                    if (PagesDisplayMode == PdfViewerPagesDisplayMode.BookMode)
                        zoom /= 2;
                }

                if (isReverse)
                    currentPageSize = new SizeF(currentPageSize.Height, currentPageSize.Width);

                return new Size((int)(currentPageSize.Width * zoom), (int)(currentPageSize.Height * zoom));
            }

            return new Size();
        }
        protected void ReleaseFrames(int keepFrom, int keepTo)
        {
            for (var f = 0; f < Frames?.Length; f++)
            {
                var frame = Frames[f];
                if ((f < keepFrom || f > keepTo) && frame.Source != null)
                {
                    frame.Source = null;
                }
            }
            GC.Collect();
        }

        private void ZoomIn()
        {
            SetCurrentValue(ZoomProperty, Math.Min(Zoom, ZoomMax));
        }

        private void ZoomOut()
        {
            SetCurrentValue(ZoomProperty, Math.Min(Zoom, ZoomMin));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            GotoPage(Page);
        }
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            SetMouseWheelDelta(e.Delta);
            
            if (IsDocumentLoaded)
            {
                if (MouseWheelMode == MouseWheelMode.Zoom)
                {
                    e.Handled = true;
                    if (e.Delta > 0)
                        ZoomIn();
                    else
                        ZoomOut();
                }
                else if (PagesDisplayMode != PdfViewerPagesDisplayMode.ContinuousMode)
                {
                    var pageStep = PagesDisplayMode == PdfViewerPagesDisplayMode.BookMode ? 2 : 1;

                    if (ViewportHeight > Frame1.ActualHeight)
                    {
                        if (e.Delta > 0) // prev page
                            PreviousPage();
                        else
                            NextPage();
                    }
                    else if (e.Delta < 0 && VerticalOffset >= ScrollableHeight && Page < PageCount - pageStep)
                    {
                        NextPage();
                        ScrollToVerticalOffset(0);
                    }
                    else if (e.Delta > 0 && VerticalOffset <= 0 && Page > 0)
                    {
                        PreviousPage();
                        ScrollToVerticalOffset(ScrollableHeight);
                    }
                }
            }
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                MouseWheelMode = MouseWheelMode.Zoom;

            switch (e.Key)
            {
                case Key.Up:
                    PerformScroll(ScrollAction.LineUp, Orientation.Vertical);
                    return;

                case Key.Down:
                    PerformScroll(ScrollAction.LineDown, Orientation.Vertical);
                    return;

                case Key.Left:
                    PerformScroll(ScrollAction.LineUp, Orientation.Horizontal);
                    return;

                case Key.Right:
                    PerformScroll(ScrollAction.LineDown, Orientation.Horizontal);
                    return;

                case Key.PageUp:
                    PerformScroll(ScrollAction.PageUp, Orientation.Vertical);
                    return;

                case Key.PageDown:
                    PerformScroll(ScrollAction.PageDown, Orientation.Vertical);
                    return;

                case Key.Home:
                    PerformScroll(ScrollAction.Home, Orientation.Vertical);
                    return;

                case Key.End:
                    PerformScroll(ScrollAction.End, Orientation.Vertical);
                    return;

                case Key.Add:
                case Key.OemPlus:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        ZoomIn();
                    return;

                case Key.Subtract:
                case Key.OemMinus:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        ZoomOut();
                    return;
            }
        }
        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control ||
                e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                MouseWheelMode = MouseWheelMode.Pan;
        }
        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            if (IsDocumentLoaded &&
                PagesDisplayMode == PdfViewerPagesDisplayMode.ContinuousMode &&
                Frames != null)
            {
                var startOffset = e.VerticalOffset;
                var height = e.ViewportHeight;
                var pageSize = CalculatePageSize(0);

                var startFrameIndex = (int)(startOffset / (pageSize.Height + FrameSpace.Top + FrameSpace.Bottom));
                var endFrameIndex = (int)((startOffset + height) / (pageSize.Height + FrameSpace.Top + FrameSpace.Bottom));

                var startPageIndex = Math.Min(Math.Max(startFrameIndex, 0), PageCount - 1);
                var endPageIndex = Math.Min(Math.Max(endFrameIndex, 0), PageCount - 1);

                Trace.WriteLine($"[{startFrameIndex},{startPageIndex}]");
                ReleaseFrames(startPageIndex, endPageIndex);

                for (var page = startPageIndex; page <= endPageIndex; page++)
                {
                    var frame = Frames[page];
                    if (frame.Source == null) // && frame.IsUserVisible())
                    {
                        RenderPage(frame, page, (int)frame.Width, (int)frame.Height);
                    }
                }

                SetCurrentValue(PageProperty, startPageIndex);
            }
        }

        public void PerformScroll(ScrollAction action, Orientation scrollBar)
        {
            if (scrollBar == Orientation.Vertical)
            {
                switch (action)
                {
                    case ScrollAction.LineUp:
                        if (VerticalOffset > SmallScrollChange)
                            ScrollToVerticalOffset(VerticalOffset - SmallScrollChange);
                        break;

                    case ScrollAction.LineDown:
                        if (VerticalOffset < ScrollableHeight - SmallScrollChange)
                            ScrollToVerticalOffset(VerticalOffset + SmallScrollChange);
                        break;

                    case ScrollAction.PageUp:
                        if (VerticalOffset > LargeScrollChange)
                            ScrollToVerticalOffset(VerticalOffset - LargeScrollChange);
                        break;

                    case ScrollAction.PageDown:
                        if (VerticalOffset < ScrollableHeight - LargeScrollChange)
                            ScrollToVerticalOffset(VerticalOffset + LargeScrollChange);
                        break;

                    case ScrollAction.Home:
                        ScrollToHome();
                        break;

                    case ScrollAction.End:
                        ScrollToEnd();
                        break;
                }
            }
            else // Horizontal
            {
                switch (action)
                {
                    case ScrollAction.LineUp:
                        if (HorizontalOffset > SmallScrollChange)
                            ScrollToVerticalOffset(HorizontalOffset - SmallScrollChange);
                        break;

                    case ScrollAction.LineDown:
                        if (HorizontalOffset < ScrollableHeight - SmallScrollChange)
                            ScrollToVerticalOffset(HorizontalOffset + SmallScrollChange);
                        break;

                    case ScrollAction.PageUp:
                        if (HorizontalOffset > LargeScrollChange)
                            ScrollToVerticalOffset(HorizontalOffset - LargeScrollChange);
                        break;

                    case ScrollAction.PageDown:
                        if (HorizontalOffset < ScrollableHeight - LargeScrollChange)
                            ScrollToVerticalOffset(HorizontalOffset + LargeScrollChange);
                        break;

                    case ScrollAction.Home:
                        ScrollToHome();
                        break;

                    case ScrollAction.End:
                        ScrollToEnd();
                        break;
                }
            }
        }
        protected void SetMouseWheelDelta(int delta)
        {
            MouseWheelUpdateTime = Environment.TickCount64;
            MouseWheelDelta = delta;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;

                UnLoad();
                GC.SuppressFinalize(this);
                GC.Collect();
            }
        }

        public void Dispose()
        {
            Dispose(!IsDisposed);
        }
    }
}
