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
    Collection<MappingItem<TSelectedItem>>, INotifyCollectionChanged, INotifyPropertyChanged
    where TSourceItem : DependencyObject where TSelectedItem : DependencyObject
{
    ObservableCollection<TSourceItem> _SourceList;
    ObservableCollection<TSelectedItem> _SelectedList;
    public MappingCollection(
        ObservableCollection<TSourceItem> SourceList,
        ObservableCollection<TSelectedItem> SelectedList):base(new MappingSource<MappingItem<TSelectedItem>>())
    {
        _SourceList = SourceList;
        _SelectedList = SelectedList;
        _SourceList.CollectionChanged += SourceList_CollectionChanged;
        _SelectedList.CollectionChanged += SelectedList_CollectionChanged;
    }

    private void SourceList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        IList<MappingItem<TSelectedItem>> items = Items;
        TSourceItem[] SourceItems;
        //TSelectedItem[] selectedItems;
        //int i;
        //int startingIndex = 0;
        //int endingIndex;
        //TSourceItem currentItem;
        switch (e.Action)
        {
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
            case NotifyCollectionChangedAction.Reset: //No Data Provided
                SourceItems = _SourceList.ToArray();

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
                break;
        default:
                break;
        }

    }

    private void SelectedList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }
 
}
