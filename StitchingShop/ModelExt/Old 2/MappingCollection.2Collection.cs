using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Runtime;

[System.Runtime.InteropServices.ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
partial class MappingCollection<TSourceItem, TSelectedItem>: IList<MappingItem<TSelectedItem>>, IList, IReadOnlyList<MappingItem<TSelectedItem>>
{
    [NonSerialized]
    private Object _syncRoot;

    public MappingItem<TSelectedItem> this[int index]
    {
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        get { return items[index]; }
        set
        {
            if (items.IsReadOnly)
            {
                throw new NotSupportedException("Insert is not supported for readonly source.");
            }

            if (index < 0 || index >= items.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            SetItem(index, value);
        }
    }

#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public void Clear()
    {
        if (items.IsReadOnly)
        {
            throw new NotSupportedException("Insert is not supported for readonly source.");
        }

        ClearItems();
    }

#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public bool Contains(MappingItem<TSelectedItem> item)
    {
        return items.Contains(item);
    }

    public IEnumerator<MappingItem<TSelectedItem>> GetEnumerator()
    {
        return items.GetEnumerator();
    }

#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public int IndexOf(MappingItem<TSelectedItem> item)
    {
        return items.IndexOf(item);
    }
    public void Insert(int index, MappingItem<TSelectedItem> item)
    {
        if (items.IsReadOnly)
        {
            throw new NotSupportedException("Insert is not supported for readonly source.");
        }
        if (index < 0 || index > items.Count)
        {
            throw new IndexOutOfRangeException("Invalid index!");
        }
        InsertItem(index, item);
    }

    public bool Remove(MappingItem<TSelectedItem> item)
    {
        if (items.IsReadOnly)
        {
            throw new NotSupportedException("Insert is not supported for readonly source.");
        }

        int index = items.IndexOf(item);
        if (index < 0) return false;
        RemoveItem(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        if (items.IsReadOnly)
        {
            throw new NotSupportedException("Insert is not supported for readonly source.");
        }

        if (index < 0 || index >= items.Count)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        RemoveItem(index);
    }

    bool ICollection<MappingItem<TSelectedItem>>.IsReadOnly
    {
        get
        {
            return items.IsReadOnly;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)items).GetEnumerator();
    }

    bool ICollection.IsSynchronized
    {
        get { return false; }
    }

    object ICollection.SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                ICollection c = items as ICollection;
                if (c != null)
                {
                    _syncRoot = c.SyncRoot;
                }
                else
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
            }
            return _syncRoot;
        }
    }

    void ICollection.CopyTo(Array array, int index)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        if (array.Rank != 1)
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
        }

        if (array.GetLowerBound(0) != 0)
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
        }

        if (index < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        if (array.Length - index < Count)
        {
            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
        }

        MappingItem<TSelectedItem>[] tArray = array as MappingItem<TSelectedItem>[];
        if (tArray != null)
        {
            items.CopyTo(tArray, index);
        }
        else
        {
            //
            // Catch the obvious case assignment will fail.
            // We can found all possible problems by doing the check though.
            // For example, if the element type of the Array is derived from MappingItem<TSelectedItem>,
            // we can't figure out if we can successfully copy the element beforehand.
            //
            Type targetType = array.GetType().GetElementType();
            Type sourceType = typeof(MappingItem<TSelectedItem>);
            if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidArrayType);
            }

            //
            // We can't cast array of value type to object[], so we don't support 
            // widening of primitive types here.
            //
            object[] objects = array as object[];
            if (objects == null)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidArrayType);
            }

            int count = items.Count;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    objects[index++] = items[i];
                }
            }
            catch (ArrayTypeMismatchException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_InvalidArrayType);
            }
        }
    }

    object IList.this[int index]
    {
        get { return items[index]; }
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

    bool IList.IsFixedSize
    {
        get
        {
            return _isFixedSize;
        }
    }


    bool IList.Contains(object value)
    {
        if (IsCompatibleObject(value))
        {
            return Contains((MappingItem<TSelectedItem>)value);
        }
        return false;
    }

    int IList.IndexOf(object value)
    {
        if (IsCompatibleObject(value))
        {
            return IndexOf((MappingItem<TSelectedItem>)value);
        }
        return -1;
    }

    void IList.Insert(int index, object value)
    {
        if (items.IsReadOnly)
        {
            throw new NotSupportedException("Insert is not supported for readonly source.");
        }
        ThrowHelper.IfNullAndNullsAreIllegalThenThrow<MappingItem<TSelectedItem>>(value, ExceptionArgument.value);

        try
        {
            Insert(index, (MappingItem<TSelectedItem>)value);
        }
        catch (InvalidCastException)
        {
            ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(MappingItem<TSelectedItem>));
        }

    }

    void IList.Remove(object value)
    {
        if (items.IsReadOnly)
        {
            throw new NotSupportedException("Insert is not supported for readonly source.");
        }

        if (IsCompatibleObject(value))
        {
            Remove((MappingItem<TSelectedItem>)value);
        }
    }

    private static bool IsCompatibleObject(object value)
    {
        // Non-null values are fine.  Only accept nulls if MappingItem<TSelectedItem> is a class or Nullable<U>.
        // Note that default(MappingItem<TSelectedItem>) is not equal to null for value types except when MappingItem<TSelectedItem> is Nullable<U>. 
        return ((value is MappingItem<TSelectedItem>) || (value == null && default(MappingItem<TSelectedItem>) == null));
    }
}
