using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxTailHelpers;

namespace OxTail.Controls
{
    public interface IFileWatcher : IDisposable
    {
        event EventHandler<EventArgs> FindFinished;

        List<HighlightedItem> SelectedItem { get; }
        FindDetails FindDetails { set; }
        HighlightCollection<HighlightItem> Patterns { get; set; }

        void Start(string filename);
        void ResetSearchCriteria();
    }
}
