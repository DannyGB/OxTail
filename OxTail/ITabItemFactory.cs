using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using OxTail.Controls;
using OxTailHelpers;
using OxTailLogic;

namespace OxTail
{
    public interface ITabItemFactory
    {
        ITabItem CreateTabItem(string filename, IHighlightsHelper hightlightsHelper);
    }
}
