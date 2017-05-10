using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
public partial class MappingCollection<T>
{
    bool _isReadOnly = false;
    bool _isFixedSize = false;
    public MappingCollection()
    {
        _items = _emptyArray;
    }
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public MappingCollection(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException("Capacity should be apossitive number.");
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
            throw new ArgumentNullException("Source collection can;t be null.");
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
}