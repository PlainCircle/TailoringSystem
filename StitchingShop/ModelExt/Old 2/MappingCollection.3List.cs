using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Collections;

partial class MappingCollection<TSourceItem, TSelectedItem> : IList<MappingItem<TSelectedItem>>, System.Collections.IList, IReadOnlyList<MappingItem<TSelectedItem>>
{
    private const int _defaultCapacity = 4;

    private MappingItem<TSelectedItem>[] _items;
    [ContractPublicPropertyName("Count")]
    private int _size;
    private int _version;
    [NonSerialized]
    private object _syncRoot;

    static readonly MappingItem<TSelectedItem>[] _emptyArray = new MappingItem<TSelectedItem>[0];


    // Gets and sets the capacity of this list.  The capacity is the size of
    // the internal array used to hold items.  When set, the internal 
    // array of the list is reallocated to the given capacity.
    // 
    public int Capacity
    {
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        get
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            return _items.Length;
        }
        set
        {
            if (value < _size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
            }
            Contract.EndContractBlock();

            if (value != _items.Length)
            {
                if (value > 0)
                {
                    MappingItem<TSelectedItem>[] newItems = new MappingItem<TSelectedItem>[value];
                    if (_size > 0)
                    {
                        Array.Copy(_items, 0, newItems, 0, _size);
                    }
                    _items = newItems;
                }
                else
                {
                    _items = _emptyArray;
                }
            }
        }
    }

    // Is this List read-only?
    bool ICollection<MappingItem<TSelectedItem>>.IsReadOnly
    {
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        get { return false; }
    }

    bool IList.IsReadOnly
    {
        get { return _isReadOnly; }
    }

    // Is this List synchronized (thread-safe)?
    bool System.Collections.ICollection.IsSynchronized
    {
        get { return false; }
    }

    // Synchronization root for this object.
    Object System.Collections.ICollection.SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
            }
            return _syncRoot;
        }
    }

    public object BinaryCompatibility { get; private set; }

    // Sets or Gets the element at the given index.
    // 
    public MappingItem<TSelectedItem> this[int index]
    {
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        get
        {
            // Following trick can reduce the range check by one
            if ((uint)index >= (uint)_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            Contract.EndContractBlock();
            return _items[index];
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        set
        {
            if ((uint)index >= (uint)_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            Contract.EndContractBlock();
            _items[index] = value;
            _version++;
        }
    }

    private static bool IsCompatibleObject(object value)
    {
        // Non-null values are fine.  Only accept nulls if MappingItem<TSelectedItem> is a class or Nullable<U>.
        // Note that default(MappingItem<TSelectedItem>) is not equal to null for value types except when MappingItem<TSelectedItem> is Nullable<U>. 
        return ((value is MappingItem<TSelectedItem>) || (value == null && default(MappingItem<TSelectedItem>) == null));
    }

    Object System.Collections.IList.this[int index]
    {
        get
        {
            return this[index];
        }
        set
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<MappingItem<TSelectedItem>>(value, ExceptionArgument.value);

            try
            {
                this[index] = (MappingItem<TSelectedItem>)value;
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(MappingItem<TSelectedItem>));
            }
        }
    }
    public void Add(MappingItem<TSelectedItem> item)
    {
        InsertItem(_size, item);
    }
    int IList.Add(object value)
    {
        if ((value == null) || !(typeof(MappingItem<TSelectedItem>).IsAssignableFrom(value.GetType())))
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<>(value, ExceptionArgument.value);
        if ((value == null) || !(typeof(MappingItem<TSelectedItem>).IsAssignableFrom(value.GetType())))
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<>(value, ExceptionArgument.value);

        try
        {
            Add((MappingItem<TSelectedItem>)value);
        }
        catch (InvalidCastException)
        {
            ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(MappingItem<TSelectedItem>));
        }

        return this.Count - 1;
    }


    // Adds the elements of the given collection to the end of this list. If
    // required, the capacity of the list is increased to twice the previous
    // capacity or the new size, whichever is larger.
    //
    public void AddRange(IEnumerable<MappingItem<TSelectedItem>> collection)
    {
        Contract.Ensures(Count >= Contract.OldValue(Count));

        InsertRange(_size, collection);
    }

    public ReadOnlyCollection<MappingItem<TSelectedItem>> AsReadOnly()
    {
        Contract.Ensures(Contract.Result<ReadOnlyCollection<MappingItem<TSelectedItem>>>() != null);
        return new ReadOnlyCollection<MappingItem<TSelectedItem>>(this);
    }

    // Searches a section of the list for a given element using a binary search
    // algorithm. Elements of the list are compared to the search value using
    // the given IComparer interface. If comparer is null, elements of
    // the list are compared to the search value using the IComparable
    // interface, which in that case must be implemented by all elements of the
    // list and the given search value. This method assumes that the given
    // section of the list is already sorted; if this is not the case, the
    // result will be incorrect.
    //
    // The method returns the index of the given value in the list. If the
    // list does not contain the given value, the method returns a negative
    // integer. The bitwise complement operator (~) can be applied to a
    // negative result to produce the index of the first element (if any) that
    // is larger than the given search value. This is also the index at which
    // the search value should be inserted into the list in order for the list
    // to remain sorted.
    // 
    // The method uses the Array.BinarySearch method to perform the
    // search.
    // 
    public int BinarySearch(int index, int count, MappingItem<TSelectedItem> item, IComparer<MappingItem<TSelectedItem>> comparer)
    {
        if (index < 0)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (count < 0)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (_size - index < count)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidOffLen);
        Contract.Ensures(Contract.Result<int>() <= index + count);
        Contract.EndContractBlock();

        return Array.BinarySearch<MappingItem<TSelectedItem>>(_items, index, count, item, comparer);
    }

    public int BinarySearch(MappingItem<TSelectedItem> item)
    {
        Contract.Ensures(Contract.Result<int>() <= Count);
        return BinarySearch(0, Count, item, null);
    }

    public int BinarySearch(MappingItem<TSelectedItem> item, IComparer<MappingItem<TSelectedItem>> comparer)
    {
        Contract.Ensures(Contract.Result<int>() <= Count);
        return BinarySearch(0, Count, item, comparer);
    }


    // Clears the contents of List.
    public void Clear()
    {
        if (_size > 0)
        {
            Array.Clear(_items, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            _size = 0;
        }
        _version++;
    }

    // Contains returns true if the specified element is in the List.
    // It does a linear, O(n) search.  Equality is determined by calling
    // item.Equals().
    //
    public bool Contains(MappingItem<TSelectedItem> item)
    {
        if ((Object)item == null)
        {
            for (int i = 0; i < _size; i++)
                if ((Object)_items[i] == null)
                    return true;
            return false;
        }
        else
        {
            EqualityComparer<MappingItem<TSelectedItem>> c = EqualityComparer<MappingItem<TSelectedItem>>.Default;
            for (int i = 0; i < _size; i++)
            {
                if (c.Equals(_items[i], item)) return true;
            }
            return false;
        }
    }

    bool System.Collections.IList.Contains(Object item)
    {
        if (IsCompatibleObject(item))
        {
            return Contains((MappingItem<TSelectedItem>)item);
        }
        return false;
    }

    public List<TOutput> ConvertAll<TOutput>(Converter<MappingItem<TSelectedItem>, TOutput> converter)
    {
        if (converter == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
        }
        // @


        Contract.EndContractBlock();

        List<TOutput> list = new List<TOutput>(_size);
        for (int i = 0; i < _size; i++)
        {
            list._items[i] = converter(_items[i]);
        }
        list._size = _size;
        return list;
    }
    public void CopyTo(MappingItem<TSelectedItem>[] array)
    {
        CopyTo(array, 0);
    }

    // Copies this List into array, which must be of a 
    // compatible array type.  
    //
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
    {
        if ((array != null) && (array.Rank != 1))
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
        }
        Contract.EndContractBlock();

        try
        {
            // Array.Copy will check for NULL.
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }
        catch (ArrayTypeMismatchException)
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidArrayType);
        }
    }

    // Copies a section of this list to the given array at the given index.
    // 
    // The method uses the Array.Copy method to copy the elements.
    // 
    public void CopyTo(int index, MappingItem<TSelectedItem>[] array, int arrayIndex, int count)
    {
        if (_size - index < count)
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidOffLen);
        }
        Contract.EndContractBlock();

        // Delegate rest of error checking to Array.Copy.
        Array.Copy(_items, index, array, arrayIndex, count);
    }

    public void CopyTo(MappingItem<TSelectedItem>[] array, int arrayIndex)
    {
        // Delegate rest of error checking to Array.Copy.
        Array.Copy(_items, 0, array, arrayIndex, _size);
    }

    // Ensures that the capacity of this list is at least the given minimum
    // value. If the currect capacity of the list is less than min, the
    // capacity is increased to twice the current capacity or to min,
    // whichever is larger.
    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > Array.MaxArrayLength) newCapacity = Array.MaxArrayLength;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }
    }

#if FEATURE_LIST_PREDICATES || FEATURE_NETCORE
        public bool Exists(Predicate<MappingItem<TSelectedItem>> match) {
            return FindIndex(match) != -1;
        }

        public MappingItem<TSelectedItem> Find(Predicate<MappingItem<TSelectedItem>> match) {
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            for(int i = 0 ; i < _size; i++) {
                if(match(_items[i])) {
                    return _items[i];
                }
            }
            return default(MappingItem<TSelectedItem>);
        }
  
        public List<MappingItem<TSelectedItem>> FindAll(Predicate<MappingItem<TSelectedItem>> match) { 
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            List<MappingItem<TSelectedItem>> list = new List<MappingItem<TSelectedItem>>(); 
            for(int i = 0 ; i < _size; i++) {
                if(match(_items[i])) {
                    list.Add(_items[i]);
                }
            }
            return list;
        }
  
        public int FindIndex(Predicate<MappingItem<TSelectedItem>> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            return FindIndex(0, _size, match);
        }
  
        public int FindIndex(int startIndex, Predicate<MappingItem<TSelectedItem>> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < startIndex + Count);
            return FindIndex(startIndex, _size - startIndex, match);
        }
 
        public int FindIndex(int startIndex, int count, Predicate<MappingItem<TSelectedItem>> match) {
            if( (uint)startIndex > (uint)_size ) {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);                
            }

            if (count < 0 || startIndex > _size - count) {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }

            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < startIndex + count);
            Contract.EndContractBlock();

            int endIndex = startIndex + count;
            for( int i = startIndex; i < endIndex; i++) {
                if( match(_items[i])) return i;
            }
            return -1;
        }
 
        public MappingItem<TSelectedItem> FindLast(Predicate<MappingItem<TSelectedItem>> match) {
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            for(int i = _size - 1 ; i >= 0; i--) {
                if(match(_items[i])) {
                    return _items[i];
                }
            }
            return default(MappingItem<TSelectedItem>);
        }

        public int FindLastIndex(Predicate<MappingItem<TSelectedItem>> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            return FindLastIndex(_size - 1, _size, match);
        }
   
        public int FindLastIndex(int startIndex, Predicate<MappingItem<TSelectedItem>> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() <= startIndex);
            return FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<MappingItem<TSelectedItem>> match) {
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() <= startIndex);
            Contract.EndContractBlock();

            if(_size == 0) {
                // Special case for 0 length List
                if( startIndex != -1) {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                }
            }
            else {
                // Make sure we're not out of range            
                if ( (uint)startIndex >= (uint)_size) {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                }
            }
            
            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if (count < 0 || startIndex - count + 1 < 0) {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }
                        
            int endIndex = startIndex - count;
            for( int i = startIndex; i > endIndex; i--) {
                if( match(_items[i])) {
                    return i;
                }
            }
            return -1;
        }
#endif // FEATURE_LIST_PREDICATES || FEATURE_NETCORE

    public void ForEach(Action<MappingItem<TSelectedItem>> action)
    {
        if (action == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }
        Contract.EndContractBlock();

        int version = _version;

        for (int i = 0; i < _size; i++)
        {
            if (version != _version && BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
            {
                break;
            }
            action(_items[i]);
        }

        if (version != _version && BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
            ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
    }

    // Returns an enumerator for this list with the given
    // permission for removal of elements. If modifications made to the list 
    // while an enumeration is in progress, the MoveNext and 
    // GetObject methods of the enumerator will throw an exception.
    //
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <internalonly/>
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    IEnumerator<MappingItem<TSelectedItem>> IEnumerable<MappingItem<TSelectedItem>>.GetEnumerator()
    {
        return new Enumerator(this);
    }

#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    public List<MappingItem<TSelectedItem>> GetRange(int index, int count)
    {
        if (index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (count < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (_size - index < count)
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidOffLen);
        }
        Contract.Ensures(Contract.Result<List<MappingItem<TSelectedItem>>>() != null);
        Contract.EndContractBlock();

        List<MappingItem<TSelectedItem>> list = new List<MappingItem<TSelectedItem>>(count);
        Array.Copy(_items, index, list._items, 0, count);
        list._size = count;
        return list;
    }


    // Returns the index of the first occurrence of a given value in a range of
    // this list. The list is searched forwards from beginning to end.
    // The elements of the list are compared to the given value using the
    // Object.Equals method.
    // 
    // This method uses the Array.IndexOf method to perform the
    // search.
    // 
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public int IndexOf(MappingItem<TSelectedItem> item)
    {
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(Contract.Result<int>() < Count);
        return Array.IndexOf(_items, item, 0, _size);
    }

#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    int System.Collections.IList.IndexOf(Object item)
    {
        if (IsCompatibleObject(item))
        {
            return IndexOf((MappingItem<TSelectedItem>)item);
        }
        return -1;
    }

    // Returns the index of the first occurrence of a given value in a range of
    // this list. The list is searched forwards, starting at index
    // index and ending at count number of elements. The
    // elements of the list are compared to the given value using the
    // Object.Equals method.
    // 
    // This method uses the Array.IndexOf method to perform the
    // search.
    // 
    public int IndexOf(MappingItem<TSelectedItem> item, int index)
    {
        if (index > _size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(Contract.Result<int>() < Count);
        Contract.EndContractBlock();
        return Array.IndexOf(_items, item, index, _size - index);
    }

    // Returns the index of the first occurrence of a given value in a range of
    // this list. The list is searched forwards, starting at index
    // index and upto count number of elements. The
    // elements of the list are compared to the given value using the
    // Object.Equals method.
    // 
    // This method uses the Array.IndexOf method to perform the
    // search.
    // 
    public int IndexOf(MappingItem<TSelectedItem> item, int index, int count)
    {
        if (index > _size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);

        if (count < 0 || index > _size - count) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(Contract.Result<int>() < Count);
        Contract.EndContractBlock();

        return Array.IndexOf(_items, item, index, count);
    }


    void System.Collections.IList.Insert(int index, Object item)
    {
        ThrowHelper.IfNullAndNullsAreIllegalThenThrow<MappingItem<TSelectedItem>>(item, ExceptionArgument.item);

        try
        {
            Insert(index, (MappingItem<TSelectedItem>)item);
        }
        catch (InvalidCastException)
        {
            ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(MappingItem<TSelectedItem>));
        }
    }

    // Inserts the elements of the given collection at a given index. If
    // required, the capacity of the list is increased to twice the previous
    // capacity or the new size, whichever is larger.  Ranges may be added
    // to the end of the list by setting index to the List's size.
    //
    public void InsertRange(int index, IEnumerable<MappingItem<TSelectedItem>> collection)
    {
        if (collection == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
        }

        if ((uint)index > (uint)_size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
        }
        Contract.EndContractBlock();

        ICollection<MappingItem<TSelectedItem>> c = collection as ICollection<MappingItem<TSelectedItem>>;
        if (c != null)
        {    // if collection is ICollection<MappingItem<TSelectedItem>>
            int count = c.Count;
            if (count > 0)
            {
                EnsureCapacity(_size + count);
                if (index < _size)
                {
                    Array.Copy(_items, index, _items, index + count, _size - index);
                }

                // If we're inserting a List into itself, we want to be able to deal with that.
                if (this == c)
                {
                    // Copy first part of _items to insert location
                    Array.Copy(_items, 0, _items, index, index);
                    // Copy last part of _items back to inserted location
                    Array.Copy(_items, index + count, _items, index * 2, _size - index);
                }
                else
                {
                    MappingItem<TSelectedItem>[] itemsToInsert = new MappingItem<TSelectedItem>[count];
                    c.CopyTo(itemsToInsert, 0);
                    itemsToInsert.CopyTo(_items, index);
                }
                _size += count;
            }
        }
        else
        {
            using (IEnumerator<MappingItem<TSelectedItem>> en = collection.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    Insert(index++, en.Current);
                }
            }
        }
        _version++;
    }

    // Returns the index of the last occurrence of a given value in a range of
    // this list. The list is searched backwards, starting at the end 
    // and ending at the first element in the list. The elements of the list 
    // are compared to the given value using the Object.Equals method.
    // 
    // This method uses the Array.LastIndexOf method to perform the
    // search.
    // 
    public int LastIndexOf(MappingItem<TSelectedItem> item)
    {
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(Contract.Result<int>() < Count);
        if (_size == 0)
        {  // Special case for empty list
            return -1;
        }
        else
        {
            return LastIndexOf(item, _size - 1, _size);
        }
    }

    // Returns the index of the last occurrence of a given value in a range of
    // this list. The list is searched backwards, starting at index
    // index and ending at the first element in the list. The 
    // elements of the list are compared to the given value using the 
    // Object.Equals method.
    // 
    // This method uses the Array.LastIndexOf method to perform the
    // search.
    // 
    public int LastIndexOf(MappingItem<TSelectedItem> item, int index)
    {
        if (index >= _size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(((Count == 0) && (Contract.Result<int>() == -1)) || ((Count > 0) && (Contract.Result<int>() <= index)));
        Contract.EndContractBlock();
        return LastIndexOf(item, index, index + 1);
    }

    // Returns the index of the last occurrence of a given value in a range of
    // this list. The list is searched backwards, starting at index
    // index and upto count elements. The elements of
    // the list are compared to the given value using the Object.Equals
    // method.
    // 
    // This method uses the Array.LastIndexOf method to perform the
    // search.
    // 
    public int LastIndexOf(MappingItem<TSelectedItem> item, int index, int count)
    {
        if ((Count != 0) && (index < 0))
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if ((Count != 0) && (count < 0))
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(((Count == 0) && (Contract.Result<int>() == -1)) || ((Count > 0) && (Contract.Result<int>() <= index)));
        Contract.EndContractBlock();

        if (_size == 0)
        {  // Special case for empty list
            return -1;
        }

        if (index >= _size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
        }

        if (count > index + 1)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
        }

        return Array.LastIndexOf(_items, item, index, count);
    }

    // Removes the element at the given index. The size of the list is
    // decreased by one.
    // 
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public bool Remove(MappingItem<TSelectedItem> item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    void System.Collections.IList.Remove(Object item)
    {
        if (IsCompatibleObject(item))
        {
            Remove((MappingItem<TSelectedItem>)item);
        }
    }

    // This method removes all items which matches the predicate.
    // The complexity is O(n).   
    public int RemoveAll(Predicate<MappingItem<TSelectedItem>> match)
    {
        if (match == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }
        Contract.Ensures(Contract.Result<int>() >= 0);
        Contract.Ensures(Contract.Result<int>() <= Contract.OldValue(Count));
        Contract.EndContractBlock();

        int freeIndex = 0;   // the first free slot in items array

        // Find the first item which needs to be removed.
        while (freeIndex < _size && !match(_items[freeIndex])) freeIndex++;
        if (freeIndex >= _size) return 0;

        int current = freeIndex + 1;
        while (current < _size)
        {
            // Find the first item which needs to be kept.
            while (current < _size && match(_items[current])) current++;

            if (current < _size)
            {
                // copy item to the free slot.
                _items[freeIndex++] = _items[current++];
            }
        }

        Array.Clear(_items, freeIndex, _size - freeIndex);
        int result = _size - freeIndex;
        _size = freeIndex;
        _version++;
        return result;
    }

    // Removes the element at the given index. The size of the list is
    // decreased by one.
    // 
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }
        Contract.EndContractBlock();
        _size--;
        if (index < _size)
        {
            Array.Copy(_items, index + 1, _items, index, _size - index);
        }
        _items[_size] = default(MappingItem<TSelectedItem>);
        _version++;
    }

    // Removes a range of elements from this list.
    // 
    public void RemoveRange(int index, int count)
    {
        if (index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (count < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (_size - index < count)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidOffLen);
        Contract.EndContractBlock();

        if (count > 0)
        {
            int i = _size;
            _size -= count;
            if (index < _size)
            {
                Array.Copy(_items, index + count, _items, index, _size - index);
            }
            Array.Clear(_items, _size, count);
            _version++;
        }
    }

    // Reverses the elements in this list.
    public void Reverse()
    {
        Reverse(0, Count);
    }

    // Reverses the elements in a range of this list. Following a call to this
    // method, an element in the range given by index and count
    // which was previously located at index i will now be located at
    // index index + (index + count - i - 1).
    // 
    // This method uses the Array.Reverse method to reverse the
    // elements.
    // 
    public void Reverse(int index, int count)
    {
        if (index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (count < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (_size - index < count)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidOffLen);
        Contract.EndContractBlock();
        Array.Reverse(_items, index, count);
        _version++;
    }

    // Sorts the elements in this list.  Uses the default comparer and 
    // Array.Sort.
    public void Sort()
    {
        Sort(0, Count, null);
    }

    // Sorts the elements in this list.  Uses Array.Sort with the
    // provided comparer.
    public void Sort(IComparer<MappingItem<TSelectedItem>> comparer)
    {
        Sort(0, Count, comparer);
    }

    // Sorts the elements in a section of this list. The sort compares the
    // elements to each other using the given IComparer interface. If
    // comparer is null, the elements are compared to each other using
    // the IComparable interface, which in that case must be implemented by all
    // elements of the list.
    // 
    // This method uses the Array.Sort method to sort the elements.
    // 
    public void Sort(int index, int count, IComparer<MappingItem<TSelectedItem>> comparer)
    {
        if (index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (count < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (_size - index < count)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidOffLen);
        Contract.EndContractBlock();

        Array.Sort<MappingItem<TSelectedItem>>(_items, index, count, comparer);
        _version++;
    }

    public void Sort(Comparison<MappingItem<TSelectedItem>> comparison)
    {
        if (comparison == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }
        Contract.EndContractBlock();

        if (_size > 0)
        {
            IComparer<MappingItem<TSelectedItem>> comparer = new Array.FunctorComparer<MappingItem<TSelectedItem>>(comparison);
            Array.Sort(_items, 0, _size, comparer);
        }
    }

    // ToArray returns a new Object array containing the contents of the List.
    // This requires copying the List, which is an O(n) operation.
    public MappingItem<TSelectedItem>[] ToArray()
    {
        Contract.Ensures(Contract.Result<MappingItem<TSelectedItem>[]>() != null);
        Contract.Ensures(Contract.Result<MappingItem<TSelectedItem>[]>().Length == Count);

        MappingItem<TSelectedItem>[] array = new MappingItem<TSelectedItem>[_size];
        Array.Copy(_items, 0, array, 0, _size);
        return array;
    }

    // Sets the capacity of this list to the size of the list. This method can
    // be used to minimize a list's memory overhead once it is known that no
    // new elements will be added to the list. To completely clear a list and
    // release all memory referenced by the list, execute the following
    // statements:
    // 
    // list.Clear();
    // list.TrimExcess();
    // 
    public void TrimExcess()
    {
        int threshold = (int)(((double)_items.Length) * 0.9);
        if (_size < threshold)
        {
            Capacity = _size;
        }
    }

#if FEATURE_LIST_PREDICATES || FEATURE_NETCORE
        public bool TrueForAll(Predicate<MappingItem<TSelectedItem>> match) {
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            for(int i = 0 ; i < _size; i++) {
                if( !match(_items[i])) {
                    return false;
                }
            }
            return true;
        } 
#endif // FEATURE_LIST_PREDICATES || FEATURE_NETCORE

    internal static IList<MappingItem<TSelectedItem>> Synchronized(List<MappingItem<TSelectedItem>> list)
    {
        return new SynchronizedList(list);
    }

    [Serializable()]
    internal class SynchronizedList : IList<MappingItem<TSelectedItem>>
    {
        private List<MappingItem<TSelectedItem>> _list;
        private Object _root;

        internal SynchronizedList(List<MappingItem<TSelectedItem>> list)
        {
            _list = list;
            _root = ((System.Collections.ICollection)list).SyncRoot;
        }

        public int Count
        {
            get
            {
                lock (_root)
                {
                    return _list.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((ICollection<MappingItem<TSelectedItem>>)_list).IsReadOnly;
            }
        }

        public void Add(MappingItem<TSelectedItem> item)
        {
            lock (_root)
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            lock (_root)
            {
                _list.Clear();
            }
        }

        public bool Contains(MappingItem<TSelectedItem> item)
        {
            lock (_root)
            {
                return _list.Contains(item);
            }
        }

        public void CopyTo(MappingItem<TSelectedItem>[] array, int arrayIndex)
        {
            lock (_root)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(MappingItem<TSelectedItem> item)
        {
            lock (_root)
            {
                return _list.Remove(item);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (_root)
            {
                return _list.GetEnumerator();
            }
        }

        IEnumerator<MappingItem<TSelectedItem>> IEnumerable<MappingItem<TSelectedItem>>.GetEnumerator()
        {
            lock (_root)
            {
                return ((IEnumerable<MappingItem<TSelectedItem>>)_list).GetEnumerator();
            }
        }

        public MappingItem<TSelectedItem> this[int index]
        {
            get
            {
                lock (_root)
                {
                    return _list[index];
                }
            }
            set
            {
                lock (_root)
                {
                    _list[index] = value;
                }
            }
        }

        public int IndexOf(MappingItem<TSelectedItem> item)
        {
            lock (_root)
            {
                return _list.IndexOf(item);
            }
        }

        public void Insert(int index, MappingItem<TSelectedItem> item)
        {
            lock (_root)
            {
                _list.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_root)
            {
                _list.RemoveAt(index);
            }
        }
    }

    [Serializable]
    public struct Enumerator : IEnumerator<MappingItem<TSelectedItem>>, System.Collections.IEnumerator
    {
        private List<MappingItem<TSelectedItem>> list;
        private int index;
        private int version;
        private MappingItem<TSelectedItem> current;

        internal Enumerator(List<MappingItem<TSelectedItem>> list)
        {
            this.list = list;
            index = 0;
            version = list._version;
            current = default(MappingItem<TSelectedItem>);
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public void Dispose()
        {
        }

        public bool MoveNext()
        {

            List<MappingItem<TSelectedItem>> localList = list;

            if (version == localList._version && ((uint)index < (uint)localList._size))
            {
                current = localList._items[index];
                index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (version != list._version)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
            }

            index = list._size + 1;
            current = default(MappingItem<TSelectedItem>);
            return false;
        }

        public MappingItem<TSelectedItem> Current
        {
            get
            {
                return current;
            }
        }

        Object System.Collections.IEnumerator.Current
        {
            get
            {
                if (index == 0 || index == list._size + 1)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                }
                return Current;
            }
        }

        void System.Collections.IEnumerator.Reset()
        {
            if (version != list._version)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
            }

            index = 0;
            current = default(MappingItem<TSelectedItem>);
        }

    }
}


