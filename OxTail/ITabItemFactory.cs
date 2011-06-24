using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTail.Controls;
using OxTailHelpers;

namespace OxTail
{
    public interface ITabItemFactory
    {       
        ITabItem CreateTabItem(string filename, HighlightCollection<HighlightItem> hightlightCollection);
    }
}
