using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTailHelpers.Data
{
    public interface IHighlightItemData
    {
        HighlightCollection<HighlightItem> Read();
        int Write(HighlightCollection<HighlightItem> items);
    }
}
