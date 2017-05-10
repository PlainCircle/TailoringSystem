using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime;
[System.Runtime.InteropServices.ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
public partial class MappingCollection<T> : IList<T>, IList, IReadOnlyList<T>
{
    static readonly T[] _emptyArray = new T[0];
    private const int _defaultCapacity = 4;

    protected bool isReadOnly = false;
    protected bool isFixedSize = false;

    private T[] _items;
    [ContractPublicPropertyName("Count")]
    private int _size;
    private int _version;
    [NonSerialized]
    private Object _syncRoot;
    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_size)
                ThrowHelper.ThrowArgumentOutOfRangeException();
            Contract.EndContractBlock();
            return _items[index];
        }
        set
        {
            SetItem(index, value);
        }
    }
    public void Add(T item)
    {
        InsertItem(_size, item);
    }
    public void Clear()
    {
        ClearItems();
    }
    public void RemoveAt(int index)
    {
        RemoveItem(index);
    }
    void ICollection.CopyTo(Array array, int index)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if (array.Rank != 1)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
        if (array.GetLowerBound(0) != 0)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
        if (index < 0)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (array.Length - index < Count)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
        Contract.EndContractBlock();

        T[] tArray = array as T[];
        if (tArray != null)
        {
            CopyTo(tArray, index);
        }
        else
        {
            //
            // Catch the obvious case assignment will fail.
            // We can found all possible problems by doing the check though.
            // For example, if the element type of the Array is derived from T,
            // we can't figure out if we can successfully copy the element beforehand.
            //
            Type targetType = array.GetType().GetElementType();
            Type sourceType = typeof(T);
            if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
            }

            //
            // We can't cast array of value type to object[], so we don't support 
            // widening of primitive types here.
            //
            object[] objects = array as object[];
            if (objects == null)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
            }

            try
            {
                for (int i = 0; i < _size; i++)
                {
                    objects[index++] = _items[i];
                }
            }
            catch (ArrayTypeMismatchException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
            }
        }
    }
    object IList.this[int index]
    {
        get { return _items[index]; }
        set
        {
            if (!IsCompatibleObject(value))
                ThrowHelper.ThrowIfNotCompatibleObject(ExceptionArgument.value);
            this[index] = (T)value;
        }
    }
    int IList.Add(object value)
    {
        if (!IsCompatibleObject(value))
            return -1;
        InsertItem(_size,(T)value);
        return _size - 1;
    }
    bool IList.Contains(object value)
    {
        if (IsCompatibleObject(value))
        {
            return Contains((T)value);
        }
        return false;
    }
    void IList.Insert(int index, object value)
    {
        if (IsCompatibleObject(value))
            Insert(index, (T)value);
    }

    public MappingCollection(int capacity)
    {
        if (capacity < 0) ThrowHelper.ThrowArgumentOutOfRangeException(
            ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        Contract.EndContractBlock();

        if (capacity == 0)
            _items = _emptyArray;
        else
            _items = new T[capacity];
    }

    // Constructs a List, copying the contents of the given collection. The
    // size and capacity of the new list will both be equal to the size of the
    // given collection.
    // 
    public MappingCollection(IEnumerable<T> collection)
    {
        if (collection == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
        Contract.EndContractBlock();

        ICollection<T> c = collection as ICollection<T>;
        if (c != null)
        {
            int count = c.Count;
            if (count == 0)
            {
                _items = _emptyArray;
            }
            else
            {
                _items = new T[count];
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

            using (IEnumerator<T> en = collection.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    Add(en.Current);
                }
            }
        }
    }

    // Gets and sets the capacity of this list.  The capacity is the size of
    // the internal array used to hold items.  When set, the internal 
    // array of the list is reallocated to the given capacity.
    // 
    public int Capacity
    {
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
                    T[] newItems = new T[value];
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

    // Read-only property describing how many elements are in the List.
    public int Count
    {
        get
        {
            Contract.Ensures(Contract.Result<int>() >= 0);
            return _size;
        }
    }
    public bool IsFixedSize
    {
        get { return (isReadOnly || isFixedSize); }
    }
    public bool IsReadOnly
    {
        get { return isReadOnly; }
    }

    // Is this List synchronized (thread-safe)?
    bool ICollection.IsSynchronized
    {
        get { return false; }
    }

    // Synchronization root for this object.
    Object ICollection.SyncRoot
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
    private static bool IsCompatibleObject(object value)
    {
        // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
        // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
        return ((value is T) || (value == null && default(T) == null));
    }

    // Adds the elements of the given collection to the end of this list. If
    // required, the capacity of the list is increased to twice the previous
    // capacity or the new size, whichever is larger.
    //
    public void AddRange(IEnumerable<T> collection)
    {
        Contract.Ensures(Count >= Contract.OldValue(Count));

        InsertRange(_size, collection);
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
        Contract.Ensures(Contract.Result<ReadOnlyCollection<T>>() != null);
        return new ReadOnlyCollection<T>(this);
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
    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
        if (index < 0)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (count < 0)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        if (_size - index < count)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        Contract.Ensures(Contract.Result<int>() <= index + count);
        Contract.EndContractBlock();

        return Array.BinarySearch<T>(_items, index, count, item, comparer);
    }

    public int BinarySearch(T item)
    {
        Contract.Ensures(Contract.Result<int>() <= Count);
        return BinarySearch(0, Count, item, null);
    }

    public int BinarySearch(T item, IComparer<T> comparer)
    {
        Contract.Ensures(Contract.Result<int>() <= Count);
        return BinarySearch(0, Count, item, comparer);
    }

    // Contains returns true if the specified element is in the List.
    // It does a linear, O(n) search.  Equality is determined by calling
    // item.Equals().
    //
    public bool Contains(T item)
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
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            for (int i = 0; i < _size; i++)
            {
                if (c.Equals(_items[i], item)) return true;
            }
            return false;
        }
    }

    public virtual MappingCollection<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        if (converter == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
        Contract.EndContractBlock();
        Type OutputType = GetType().GetGenericTypeDefinition().MakeGenericType(typeof(TOutput));
        MappingCollection<TOutput> output = (MappingCollection<TOutput>)Activator.CreateInstance(OutputType);

        if (output._items.Length < _size)
        {
            TOutput[] newItems = new TOutput[_size];
            if (output._items.Length > 0)
                Array.Copy(output._items, newItems, output._items.Length);
            output._items = newItems;
        }

        for (int i = 0; i < _size; i++)
            output._items[i] = converter(_items[i]);
        output._size = _size;
        return output;
    }
    public void CopyTo(T[] array)
    {
        CopyTo(array, 0);
    }

    // Copies a section of this list to the given array at the given index.
    // 
    // The method uses the Array.Copy method to copy the elements.
    // 
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        if (_size - index < count)
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }
        Contract.EndContractBlock();

        // Delegate rest of error checking to Array.Copy.
        Array.Copy(_items, index, array, arrayIndex, count);
    }

    public void CopyTo(T[] array, int arrayIndex)
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
            if ((uint)newCapacity > ThrowHelper.MaxArrayLength) newCapacity = ThrowHelper.MaxArrayLength;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }
    }

#if FEATURE_LIST_PREDICATES || FEATURE_NETCORE
        public bool Exists(Predicate<T> match) {
            return FindIndex(match) != -1;
        }

        public T Find(Predicate<T> match) {
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            for(int i = 0 ; i < _size; i++) {
                if(match(_items[i])) {
                    return _items[i];
                }
            }
            return default(T);
        }
  
        public List<T> FindAll(Predicate<T> match) { 
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            List<T> list = new List<T>(); 
            for(int i = 0 ; i < _size; i++) {
                if(match(_items[i])) {
                    list.Add(_items[i]);
                }
            }
            return list;
        }
  
        public int FindIndex(Predicate<T> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            return FindIndex(0, _size, match);
        }
  
        public int FindIndex(int startIndex, Predicate<T> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < startIndex + Count);
            return FindIndex(startIndex, _size - startIndex, match);
        }
 
        public int FindIndex(int startIndex, int count, Predicate<T> match) {
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
 
        public T FindLast(Predicate<T> match) {
            if( match == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            for(int i = _size - 1 ; i >= 0; i--) {
                if(match(_items[i])) {
                    return _items[i];
                }
            }
            return default(T);
        }

        public int FindLastIndex(Predicate<T> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            return FindLastIndex(_size - 1, _size, match);
        }
   
        public int FindLastIndex(int startIndex, Predicate<T> match) {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() <= startIndex);
            return FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match) {
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

    public void ForEach(Action<T> action)
    {
        if (action == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }
        Contract.EndContractBlock();

        int version = _version;

        for (int i = 0; i < _size; i++)
        {
            if (version != _version)
                break;
            action(_items[i]);
        }
        if (version != _version)
            ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
    }
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }
    public MappingCollection<T> GetRange(int index, int count)
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
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }
        Contract.Ensures(Contract.Result<MappingCollection<T>>() != null);
        Contract.EndContractBlock();

        MappingCollection<T> list = new MappingCollection<T>(count);
        Array.Copy(_items, index, list._items, 0, count);
        list._size = count;
        return list;
    }
    public int IndexOf(T item)
    {
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(Contract.Result<int>() < Count);
        return Array.IndexOf(_items, item, 0, _size);
    }
    int IList.IndexOf(Object item)
    {
        if (IsCompatibleObject(item))
        {
            return IndexOf((T)item);
        }
        return -1;
    }
    public int IndexOf(T item, int index)
    {
        if (index > _size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(Contract.Result<int>() < Count);
        Contract.EndContractBlock();
        return Array.IndexOf(_items, item, index, _size - index);
    }
    public int IndexOf(T item, int index, int count)
    {
        if (index > _size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);

        if (count < 0 || index > _size - count) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(Contract.Result<int>() < Count);
        Contract.EndContractBlock();

        return Array.IndexOf(_items, item, index, count);
    }
    public void Insert(int index, T item)
    {
        InsertItem(index, item);
    }
    public void InsertRange(int index, IEnumerable<T> collection)
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

        ICollection<T> c = collection as ICollection<T>;
        if (c != null)
        {    // if collection is ICollection<T>
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
                    T[] itemsToInsert = new T[count];
                    c.CopyTo(itemsToInsert, 0);
                    itemsToInsert.CopyTo(_items, index);
                }
                _size += count;
            }
        }
        else
        {
            using (IEnumerator<T> en = collection.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    Insert(index++, en.Current);
                }
            }
        }
        _version++;
    }
    public int LastIndexOf(T item)
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
    public int LastIndexOf(T item, int index)
    {
        if (index >= _size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
        Contract.Ensures(Contract.Result<int>() >= -1);
        Contract.Ensures(((Count == 0) && (Contract.Result<int>() == -1)) || ((Count > 0) && (Contract.Result<int>() <= index)));
        Contract.EndContractBlock();
        return LastIndexOf(item, index, index + 1);
    }
    public int LastIndexOf(T item, int index, int count)
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
    public bool Remove(T item)
    {
        if (isReadOnly)
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);

        int index = IndexOf(item);
        if (index < 0) return false;
        RemoveItem(index);
        return true;
    }
    void IList.Remove(Object item)
    {
        if (IsCompatibleObject(item))
        {
            Remove((T)item);
        }
    }
    public int RemoveAll(Predicate<T> match)
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
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
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
    public void Reverse()
    {
        Reverse(0, Count);
    }
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
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        Contract.EndContractBlock();
        Array.Reverse(_items, index, count);
        _version++;
    }
    public void Sort()
    {
        Sort(0, Count, null);
    }
    public void Sort(IComparer<T> comparer)
    {
        Sort(0, Count, comparer);
    }
    public void Sort(int index, int count, IComparer<T> comparer)
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
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        Contract.EndContractBlock();

        Array.Sort<T>(_items, index, count, comparer);
        _version++;
    }
    public void Sort(Comparison<T> comparison)
    {
        if (comparison == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        }
        Contract.EndContractBlock();

        if (_size > 0)
        {
            IComparer<T> comparer = new ThrowHelper.FunctorComparer<T>(comparison);
            Array.Sort(_items, 0, _size, comparer);
        }
    }
    public T[] ToArray()
    {
        Contract.Ensures(Contract.Result<T[]>() != null);
        Contract.Ensures(Contract.Result<T[]>().Length == Count);

        T[] array = new T[_size];
        Array.Copy(_items, 0, array, 0, _size);
        return array;
    }
    public void TrimExcess()
    {
        int threshold = (int)(((double)_items.Length) * 0.9);
        if (_size < threshold)
        {
            Capacity = _size;
        }
    }
    internal static IList<T> Synchronized(List<T> list)
    {
        return new SynchronizedList(list);
    }
    [Serializable()]
    internal class SynchronizedList : IList<T>
    {
        private List<T> _list;
        private Object _root;

        internal SynchronizedList(List<T> list)
        {
            _list = list;
            _root = ((ICollection)list).SyncRoot;
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
                return ((ICollection<T>)_list).IsReadOnly;
            }
        }

        public void Add(T item)
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

        public bool Contains(T item)
        {
            lock (_root)
            {
                return _list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_root)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (_root)
            {
                return _list.Remove(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_root)
            {
                return _list.GetEnumerator();
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (_root)
            {
                return ((IEnumerable<T>)_list).GetEnumerator();
            }
        }

        public T this[int index]
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

        public int IndexOf(T item)
        {
            lock (_root)
            {
                return _list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
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
    public struct Enumerator : IEnumerator<T>, IEnumerator
    {
        private MappingCollection<T> list;
        private int index;
        private int version;
        private T current;

        internal Enumerator(MappingCollection<T> list)
        {
            this.list = list;
            index = 0;
            version = list._version;
            current = default(T);
        }
        public void Dispose()
        {
        }

        public bool MoveNext()
        {

            MappingCollection<T> localList = list;

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
            current = default(T);
            return false;
        }

        public T Current
        {
            get
            {
                return current;
            }
        }

        Object IEnumerator.Current
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

        void IEnumerator.Reset()
        {
            if (version != list._version)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
            }

            index = 0;
            current = default(T);
        }

    }
}