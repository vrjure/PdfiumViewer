using PdfiumViewer.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PdfiumViewer
{
    public partial class PDFViewer
    {
        private static Brush _defaultMatchBrush = new SolidColorBrush(Colors.Yellow) { Opacity = 0.35 };
        private static Brush _defaultCurrentMatchBrush = new SolidColorBrush(Colors.Orange) { Opacity = 0.35 };

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

        public static readonly DependencyProperty MatchBrushProperty = DependencyProperty.Register(nameof(MatchBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(_defaultMatchBrush, PropertyChanged));
        /// <summary>
        /// Gets or sets the color of matched search terms.
        /// </summary>
        public Brush MatchBrush
        {
            get => (Brush)GetValue(MatchBrushProperty);
            set => SetValue(PageCountProperty, value);
        }

        public static readonly DependencyProperty MatchBorderBrushProperty = DependencyProperty.Register(nameof(MatchBorderBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(_defaultMatchBrush, PropertyChanged));
        /// <summary>
        /// Gets or sets the border color of matched search terms.
        /// </summary>
        public Brush MatchBorderBrush
        {
            get => (Brush)GetValue(MatchBorderBrushProperty);
            set => SetValue(MatchBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MatchBorderThicknessProperty = DependencyProperty.Register(nameof(MatchBorderThickness), typeof(double), typeof(PDFViewer), new PropertyMetadata(1d, PropertyChanged));
        public double MatchBorderThickness
        {
            get => (double)GetValue(MatchBorderThicknessProperty);
            set => SetValue(MatchBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty CurrentMatchBrushProperty = DependencyProperty.Register(nameof(CurrentMatchBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(_defaultCurrentMatchBrush, PropertyChanged));
        /// <summary>
        /// Gets or sets the color of the current match.
        /// </summary>
        public Brush CurrentMatchBrush
        {
            get => (Brush)GetValue(CurrentMatchBrushProperty);
            set => SetValue(CurrentMatchBrushProperty, value);
        }

        public static readonly DependencyProperty CurrentMatchBorderBrushProperty = DependencyProperty.Register(nameof(CurrentMatchBorderBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(_defaultCurrentMatchBrush, PropertyChanged));
        /// <summary>
        /// Gets or sets the border color of the current match.
        /// </summary>
        public Brush CurrentMatchBorderBrush
        {
            get => (Brush)GetValue(CurrentMatchBorderBrushProperty);
            set => SetValue(CurrentMatchBorderBrushProperty, value);
        }

        public static readonly DependencyProperty CurrentMatchBorderThicknessProperty = DependencyProperty.Register(nameof(CurrentMatchBorderThickness), typeof(double), typeof(PDFViewer), new PropertyMetadata(1d, PropertyChanged));
        /// <summary>
        /// Gets or sets the border width of the current match.
        /// </summary>
        public double CurrentMatchBorderThickness
        {
            get => (double)GetValue(CurrentMatchBorderThicknessProperty);
            set => SetValue(CurrentMatchBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty HighlightAllMatchesProperty = DependencyProperty.Register(nameof(HighlightAllMatches), typeof(bool), typeof(PDFViewer), new PropertyMetadata(false, PropertyChanged));
        public bool HighlightAllMatches
        {
            get => (bool)GetValue(HighlightAllMatchesProperty);
            set => SetValue(HighlightAllMatchesProperty, value);
        }

        private void OnMatchesChanged()
        {
            RenderMarkers();

            if (Markers.Count > 0)
            {
                SetCurrentValue(MatchIndexProperty, 0);
            }
        }

        private void RenderMarkers()
        {
            Markers ??= new PdfMarkerCollection();
            Markers?.Clear();

            var matches = Matches;
            if (matches == null || matches.Items == null || matches.Items.Count == 0 || Document == null)
            {
                Markers?.Clear();
            }
            else
            {
                if (HighlightAllMatches)
                {
                    var list = new List<IPdfMarker>();
                    for (int i = 0; i < matches.Items.Count; i++)
                    {
                        var item = matches.Items[i];
                        if (item.Page < RenderRange.RenderStartIndex || item.Page > RenderRange.RenderEndIndex)
                        {
                            continue;
                        }
                        list.Add(ToMarker(item, i, i == MatchIndex));
                    }

                    Markers.AddRange(list);
                }
                else if (MatchIndex >= 0 && MatchIndex < matches.Items.Count)
                {
                    var marker = ToMarker(matches.Items[MatchIndex], MatchIndex, true);
                    Markers.Add(marker);
                }
            }
        }

        private PdfMarker ToMarker(PdfMatch item, int matchIndex, bool current)
        {
            var bounds = Document.Pages[item.Page].GetTextBounds(item.TextSpan.Offset, item.TextSpan.Length);
            return new PdfMatchMarker(item.Page, matchIndex, bounds.ToArray(), current);
        }

        private void OnMatchIndexChanged(int newIndex, int oldIndex)
        {
            if (Markers == null)
            {
                return;
            }

            if (HighlightAllMatches)
            {
                for (int i = 0; i < Markers.Count; i++)
                {
                    var marker = Markers[i];
                    if (marker is not PdfMatchMarker)
                    {
                        continue;
                    }

                    var matchMarker = (PdfMatchMarker)marker;
                    if (matchMarker.MatchIndex == newIndex)
                    {
                        matchMarker.Current = true;
                        Markers[i] = marker;
                    }
                    else if (matchMarker.MatchIndex == oldIndex)
                    {
                        matchMarker.Current = false;
                        Markers[i] = marker;
                    }
                }
            }
            else if (Markers.Count > 0 && newIndex >= 0 && newIndex < Matches.Items.Count)
            {
                Markers[0] = ToMarker(Matches.Items[newIndex], newIndex, true);
            }

            if (Matches?.Items != null && newIndex >= 0 && newIndex < Matches.Items.Count)
            {
                SetCurrentValue(PageProperty, Matches.Items[newIndex].Page);
            }
        }
    }
}
