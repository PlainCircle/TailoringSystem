using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MappingCollection<TSourceItem, TSelectedItem>
{
    ObservableCollection<TSourceItem> _SourceList;
    ObservableCollection<TSelectedItem> _SelectedList;
    public MappingCollection(
        ObservableCollection<TSourceItem> SourceList,
        ObservableCollection<TSelectedItem> SelectedList)
    {
        _items = _emptyArray;
        _SourceList = SourceList;
        _SelectedList = SelectedList;
        //_SourceList.CollectionChanged += SourceList_CollectionChanged;
        //_SelectedList.CollectionChanged += SelectedList_CollectionChanged;
    }
    public MappingCollection(
        ObservableCollection<TSourceItem> SourceList,
        ObservableCollection<TSelectedItem> SelectedList,
        IEnumerable<MappingItem<TSelectedItem>> collection):this(SourceList, SelectedList)
    {
        if (collection == null)
            throw new ArgumentNullException("Collection can't be null.");
        Contract.EndContractBlock();

        ICollection<MappingItem<TSelectedItem>> c = collection as ICollection<MappingItem<TSelectedItem>>;
        if (c != null)
        {
            int count = c.Count;
            if (count == 0)
            {
                _items = _emptyArray;
            }
            else
            {
                _items = new MappingItem<TSelectedItem>[count];
                c.CopyTo(_items, 0);
                _size = count;
            }
        }
        else
        {
            _size = 0;
            _items = _emptyArray;
            // This enumerable could be empty.  Let Add allocate a new array, if needed.
            // Note it will also go to _defaultCapacity first, not 1, then 2, etc.

            using (IEnumerator<MappingItem<TSelectedItem>> en = collection.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    Add(en.Current);
                }
            }
        }
    }

    //Read-only property describing how many elements are in the List.
    public int Count
    {
        get
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            return _size;
        }
    }

}