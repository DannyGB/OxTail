using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace OxTailLogic
{
    public interface ISystemTray
    {
        Icon Icon { get;}
        ContextMenu ContextMenu { get;}
    }
}
