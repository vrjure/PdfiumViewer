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

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(PdfDocument), typeof(PDFViewer), new FrameworkPropertyMetadata(null, PropertyChanged));
        public PdfDocument Document
        {
            get => (PdfDocument)GetValue(DocumentProperty);
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

        private static readonly DependencyPropertyKey RenderStartIndexPropertyKey = DependencyProperty.RegisterReadOnly(nameof(RenderStartIndex), typeof(int), typeof(PDFViewer), new PropertyMetadata(0, PropertyChanged));
        public static readonly DependencyProperty RenderStartIndexProperty = RenderStartIndexPropertyKey.DependencyProperty;
        public int RenderStartIndex
        {
            get => (int)GetValue(RenderStartIndexPropertyKey.DependencyProperty);
            private set => SetValue(RenderStartIndexPropertyKey, value);
        }

        private static readonly DependencyPropertyKey RenderEndIndexPropertyKey = DependencyProperty.RegisterReadOnly(nameof(RenderEndIndex), typeof(int), typeof(PDFViewer), new PropertyMetadata(0, PropertyChanged));
        public static readonly DependencyProperty RenderEndIndexProperty = RenderEndIndexPropertyKey.DependencyProperty;
        public int RenderEndIndex
        {
            get => (int)GetValue(RenderEndIndexPropertyKey.DependencyProperty);
            private set => SetValue(RenderEndIndexPropertyKey, value);
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
            return new PDFViewerItemContainer();
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
            else if (e.Property == HighlightAllMatchesProperty || e.Property == MatchesProperty)
            {
                v.OnMatchesChanged();
            }
            else if (e.Property == MatchIndexProperty)
            {
                v.OnMatchIndexChanged((int)e.NewValue, (int)e.OldValue);
            }
            else if (e.Property == RenderStartIndexProperty || e.Property == RenderEndIndexProperty)
            {
                v.RenderMarkers();
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

            for (var i = 0; i < PageCount; i++)
            {
                var size = Document.GetPageSize(i);
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

            RenderStartIndex = (int)(offset_v / pageSize.Height);
            RenderEndIndex = (int)((offset_v + viewPort_h) / pageSize.Height);

            Debug.WriteLine($"[{RenderStartIndex},{RenderEndIndex}],[{e.VerticalChange}, {e.VerticalOffset}]");

            Render();
        }

        private void Render(bool force = false)
        {
            if (RenderStartIndex <= RenderEndIndex && RenderStartIndex >= 0)
            {
                Render(RenderStartIndex, Math.Min(RenderEndIndex, Items.Count - 1), force);
            }

            _scrollPage = (RenderEndIndex + RenderStartIndex) / 2;
            if (_scrollPage < Items.Count)
            {
                SetCurrentValue(PageProperty, _scrollPage);
            }
        }

        private void Render(int startIndex, int endIndex, bool force = false)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var frame = Items[i] as Image;

                if (i < startIndex || i > endIndex)
                {
                    frame.Source = null;
                }
                else if (force || frame.Source == null || frame.Width != (int)frame.Source.Width || frame.Height != (int)frame.Source.Height)
                {
                    RenderPage(frame, i, (int)frame.Width, (int)frame.Height);
                }
            }
        }

        private void RenderPage(Image frame, int page, int width, int height)
        {
            if (frame == null) return;
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

            frame.Width = width;
            frame.Height = height;
            frame.Source = bitmapImage;
        }
    }
}
