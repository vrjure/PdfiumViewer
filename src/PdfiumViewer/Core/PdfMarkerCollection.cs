using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PdfiumViewer.Core
{
    internal class PdfMarkerCollection : Collection<IPdfMarker>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void ClearItems()
        {
            base.ClearItems();

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void InsertItem(int index, IPdfMarker item)
        {
            base.InsertItem(index, item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this[index], index));
        }

        protected override void SetItem(int index, IPdfMarker item)
        {
            base.SetItem(index, item);

           CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, index));
        }
    }
}
