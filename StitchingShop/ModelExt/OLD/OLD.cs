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

public class MappingCollection<TSourceItem, TSelectedItem> :
    IList<TSelectedItem>, ICollection<TSelectedItem>, IList, ICollection, IReadOnlyList<TSelectedItem>, IReadOnlyCollection<TSelectedItem>,
    IEnumerable<TSelectedItem>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    where TSourceItem : DependencyObject where TSelectedItem : DependencyObject
{
    ObservableCollection<TSourceItem> _SourceList;
    ObservableCollection<TSelectedItem> _SelectedList;
    ObservableCollection<TSelectedItem> _OverridedSelectedList;
    DependencyProperty _SourceItemRefranceProperty;
    DependencyProperty _SelectedValueProperty;

    public MappingCollection(
        ObservableCollection<TSourceItem> SourceList,
        ObservableCollection<TSelectedItem> SelectedList,
        DependencyProperty SourceItemRefranceProperty, DependencyProperty SelectedValueProperty)
    {
        _SelectedValueProperty = SelectedValueProperty;
        _SourceItemRefranceProperty = SourceItemRefranceProperty;

        _SourceList = SourceList;
        _SelectedList = SelectedList;
        _SelectedList.CollectionChanged += _SelectedList_CollectionChanged;
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

    #region Local Implementation
    public TSelectedItem this[int index]
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
    #endregion

    #region Direct Implementation
    public int Count
    {
        get
        {
            return ((IList<TSelectedItem>)_OverridedSelectedList).Count;
        }
    }
    public bool IsReadOnly
    {
        get
        {
            return ((IList<TSelectedItem>)_SourceList).IsReadOnly;
        }
    }
    public bool IsFixedSize
    {
        get
        {
            return ((IList)_SourceList).IsFixedSize;
        }
    }
    bool ICollection.IsSynchronized
    {
        get
        {
            return ((ICollection)_SourceList).IsSynchronized;
        }
    }
    object ICollection.SyncRoot
    {
        get
        {
            return ((ICollection)_SourceList).SyncRoot;
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
            this[index] = value as TSelectedItem;
        }
    }
    public int IndexOf(TSelectedItem item)
    {
        return _OverridedSelectedList.IndexOf(item);
    }
    public void Insert(int index, TSelectedItem item)
    {
        _OverridedSelectedList.Insert(index, item);
    }
    public void RemoveAt(int index)
    {
        _OverridedSelectedList.RemoveAt(index);
    }
    public void Add(TSelectedItem item)
    {
        _OverridedSelectedList.Add(item);
    }
    public void Clear()
    {
        _SourceList.Clear();
    }
    public bool Contains(TSelectedItem item)
    {
        return _OverridedSelectedList.Contains(item);
    }
    public void CopyTo(TSelectedItem[] array, int arrayIndex)
    {
        _OverridedSelectedList.CopyTo(array, arrayIndex);
    }
    public bool Remove(TSelectedItem item)
    {
        return _OverridedSelectedList.Remove(item);
    }
    public IEnumerator<TSelectedItem> GetEnumerator()
    {
        return _OverridedSelectedList.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _OverridedSelectedList.GetEnumerator();
    }
    int IList.IndexOf(object value)
    {
        return IndexOf(value as TSelectedItem);
    }
    void IList.Remove(object value)
    {
        this.Remove(value as TSelectedItem);
    }
    void ICollection.CopyTo(Array array, int index)
    {
        this.CopyTo(array as TSelectedItem[], index);
    }
    int IList.Add(object value)
    {
        Add(value as TSelectedItem);
        int index = Count - 1;
        if (index >= 0)
        {
            TSelectedItem lastItem = _OverridedSelectedList[index];
            if (lastItem == value)
                return index;
            index = -1;
        }
        return index;
    }
    bool IList.Contains(object value)
    {
        return Contains(value as TSelectedItem);
    }
    void IList.Insert(int index, object value)
    {
        Insert(index, value as TSelectedItem);
    }
    #endregion

}
