using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;
using OxTailHelpers.Data;
using System.ComponentModel;

namespace OxTailLogic
{
    public class HighlightsHelper : IHighlightsHelper
    {
        private HighlightCollection<HighlightItem> patterns;
        private readonly IHighlightItemData HighlightData;

        public HighlightsHelper(IHighlightItemData highlightData )
        {
            this.HighlightData = highlightData;
            this.patterns = this.HighlightData.Read();
            this.patterns.ApplySort(null, ListSortDirection.Descending);
        }

        public HighlightCollection<HighlightItem> Patterns
        {
            get
            {
                return this.patterns;
            }

            private set
            {
                this.patterns = value;
            }
        }

        public HighlightCollection<HighlightItem> Write()
        {
            this.Patterns = this.HighlightData.Write(this.Patterns);
            return this.Patterns;
        }
    }
}
