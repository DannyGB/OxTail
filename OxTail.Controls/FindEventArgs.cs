using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailLogic.PatternMatching;

namespace OxTail.Controls
{
    public class FindEventArgs : EventArgs
    {
        public string FindCriteria { get; private set; }
        public FindOptions Options { get; private set; }

        public FindEventArgs(string findCriteria, FindOptions options)
        {
            this.FindCriteria = findCriteria;
            this.Options = options;
        }
    }
}
