using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public partial class MappingCollection<TSourceItem, TSelectedItem> :
    IList<MappingItem<TSelectedItem>>, ICollection<MappingItem<TSelectedItem>>, IList, ICollection, IReadOnlyList<MappingItem<TSelectedItem>>, IReadOnlyCollection<MappingItem<TSelectedItem>>,
    IEnumerable<MappingItem<TSelectedItem>>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    where TSourceItem : DependencyObject where TSelectedItem : DependencyObject
{
    ObservableCollection<TSourceItem> _SourceList;
    ObservableCollection<TSelectedItem> _SelectedList;
    ObservableCollection<MappingItem<TSelectedItem>> _OverridedSelectedList;

    public MappingCollection(
        ObservableCollection<TSourceItem> SourceList,
        ObservableCollection<TSelectedItem> SelectedList)
    {
        _SourceList = SourceList;
        _SelectedList = SelectedList;
        _SourceList.CollectionChanged += SourceList_CollectionChanged;
        _SelectedList.CollectionChanged += SelectedList_CollectionChanged;
    }

    private void SourceList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        //TSelectedItem[] selectedItems;
        //int i;
        //int startingIndex = 0;
        //int endingIndex;
        //TSourceItem currentItem;
        //switch (e.Action)
        //{
        //    case NotifyCollectionChangedAction.Add: //IList NewItems
        //    case NotifyCollectionChangedAction.Remove: //IList OldItems
        //    case NotifyCollectionChangedAction.Replace: //If NewItems and OldItems
        //        if (e.OldItems != null)
        //        {
        //            foreach (TSelectedItem oldselecteditem in e.OldItems)
        //            {
        //                currentItem = oldselecteditem.GetValue(_SourceItemRefranceProperty) as TSourceItem;
        //                if (currentItem != null)
        //                    currentItem.ClearValue(_SelectedItemProperty);
        //            }
        //        }
        //        if (e.NewItems != null)
        //        {
        //            foreach (TSelectedItem newselecteditem in e.OldItems)
        //            {
        //                currentItem = newselecteditem.GetValue(_SourceItemRefranceProperty) as TSourceItem;
        //                if (currentItem != null)
        //                    currentItem.SetValue(_SelectedItemProperty, newselecteditem);
        //            }
        //        }
        //        break;
        //    case NotifyCollectionChangedAction.Move:
        //        break; //Do Nothing
        //    case NotifyCollectionChangedAction.Reset: //No Data Provided
        //        this.Clear();
        //        selectedItems = _SelectedList.ToArray();
        //        endingIndex = selectedItems.Length - 1;
        //        using (IEnumerator<TSourceItem> enumerator = _SourceList.GetEnumerator())
        //        {
        //            while (enumerator.MoveNext())
        //            {
        //                //Find in cashe list
        //                currentItem = enumerator.Current;
        //                i = startingIndex;
        //                while (i <= endingIndex)
        //                {
        //                    if (selectedItems[i].GetValue(_SourceItemRefranceProperty) == currentItem)
        //                    {
        //                        if (2 * i < startingIndex + endingIndex)
        //                        {
        //                            selectedItems[i] = selectedItems[startingIndex];
        //                            startingIndex++;
        //                        }
        //                        else
        //                        {
        //                            selectedItems[i] = selectedItems[endingIndex];
        //                            endingIndex--;
        //                        }
        //                        break;
        //                    }
        //                    i++;
        //                }
        //                if (i <= endingIndex)
        //                    currentItem.SetValue(_SelectedItemProperty, selectedItems[i]);
        //                else
        //                    currentItem.ClearValue(_SelectedItemProperty);
        //            }
        //        }
        //        break;
        //    default:
        //        break;
        //}

    }

    private void SelectedList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
    {
        add
        {
            _OverridedSelectedList.CollectionChanged += value;
        }

        remove
        {
            _OverridedSelectedList.CollectionChanged -= value;
        }
    }
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            ((INotifyPropertyChanged)_OverridedSelectedList).PropertyChanged += value;
        }

        remove
        {
            ((INotifyPropertyChanged)_OverridedSelectedList).PropertyChanged -= value;
        }
    }

    public int Count
    {
        get
        {
            return _SourceList.Count;
        }
    }
    public bool IsReadOnly
    {
        get
        {
            return ((IList<MappingItem<TSelectedItem>>)_SourceList).IsReadOnly;
        }
    }
    bool IList.IsReadOnly
    {
        get
        {
            return this.IsReadOnly;
        }
    }
    bool IList.IsFixedSize
    {
        get
        {
            return ((IList)_SourceList).IsFixedSize;
        }
    }
    int ICollection.Count
    {
        get
        {
            return this.Count;
        }
    }
    object ICollection.SyncRoot
    {
        get
        {
            return ((ICollection)_SelectedList).SyncRoot;
        }
    }
    bool ICollection.IsSynchronized
    {
        get
        {
            return ((ICollection)_SelectedList).IsSynchronized;
        }
    }
    object IList.this[int index]
    {
        get
        {
            return this[index];
        }
        set
        {
            this[index] = (MappingItem<TSelectedItem>)value;
        }
    }
    public MappingItem<TSelectedItem> this[int index]
    {
        get
        {
            return _OverridedSelectedList[index];
        }

        set
        {
            _OverridedSelectedList[index] = value;
        }
    }
    public int IndexOf(MappingItem<TSelectedItem> item)
    {
        return ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).IndexOf(item);
    }
    public void Insert(int index, MappingItem<TSelectedItem> item)
    {
        ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).Insert(index, item);
    }
    public void RemoveAt(int index)
    {
        ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).RemoveAt(index);
    }
    public void Add(MappingItem<TSelectedItem> item)
    {
        ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).Add(item);
    }
    public void Clear()
    {
        ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).Clear();
    }
    public bool Contains(MappingItem<TSelectedItem> item)
    {
        return ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).Contains(item);
    }
    public void CopyTo(MappingItem<TSelectedItem>[] array, int arrayIndex)
    {
        ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).CopyTo(array, arrayIndex);
    }
    public bool Remove(MappingItem<TSelectedItem> item)
    {
        return ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).Remove(item);
    }
    public IEnumerator<MappingItem<TSelectedItem>> GetEnumerator()
    {
        return ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IList<MappingItem<TSelectedItem>>)_OverridedSelectedList).GetEnumerator();
    }
    int IList.Add(object value)
    {
        this.Add((MappingItem<TSelectedItem>)value);
        return this.Count - 1;
    }
    bool IList.Contains(object value)
    {
        return this.Contains(value);
    }
    void IList.Clear()
    {
        this.Clear();
    }
    int IList.IndexOf(object value)
    {
        return this.IndexOf((MappingItem<TSelectedItem>)value);
    }
    void IList.Insert(int index, object value)
    {
        this.Insert(index, (MappingItem<TSelectedItem>)value);
    }
    void IList.Remove(object value)
    {
        this.Remove((MappingItem<TSelectedItem>)value);
    }
    void IList.RemoveAt(int index)
    {
        this.RemoveAt(index);
    }
    void ICollection.CopyTo(Array array, int index)
    {
        this.CopyTo((MappingItem<TSelectedItem>[])array, index);
    }
}
