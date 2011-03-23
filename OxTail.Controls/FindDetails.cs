using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailLogic.PatternMatching;

namespace OxTail.Controls
{
    /// <summary>
    /// Find options
    /// </summary>
    public enum FindOptions
    {
        CurrentDocument,
        AllOpenDocuments
    }

    public class FindDetails
    {
        public event EventHandler<FindEventArgs> Initiated;
        public string FindCriteria { get; private set; }
        public FindOptions Options { get; private set; }
        public int LastFindIndex { get; set; }

        public FindDetails(string findCriteria, FindOptions options)
            : this(findCriteria, options, 0)
        {
        }

        public FindDetails(string findCriteria, FindOptions options, int lastFindIndex)
        {
            this.FindCriteria = findCriteria;
            this.LastFindIndex = lastFindIndex;
            this.Options = options;
        }

        public void InitiateSearch()
        {
            if (this.Initiated != null)
            {
                this.Initiated(this, new FindEventArgs(this));
            }
        }
    }
}
