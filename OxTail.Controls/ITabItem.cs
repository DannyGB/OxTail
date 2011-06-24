using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxTail.Controls
{
    public interface ITabItem
    {
        event EventHandler<EventArgs> FindFinished;
        string Uid { get; set; }
    }
}
