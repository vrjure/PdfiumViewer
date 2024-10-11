using PdfiumViewer.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PdfiumViewer
{
    public partial class PDFViewer
    {
        private Point _ignore = new Point(5,5);
        private Point _lastMouse = new Point(0, 0);
        private int _lastTextIndex = -1;

        private Dictionary<int, IPdfMarker> _selectionMarkers = new Dictionary<int, IPdfMarker>();
        private Stack<PdfSelection> _selectionChanges = new Stack<PdfSelection>();

        private static Brush _defaultSelectionBrush = new SolidColorBrush(Colors.Yellow) { Opacity = 0.35 };
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(_defaultSelectionBrush, PropertyChanged));
        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }

        public static readonly DependencyProperty SelectionBorderBrushProperty = DependencyProperty.Register(nameof(SelectionBorderBrush), typeof(Brush), typeof(PDFViewer), new PropertyMetadata(_defaultSelectionBrush, PropertyChanged));
        public Brush SelectionBorderBrush
        {
            get => (Brush)GetValue(SelectionBorderBrushProperty);
            set => SetValue(SelectionBorderBrushProperty, value);
        }

        public static readonly DependencyProperty SelectionBorderThicknessProperty = DependencyProperty.Register(nameof(SelectionBorderThickness), typeof(double), typeof(PDFViewer), new PropertyMetadata(1d, PropertyChanged));
        public double SelectionBorderThickness
        {
            get => (double)GetValue(SelectionBorderThicknessProperty);
            set => SetValue(SelectionBorderThicknessProperty, value);
        }

        private void Container_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Debug.WriteLine("mouse level");
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
       
            if(Document != null && e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Debug.WriteLine("CTRL + C");
                var txt = Document.GetSelectionText();
                if (!string.IsNullOrEmpty(txt))
                {
                    Clipboard.SetText(txt);
                }
            }
        }

        private void Container_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Debug.WriteLine("mouse up");
            }
        }

        private void Container_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) 
            {
                var container = sender as PDFViewerItemContainer;
                if (container == null || Document == null) return;

                OnSelection(e.GetPosition(container.OverlayLayer), container);
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                BeginSelection();
                Debug.WriteLine("left mouse down");
            }
        }

        private void Container_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var container = sender as PDFViewerItemContainer;
                if (container == null || Document == null) return;

                var mousePoint = e.GetPosition(container.OverlayLayer);
                OnSelection(mousePoint, container);
            }
        }

        private void OnSelection(Point mousePoint, PDFViewerItemContainer container)
        {
            if (Document == null || Math.Abs(mousePoint.X - _lastMouse.X) < _ignore.X && Math.Abs(mousePoint.Y - _lastMouse.Y) < _ignore.Y)
            {
                return;
            }

            _lastMouse = mousePoint;

            var pageIndex = ItemContainerGenerator.IndexFromContainer(container);
            if (pageIndex < 0)
            {
                return;
            }

            var page = Document.Pages[pageIndex];
            var pdfPoint = page.DeviceToPage(new Point(mousePoint.X / Zoom, mousePoint.Y / Zoom));
            var index = page.GetCharIndexAtPos(pdfPoint.X, pdfPoint.Y, _ignore.X, _ignore.Y);

            if (index < 0 || index == _lastTextIndex) return;

            _lastTextIndex = index;

            var selections = Document.Selections?._selections;
            if (selections == null)
            {
                return;
            }

            if (!selections.TryPeek(out PdfSelection selection))
            {
                selection = new PdfSelection(pageIndex, index);
                selections.Push(selection);
                _selectionChanges.Push(selection);
            }
            else if (pageIndex > selection.PageIndex)
            {
                var first = selections.Last();

                if (selection.PageIndex < first.PageIndex && pageIndex >= first.PageIndex)// like: page index = 3,2,3
                {
                    while (selections.Count > 1)
                    {
                        var temp = selections.Pop();
                        RemoveSelection(temp.PageIndex);
                    }
                    selection = selections.Peek();
                }

                if (pageIndex > selection.PageIndex)
                {
                    selection.EndSelectionIndex = Document.Pages[selection.PageIndex].GetCountChars() - 1;
                    _selectionChanges.Push(selection);

                    for (int i = selection.PageIndex + 1; i <= pageIndex; i++)
                    {
                        if (i != pageIndex)
                        {
                            var chars = Document.Pages[i].GetCountChars();
                            if (chars > 0)
                            {
                                selection = new PdfSelection(i, 0, chars - 1, true);
                                selections.Push(selection);
                                _selectionChanges.Push(selection);
                            }
                            else
                            {
                                selection = new PdfSelection(i, 0);
                                selections.Push(selection);
                                _selectionChanges.Push(selection);
                            }
                        }
                        else
                        {
                            selection = new PdfSelection(i, 0, index);
                            selections.Push(selection);
                            _selectionChanges.Push(selection);
                        }
                    }
                }
                else
                {
                    selection.EndSelectionIndex = index;
                    _selectionChanges.Push(selection);
                }
            }
            else if (pageIndex < selection.PageIndex)
            {
                var first = selections.Last();

                if (selection.PageIndex > first.PageIndex && pageIndex <= first.PageIndex) //like: page index = 3,4,3
                {
                    while (selections.Count > 1)
                    {
                        var temp = selections.Pop();
                        RemoveSelection(temp.PageIndex);
                    }
                    selection = selections.Peek();
                }

                if (pageIndex < selection.PageIndex)
                {
                    selection.EndSelectionIndex = 0;
                    _selectionChanges.Push(selection);

                    var endIndex = selection.PageIndex;
                    for (int i = pageIndex; i < endIndex; i++)
                    {
                        if (i == pageIndex)
                        {
                            var chars = Document.Pages[i].GetCountChars();
                            selection = new PdfSelection(i, index, chars - 1, true);
                            selections.Push(selection);
                            _selectionChanges.Push(selection);
                        }
                        else
                        {
                            var chars = Document.Pages[i].GetCountChars();
                            selection = new PdfSelection(i, 0, chars - 1, true);
                            selections.Push(selection);
                            _selectionChanges.Push(selection);
                        }
                    }
                }
                else
                {
                    selection.EndSelectionIndex = index;
                    _selectionChanges.Push(selection);
                }
            }
            else
            {
                selection.SetEndIndex(index);
                _selectionChanges.Push(selection);
            }

            OnSelectionsChanged();

#if DEBUG
            Debug.WriteLine("current total selections start");
            foreach (var item in selections)
            {
                Debug.WriteLine($"{item}");
            }
            Debug.WriteLine("current total selections end");
#endif
        }

        private void OnSelectionsChanged()
        {
            while (_selectionChanges.TryPop(out var item))
            {
                Debug.WriteLine(item.ToString());
                var rects = Document.Pages[item.PageIndex].GetTextBounds(item.StartIndex, Math.Abs(item.EndIndex - item.StartIndex) + 1);

                if (!_selectionMarkers.TryGetValue(item.PageIndex, out IPdfMarker marker))
                {
                    marker = new PdfMarker(item.PageIndex, rects);
                    _selectionMarkers.Add(item.PageIndex, marker);
                }
                else
                {
                    marker.Bounds = rects.ToArray();
                }

                var currentContainer = ItemContainerGenerator.ContainerFromIndex(item.PageIndex) as PDFViewerItemContainer;
                currentContainer.AddOrUpdateMarker(marker, Zoom, SelectionBrush, SelectionBorderBrush, SelectionBorderThickness);
            }
        }

        private void RefreshSelection(bool force = false)
        {
            if (RenderRange == RenderRange.Invalid)
            {
                return;
            }

            for (int i = RenderRange.RenderStartIndex; i <= RenderRange.RenderEndIndex; i++)
            {
                if (_selectionMarkers.TryGetValue(i, out IPdfMarker marker))
                {
                    if (force)
                    {
                        marker.IsBoundsChanged = true;
                    }
                    var currentContainer = ItemContainerGenerator.ContainerFromIndex(i) as PDFViewerItemContainer;
                    currentContainer.AddOrUpdateMarker(marker, Zoom, SelectionBrush, SelectionBorderBrush, SelectionBorderThickness);
                }
            }
        }

        private void BeginSelection()
        {
            EndSelection();
            ClearSelections();
        }

        private void EndSelection()
        {
            _lastTextIndex = -1;
            Document?.Selections?._selections?.Clear();
        }

        private void ClearSelections()
        {
            foreach (var marker in _selectionMarkers)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(marker.Key) as PDFViewerItemContainer;
                container.RemoveMarker(marker.Value);
            }

            _selectionMarkers.Clear();
        }

        private void RemoveSelection(int pageIndex)
        {
            if(_selectionMarkers.TryGetValue(pageIndex, out IPdfMarker marker))
            {
                var container = ItemContainerGenerator.ContainerFromIndex(pageIndex) as PDFViewerItemContainer;
                container.RemoveMarker(marker);
                _selectionMarkers.Remove(pageIndex);
            }
        }
    }
}
