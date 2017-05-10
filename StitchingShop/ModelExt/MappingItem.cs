using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MappingItem<TSelectedItem>
{
    public MappingItem(TSelectedItem Item)
    {
        this.Item = Item;
    }
    public TSelectedItem Item { get; set; }
}
