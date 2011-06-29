using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;

namespace OxTailLogic
{
    public interface IHighlightsHelper
    {
        HighlightCollection<HighlightItem> Patterns { get; }

        HighlightCollection<HighlightItem> Write();
    }
}
