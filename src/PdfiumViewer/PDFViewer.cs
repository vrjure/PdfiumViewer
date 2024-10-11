using System;
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

        public static readonly DependencyProperty RotateProperty = DependencyProperty.Register(nameof(Rotate), typeof(PdfRotation), typeof(PDFViewer), new FrameworkPropertyMetadata(PdfRotation.Rotate0, PropertyChanged));
        public PdfRotation Rotate
        {
            get => (PdfRotation)GetValue(RotateProperty);
            set => SetValue(RotateProperty, value);
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

        private static readonly DependencyPropertyKey RenderRangePropertyKey = DependencyProperty.RegisterReadOnly(nameof(RenderRange), typeof(RenderRange), typeof(PDFViewer), new PropertyMetadata(RenderRange.Invalid, PropertyChanged));
        public static readonly DependencyProperty RenderRangeProperty = RenderRangePropertyKey.DependencyProperty;
        public RenderRange RenderRange
        {
            get => (RenderRange)GetValue(RenderRangePropertyKey.DependencyProperty);
            private set => SetValue(RenderRangePropertyKey, value);
        }

        private const double DefaultZoomMin = 0.1;
        private const double DefaultZoomMax = 5;

        static PDFViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PDFViewer), new FrameworkPropertyMetadata(typeof(PDFViewer)));
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


        private int _scrollPage;

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
            else if (e.Property == ZoomProperty)
            {
                v.PageSizeRefresh();
            }
            else if (e.Property == ZoomMinProperty || e.Property == ZoomMaxProperty)
            {
                if (v.Zoom < v.ZoomMin)
                {
                    v.Zoom = v.ZoomMin;
                }
                else if (v.Zoom > v.ZoomMax)
                {
                    v.Zoom = v.ZoomMax;
                }
            }
            else if (e.Property == DpiProperty)
            {
                v.Render(true);
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
                    v.Render();
                    v.RenderMarkers();
                    v.RefreshSelection();
                }
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

            PageSizeRefresh();
        }

        private void PageSizeRefresh()
        {
            SetCurrentValue(PageCountProperty, GetPageCount());
            if (PageCount <= 0)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                for (var i = 0; i < PageCount; i++)
                {
                    var size = Document.Pages[i].Size;
                    var width = size.Width * Zoom;
                    var height = size.Height * Zoom;
                    if (i < Items.Count && Items[i] is Image img)
                    {
                        img.Width = width;
                        img.Height = height;
                    }
                    else
                    {
                        var image = new Image() { Width = width, Height = height };
                        Items.Add(image);
                    }
                }

                Render();
                RenderMarkers();
                RefreshSelection(true);
            }));
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
            var offset = 0d;
            for (int i = 0; i < Page; i++)
            {
                offset += (ContainerFromElement(Items[i] as Image) as FrameworkElement).ActualHeight;
            }

            Scroll.ScrollToVerticalOffset(offset);
        }

        private void _scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Document == null || Items == null || Items.Count == 0)
            {
                return;
            }

            var firstContainer = ContainerFromElement(Items[0] as Image) as FrameworkElement;
            var pageSize = firstContainer.RenderSize;
            var viewPort_h = e.ViewportHeight;

            var offset_v = e.VerticalOffset;

            var renderStartIndex = Math.Max((int)((offset_v - viewPort_h) / pageSize.Height), 0);
            var renderEndIndex = Math.Min((int)((offset_v + viewPort_h) / pageSize.Height), PageCount - 1);

            RenderRange = new RenderRange(Math.Max(renderStartIndex, 0), Math.Min(renderEndIndex, Items.Count - 1));
            Debug.WriteLine($"[{RenderRange.RenderStartIndex},{RenderRange.RenderEndIndex}],[{e.VerticalOffset}, {e.ExtentHeight}]");

            _scrollPage = (int)Math.Round(e.VerticalOffset / (ContainerFromElement(Items[0] as Image) as FrameworkElement).ActualHeight);
            if (_scrollPage < Items.Count)
            {
                SetCurrentValue(PageProperty, _scrollPage);
            }
        }

        private void Render(bool force = false)
        {
            if (this.RenderRange != RenderRange.Invalid && RenderRange.RenderStartIndex <= RenderRange.RenderEndIndex && RenderRange.RenderStartIndex >= 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Render(RenderRange.RenderStartIndex, Math.Min(RenderRange.RenderEndIndex, Items.Count - 1), force);
                }));
            }
        }

        private void Render(int startIndex, int endIndex, bool force = false)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                var frame = Items[i] as Image;
                if (force || frame.Source == null || frame.Width != (int)frame.Source.Width || frame.Height != (int)frame.Source.Height)
                {
                    RenderPage(frame, i, (int)frame.Width, (int)frame.Height);
                }
            }

            for (int i = startIndex - 1; i >= 0; i--)
            {
                var frame = Items[i] as Image;
                if (frame.Source != null)
                {
                    frame.Source = null;
                }
            }

            var count = Items.Count;
            for (int i = endIndex + 1; i < count; i++)
            {
                var frame = Items[i] as Image;
                if (frame.Source != null)
                {
                    frame.Source = null;
                }
            }
        }

        private void RenderPage(Image frame, int page, int width, int height)
        {
            if (frame == null || Document == null) return;

            var image = Document.Pages[page].Render(width, height, Dpi, Dpi, Rotate, Flags);
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

            frame.Width = width;
            frame.Height = height;
            frame.Source = bitmapImage;

            Document.Pages[page].Close();
        }
    }
}
