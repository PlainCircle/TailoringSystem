
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime;
using System.Runtime.CompilerServices;

#if !FEATURE_NETCORE
[Serializable()]
[TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
public partial class MappingCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged
{
    //------------------------------------------------------
    //
    //  Constructors
    //
    //------------------------------------------------------

    #region Constructors
#if !FEATURE_CORECLR
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
    public MappingCollection()
    {
        _items = _emptyArray;
    }

    #endregion Constructors


    //------------------------------------------------------
    //
    //  Public Methods
    //
    //------------------------------------------------------

    #region Public Methods

    /// <summary>
    /// Move item at oldIndex to newIndex.
    /// </summary>
    public void Move(int oldIndex, int newIndex)
    {
        MoveItem(oldIndex, newIndex);
    }

    #endregion Public Methods


    //------------------------------------------------------
    //
    //  Public Events
    //
    //------------------------------------------------------

    #region Public Events

    //------------------------------------------------------
    #region INotifyPropertyChanged implementation
    /// <summary>
    /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            PropertyChanged += value;
        }
        remove
        {
            PropertyChanged -= value;
        }
    }
    #endregion INotifyPropertyChanged implementation


    //------------------------------------------------------
    /// <summary>
    /// Occurs when the collection changes, either by adding or removing an item.
    /// </summary>
    /// <remarks>
    /// see <seealso cref="INotifyCollectionChanged"/>
    /// </remarks>
#if !FEATURE_NETCORE
    [field: NonSerializedAttribute()]
#endif
    public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion Public Events


    //------------------------------------------------------
    //
    //  Protected Methods
    //
    //------------------------------------------------------

    #region Protected Methods

    /// <summary>
    /// Called by base class Collection&lt;T&gt; when the list is being cleared;
    /// raises a CollectionChanged event to any listeners.
    /// </summary>
    protected virtual void ClearItems()
    {
        if (isReadOnly)
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        CheckReentrancy();
        Contract.EndContractBlock();

        if (_size > 0)
        {
            Array.Clear(_items, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            _size = 0;
        }
        _version++;

        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnCollectionReset();
    }

    /// <summary>
    /// Called by base class Collection&lt;T&gt; when an item is removed from list;
    /// raises a CollectionChanged event to any listeners.
    /// </summary>
    protected virtual void RemoveItem(int index)
    {
        if (isReadOnly)
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        if (index < 0 || (uint)index > (uint)_size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
        CheckReentrancy();
        Contract.EndContractBlock();

        T removedItem = this[index];
        _size--;
        if (index < _size)
        {
            Array.Copy(_items, index + 1, _items, index, _size - index);
        }
        _items[_size] = default(T);
        _version++;

        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
    }

    /// <summary>
    /// Called by base class Collection&lt;T&gt; when an item is added to list;
    /// raises a CollectionChanged event to any listeners.
    /// </summary>
    protected virtual void InsertItem(int index, T item)
    {
        if (isReadOnly)
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        if (index < 0 || (uint)index > (uint)_size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
        CheckReentrancy();
        Contract.EndContractBlock();

        if (_size == _items.Length) EnsureCapacity(_size + 1);
        if (index < _size)
        {
            Array.Copy(_items, index, _items, index + 1, _size - index);
        }
        _items[index] = item;
        _size++;
        _version++;

        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
    }

    /// <summary>
    /// Called by base class Collection&lt;T&gt; when an item is set in list;
    /// raises a CollectionChanged event to any listeners.
    /// </summary>
    protected virtual void SetItem(int index, T item)
    {
        CheckReentrancy();
        T originalItem = this[index];

        _items[index] = item;

        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
    }

    /// <summary>
    /// Called by base class MappingCollection&lt;T&gt; when an item is to be moved within the list;
    /// raises a CollectionChanged event to any listeners.
    /// </summary>
    protected virtual void MoveItem(int oldIndex, int newIndex)
    {
        if (isReadOnly)
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        if (oldIndex < 0 || (uint)oldIndex >= (uint)_size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.oldIndex, ExceptionResource.ArgumentOutOfRange_ListInsert);
        if (newIndex < 0 || (uint)newIndex >= (uint)_size)
            ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.newIndex, ExceptionResource.ArgumentOutOfRange_ListInsert);
        CheckReentrancy();
        Contract.EndContractBlock();

        T removedItem = _items[oldIndex];
        int g = (oldIndex < newIndex) ? 1 : -1;
        int i = oldIndex;
        while(i != newIndex)
        {
            _items[i] = _items[i + g];
            i = i + g;
        }
        _items[newIndex] = removedItem;
        _version++;

        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
    }


    /// <summary>
    /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, e);
        }
    }

    /// <summary>
    /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
#if !FEATURE_NETCORE
    [field: NonSerializedAttribute()]
#endif
    protected virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this MappingCollection will raise
    /// a collection changed event through this virtual method.
    /// </summary>
    /// <remarks>
    /// When overriding this method, either call its base implementation
    /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
    /// </remarks>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (CollectionChanged != null)
        {
            using (BlockReentrancy())
            {
                CollectionChanged(this, e);
            }
        }
    }

    /// <summary>
    /// Disallow reentrant attempts to change this collection. E.g. a event handler
    /// of the CollectionChanged event is not allowed to make changes to this collection.
    /// </summary>
    /// <remarks>
    /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
    /// <code>
    ///         using (BlockReentrancy())
    ///         {
    ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
    ///         }
    /// </code>
    /// </remarks>
    protected IDisposable BlockReentrancy()
    {
        _monitor.Enter();
        return _monitor;
    }

    /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
    /// <exception cref="InvalidOperationException"> raised when changing the collection
    /// while another collection change is still being notified to other listeners </exception>
    protected void CheckReentrancy()
    {
        if (_monitor.Busy)
        {
            // we can allow changes if there's only one listener - the problem
            // only arises if reentrant changes make the original event args
            // invalid for later listeners.  This keeps existing code working
            // (e.g. Selector.SelectedItems).
            if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                throw new InvalidOperationException("Can't allow new change if old change is being notified!");
        }
    }

    #endregion Protected Methods


    //------------------------------------------------------
    //
    //  Private Methods
    //
    //------------------------------------------------------

    #region Private Methods
    /// <summary>
    /// Helper to raise a PropertyChanged event  />).
    /// </summary>
    private void OnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
    }

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
    }

    /// <summary>
    /// Helper to raise CollectionChanged event to any listeners
    /// </summary>
    private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
    }

    /// <summary>
    /// Helper to raise CollectionChanged event with action == Reset to any listeners
    /// </summary>
    private void OnCollectionReset()
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    #endregion Private Methods

    //------------------------------------------------------
    //
    //  Private Types
    //
    //------------------------------------------------------

    #region Private Types

    // this class helps prevent reentrant calls
#if !FEATURE_NETCORE
    [Serializable()]
    [TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
    private class SimpleMonitor : IDisposable
    {
        public void Enter()
        {
            ++_busyCount;
        }

        public void Dispose()
        {
            --_busyCount;
        }

        public bool Busy { get { return _busyCount > 0; } }

        int _busyCount;
    }

    #endregion Private Types

    //------------------------------------------------------
    //
    //  Private Fields
    //
    //------------------------------------------------------

    #region Private Fields

    private const string CountString = "Count";

    // This must agree with Binding.IndexerName.  It is declared separately
    // here so as to avoid a dependency on PresentationFramework.dll.
    private const string IndexerName = "Item[]";

    private SimpleMonitor _monitor = new SimpleMonitor();

    #endregion Private Fields
}