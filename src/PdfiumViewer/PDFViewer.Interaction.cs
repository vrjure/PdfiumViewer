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

namespace PdfiumViewer
{
    public partial class PDFViewer
    {
        private bool _isMouseDown = false;
        private Point _ignore = new Point(5,5);
        private Point _lastMouse = new Point(0, 0);
        private int _lastTextIndex = -1;

        private Dictionary<int, IPdfMarker> _selectionMarkers = new Dictionary<int, IPdfMarker>();

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
                OnSelectionsChanged();
            }
        }

        private void OnSelection(Point mousePoint, PDFViewerItemContainer container)
        {
            if (Math.Abs(mousePoint.X - _lastMouse.X) < _ignore.X && Math.Abs(mousePoint.Y - _lastMouse.Y) < _ignore.Y)
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
            var pdfPoint = page.PointToPdf(new Point(mousePoint.X / Zoom, mousePoint.Y / Zoom));
            var index = page.GetCharIndexAtPos(pdfPoint.X, pdfPoint.Y, _ignore.X, _ignore.Y);

            if (index < 0 || index == _lastTextIndex) return;

            _lastTextIndex = index;

            var _selections = Document._selections;
            if (!_selections.TryPeek(out PdfSelection selection))
            {
                selection = new PdfSelection(pageIndex, index);
                _selections.Push(selection);
            }
            else if (pageIndex > selection.PageIndex)
            {
                selection.EndSelectionIndex = Document.Pages[selection.PageIndex].GetCountChars() - 1;
                for (int i = selection.PageIndex + 1; i <= pageIndex; i++)
                {
                    if (i != pageIndex)
                    {
                        var chars = Document.Pages[i].GetCountChars();
                        if (chars > 0)
                        {
                            selection = new PdfSelection(i, 0, chars - 1, true);
                            _selections.Push(selection);
                        }
                        else
                        {
                            selection = new PdfSelection(i, 0);
                            _selections.Push(selection);
                        }

                    }
                    else
                    {
                        selection = new PdfSelection(i, 0, index);
                        _selections.Push(selection);
                    }
                }
            }
            else if (pageIndex < selection.PageIndex)
            {
                var first = _selections.First();
                if (pageIndex > first.PageIndex)
                {
                    for (int i = pageIndex + 1; i < selection.PageIndex; i++)
                    {
                        _selections.TryPop(out var _);
                        RemoveMarker(i);
                    }

                    if (_selections.TryPeek(out selection))
                    {
                        selection.SetEndIndex(index);
                    }
                }
                else if (pageIndex < selection.PageIndex)
                {
                    selection.EndSelectionIndex = 0;
                    for (int i = pageIndex; i < selection.PageIndex; i++)
                    {
                        if (i == pageIndex)
                        {
                            var chars = Document.Pages[i].GetCountChars();
                            selection = new PdfSelection(i, index, chars - 1, true);
                            _selections.Push(selection);
                        }
                        else
                        {
                            var chars = Document.Pages[i].GetCountChars();
                            selection = new PdfSelection(i, 0, chars - 1, true);
                            _selections.Push(selection);
                        }
                    }
                }
            }
            else
            {
                selection.SetEndIndex(index);
            }
        }

        private void OnSelectionsChanged()
        {
            var selections = Document?._selections;
            if (selections == null || selections.Count == 0) return;

            foreach (var item in selections)
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
                currentContainer.AddOrUpdateMarker(marker, Zoom, MatchBrush, MatchBorderBrush, MatchBorderThickness);
            }
        }

        private void BeginSelection()
        {
            EndSelection();
            ClearMarker();
        }

        private void EndSelection()
        {
            _lastTextIndex = -1;
            Document?._selections?.Clear();
        }

        private void ClearMarker()
        {
            foreach (var marker in _selectionMarkers)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(marker.Key) as PDFViewerItemContainer;
                container.RemoveMarker(marker.Value);
            }

            _selectionMarkers.Clear();
        }

        private void RemoveMarker(int pageIndex)
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
