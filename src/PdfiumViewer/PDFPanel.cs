using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PdfiumViewer
{
    internal class PDFPanel : Panel
    {
        static PDFPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PDFPanel), new FrameworkPropertyMetadata(typeof(PDFPanel)));
        }

        public static readonly DependencyProperty PageModeProperty = DependencyProperty.Register(nameof(PageMode), typeof(PdfPageMode), typeof(PDFPanel), new FrameworkPropertyMetadata(PdfPageMode.Continuous, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public PdfPageMode PageMode
        {
            get => (PdfPageMode)GetValue(PageModeProperty);
            set => SetValue(PageModeProperty, value);
        }

        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register(nameof(Rotation), typeof(PdfRotation), typeof(PDFPanel), new FrameworkPropertyMetadata(PdfRotation.Rotate0, FrameworkPropertyMetadataOptions.AffectsArrange, PropertyChanged));
        public PdfRotation Rotation
        {
            get => (PdfRotation)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as PDFPanel;
            if (panel == null) return;

            if (e.Property == RotationProperty)
            {
                panel.Rotate();
            }
        }

        private void Rotate()
        {
            if (Children.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var w = availableSize.Width == double.PositiveInfinity ? 0 : availableSize.Width;
            var h = availableSize.Height == double.PositiveInfinity ? 0 : availableSize.Height;
            if (Children.Count == 0)
            {
                return new Size(w, h);
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.Measure(availableSize);
            }

            var first = Children[0];
            
            var finalSize = first.DesiredSize;
            switch (PageMode)
            {
                case PdfPageMode.Continuous:
                    finalSize.Height = first.DesiredSize.Height * Children.Count;
                    break;
                case PdfPageMode.Double:
                    finalSize.Width = first.DesiredSize.Width * 2;
                    finalSize.Height = first.DesiredSize.Height * (int)Math.Ceiling(Children.Count / 2d);
                    break;
            }

            return finalSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0) return finalSize;

            var pageMode = PageMode;

            var offset_y = 0d;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i] as Control;

                var itemSize = child.DesiredSize;

                var offset_x = (finalSize.Width - itemSize.Width) / 2;

                if (pageMode == PdfPageMode.Double)
                {
                    var offset_x_1 = (finalSize.Width - itemSize.Width * 2) / 2;
                    var offset_x_2 = offset_x_1 + itemSize.Width;

                    if (i % 2 == 0)
                    {
                        offset_x = offset_x_1;
                    }
                    else
                    {
                        offset_x = offset_x_2;
                    }
                }

                child.Arrange(new Rect(offset_x, offset_y, itemSize.Width, itemSize.Height));

                offset_y = pageMode switch
                {
                    PdfPageMode.Continuous => offset_y + itemSize.Height,
                    PdfPageMode.Double => offset_y + itemSize.Height * (i % 2 == 0 ? 0 : 1),
                    _ => 0
                };

                var trans = child.RenderTransform as RotateTransform;
                if (trans == null)
                {
                    trans = new RotateTransform();
                    child.RenderTransformOrigin = new Point(0.5, 0.5);
                    child.LayoutTransform = trans;
                }

                trans.Angle = Rotation switch
                {
                    PdfRotation.Rotate90 => 90,
                    PdfRotation.Rotate180 => 180,
                    PdfRotation.Rotate270 => 270,
                    _ => 0
                };
            }
            return finalSize;
        }


    }
}
