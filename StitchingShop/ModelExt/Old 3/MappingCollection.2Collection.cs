using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;

[System.Runtime.InteropServices.ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
partial class MappingCollection<T> : IList<T>, IList, IReadOnlyList<T>
{
    public void Add(T item)
    {
        InsertItem(_size, item);
    }

#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public void Clear()
    {
        if (_isReadOnly)
            throw new InvalidOperationException("Can't change the readonly collection.");
        ClearItems();
    }
    // Removes the element at the given index. The size of the list is
    // decreased by one.
    // 
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public bool Remove(T item)
    {
        if (_isReadOnly)
            throw new InvalidOperationException("Can't change the readonly collection.");
        int index = IndexOf(item);
        if (index < 0) return false;
        RemoveItem(index);
        return true;
    }

    bool ICollection.IsSynchronized
    {
        get { return false; }
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

        T[] tArray = array as T[];
        if (tArray != null)
        {
            Array.Copy(_items, 0, array, 0, _size);
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
            try
            {
                for (int i = 0; i < _size; i++)
                {
                    objects[index++] = _items[i];
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
        get { return this[index]; }
        set { this[index] = (T)value; }
    }

    int IList.Add(object value)
    {
        if (_isReadOnly)
            throw new InvalidOperationException("Can't change the readonly collection.");
        if (!IsCompatibleObject(value))
            return -1;
        Add((T)value);
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

    int IList.IndexOf(object value)
    {
        if (IsCompatibleObject(value))
        {
            return IndexOf((T)value);
        }
        return -1;
    }
    void IList.Remove(object value)
    {
        if (_isReadOnly)
            throw new InvalidOperationException("Can't change the readonly collection.");

        if (IsCompatibleObject(value))
        {
            Remove((T)value);
        }
    }

    private static bool IsCompatibleObject(object value)
    {
        // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
        // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
        return ((value is T) || (value == null && default(T) == null));
    }
}