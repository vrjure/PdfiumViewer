using PdfiumViewer.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PdfiumViewer
{
    public partial class PDFViewer
    {
        public static readonly DependencyProperty MatchesProperty = DependencyProperty.Register(nameof(Matches), typeof(PdfMatches), typeof(PDFViewer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChanged));
        public PdfMatches Matches
        {
            get => (PdfMatches)GetValue(MatchesProperty);
            set => SetValue(MatchesProperty, value);
        }

        public static readonly DependencyProperty MatchIndexProperty = DependencyProperty.Register(nameof(MatchIndex), typeof(int), typeof(PDFViewer), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChanged, MatchIndexCoerceValueChanged));
        public int MatchIndex
        {
            get => (int)GetValue(MatchIndexProperty);
            set => SetValue(MatchIndexProperty, value);
        }

        public static readonly DependencyProperty MatchBrushProperty = DependencyProperty.Register(nameof(MatchBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(Brushes.Yellow));
        /// <summary>
        /// Gets or sets the color of matched search terms.
        /// </summary>
        public Brush MatchBrush
        {
            get => (Brush)GetValue(MatchBrushProperty);
            set => SetValue(PageCountProperty, value);
        }

        public static readonly DependencyProperty MatchBorderBrushProperty = DependencyProperty.Register(nameof(MatchBorderBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(Brushes.Yellow));
        /// <summary>
        /// Gets or sets the border color of matched search terms.
        /// </summary>
        public Brush MatchBorderBrush
        {
            get => (Brush)GetValue(MatchBorderBrushProperty);
            set => SetValue(MatchBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MatchBorderThicknessProperty = DependencyProperty.Register(nameof(MatchBorderThickness), typeof(double), typeof(PDFViewer), new PropertyMetadata(1d));
        public double MatchBorderThickness
        {
            get => (double)GetValue(MatchBorderThicknessProperty);
            set => SetValue(MatchBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty CurrentMatchBrushProperty = DependencyProperty.Register(nameof(CurrentMatchBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(Brushes.Orange));
        /// <summary>
        /// Gets or sets the color of the current match.
        /// </summary>
        public Brush CurrentMatchBrush
        {
            get => (Brush)GetValue(CurrentMatchBrushProperty);
            set => SetValue(CurrentMatchBrushProperty, value);
        }

        public static readonly DependencyProperty CurrentMatchBorderBrushProperty = DependencyProperty.Register(nameof(CurrentMatchBorderBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(Brushes.Orange));
        /// <summary>
        /// Gets or sets the border color of the current match.
        /// </summary>
        public Brush CurrentMatchBorderBrush
        {
            get => (Brush)GetValue(CurrentMatchBorderBrushProperty);
            set => SetValue(CurrentMatchBorderBrushProperty, value);
        }

        public static readonly DependencyProperty CurrentMatchBorderThicknessProperty = DependencyProperty.Register(nameof(CurrentMatchBorderThickness), typeof(double), typeof(PDFViewer), new PropertyMetadata(1d));
        /// <summary>
        /// Gets or sets the border width of the current match.
        /// </summary>
        public double CurrentMatchBorderThickness
        {
            get => (double)GetValue(CurrentMatchBorderThicknessProperty);
            set => SetValue(CurrentMatchBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty HighlightAllMatchesProperty = DependencyProperty.Register(nameof(HighlightAllMatches), typeof(bool), typeof(PDFViewer), new PropertyMetadata(true));
        public bool HighlightAllMatches
        {
            get => (bool)GetValue(HighlightAllMatchesProperty);
            set => SetValue(HighlightAllMatchesProperty, value);
        }

        private void OnMatchesChanged(PdfMatches matches)
        {
            Markers ??= new PdfMarkerCollection();
            Markers?.Clear();

            if (matches == null || matches.Items == null || matches.Items.Count == 0 || Document == null)
            {
                Markers?.Clear();
            }
            else
            {

                if (HighlightAllMatches)
                {
                    var list = new List<PdfMarker>();
                    for (int i = 0; i < matches.Items.Count; i++)
                    {
                        var item = matches.Items[i];
                        list.Add(ToMarker(item, i == MatchIndex));
                    }

                    Markers.AddRange(list);
                }
                else if (MatchIndex >= 0 && MatchIndex < matches.Items.Count)
                {
                    var marker = ToMarker(matches.Items[MatchIndex], true);
                    Markers.Add(marker);
                }
            }

            if (Markers.Count > 0)
            {
                SetCurrentValue(MatchIndexProperty, 0);
            }
        }

        private PdfMarker ToMarker(PdfMatch item, bool current)
        {
            var textBounds = Document.GetTextBounds(item.TextSpan);

            var rects = new Rect[textBounds.Count];

            for (int i = 0; i < textBounds.Count; i++)
            {
                var bound = textBounds[i].Bounds;
                rects[i] = new Rect(bound.Left, bound.Top, bound.Width, bound.Height);
            }

            return new PdfMarker(item.Page, rects, current);
        }

        private void OnMatchIndexChanged(int newIndex, int oldIndex)
        {
            if (Matches?.Items == null)
            {
                return;
            }

            if (HighlightAllMatches)
            {
                if (oldIndex >= 0 && oldIndex < Matches.Items.Count)
                {
                    var marker = Markers[oldIndex];
                    marker.Current = false;
                    Markers[oldIndex] = marker;
                }

                if (newIndex >= 0 && newIndex < Matches.Items.Count)
                {
                    var marker = Markers[newIndex];
                    marker.Current = true;
                    Markers[newIndex] = marker;
                    SetCurrentValue(PageProperty, marker.Page);
                }
            }
            else
            {
                if (newIndex >= 0 && newIndex < Matches.Items.Count)
                {
                    Markers[0] = ToMarker(Matches.Items[newIndex], true);
                    SetCurrentValue(PageProperty, Markers[0].Page);
                }
            }

        }
    }
}
